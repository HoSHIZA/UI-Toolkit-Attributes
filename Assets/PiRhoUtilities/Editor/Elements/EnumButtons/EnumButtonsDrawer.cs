using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(EnumButtonsAttribute))]
    internal class EnumButtonsDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_WARNING = "(PUEBDIT) invalid type for EnumButtonsAttribute on field '{0}': EnumButtons can only be applied to Enum fields";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				var flags = attribute as EnumButtonsAttribute;
				var fieldType = this.GetFieldType();
				var field = new EnumButtonsField
				{
					Type = fieldType,
					value = Enum.ToObject(fieldType, property.intValue) as Enum
				};

				if (flags.Flags.HasValue)
                {
                    field.UseFlags = true;
                }

                return field.ConfigureProperty(property);
			}
			else
			{
				Debug.LogWarningFormat(INVALID_TYPE_WARNING, property.propertyPath);
				return new FieldContainer(property.displayName);
			}
		}
	}
}