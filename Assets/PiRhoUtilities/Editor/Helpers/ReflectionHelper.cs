using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public static class ReflectionHelper
	{
		private static readonly Type[] _noParameters = new Type[0];
		private static readonly Type[] _oneParameter = new Type[1];
		private static readonly Type[] _twoParameters = new Type[2];

		private static readonly object[] _oneArgument = new object[1];
		private static readonly object[] _twoArguments = new object[2];

		public static bool SetupValueSourceCallback<TFieldType>(string sourceName, Type declaringType, SerializedProperty property, VisualElement element, TFieldType defaultValue, bool autoUpdate, Action<TFieldType> updateAction)
		{
			if (!string.IsNullOrEmpty(sourceName))
			{
				var valueFunction = GetValueFunction(sourceName, declaringType, property, element, out var needsSchedule, updateAction);
				if (valueFunction == null)
                {
                    return false;
                }

                SetupSchedule(element, () => updateAction(valueFunction()), autoUpdate && needsSchedule);
			}
			else
			{
				updateAction(defaultValue);
			}

			return true;
		}

		public static Func<TFieldType> CreateValueSourceFunction<TFieldType>(string sourceName, SerializedProperty property, VisualElement element, Type declaringType, TFieldType defaultValue)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetValueFunction<TFieldType>(sourceName, declaringType, property, element, out var _, null);
            }

            return () => defaultValue;
		}

		public static Func<TFieldType> CreateFunctionCallback<TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetValueFromMethodFunction<TFieldType>(sourceName, declaringType, property);
            }

            return null;
		}

		public static Func<TParameterOne, TFieldType> CreateFunctionCallback<TParameterOne, TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetValueFromMethodFunction<TParameterOne, TFieldType>(sourceName, declaringType, property);
            }

            return null;
		}

		public static Func<TParameterOne, TParameterTwo, TFieldType> CreateFunctionCallback<TParameterOne, TParameterTwo, TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetValueFromMethodFunction<TParameterOne, TParameterTwo, TFieldType>(sourceName, declaringType, property);
            }

            return null;
		}

		public static Action CreateActionCallback(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetCallback(sourceName, declaringType, property);
            }

            return null;
		}

		public static Action<TParameterOne> CreateActionCallback<TParameterOne>(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetCallback<TParameterOne>(sourceName, declaringType, property);
            }

            return null;
		}

		public static Action<TParameterOne, TParameterTwo> CreateActionCallback<TParameterOne, TParameterTwo>(string sourceName, Type declaringType, SerializedProperty property)
		{
			if (!string.IsNullOrEmpty(sourceName))
            {
                return GetCallback<TParameterOne, TParameterTwo>(sourceName, declaringType, property);
            }

            return null;
		}

		private static Func<TFieldType> GetValueFunction<TFieldType>(string sourceName, Type declaringType, SerializedProperty property, VisualElement element, out bool needsSchedule, Action<TFieldType> updateAction)
		{
			needsSchedule = true;

			{ // Property
				var changeTrigger = GetSerializedPropertyTrigger(sourceName, property, updateAction);
				if (changeTrigger != null)
				{
					needsSchedule = false;
					element.Add(changeTrigger);
					return () => changeTrigger.value;
				}
			}

			{ // Method
				var valueFunction = GetValueFromMethodFunction<TFieldType>(sourceName, declaringType, property);
				if (valueFunction != null)
                {
                    return valueFunction;
                }
            }

			{ // Property
				var valueFunction = GetValueFromPropertyFunction<TFieldType>(sourceName, declaringType, property);
				if (valueFunction != null)
                {
                    return valueFunction;
                }
            }

			{ // Field
				var valueFunction = GetValueFromFieldFunction<TFieldType>(sourceName, declaringType, property, ref needsSchedule);
				if (valueFunction != null)
                {
                    return valueFunction;
                }
            }

			return null;
		}

		private static ChangeTrigger<TFieldType> GetSerializedPropertyTrigger<TFieldType>(string sourceName, SerializedProperty property, Action<TFieldType> updateAction)
		{
			var propertyType = SerializedPropertyExtensions.GetPropertyType<TFieldType>();
			
			if (propertyType != SerializedPropertyType.Generic && propertyType != SerializedPropertyType.ManagedReference && propertyType != SerializedPropertyType.ExposedReference && propertyType != SerializedPropertyType.FixedBufferSize)
			{
				var sibling = property.GetSibling(sourceName);

				if (sibling != null && sibling.propertyType != SerializedPropertyType.Generic)
                {
                    return new ChangeTrigger<TFieldType>(sibling, (changedProperty, oldValue, newValue) => updateAction?.Invoke(newValue));
                }
            }

			return null;
		}

		private static Func<TFieldType> GetValueFromMethodFunction<TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _noParameters, null);

			if (method != null)
			{
				if (typeof(TFieldType).IsAssignableFrom(method.ReturnType))
				{
					var owner = method.IsStatic ? null : property.GetOwner<object>();
					return () => (TFieldType)method.Invoke(owner, null);
				}
			}

			return null;
		}

		private static Func<TParameterOne, TFieldType> GetValueFromMethodFunction<TParameterOne, TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			_oneParameter[0] = typeof(TParameterOne);

			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _oneParameter, null);

			if (method != null)
			{
				if (typeof(TFieldType).IsAssignableFrom(method.ReturnType))
				{
					var owner = method.IsStatic ? null : property.GetOwner<object>();
					return (parameterOne) =>
					{
						_oneArgument[0] = parameterOne;
						return (TFieldType)method.Invoke(owner, _oneArgument);
					};
				}
			}

			return null;
		}

		private static Func<TParameterOne, TParameterTwo, TFieldType> GetValueFromMethodFunction<TParameterOne, TParameterTwo, TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			_twoParameters[0] = typeof(TParameterOne);
			_twoParameters[1] = typeof(TParameterTwo);

			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _twoParameters, null);

			if (method != null)
			{
				if (typeof(TFieldType).IsAssignableFrom(method.ReturnType))
				{
					var owner = method.IsStatic ? null : property.GetOwner<object>();
					return (parameterOne, parameterTwo) =>
					{
						_twoArguments[0] = parameterOne;
						_twoArguments[1] = parameterTwo;
						return (TFieldType)method.Invoke(owner, _twoArguments);
					};
				}
			}

			return null;
		}

		private static Action GetCallback(string sourceName, Type declaringType, SerializedProperty property)
		{
			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _noParameters, null);

			if (method != null)
			{
				var owner = method.IsStatic ? null : property.GetOwner<object>();
				return () => method.Invoke(owner, null);
			}

			return null;
		}

		private static Action<TParameterOne> GetCallback<TParameterOne>(string sourceName, Type declaringType, SerializedProperty property)
		{
			_oneParameter[0] = typeof(TParameterOne);

			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _oneParameter, null);

			if (method != null)
			{
				var owner = method.IsStatic ? null : property.GetOwner<object>();
				return (parameterOne) =>
				{
					_oneArgument[0] = parameterOne;
					method.Invoke(owner, _oneArgument);
				};
			}

			return null;
		}

		private static Action<TParameterOne, TParameterTwo> GetCallback<TParameterOne, TParameterTwo>(string sourceName, Type declaringType, SerializedProperty property)
		{
			_twoParameters[0] = typeof(TParameterOne);
			_twoParameters[1] = typeof(TParameterTwo);

			var method = declaringType.GetMethod(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard | CallingConventions.HasThis, _twoParameters, null);

			if (method != null)
			{
				var owner = method.IsStatic ? null : property.GetOwner<object>();
				return (parameterOne, parameterTwo) =>
				{
					_twoArguments[0] = parameterOne;
					_twoArguments[1] = parameterTwo;
					method.Invoke(owner, _twoArguments);
				};
			}

			return null;
		}

		private static Func<TFieldType> GetValueFromPropertyFunction<TFieldType>(string sourceName, Type declaringType, SerializedProperty property)
		{
			var prop = declaringType.GetProperty(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, typeof(TFieldType), _noParameters, null);

			if (prop != null)
			{
				if (prop.CanRead)
				{
					var owner = prop.GetGetMethod(true).IsStatic ? null : property.GetOwner<object>();
					return () => (TFieldType)prop.GetValue(owner);
				}
			}

			return null;
		}

		private static Func<TFieldType> GetValueFromFieldFunction<TFieldType>(string sourceName, Type declaringType, SerializedProperty property, ref bool needsSchedule)
		{
			var field = declaringType.GetField(sourceName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (field != null)
			{
				if (field.IsLiteral || field.IsInitOnly)
                {
                    needsSchedule = false;
                }

                if (typeof(TFieldType) == field.FieldType)
				{
					var owner = field.IsStatic ? null : property.GetOwner<object>();
					return () => (TFieldType)field.GetValue(owner);
				}
			}

			return null;
		}

		private static void SetupSchedule(VisualElement element, Action action, bool autoUpdate)
		{
			action.Invoke();

			if (autoUpdate)
            {
                element.schedule.Execute(action).Every(100);
            }
        }
	}
}
