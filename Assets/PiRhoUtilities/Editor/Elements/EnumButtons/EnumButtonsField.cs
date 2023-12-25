using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
	public partial class EnumButtonsField : BaseField<Enum>
	{
		#region Class Names

		public const string STYLESHEET = "EnumButtonsStyle.uss";
		public const string USS_CLASS_NAME = "pirho-enum-buttons-field";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";
		public const string INPUT_USS_CLASS_NAME = USS_CLASS_NAME + "__input";
		public const string BUTTON_USS_CLASS_NAME = INPUT_USS_CLASS_NAME + "__button";
		public const string ACTIVE_BUTTON_USS_CLASS_NAME = BUTTON_USS_CLASS_NAME + "--active";
		public const string FIRST_BUTTON_USS_CLASS_NAME = BUTTON_USS_CLASS_NAME + "--first";
		public const string LAST_BUTTON_USS_CLASS_NAME = BUTTON_USS_CLASS_NAME + "--last";

		#endregion

		#region Log Messages

		private const string INVALID_TYPE_WARNING = "(PUEBFIT) failed to setup EnumButtonsField: the type '{0}' is not an enum";
		private const string INVALID_VALUE_WARNING = "(PUEBFIV) failed to set EnumButtonsField value: '{0}' is not a valid value for the enum '{1}'";

		#endregion

		#region Private Members

		private readonly EnumButtonsControl _control;

		#endregion

		#region Public Interface

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("type")]
#endif
		public Type Type
		{
			get => _control.Type;
			set => _control.Type = value;
		}

#if UNITY_2023_2_OR_NEWER
        [UxmlAttribute("flags")]
#endif
		public bool UseFlags
		{
			get => Type != null && _control.UseFlags;
			set => _control.UseFlags = value;
		}

#if UNITY_2023_2_OR_NEWER
        private string _value;
        
        [UxmlAttribute("value")]
        private string Value
        {
            get => _value;
            set
            {
                if (Type != null && value != string.Empty && Enum.TryParse(Type, value, out var result) && result is Enum @enum)
                {
                    SetValueWithoutNotify(@enum);

                    _value = this.value.ToString();
                    
                    return;
                }

                _value = value;
                
                // if (Type == null)
                // {
                //     _value = string.Empty;
                //
                //     return;
                // }
                //
                // _value = value;
                //
                // if (string.IsNullOrEmpty(value))
                // {
                //     Debug.Log(value);
                //     SetValueWithoutNotify(default);
                //     _value = this.value.ToString();
                //     
                //     return;
                // }
                //
                // if (Enum.TryParse(Type, value, out var result) && result is Enum @enum)
                // {
                //     SetValueWithoutNotify(@enum);
                //     _value = this.value.ToString();
                //     
                //     return;
                // }
                //
                // _value = value;
            }
        }
#endif

		public EnumButtonsField() : this(null)
		{
		}

		public EnumButtonsField(string label) : base(label, null)
		{
			_control = new EnumButtonsControl();
			_control.AddToClassList(INPUT_USS_CLASS_NAME);
			_control.RegisterCallback<ChangeEvent<Enum>>(evt =>
			{
				base.value = evt.newValue;
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(LABEL_USS_CLASS_NAME);

			AddToClassList(USS_CLASS_NAME);
			this.SetVisualInput(_control);
			this.AddStyleSheet(STYLESHEET);
		}

		public override void SetValueWithoutNotify(Enum newValue)
		{
			base.SetValueWithoutNotify(newValue);
			_control.SetValueWithoutNotify(newValue);
		}

		#endregion

		#region Binding

#if UNITY_2023_2_OR_NEWER
		protected override void HandleEventBubbleUp(EventBase evt)
		{
			base.HandleEventBubbleUp(evt);
#else
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
#endif

			if (this.TryGetPropertyBindEvent(evt, out var property))
			{
				BindingExtensions.DefaultEnumBind(this, property);
				evt.StopPropagation();
			}
		}

		#endregion

		#region Visual Input

		private class EnumButtonsControl : VisualElement
		{
			private Type _type;
			public Type Type
			{
				get => _type;
				set => SetType(value);
			}

			private bool? _useFlags;
			public bool UseFlags
			{
				get => _useFlags.GetValueOrDefault(Type.HasAttribute<FlagsAttribute>());
				set => _useFlags = value;
			}

			private readonly UQueryState<Button> _buttons;

			private Enum _value;
			private string[] _names;
			private Array _values;

			public EnumButtonsControl()
			{
				_buttons = this.Query<Button>().Build();
			}

			public void SetValueWithoutNotify(Enum value)
			{
				if (value == null || Type != value.GetType())
				{
					Debug.LogWarningFormat(INVALID_VALUE_WARNING, value, Type);
				}
				else if (!Equals(_value, value))
                {
                    _value = value;
                    _buttons.ForEach(button =>
                    {
                        var index = IndexOf(button);

                        button.EnableInClassList(FIRST_BUTTON_USS_CLASS_NAME, index == 0);
                        button.EnableInClassList(LAST_BUTTON_USS_CLASS_NAME, index == _names.Length - 1);

                        if (UseFlags)
                        {
                            var current = GetIntFromEnum(Type, _value);
                            var buttonValue = GetIntFromEnum(Type, button.userData as Enum);
                            var enable = (buttonValue != 0 && (current & buttonValue) == buttonValue) || (current == 0 && buttonValue == 0);

                            button.EnableInClassList(ACTIVE_BUTTON_USS_CLASS_NAME, enable);

#if !UNITY_2020_1_OR_NEWER
                            button.style.backgroundColor = enable
                                ? StyleConst.UnityColors.Button.BackgroundPressed
                                : StyleConst.UnityColors.Button.Background;
#endif
                        }
                        else
                        {
                            var enable = _value.Equals(button.userData as Enum);
                            
                            button.EnableInClassList(ACTIVE_BUTTON_USS_CLASS_NAME, enable);
                            
#if !UNITY_2020_1_OR_NEWER
                            button.style.backgroundColor = enable
                                ? StyleConst.UnityColors.Button.BackgroundPressed
                                : StyleConst.UnityColors.Button.Background;
#endif
                        }
                    });
                }
			}

			private void SetType(Type type)
			{
				if (type != _type)
				{
					_type = type;

					Clear();

					if (_type == null || !_type.IsEnum)
					{
						Debug.LogWarningFormat(INVALID_TYPE_WARNING, _type);
					}
					else
					{
						_names = Enum.GetNames(_type);
						_values = Enum.GetValues(_type);

						var value = _values.Length > 0 ? _values.GetValue(0) as Enum : Enum.ToObject(type, 0) as Enum;

						Rebuild();
						this.SendChangeEvent(_value, value);
					}
				}
			}

			private void Rebuild()
			{
				if (_names.Length > 0)
				{
					for (var i = 0; i < _names.Length; i++)
					{
						var index = i;
						var button = new Button(() => Toggle(index))
						{
							text = _names[i],
							userData = _values.GetValue(i)
						};

						button.AddToClassList(BUTTON_USS_CLASS_NAME);
						Add(button);
					}
				}
			}

			private void Toggle(int index)
			{
				var selected = _values.GetValue(index) as Enum;

				if (UseFlags)
				{
					var current = GetIntFromEnum(Type, _value);
					var buttonValue = GetIntFromEnum(Type, selected);

					if ((buttonValue != 0 && (current & buttonValue) == buttonValue) || (current == 0 && buttonValue == 0))
					{
						if (buttonValue != ~0)
                        {
                            current &= ~buttonValue;
                        }
                    }
					else
					{
						if (buttonValue == 0)
                        {
                            current = 0;
                        }
                        else
                        {
                            current |= buttonValue;
                        }
                    }

					selected = GetEnumFromInt(Type, current);
				}

				this.SendChangeEvent(_value, selected);
			}

			private Enum GetEnumFromInt(Type type, int value)
			{
				return Enum.ToObject(type, value) as Enum;
			}

			private int GetIntFromEnum(Type type, Enum value)
			{
				return (int)Enum.Parse(type, value.ToString());
			}
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_2_OR_NEWER
		public new class UxmlFactory : UxmlFactory<EnumButtonsField, UxmlTraits> { }
		public new class UxmlTraits : BaseField<Enum>.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _type = new UxmlStringAttributeDescription { name = "type", use = UxmlAttributeDescription.Use.Required };
			private readonly UxmlBoolAttributeDescription _flags = new UxmlBoolAttributeDescription { name = "flags" };
			private readonly UxmlStringAttributeDescription _value = new UxmlStringAttributeDescription { name = "value" };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(element, bag, cc);

				var field = element as EnumButtonsField;
				var typeName = _type.GetValueFromBag(bag, cc);
                
                if (string.IsNullOrEmpty(typeName))
                {
                    return;
                }

                var type = TypeHelper.FindType(typeName);

				var flags = false;
				if (_flags.TryGetValueFromBag(bag, cc, ref flags))
                {
                    field.UseFlags = flags;
                }

                field.Type = type;

				var valueName = _value.GetValueFromBag(bag, cc);
				if (type != null && !string.IsNullOrEmpty(valueName))
				{
					if (TryParseValue(type, valueName, out var value))
                    {
                        field.SetValueWithoutNotify(value);
                    }
                    else
                    {
                        Debug.LogWarningFormat(INVALID_VALUE_WARNING, valueName, type.Name);
                    }
                }
			}

			private bool TryParseValue(Type type, string valueName, out Enum value)
			{
				try
				{
					value = Enum.Parse(type, valueName) as Enum;
					return true;
				}
				catch (Exception exception) when (exception is ArgumentException || exception is OverflowException)
				{
					value = null;
					return false;
				}
			}
		}
#endif

		#endregion
	}
}
