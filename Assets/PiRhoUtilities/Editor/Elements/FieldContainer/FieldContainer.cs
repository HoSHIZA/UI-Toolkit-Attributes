﻿using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class FieldContainer : VisualElement
	{
		#region Class Names

		public const string USS_CLASS_NAME = "pirho-field-container";

		#endregion

		#region Public Interface

		public Label Label { get; private set; }

		public FieldContainer(string label) : this(label, null)
		{
		}

		public FieldContainer(string label, string tooltip)
		{
			Label = new Label { tooltip = tooltip };
			Label.AddToClassList(PropertyField.labelUssClassName);
			Label.AddToClassList(BaseFieldExtensions.LabelUssClassName);

			SetLabel(label);

			AddToClassList(BaseFieldExtensions.UssClassName);
			AddToClassList(USS_CLASS_NAME);
		}

		public void SetLabel(string label)
		{
			if (Label.text != label)
			{
				Label.text = label;

				if (string.IsNullOrEmpty(Label.text))
				{
					AddToClassList(BaseFieldExtensions.NoLabelVariantUssClassName);
					Label.RemoveFromHierarchy();
				}
				else
				{
					if (!Contains(Label))
					{
						Insert(0, Label);
						RemoveFromClassList(BaseFieldExtensions.NoLabelVariantUssClassName);
					}
				}
			}
		}

		#endregion
	}
}
