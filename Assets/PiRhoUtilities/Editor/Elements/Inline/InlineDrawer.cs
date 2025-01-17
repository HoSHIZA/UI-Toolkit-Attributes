﻿using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(InlineAttribute))]
	public class InlineDrawer : PropertyDrawer
	{
		public const string STYLESHEET = "InlineStyle.uss";
		public const string USS_CLASS_NAME = "pirho-inline";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string CHILDREN_USS_CLASS_NAME = USS_CLASS_NAME + "__children";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var showMemberLabels = (attribute as InlineAttribute).ShowMemberLabels;
			return new InlineField(property, showMemberLabels);
		}
	}
}