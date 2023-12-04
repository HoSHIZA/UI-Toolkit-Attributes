using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(ComboBoxAttribute))]
    internal class ComboBoxDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_ERROR = "(PUCBDIT) invalid type for ComboBoxAttribute on field '{0}': ComboBox can only be applied to string fields";
		private const string INVALID_OPTIONS_ERROR = "(PUCBDIO) invalid options source for ComboBoxAttribute on field '{0}': a List<string> field, method, or property nameed '{1}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				var comboBoxAttribute = attribute as ComboBoxAttribute;
				var comboBox = new ComboBoxField();

				void Options(IEnumerable<string> value) => comboBox.Options = value.ToList();

				if (!ReflectionHelper.SetupValueSourceCallback(comboBoxAttribute.OptionsSource, fieldInfo.DeclaringType, property, comboBox, comboBoxAttribute.Options, comboBoxAttribute.AutoUpdate, Options))
                {
                    Debug.LogWarningFormat(INVALID_OPTIONS_ERROR, property.propertyPath, comboBoxAttribute.OptionsSource);
                }

                return comboBox.ConfigureProperty(property);
			}
			else
			{
				Debug.LogErrorFormat(INVALID_TYPE_ERROR, property.propertyPath);
				return new FieldContainer(property.displayName);
			}
		}
	}
}