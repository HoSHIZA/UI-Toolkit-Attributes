using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(StretchAttribute))]
    internal class StretchDrawer : PropertyDrawer
	{
		public const string STYLESHEET = "StretchStyle.uss";
		public const string USS_CLASS_NAME = "pirho-stretch";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			element.AddToClassList(USS_CLASS_NAME);
			element.AddStyleSheet(STYLESHEET);

			return element;
		}
	}
}