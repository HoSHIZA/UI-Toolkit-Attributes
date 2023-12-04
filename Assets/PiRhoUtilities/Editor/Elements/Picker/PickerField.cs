using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public abstract class PickerField<T> : BaseField<T> where T : class
	{
		#region Class Names

		public const string STYLESHEET = "PickerStyle.uss";
		public const string USS_CLASS_NAME = "pirho-picker-field";
		public const string INPUT_USS_CLASS_NAME = USS_CLASS_NAME + "__input";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string BUTTON_USS_CLASS_NAME = INPUT_USS_CLASS_NAME + "__button";
		public const string ICON_USS_CLASS_NAME = BUTTON_USS_CLASS_NAME + "__icon";
		public const string INPUT_LABEL_USS_CLASS_NAME = BUTTON_USS_CLASS_NAME + "__label";

		#endregion

		#region Members

		protected readonly PickerControl Control;

		#endregion

		#region Public Interface

		protected PickerField(string label, PickerControl control) : base(label, control)
		{
			Control = control;
			Control.AddToClassList(INPUT_USS_CLASS_NAME);
			Control.RegisterCallback<ChangeEvent<T>>(evt =>
			{
				if (evt.currentTarget == Control)
				{
					base.value = evt.newValue;
					evt.StopImmediatePropagation();
				}
			});

			labelElement.AddToClassList(LABEL_USS_CLASS_NAME);

			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);
		}

		public override void SetValueWithoutNotify(T newValue)
		{
			base.SetValueWithoutNotify(newValue);
			Control.SetValueWithoutNotify(newValue);
		}

		#endregion

		#region Visual Input

		protected abstract class PickerControl : VisualElement
		{
			private readonly Button _button;
			private readonly Image _icon;
			private readonly TextElement _label;
			private readonly VisualElement _arrow;
			
			protected PickerProvider<T> Provider;

			public abstract void SetValueWithoutNotify(T newValue);

			public PickerControl()
			{
				_button = new Button();
				_button.AddToClassList(BUTTON_USS_CLASS_NAME);
				_button.AddToClassList(BasePopupField<T, T>.inputUssClassName);
				_button.clicked += () =>
				{
					if (Provider)
                    {
                        SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(new Vector2(worldBound.center.x, worldBound.yMax + worldBound.height - 4)), worldBound.width), Provider);
                    }
                };

				_icon = new Image { pickingMode = PickingMode.Ignore };
				_icon.AddToClassList(ICON_USS_CLASS_NAME);

				_label = new TextElement { pickingMode = PickingMode.Ignore };
				_label.AddToClassList(INPUT_LABEL_USS_CLASS_NAME);

				_arrow = new VisualElement { pickingMode = PickingMode.Ignore };
				_arrow.AddToClassList(BasePopupField<T, T>.arrowUssClassName);

				_button.Add(_icon);
				_button.Add(_label);
				_button.Add(_arrow);

				Add(_button);
			}

			protected void SetLabel(Texture icon, string text)
			{
				((INotifyValueChanged<string>)_label).SetValueWithoutNotify(text);
				_icon.image = icon;
				_icon.SetDisplayed(icon);
			}
		}

		#endregion
	}
}
