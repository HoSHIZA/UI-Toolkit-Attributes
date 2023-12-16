using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement("Popup")]
#endif
	public partial class PopupField<T> : BaseField<T>
	{
		#region Class Names
        
		public const string STYLESHEET = "Popup/PopupStyle.uss";
		public const string USS_CLASS_NAME = "pirho-popup-field";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string INPUT_USS_CLASS_NAME = USS_CLASS_NAME + "__input";

		#endregion
        
		#region Log Messages

		private const string INVALID_VALUES_ERROR = "(PUPFIV) invalid values for PopupField: Values must not be null and must have a Count > 0";
		private const string INVALID_OPTIONS_WARNING = "(PUPFIO) invalid Options for PopupField: the number of Options does not match the number of Values";
		private const string MISSING_VALUE_WARNING = "(PUPFMV) the value of PopupField did not exsist in the list of values: Changing to the first valid value";

		#endregion

		#region Private Members

#if UNITY_2022_1_OR_NEWER
		private UnityEngine.UIElements.PopupField<T> _popup;
#else
        private UnityEditor.UIElements.PopupField<T> _popup;
#endif

		private List<T> _values;
		private List<string> _options;

		#endregion

		#region Public Interface

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("values")]
        private string[] StringValues
        {
            get => Values.Select(val => val.ToString()).ToArray();
            set => ParseValues(value);
        }

        public List<T> Values
        {
            get => _values;
            set => SetValues(value, Options);
        }

        [UxmlAttribute("options")]
        public List<string> Options
        {
            get => _options;
            set => SetValues(Values, value);
        }
#else
		public List<T> Values => _values;
		public List<string> Options => _options;
#endif

		public PopupField() : this(null)
		{
		}

		public PopupField(string label) : base(label, null)
		{
			AddToClassList(USS_CLASS_NAME);
		}

		public PopupField(string label, List<T> values, List<string> options = null) : this(label)
		{
			SetValues(values, options);
		}

		public PopupField(List<T> values, List<string> options = null) : this(null, values, options)
		{
		}

		public void SetValues(List<T> values, List<string> options = null)
		{
			if (values != _values || options != _options)
			{
				DestroyPopup();

				_options = options;
				_values = values;

				if (_values != null && _values.Count > 0)
				{
					if (!ValidateValue(value))
                    {
                        base.value = _values[0];
                    }

                    CreatePopup();
				}
				else
				{
					_values = null;
					Debug.LogErrorFormat(INVALID_VALUES_ERROR);
				}

				if (_options != null && _values != null && _options.Count != _values.Count)
				{
					_options = null;
					Debug.LogWarningFormat(INVALID_OPTIONS_WARNING);
				}
			}
		}

		public override void SetValueWithoutNotify(T newValue)
		{
			if (ValidateValue(newValue))
			{
				base.SetValueWithoutNotify(newValue);
				_popup?.SetValueWithoutNotify(newValue);
			}
			else
			{
				if (_values != null)
                {
                    base.SetValueWithoutNotify(_values[0]);
                }

                Debug.LogWarningFormat(MISSING_VALUE_WARNING);
			}
		}

		#endregion

		#region Popup Management

		private void CreatePopup()
		{
#if UNITY_2022_1_OR_NEWER
            _popup = new UnityEngine.UIElements.PopupField<T>(_values, value, Format, Format);
#else
            _popup = new UnityEditor.UIElements.PopupField<T>(_values, value, Format, Format);
#endif
            
			_popup.AddToClassList(INPUT_USS_CLASS_NAME);
			_popup.RegisterCallback<ChangeEvent<T>>(evt =>
			{
				if (_values.Contains(evt.newValue)) // ChangeEvent<string> is posted by _popup's TextElement but target is still _popup for some reason
                {
                    base.value = evt.newValue;
                }

                evt.StopImmediatePropagation();
			});

			this.SetVisualInput(_popup);
		}

		private void DestroyPopup()
		{
			_popup?.RemoveFromHierarchy();
			_popup = null;
		}

		private string Format(T val)
		{
			var index = _values.IndexOf(val);

			if (_options == null || index < 0 || index >= _options.Count)
            {
                return val.ToString();
            }

            return _options[index];
		}

		private bool ValidateValue(T val)
		{
			return _values != null && _values.Contains(val);
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public class UxmlTraits<TAttributeType> : BaseFieldTraits<T, TAttributeType> where TAttributeType : TypedUxmlAttributeDescription<T>, new()
		{
			private readonly UxmlStringAttributeDescription _options = new UxmlStringAttributeDescription { name = "options" };
			private readonly UxmlStringAttributeDescription _values = new UxmlStringAttributeDescription { name = "values" };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(element, bag, cc);

				var field = (PopupField<T>)element;
				var options = _options.GetValueFromBag(bag, cc).Split(',');
				var values = _values.GetValueFromBag(bag, cc).Split(',');

				var optionsList = options.ToList();
				var valuesList = field.ParseValues(values);

				field.SetValues(valuesList, optionsList);
			}
		}
#endif

		protected virtual List<T> ParseValues(string[] from) { return null; }

		#endregion
	}

	#region Concrete Classes

#if UNITY_2023_2_OR_NEWER
    [UxmlElement("PopupInt")]
#endif
	public partial class PopupIntField : PopupField<int>
	{
		public PopupIntField() { }
		public PopupIntField(string label) : base(label) { }
		public PopupIntField(string label, List<int> values, List<string> options = null) : base(label, values, options) { }
		public PopupIntField(List<int> values, List<string> options = null) : base(values, options) { }

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<PopupIntField, UxmlTraits<UxmlIntAttributeDescription>> { }
#endif
        
		protected override List<int> ParseValues(string[] from)
		{
			return from.Select(f => int.TryParse(f, out var result) ? result : default).ToList();
		}
	}

#if UNITY_2023_2_OR_NEWER
    [UxmlElement("PopupFloat")]
#endif
	public partial class PopupFloatField : PopupField<float>
	{
		public PopupFloatField() { }
		public PopupFloatField(string label) : base(label) { }
		public PopupFloatField(string label, List<float> values, List<string> options = null) : base(label, values, options) { }
		public PopupFloatField(List<float> values, List<string> options = null) : base(values, options) { }

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<PopupFloatField, UxmlTraits<UxmlFloatAttributeDescription>> { }
#endif
        
		protected override List<float> ParseValues(string[] from)
		{
			return from.Select(f => float.TryParse(f, out var result) ? result : default).ToList();
		}
	}

#if UNITY_2023_2_OR_NEWER
    [UxmlElement("PopupString")]
#endif
	public partial class PopupStringField : PopupField<string>
	{
		public PopupStringField() { }
		public PopupStringField(string label) : base(label) { }
		public PopupStringField(string label, List<string> values, List<string> options = null) : base(label, values, options) { }
		public PopupStringField(List<string> values, List<string> options = null) : base(values, options) { }

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<PopupStringField, UxmlTraits<UxmlStringAttributeDescription>> { }
#endif

		protected override List<string> ParseValues(string[] from)
		{
			return from.ToList();
		}
	}

	#endregion
}
