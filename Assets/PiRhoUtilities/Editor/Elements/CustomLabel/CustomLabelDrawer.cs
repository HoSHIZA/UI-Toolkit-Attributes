using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(CustomLabelAttribute))]
    internal class CustomLabelDrawer : PropertyDrawer
	{
		private const string INVALID_SOURCE_ERROR = "(PUCLDIS) invalid value source for CustomLabelAttribute on field '{0}': a string field, method, or property named '{1}' could not be found";

		private static readonly string[] _labelClasses = new string[] { PropertyField.labelUssClassName, BaseFieldExtensions.LabelUssClassName, Frame.LABEL_USS_CLASS_NAME };

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var labelAttribute = attribute as CustomLabelAttribute;

			foreach (var className in _labelClasses)
			{
				var label = element.Q<Label>(className: className);
				if (label != null)
				{
					void SetLabel(string value) => label.text = value;

					if (!ReflectionHelper.SetupValueSourceCallback(labelAttribute.LabelSource, fieldInfo.DeclaringType, property, label, labelAttribute.Label, labelAttribute.AutoUpdate, SetLabel))
                    {
                        Debug.LogWarningFormat(INVALID_SOURCE_ERROR, property.propertyPath, labelAttribute.LabelSource);
                    }

                    break;
				}
			}

			return element;
		}
	}
}