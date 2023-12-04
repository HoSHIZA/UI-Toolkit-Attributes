using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(MaximumLengthAttribute))]
    internal class MaximumLengthDrawer : PropertyDrawer
	{
		private const string INVALID_DRAWER_WARNING = "(PUMLDID) invalid drawer for MaximumLengthAttribute on field {0}: MaximumLength can only be applied to fields with a TextField";
		private const string INVALID_SOURCE_ERROR = "(PUMLDIS) invalid source for MaximumLengthAttribute on field '{0}': an int field, method, or property named '{1}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var maxLengthAttribute = attribute as MaximumLengthAttribute;
			var input = element.Q<TextField>();

			if (input != null)
			{
				void SetMaxLength(int value) => input.maxLength = value;

				if (!ReflectionHelper.SetupValueSourceCallback(maxLengthAttribute.MaximumLengthSource, fieldInfo.DeclaringType, property, input, maxLengthAttribute.MaximumLength, maxLengthAttribute.AutoUpdate, SetMaxLength))
                {
                    Debug.LogWarningFormat(INVALID_SOURCE_ERROR, property.propertyPath, maxLengthAttribute.MaximumLengthSource);
                }
            }
			else
			{
				Debug.LogWarningFormat(INVALID_DRAWER_WARNING, property.propertyPath);
			}

			return element;
		}
	}
}