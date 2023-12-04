using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(EulerAttribute))]
    internal class EulerDrawer : PropertyDrawer
	{
		private const string INVALID_TYPE_WARNING = "(PUEEDIT) invalid type for EulerAttribute on field '{0}': Euler can only be applied to Quaternion fields";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.Quaternion)
            {
                return new EulerField().ConfigureProperty(property);
            }
            else
            {
                Debug.LogWarningFormat(INVALID_TYPE_WARNING, property.propertyPath);
            }

            return new FieldContainer(property.displayName);
		}
	}
}