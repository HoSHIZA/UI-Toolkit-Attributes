using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class InlineField : VisualElement
	{
		public const string STYLESHEET = "InlineStyle.uss";
		public const string USS_CLASS_NAME = "pirho-inline";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string CHILDREN_USS_CLASS_NAME = USS_CLASS_NAME + "__children";

		public InlineField(SerializedProperty property, bool showMemberLabels)
		{
			var childContainer = new VisualElement();
			childContainer.AddToClassList(CHILDREN_USS_CLASS_NAME);

			if (!showMemberLabels)
			{
				var label = new FieldContainer(property.displayName);
				label.AddToClassList(LABEL_USS_CLASS_NAME);
				Add(label);
			}

			foreach (var child in property.Children())
			{
				var field = new PropertyField(child);
				if (!showMemberLabels)
                {
                    field.SetFieldLabel(null);
                }

                childContainer.Add(field);
			}

			Add(childContainer);
			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);
		}
	}
}
