using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("Slider")]
#endif
	public abstract partial class SliderField<TValueType> : BaseField<TValueType>
	{
		#region Class Names

		public const string STYLESHEET = "SliderStyle.uss";
		public const string USS_CLASS_NAME = "pirho-slider-field";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string INPUT_USS_CLASS_NAME = USS_CLASS_NAME + "__input";

		#endregion

		#region Members

		protected readonly SliderControl Control;

		#endregion

		#region Public Interface
        
#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("min")]
#endif
		public TValueType Minimum
		{
			get => Control.Minimum;
			set => Control.Minimum = value;
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("max")]
#endif
		public TValueType Maximum
		{
			get => Control.Maximum;
			set => Control.Maximum = value;
		}

		protected SliderField(string label, SliderControl control) : base(label, control)
		{
			Control = control;
			Control.AddToClassList(INPUT_USS_CLASS_NAME);
			Control.RegisterCallback<ChangeEvent<TValueType>>(evt =>
			{
				base.value = evt.newValue;
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(LABEL_USS_CLASS_NAME);

			AddToClassList(USS_CLASS_NAME);
			this.AddStyleSheet(STYLESHEET);
		}

		public override void SetValueWithoutNotify(TValueType newValue)
		{
			base.SetValueWithoutNotify(newValue);
			Control.SetValueWithoutNotify(newValue);
		}

		#endregion

		#region Visual Input

		protected abstract class SliderControl : VisualElement
		{
			public const string SLIDER_USS_CLASS_NAME = INPUT_USS_CLASS_NAME + "__slider";
			public const string TEXT_USS_CLASS_NAME = INPUT_USS_CLASS_NAME + "__text";

			public abstract TValueType Minimum { get; set; }
			public abstract TValueType Maximum { get; set; }

			public abstract void SetValueWithoutNotify(TValueType value);
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public abstract class UxmlTraits<TAttributeType> : BaseFieldTraits<TValueType, TAttributeType> where TAttributeType : TypedUxmlAttributeDescription<TValueType>, new()
		{
			protected readonly TAttributeType Minimum = new TAttributeType { name = "minimum" };
			protected readonly TAttributeType Maximum = new TAttributeType { name = "maximum" };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				var field = element as SliderField<TValueType>;
				field.Minimum = Minimum.GetValueFromBag(bag, cc);
				field.Maximum = Maximum.GetValueFromBag(bag, cc);

				base.Init(element, bag, cc);
			}
		}
#endif

		#endregion
	}
}
