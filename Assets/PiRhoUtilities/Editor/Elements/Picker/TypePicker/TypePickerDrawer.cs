using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_WARNING = "(PUTPDIT) Invalid type for TypePickerAttribute on field {0}: TypePicker can only be applied to string fields";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				var typeAttribute = attribute as TypePickerAttribute;
				return new TypePickerField(typeAttribute.BaseType, typeAttribute.ShowAbstract).ConfigureProperty(property);
			}
			else
			{
				Debug.LogWarningFormat(INVALID_TYPE_WARNING, property.propertyPath);
				return new FieldContainer(property.displayName);
			}
		}
	}
}
