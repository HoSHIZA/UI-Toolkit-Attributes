using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);

            element.SetEnabled(false);
            
            if (element is Frame)
            {
                var collapse = element.Q(Frame.COLLAPSE_BUTTON_USS_CLASS_NAME);

                collapse?.SetEnabled(true);
            }
            
            return element;
		}
	}
}