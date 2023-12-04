using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(PlaceholderAttribute))]
    internal class PlaceholderDrawer : PropertyDrawer
	{
		private const string INVALID_DRAWER_WARNING = "(PUPDID) invalid drawer for PlaceholderAttribute on field '{0}': Placeholder can only be applied to fields with TextField drawers";
		private const string INVALID_SOURCE_ERROR = "(PUPDIS) invalid value source for PlaceholderAttribute on field '{0}': a string field, method, or property named '{1}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var placeholderAttribute = attribute as PlaceholderAttribute;
			var textField = element.Q<TextField>();

			if (textField != null)
			{
				var placeholder = new Placeholder();
				placeholder.AddToField(textField);

				void SetText(string value) => placeholder.text = value;

				if (!ReflectionHelper.SetupValueSourceCallback(placeholderAttribute.TextSource, fieldInfo.DeclaringType, property, element, placeholderAttribute.Text, placeholderAttribute.AutoUpdate, SetText))
                {
                    Debug.LogWarningFormat(INVALID_SOURCE_ERROR, property.propertyPath, placeholderAttribute.TextSource);
                }

                return element;
			}
			else
			{
				Debug.LogWarningFormat(INVALID_DRAWER_WARNING, property.propertyPath);
				return new FieldContainer(property.displayName);
			}
		}
	}
}