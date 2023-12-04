using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(ObjectPickerAttribute))]
	public class ObjectPickerDrawer : PropertyDrawer
	{
		private const string INVALID_PROPERTY_TYPE_WARNING = "(PUOPDIPT) invalid type for ObjectPickerAttribute on field {0}: ObjectPicker can only be applied to Object or derived fields";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				var type = this.GetFieldType();

				return new ObjectPickerField(type).ConfigureProperty(property);
			}
			else
			{
				Debug.LogWarningFormat(property.serializedObject.targetObject, INVALID_PROPERTY_TYPE_WARNING, property.propertyPath);
			}

			return new FieldContainer(property.displayName);
		}
	}
}
