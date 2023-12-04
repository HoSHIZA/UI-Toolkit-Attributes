using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(PopupAttribute))]
    internal class PopupDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_ERROR = "(PUPDIT) invalid type for PopupAttribute on field '{0}': Popup can only be applied to int, float, or string fields";
		private const string INVALID_VALUES_SOURCE_ERROR = "(PUPDIVS) invalid values source for PopupAttribute on field '{0}': a field, method, or property of type '{1}' named '{2}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var popupAttribute = attribute as PopupAttribute;

			if (property.propertyType == SerializedPropertyType.Integer)
            {
                return CreatePopup(property, popupAttribute, popupAttribute.IntValues);
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                return CreatePopup(property, popupAttribute, popupAttribute.FloatValues);
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                return CreatePopup(property, popupAttribute, popupAttribute.StringValues);
            }
            else
            {
                Debug.LogErrorFormat(INVALID_TYPE_ERROR, property.propertyPath);
            }

            return new FieldContainer(property.displayName);
		}

		private VisualElement CreatePopup<T>(SerializedProperty property, PopupAttribute popupAttribute, List<T> defaultValues)
		{
			var popup = new PopupField<T>();

			void SetValues(List<T> values) => popup.SetValues(values);
			void SetValuesWithOptions(PopupValues<T> options) => popup.SetValues(options.Values, options.Options);

			if (!ReflectionHelper.SetupValueSourceCallback(popupAttribute.ValuesSource, fieldInfo.DeclaringType, property, popup, defaultValues, popupAttribute.AutoUpdate, SetValues))
			{
				var defaultOptions = new PopupValues<T> { Values = defaultValues, Options = popupAttribute.Options };

				if (!ReflectionHelper.SetupValueSourceCallback(popupAttribute.ValuesSource, fieldInfo.DeclaringType, property, popup, defaultOptions, popupAttribute.AutoUpdate, SetValuesWithOptions))
                {
                    Debug.LogWarningFormat(INVALID_VALUES_SOURCE_ERROR, property.propertyPath, nameof(PopupValues<T>), popupAttribute.ValuesSource);
                }
            }

			return popup.ConfigureProperty(property);
		}
	}
}
