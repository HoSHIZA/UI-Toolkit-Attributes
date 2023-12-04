using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(MultilineAttribute))]
    internal class MultilineDrawer : PropertyDrawer
	{
		private const string INVALID_DRAWER_WARNING = "(PUMDID) invalid drawer for MultilineAttribute on field {0}: the element does not have a TextField";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var input = element.Q<TextField>();

			if (input != null)
            {
                input.multiline = true;
            }
            else
            {
                Debug.LogWarningFormat(INVALID_DRAWER_WARNING, property.propertyPath);
            }

            return element;
		}
	}
}