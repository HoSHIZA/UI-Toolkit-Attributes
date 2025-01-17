﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public static class PropertyDrawerExtensions
	{
        #region Internal Lookups

        private const string CHANGED_INTERNALS_ERROR = "(PUPDECI) failed to setup PropertyDrawer: Unity internals have changed";

        // ReSharper disable InconsistentNaming
        private static FieldInfo m_FieldInfo;
        private static FieldInfo m_Attribute;
        // ReSharper restore InconsistentNaming

        private const string SCRIPT_ATTRIBUTE_UTILITY_TYPE_NAME = "UnityEditor.ScriptAttributeUtility, UnityEditor";
#if UNITY_2023_3_OR_NEWER
        private static readonly object[] _getDrawerTypeForTypeParameters = new object[3];
#else
        private static readonly object[] _getDrawerTypeForTypeParameters = new object[1];
#endif
        
        private static readonly MethodInfo _getDrawerTypeForTypeMethod;

        static PropertyDrawerExtensions()
        {
            var propertyDrawer = typeof(PropertyDrawer);
            var fieldInfo = propertyDrawer.GetField(nameof(m_FieldInfo), BindingFlags.Instance | BindingFlags.NonPublic);
            var attribute = propertyDrawer.GetField(nameof(m_Attribute), BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo != null && fieldInfo.FieldType == typeof(FieldInfo))
            {
                m_FieldInfo = fieldInfo;
            }

            if (attribute != null && attribute.FieldType == typeof(PropertyAttribute))
            {
                m_Attribute = attribute;
            }

            var scriptAttributeUtilityType = Type.GetType(SCRIPT_ATTRIBUTE_UTILITY_TYPE_NAME);
            var getDrawerTypeForTypeMethod = scriptAttributeUtilityType?.GetMethod(nameof(GetDrawerTypeForType), 
                BindingFlags.Static | BindingFlags.NonPublic);

            if (getDrawerTypeForTypeMethod != null &&
#if UNITY_2023_3_OR_NEWER
                getDrawerTypeForTypeMethod.HasSignature(typeof(Type), typeof(Type), null, typeof(bool)))
#else
                getDrawerTypeForTypeMethod.HasSignature(typeof(Type), typeof(Type)))
#endif
            {
                _getDrawerTypeForTypeMethod = getDrawerTypeForTypeMethod;
            }

            if (_getDrawerTypeForTypeMethod == null || m_FieldInfo == null || m_Attribute == null)
            {
                Debug.LogError(CHANGED_INTERNALS_ERROR);
            }
        }

        #endregion

		#region Helper Methods

		public static Type GetDrawerTypeForType(Type type)
		{
			_getDrawerTypeForTypeParameters[0] = type;
            
#if UNITY_2023_3_OR_NEWER
            _getDrawerTypeForTypeParameters[1] = new Type[] { UnityEngine.Rendering.GraphicsSettings.currentRenderPipelineAssetType };
            _getDrawerTypeForTypeParameters[2] = !type.IsValueType;
#endif
            
			var result = _getDrawerTypeForTypeMethod?.Invoke(null, _getDrawerTypeForTypeParameters) as Type;
            
            return result;
        }

		#endregion

		#region Extension Methods

		public static void SetFieldInfo(this PropertyDrawer drawer, FieldInfo value)
		{
			m_FieldInfo?.SetValue(drawer, value);
		}

		public static void SetAttribute(this PropertyDrawer drawer, PropertyAttribute value)
		{
			m_Attribute?.SetValue(drawer, value);
		}

		public static Type GetFieldType(this PropertyDrawer drawer)
		{
			return drawer.fieldInfo.GetFieldType();
		}

		public static string GetTooltip(this PropertyDrawer drawer)
		{
			return drawer.fieldInfo.GetTooltip();
		}

		public static VisualElement CreateNextElement(this PropertyDrawer drawer, SerializedProperty property)
        {
			var nextDrawer = GetNextDrawer(drawer);

			if (nextDrawer != null)
			{
				var element = nextDrawer.CreatePropertyGUI(property);

				return element ?? new ImGuiDrawer(property, nextDrawer);
			}

			return property.CreateField();
		}

		public static PropertyDrawer GetNextDrawer(this PropertyDrawer drawer)
		{
			var nextAttribute = GetNextAttribute(drawer);
			var drawerType = GetDrawerTypeForType(nextAttribute?.GetType() ?? GetFieldType(drawer));

			if (drawerType != null)
			{
				var nextDrawer = drawerType.CreateInstance<PropertyDrawer>();
				nextDrawer.SetFieldInfo(drawer.fieldInfo);
				nextDrawer.SetAttribute(nextAttribute);
                
				return nextDrawer;
			}

			return null;
		}

		public static PropertyAttribute GetNextAttribute(this PropertyDrawer drawer)
        {
            var attributes = drawer.fieldInfo
                .GetCustomAttributes<PropertyAttribute>()
#if UNITY_2021_1_OR_NEWER
                .OrderBy(attribute => attribute.order)
#else
                .OrderByDescending(attribute => attribute.order)
#endif
                .SkipWhile(attribute => attribute.GetType() != drawer.attribute.GetType())
                .Where(attribute =>
                {
                    var drawerType = GetDrawerTypeForType(attribute.GetType());
                    return drawerType != null && drawerType.IsCreatableAs<PropertyDrawer>();
                });
            
            return attributes.ElementAtOrDefault(1);
        }

		public static VisualElement CreateNextElement(FieldInfo field, Attribute attribute, SerializedProperty property)
		{
			var nextDrawer = GetNextDrawer(field, attribute);

			if (nextDrawer != null)
			{
				var element = nextDrawer.CreatePropertyGUI(property);
                
				return element ?? new ImGuiDrawer(property, nextDrawer);
			}

			return property.CreateField();
		}

		public static PropertyDrawer GetNextDrawer(FieldInfo field, Attribute attribute)
		{
			var nextAttribute = GetNextAttribute(field, attribute);
			var drawerType = GetDrawerTypeForType(nextAttribute?.GetType() ?? field.GetFieldType());

			if (drawerType != null)
			{
				var nextDrawer = drawerType.CreateInstance<PropertyDrawer>();
				nextDrawer.SetFieldInfo(field);
				nextDrawer.SetAttribute(nextAttribute);
				return nextDrawer;
			}

			return null;
		}

		public static PropertyAttribute GetNextAttribute(FieldInfo field, Attribute thisAttribute)
		{
			return field.GetCustomAttributes<PropertyAttribute>()
				.OrderByDescending(attribute => attribute.order)
				.ThenBy(attribute => attribute.GetType().FullName) // GetCustomAttributes might return a different order so a secondary sort is needed even though it is a stable sort
				.SkipWhile(attribute => attribute.GetType() != thisAttribute.GetType())
				.Where(attribute =>
				{
					var drawerType = GetDrawerTypeForType(attribute.GetType());
					return drawerType != null && drawerType.IsCreatableAs<PropertyDrawer>();
				})
				.ElementAtOrDefault(1);
		}

		#endregion
	}
}