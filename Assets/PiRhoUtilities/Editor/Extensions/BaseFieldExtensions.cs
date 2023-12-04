using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public static class BaseFieldExtensions
	{
		#region Internal Lookups

		private const string CHANGED_INTERNALS_ERROR = "(PUBFECI) failed to setup BaseField: Unity internals have changed";

		public static readonly string UssClassName = BaseField<int>.ussClassName;
		public static readonly string LabelUssClassName = BaseField<int>.labelUssClassName;
		public static readonly string NoLabelVariantUssClassName = BaseField<int>.noLabelVariantUssClassName;
        
		private const string VISUAL_INPUT_NAME = "visualInput";
        
		private const string CONFIGURE_FIELD_NAME = "ConfigureField";
        
		private static readonly MethodInfo _configureFieldMethod;
        
#if UNITY_2021_1_OR_NEWER
		private static readonly object[] _configureFieldParameters = new object[3];
#else
		private static readonly object[] _configureFieldParameters = new object[2];
#endif
        
		private static readonly PropertyField _configureFieldInstance;

		static BaseFieldExtensions()
		{
			var configureFieldMethod = typeof(PropertyField).GetMethod(CONFIGURE_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);

			if (configureFieldMethod != null && configureFieldMethod.HasSignature(typeof(VisualElement), 
#if UNITY_2021_1_OR_NEWER
                    null, typeof(SerializedProperty), null))
#else
                    null, typeof(SerializedProperty)))
#endif
			{
				_configureFieldMethod = configureFieldMethod;
				_configureFieldInstance = new PropertyField();
			}

			if (_configureFieldMethod == null)
            {
                Debug.LogError(CHANGED_INTERNALS_ERROR);
            }
        }

		// can't do these lookups in a static constructor since they are dependent on the generic type

		private static PropertyInfo GetProperty<T>(string name, BindingFlags flags)
		{
			return GetProperty(typeof(BaseField<T>), name, flags);
		}

		private static PropertyInfo GetProperty(Type type, string name, BindingFlags flags)
		{
			var property = type.GetProperty(name, flags);

			if (property == null)
            {
                Debug.LogError(CHANGED_INTERNALS_ERROR);
            }

            return property;
		}

		#endregion

		#region Extension Methods

		public static VisualElement GetVisualInput<T>(this BaseField<T> field)
		{
			return GetProperty<T>(VISUAL_INPUT_NAME, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(field) as VisualElement;
		}

		public static void SetVisualInput<T>(this BaseField<T> field, VisualElement element)
		{
			GetProperty<T>(VISUAL_INPUT_NAME, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(field, element);
		}

		public static VisualElement ConfigureProperty<T>(this BaseField<T> field, SerializedProperty property)
		{
			field.labelElement.tooltip = property.GetTooltip(); // it seems like this should happen internally somewhere but it doesn't

			// ConfigureField is effectively static, with one unimportant exception, so it can be called with a dummy
			// instance. The exception is label, which will be null on the dummy instance resulting in the desired
			// effect anyway (using the property name for the label).

			var method = _configureFieldMethod?.MakeGenericMethod(field.GetType(), typeof(T));

			_configureFieldParameters[0] = field;
			_configureFieldParameters[1] = property;
#if UNITY_2021_1_OR_NEWER
            _configureFieldParameters[2] = null;
#endif

			return method?.Invoke(_configureFieldInstance, _configureFieldParameters) as VisualElement;
		}

		#endregion
	}
}