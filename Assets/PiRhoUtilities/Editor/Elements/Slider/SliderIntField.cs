using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class SliderIntField : SliderField<int>
	{
		#region Defaults

		// These match unity's internal defaults
		public const int DEFAULT_MINIMUM = 0;
		public const int DEFAULT_MAXIMUM = 10;

		#endregion

		#region Public Interface

		public SliderIntField() : this(null)
		{
		}

		public SliderIntField(string label) : base(label, new SliderIntControl())
		{
		}

		public SliderIntField(string label, int min, int max) : this(label)
		{
			Minimum = min;
			Maximum = max;
		}

		public SliderIntField(int min, int max) : this(null, min, max)
		{
		}

		#endregion

		#region Visual Input

		private class SliderIntControl : SliderControl
		{
			public override int Minimum { get => _slider.lowValue; set => _slider.lowValue = value; }
			public override int Maximum { get => _slider.highValue; set => _slider.highValue = value; }

			private readonly SliderInt _slider;
			private readonly IntegerField _text;

			public SliderIntControl()
			{
				_slider = new SliderInt(DEFAULT_MINIMUM, DEFAULT_MAXIMUM);
				_slider.AddToClassList(SLIDER_USS_CLASS_NAME);
				_text = new IntegerField();
				_text.AddToClassList(TEXT_USS_CLASS_NAME);

				Add(_slider);
				Add(_text);
			}

			public override void SetValueWithoutNotify(int value)
			{
				_slider.SetValueWithoutNotify(value);
				_text.SetValueWithoutNotify(value);
			}
		}

		#endregion

		#region UXML Support


		public new class UxmlFactory : UxmlFactory<SliderIntField, UxmlTraits> { }
		public new class UxmlTraits : UxmlTraits<UxmlIntAttributeDescription>
		{
			public UxmlTraits()
			{
				Minimum.defaultValue = DEFAULT_MINIMUM;
				Maximum.defaultValue = DEFAULT_MAXIMUM;
			}
		}

		#endregion
	}
}
