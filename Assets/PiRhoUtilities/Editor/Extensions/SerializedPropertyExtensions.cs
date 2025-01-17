﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
	public static class SerializedPropertyExtensions
	{
		#region Internal Lookups

		private const string CHANGED_INTERNALS_ERROR = "(PUPFECI) failed to setup SerializedProperty: Unity internals have changed";

#if UNITY_2021_1_OR_NEWER
        private const string CREATE_FIELD_FROM_PROPERTY_NAME = "CreateOrUpdateFieldFromProperty";
		private static readonly object[] _createFieldFromPropertyParameters = new object[2];
#else
		private const string CREATE_FIELD_FROM_PROPERTY_NAME = "CreateFieldFromProperty";
		private static readonly object[] _createFieldFromPropertyParameters = new object[1];
#endif

		private static readonly MethodInfo _createFieldFromPropertyMethod;
		private static readonly PropertyField _createFieldFromPropertyInstance;

		private const string GRADIENT_VALUE_NAME = "gradientValue";
		private static readonly PropertyInfo _gradientValueProperty;

		private const string HAS_VISIBLE_CHILD_FIELDS_NAME = "HasVisibleChildFields";
		private static readonly MethodInfo _hasVisibleChildFieldsMethod;
        
#if UNITY_2020_1_OR_NEWER
		private static readonly object[] _hasVisibleChildFieldsParameters = new object[2];
#else
		private static readonly object[] _hasVisibleChildFieldsParameters = new object[1];
#endif

#if UNITY_2020_1_OR_NEWER
        private static readonly MethodInfo _setPropertyMethod;
        
        // ReSharper disable once InconsistentNaming
        private static readonly FieldInfo m_ParentPropertyField;
#endif

		static SerializedPropertyExtensions()
		{
            var createFieldFromPropertyMethod = typeof(PropertyField).GetMethod(CREATE_FIELD_FROM_PROPERTY_NAME, 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (createFieldFromPropertyMethod != null && 
#if UNITY_2021_1_OR_NEWER
                createFieldFromPropertyMethod.HasSignature(typeof(VisualElement), typeof(SerializedProperty), typeof(object)))
#else
                createFieldFromPropertyMethod.HasSignature(typeof(VisualElement), typeof(SerializedProperty)))
#endif
            {
                _createFieldFromPropertyMethod = createFieldFromPropertyMethod;
                _createFieldFromPropertyInstance = new PropertyField();
            }

            var gradientValueProperty = typeof(SerializedProperty).GetProperty(GRADIENT_VALUE_NAME, BindingFlags.Instance |
#if UNITY_2022_1_OR_NEWER
                BindingFlags.Public
#else
                BindingFlags.NonPublic
#endif
                );

            if (gradientValueProperty != null && gradientValueProperty.PropertyType == typeof(Gradient) &&
                gradientValueProperty.CanRead && gradientValueProperty.CanWrite)
            {
                _gradientValueProperty = gradientValueProperty;
            }

            var hasVisibleChildFieldsMethod = typeof(EditorGUI).GetMethod(HAS_VISIBLE_CHILD_FIELDS_NAME, BindingFlags.Static | BindingFlags.NonPublic);

            if (hasVisibleChildFieldsMethod != null && hasVisibleChildFieldsMethod.HasSignature(typeof(bool),
#if UNITY_2020_1_OR_NEWER
                    typeof(SerializedProperty), typeof(bool)))
#else 
                    typeof(SerializedProperty)))
#endif
            {
                _hasVisibleChildFieldsMethod = hasVisibleChildFieldsMethod;
            }

#if UNITY_2020_1_OR_NEWER
            _setPropertyMethod = typeof(VisualElement)
                .GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.NonPublic);

            m_ParentPropertyField = typeof(PropertyField)
                .GetField("m_ParentPropertyField", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
                
            if (_createFieldFromPropertyMethod == null || _gradientValueProperty == null || _hasVisibleChildFieldsMethod == null)
            {
                Debug.LogError(CHANGED_INTERNALS_ERROR);
            }
		}

		#endregion

		#region Extension Methods

		public static string GetTooltip(this SerializedProperty property)
		{
			var obj = property.GetOwner<object>();
			var type = obj?.GetType();
			var field = type?.GetField(property.name);
            
			return field?.GetTooltip();
		}

		public static IEnumerable<SerializedProperty> Children(this SerializedProperty property)
		{
			if (property.isArray)
			{
				for (var i = 0; i < property.arraySize; i++)
                {
                    yield return property.GetArrayElementAtIndex(i);
                }
            }
			else if (string.IsNullOrEmpty(property.propertyPath))
			{
				var iterator = property.Copy();
				var valid = iterator.NextVisible(true);

				while (valid)
				{
					yield return iterator.Copy();
                    
					valid = iterator.NextVisible(false);
				}
			}
			else
			{
				var iterator = property.Copy();
				var end = iterator.GetEndProperty();
				var valid = iterator.NextVisible(true);

				while (valid && !SerializedProperty.EqualContents(iterator, end))
				{
					yield return iterator.Copy();
                    
					valid = iterator.NextVisible(false);
				}
			}
		}

		// this property is internal for some reason
		public static Gradient GetGradientValue(this SerializedProperty property)
		{
			return _gradientValueProperty.GetValue(property) as Gradient;
		}

		public static void SetGradientValue(this SerializedProperty property, Gradient gradient)
		{
			_gradientValueProperty.SetValue(property, gradient);
		}

		public static Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference 
                ? ParseType(property.managedReferenceFieldTypename) 
                : null;
        }

		public static Type GetManagedReferenceValueType(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference 
                ? ParseType(property.managedReferenceFullTypename) 
                : null;
        }

		public static bool HasManagedReferenceValue(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference && 
                   !string.IsNullOrEmpty(property.managedReferenceFullTypename);
        }

		private static readonly Regex _unityType = new Regex(@"(\S+) ([^/]+)(?:/(.+))?", RegexOptions.Compiled);
		private static readonly Dictionary<string, Type> _unityTypeMap = new Dictionary<string, Type>();

		private static Type ParseType(string unityName)
		{
			if (!_unityTypeMap.TryGetValue(unityName, out var type))
			{
				var match = _unityType.Match(unityName);

				if (match.Success)
				{
					// unity format is "Assembly Type" or "Assembly Parent/Type"
					// c# format is "Type, Assembly" or "Parent+Type, Assembly" but Assembly must be fully qualified

					var assembly = match.Groups[1].Value;
					var name = match.Groups[2].Value;
					var nested = match.Groups[3].Value;
					var fullName = string.IsNullOrEmpty(nested) ? name : $"{name}+{nested}";

					var a = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(am => am.GetName().Name == assembly);
					type = a.GetType(fullName);
                    
					_unityTypeMap.Add(unityName, type);
				}
			}

			return type;
		}

		public static object GetManagedReferenceValue(this SerializedProperty property)
        {
            return property.GetObject<object>(); // PENDING: slow but property.managedReferenceValue is write only
        }

		public static VisualElement CreateField(this SerializedProperty property)
        {
            _createFieldFromPropertyParameters[0] = property;
            
#if UNITY_2021_1_OR_NEWER
            _createFieldFromPropertyParameters[1] = property.GetManagedReferenceValue();
#endif

            var element = _createFieldFromPropertyMethod?
                .Invoke(_createFieldFromPropertyInstance, _createFieldFromPropertyParameters) as VisualElement;
            
            return element;
        }

#if UNITY_2020_1_OR_NEWER
        public static VisualElement CreateFoldout(this SerializedProperty property)
        {
            property = property.Copy();
            
            var foldout = new Foldout
            {
                text = property.displayName,
                value = property.isExpanded,
                bindingPath = property.propertyPath,
                name = "unity-foldout-" + property.propertyPath
            };

            // Get Foldout label.
            var foldoutToggle = foldout.Q<Toggle>(className: Foldout.toggleUssClassName);
            var foldoutLabel = foldoutToggle.Q<Label>(className: Toggle.textUssClassName);
            foldoutLabel.bindingPath = property.propertyPath;
            
            _setPropertyMethod?.Invoke(foldoutLabel, new object[]{ new PropertyName("unity-foldout-bound-title"), true });
            
            var endProperty = property.GetEndProperty();
            property.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(property, endProperty))
                    break;

                var field = new PropertyField(property);

                m_ParentPropertyField?.SetValue(field, _createFieldFromPropertyInstance);
                
                field.name = "unity-property-field-" + property.propertyPath;
                
                foldout.Add(field);
            } while (property.NextVisible(false));

            return foldout;
        }
        
        public static void RefreshChildrenProperties(this SerializedProperty property)
        {
            typeof(PropertyField)
                .GetMethod("RefreshChildrenProperties", BindingFlags.Instance | BindingFlags.NonPublic)?
                .Invoke(_createFieldFromPropertyInstance, new object[] { property, true });
        }
#endif

		public static bool HasVisibleChildFields(this SerializedProperty property)
		{
            _hasVisibleChildFieldsParameters[0] = property;

#if UNITY_2020_1_OR_NEWER
            _hasVisibleChildFieldsParameters[1] = property.hasVisibleChildren;
#endif
            
            return (bool)_hasVisibleChildFieldsMethod.Invoke(null, _hasVisibleChildFieldsParameters);
		}

		public static void SetToDefault(this SerializedProperty property)
		{
			if (property.isArray)
			{
				property.ClearArray();
			}
			else if (property.hasChildren && property.propertyType != SerializedPropertyType.ManagedReference)
			{
				foreach (var child in property.Children())
                {
                    child.SetToDefault();
                }
            }
			else
			{
				switch (property.propertyType)
				{
					case SerializedPropertyType.Generic: break;
					case SerializedPropertyType.Integer: property.intValue = default; break;
					case SerializedPropertyType.Boolean: property.boolValue = default; break;
					case SerializedPropertyType.Float: property.floatValue = default; break;
					case SerializedPropertyType.String: property.stringValue = string.Empty; break;
					case SerializedPropertyType.Color: property.colorValue = default; break;
					case SerializedPropertyType.ObjectReference: property.objectReferenceValue = default; break;
					case SerializedPropertyType.LayerMask: property.intValue = 0; break;
					case SerializedPropertyType.Enum: property.enumValueIndex = 0; break;
					case SerializedPropertyType.Vector2: property.vector2Value = default; break;
					case SerializedPropertyType.Vector3: property.vector3Value = default; break;
					case SerializedPropertyType.Vector4: property.vector4Value = default; break;
					case SerializedPropertyType.Rect: property.rectValue = default; break;
					case SerializedPropertyType.ArraySize: property.intValue = 0; break;
					case SerializedPropertyType.Character: property.intValue = default; break;
					case SerializedPropertyType.AnimationCurve: property.animationCurveValue = default; break;
					case SerializedPropertyType.Bounds: property.boundsValue = default; break;
					case SerializedPropertyType.Gradient: property.SetGradientValue(default); break;
					case SerializedPropertyType.Quaternion: property.quaternionValue = default; break;
					case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = default; break;
					case SerializedPropertyType.FixedBufferSize: property.intValue = 0; break;
					case SerializedPropertyType.Vector2Int: property.vector2IntValue = default; break;
					case SerializedPropertyType.Vector3Int: property.vector3IntValue = default; break;
					case SerializedPropertyType.RectInt: property.rectIntValue = default; break;
					case SerializedPropertyType.BoundsInt: property.boundsIntValue = default; break;
					case SerializedPropertyType.ManagedReference: property.managedReferenceValue = default; break;
				}
			}
		}

		public static SerializedPropertyType GetPropertyType<T>()
		{
			var type = typeof(T);

			// SerializedPropertyType.Generic
			if (type == typeof(int))
            {
                return SerializedPropertyType.Integer;
            }
            else if (type == typeof(bool))
            {
                return SerializedPropertyType.Boolean;
            }
            else if (type == typeof(float))
            {
                return SerializedPropertyType.Float;
            }
            else if (type == typeof(string))
            {
                return SerializedPropertyType.String;
            }
            else if (type == typeof(Color))
            {
                return SerializedPropertyType.Color;
            }
            else if (typeof(Object).IsAssignableFrom(type))
            {
                return SerializedPropertyType.ObjectReference;
            }
            else if (type == typeof(LayerMask))
            {
                return SerializedPropertyType.LayerMask;
            }
            else if (type == typeof(Enum) || type.IsEnum)
            {
                return SerializedPropertyType.Enum;
            }
            else if (type == typeof(Vector2))
            {
                return SerializedPropertyType.Vector2;
            }
            else if (type == typeof(Vector3))
            {
                return SerializedPropertyType.Vector3;
            }
            else if (type == typeof(Vector4))
            {
                return SerializedPropertyType.Vector4;
            }
            else if (type == typeof(Rect))
            {
                return SerializedPropertyType.Rect;
            }
            // SerializedPropertyType.ArraySize - stored as ints
			else if (type == typeof(char))
            {
                return SerializedPropertyType.Character;
            }
            else if (type == typeof(AnimationCurve))
            {
                return SerializedPropertyType.AnimationCurve;
            }
            else if (type == typeof(Bounds))
            {
                return SerializedPropertyType.Bounds;
            }
            else if (type == typeof(Gradient))
            {
                return SerializedPropertyType.Gradient;
            }
            else if (type == typeof(Quaternion))
            {
                return SerializedPropertyType.Quaternion;
            }
            // SerializedPropertyType.ExposedReference
			// SerializedPropertyType.FixedBufferSize
			else if (type == typeof(Vector2Int))
            {
                return SerializedPropertyType.Vector2Int;
            }
            else if (type == typeof(Vector3Int))
            {
                return SerializedPropertyType.Vector3Int;
            }
            else if (type == typeof(RectInt))
            {
                return SerializedPropertyType.RectInt;
            }
            else if (type == typeof(BoundsInt))
            {
                return SerializedPropertyType.BoundsInt;
            }
            // SerializedPropertyType.ManagedReference - no way to know

			return SerializedPropertyType.Generic;
		}

		public static bool TryGetValue<T>(this SerializedProperty property, out T value)
		{
			// this boxes for value types but I don't think there's a way around that without dynamic code generation

			var type = typeof(T);

			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic: break;
				case SerializedPropertyType.Integer: if (type == typeof(int)) { value = (T)(object)property.intValue; return true; } break;
				case SerializedPropertyType.Boolean: if (type == typeof(bool)) { value = (T)(object)property.boolValue; return true; } break;
				case SerializedPropertyType.Float: if (type == typeof(float)) { value = (T)(object)property.floatValue; return true; } break;
				case SerializedPropertyType.String: if (type == typeof(string)) { value = (T)(object)property.stringValue; return true; } break;
				case SerializedPropertyType.Color: if (type == typeof(Color)) { value = (T)(object)property.colorValue; return true; } break;
				case SerializedPropertyType.ObjectReference: if (typeof(Object).IsAssignableFrom(type)) { value = (T)(object)property.objectReferenceValue; return true; } break;
				case SerializedPropertyType.LayerMask: if (type == typeof(LayerMask)) { value = (T)(object)(LayerMask)property.intValue; return true; } else if (type == typeof(int)) { value = (T)(object)property.intValue; return true; } break;
				case SerializedPropertyType.Enum: if (type == typeof(Enum) || type.IsEnum) { value = (T)(object) property.GetEnumValue(); return true; } break;
				case SerializedPropertyType.Vector2: if (type == typeof(Vector2)) { value = (T)(object)property.vector2Value; return true; } break;
				case SerializedPropertyType.Vector3: if (type == typeof(Vector3)) { value = (T)(object)property.vector3Value; return true; } break;
				case SerializedPropertyType.Vector4: if (type == typeof(Vector4)) { value = (T)(object)property.vector4Value; return true; } break;
				case SerializedPropertyType.Rect: if (type == typeof(Rect)) { value = (T)(object)property.rectValue; return true; } break;
				case SerializedPropertyType.ArraySize: if (type == typeof(int)) { value = (T)(object)property.intValue; return true; } break;
				case SerializedPropertyType.Character: if (type == typeof(char)) { value = (T)(object)(char)property.intValue; return true; } break;
				case SerializedPropertyType.AnimationCurve: if (type == typeof(AnimationCurve)) { value = (T)(object)property.animationCurveValue; return true; } break;
				case SerializedPropertyType.Bounds: if (type == typeof(Bounds)) { value = (T)(object)property.boundsValue; return true; } break;
				case SerializedPropertyType.Gradient: if (type == typeof(Gradient)) { value = (T)(object)property.GetGradientValue(); return true; } break;
				case SerializedPropertyType.Quaternion: if (type == typeof(Quaternion)) { value = (T)(object)property.quaternionValue; return true; } break;
				case SerializedPropertyType.ExposedReference: break;
				case SerializedPropertyType.FixedBufferSize: break;
				case SerializedPropertyType.Vector2Int: if (type == typeof(Vector2Int)) { value = (T)(object)property.vector2IntValue; return true; } break;
				case SerializedPropertyType.Vector3Int: if (type == typeof(Vector3Int)) { value = (T)(object)property.vector3IntValue; return true; } break;
				case SerializedPropertyType.RectInt: if (type == typeof(RectInt)) { value = (T)(object)property.rectIntValue; return true; } break;
				case SerializedPropertyType.BoundsInt: if (type == typeof(BoundsInt)) { value = (T)(object)property.boundsIntValue; return true; } break;
				case SerializedPropertyType.ManagedReference: var managed = property.GetManagedReferenceValue(); if (managed == null) { value = default; return true; } else if (managed is T t) { value = t; return true; } break;
			}

			value = default;
            
			return false;
		}

		public static bool TrySetValue(this SerializedProperty property, object value)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic: return false;
				case SerializedPropertyType.Integer: if (value is int i) { property.intValue = i; return true; } return false;
				case SerializedPropertyType.Boolean: if (value is bool b) { property.boolValue = b; return true; } return false;
				case SerializedPropertyType.Float: if (value is float f) { property.floatValue = f; return true; } return false;
				case SerializedPropertyType.String: if (value is string s) { property.stringValue = s; return true; } return false;
				case SerializedPropertyType.Color: if (value is Color color) { property.colorValue = color; return true; } return false;
				case SerializedPropertyType.ObjectReference: if (value is Object o) { property.objectReferenceValue = o; return true; } return false;
				case SerializedPropertyType.LayerMask: if (value is LayerMask mask) { property.intValue = mask; return true; } return false;
				case SerializedPropertyType.Enum: if (value.GetType().IsEnum || value.GetType() == typeof(Enum)) { return property.SetEnumValue((Enum)value); } return false;
				case SerializedPropertyType.Vector2: if (value is Vector2 v2) { property.vector2Value = v2; return true; } return false;
				case SerializedPropertyType.Vector3: if (value is Vector3 v3) { property.vector3Value = v3; return true; } return false;
				case SerializedPropertyType.Vector4: if (value is Vector4 v4) { property.vector4Value = v4; return true; } return false;
				case SerializedPropertyType.Rect: if (value is Rect rect) { property.rectValue = rect; return true; } return false;
				case SerializedPropertyType.ArraySize: if (value is int size) { property.intValue = size; return true; } return false;
				case SerializedPropertyType.Character: if (value is int character) { property.intValue = character; return true; } return false;
				case SerializedPropertyType.AnimationCurve: if (value is AnimationCurve curve) { property.animationCurveValue = curve; return true; } return false;
				case SerializedPropertyType.Bounds: if (value is Bounds bounds) { property.boundsValue = bounds; return true; } return false;
				case SerializedPropertyType.Gradient: if (value is Gradient gradient) { property.SetGradientValue(gradient); return true; } return false;
				case SerializedPropertyType.Quaternion: if (value is Quaternion q) { property.quaternionValue = q; return true; } return false;
				case SerializedPropertyType.ExposedReference: return false;
				case SerializedPropertyType.FixedBufferSize: return false;
				case SerializedPropertyType.Vector2Int: if (value is Vector2Int v2I) { property.vector2IntValue = v2I; return true; } return false;
				case SerializedPropertyType.Vector3Int: if (value is Vector3Int v3I) { property.vector3IntValue = v3I; return true; } return false;
				case SerializedPropertyType.RectInt: if (value is RectInt recti) { property.rectIntValue = recti; return true; } return false;
				case SerializedPropertyType.BoundsInt: if (value is BoundsInt boundsi) { property.boundsIntValue = boundsi; return true; } return false;
				case SerializedPropertyType.ManagedReference: if (property.GetManagedReferenceFieldType().IsInstanceOfType(value)) { property.managedReferenceValue = value; return true; } return false;
			}

			return false;
		}
        
		public static Enum GetEnumValue(this SerializedProperty property)
		{
			var type = property.GetObject<object>().GetType();
            
			return (Enum)Enum.Parse(type, property.intValue.ToString());
		}

		public static bool SetEnumValue(this SerializedProperty property, Enum value)
		{
			var index = Array.IndexOf(property.enumNames, value.ToString());

			if (index >= 0)
			{
				property.enumValueIndex = index;
				return true;
			}

			return false;
		}

		public static void ResizeArray(this SerializedProperty arrayProperty, int newSize)
		{
			var size = arrayProperty.arraySize;
			arrayProperty.arraySize = newSize;

			// new items will be a copy of the previous last item so this resets them to their default value
			for (var i = size; i < newSize; i++)
            {
                SetToDefault(arrayProperty.GetArrayElementAtIndex(i));
            }
        }

		public static void RemoveFromArray(this SerializedProperty arrayProperty, int index)
		{
			// If an object is removed from an array of ObjectReference, DeleteArrayElementAtIndex will set the item
			// to null instead of removing it. If the entry is already null it will be removed as expected.
			var item = arrayProperty.GetArrayElementAtIndex(index);

			// TODO: check how this behaves with managedReferenceValue
			if (item.propertyType == SerializedPropertyType.ObjectReference && item.objectReferenceValue != null)
            {
                item.objectReferenceValue = null;
            }

            arrayProperty.DeleteArrayElementAtIndex(index);
		}

		public static SerializedProperty GetSibling(this SerializedProperty property, string siblingName)
		{
			var path = property.propertyPath;
			var index = property.propertyPath.LastIndexOf('.');
			var siblingPath = index > 0 ? path.Substring(0, index) + "." + siblingName : siblingName;

			return property.serializedObject.FindProperty(siblingPath);
		}

		public static SerializedProperty GetParent(this SerializedProperty property)
		{
			var path = property.propertyPath;
			var index = property.propertyPath.LastIndexOf('.');

			if (index < 0)
            {
                return property.serializedObject.GetIterator();
            }

            var parentPath = path.Substring(0, index);
            
			return property.serializedObject.FindProperty(parentPath);
		}

		public static T GetOwner<T>(this SerializedProperty property) where T : class
		{
			var index = 1;
			var obj = property.GetAncestorObject<object>(index);
            
			while (obj is IList || obj is IDictionary)
            {
                obj = property.GetAncestorObject<object>(++index);
            }

            return obj as T;
		}

		public static T GetObject<T>(this SerializedProperty property) where T : class
		{
			return property.GetAncestorObject<T>(0);
		}

		public static T GetParentObject<T>(this SerializedProperty property) where T : class
		{
			return property.GetAncestorObject<T>(1);
		}

		public static T GetAncestorObject<T>(this SerializedProperty property, int generations) where T : class
		{
			var obj = (object)property.serializedObject.targetObject;
			var elements = property.propertyPath.Replace("Array.data[", "[").Split('.');
			var count = elements.Length - generations;

			for (var i = 0; i < count; i++)
			{
				var element = elements[i];

				if (element.StartsWith("["))
				{
					var indexString = element.Substring(1, element.Length - 2);
					var index = Convert.ToInt32(indexString);

					obj = GetIndexed(obj, index);
				}
				else
				{
					obj = obj.GetType().GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
				}
			}

			return obj as T;
		}

		public static Type GetType(this SerializedProperty property)
		{
			return GetObject<object>(property).GetType();
		}

		private static object GetIndexed(object obj, int index)
		{
			if (obj is Array array)
            {
                return array.GetValue(index);
            }

            if (obj is IList list)
            {
                return list[index];
            }

            return null;
		}

		#endregion
	}
}