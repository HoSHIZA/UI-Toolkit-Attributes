﻿using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("SliderFloat")]
#endif
	public partial class SliderFloatField : SliderField<float>
	{
		#region Defaults

		// These match unity's internal defaults
		public const float DEFAULT_MINIMUM = 0;
		public const float DEFAULT_MAXIMUM = 10;

		#endregion

		#region Public Interface

		public SliderFloatField() : this(null)
		{
		}

		public SliderFloatField(string label) : base(label, new SliderFloatControl())
		{
		}

		public SliderFloatField(string label, float min, float max) : this(label)
		{
			Minimum = min;
			Maximum = max;
		}

		public SliderFloatField(float min, float max) : this(null, min, max)
		{
		}

		#endregion

		#region Visual Input

		private class SliderFloatControl : SliderControl
		{
			public override float Minimum { get => _slider.lowValue; set => _slider.lowValue = value; }
			public override float Maximum { get => _slider.highValue; set => _slider.highValue = value; }

			private readonly Slider _slider;
			private readonly FloatField _text;

			public SliderFloatControl()
			{
				_slider = new Slider(DEFAULT_MINIMUM, DEFAULT_MAXIMUM);
				_slider.AddToClassList(SLIDER_USS_CLASS_NAME);
				_text = new FloatField();
				_text.AddToClassList(TEXT_USS_CLASS_NAME);

				Add(_slider);
				Add(_text);
			}

			public override void SetValueWithoutNotify(float value)
			{
				_slider.SetValueWithoutNotify(value);
				_text.SetValueWithoutNotify(value);
			}
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<SliderFloatField, UxmlTraits> { }
		public new class UxmlTraits : UxmlTraits<UxmlFloatAttributeDescription>
		{
			public UxmlTraits()
			{
				Minimum.defaultValue = DEFAULT_MINIMUM;
				Maximum.defaultValue = DEFAULT_MAXIMUM;
			}
		}
#endif

		#endregion
	}
}
