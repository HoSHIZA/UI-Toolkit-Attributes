using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	public class MessageBox : VisualElement
	{
		#region Log Messages

		private const string INVALID_TYPE_WARNING = "(PUEMBIT) invalid message type '{0}' for MessageBox: message-type must be one of 'Info', 'Warning', or 'Error'";

		#endregion

		#region Class Names

		public const string STYLESHEET = "MessageBox.uss";
		public const string USS_CLASS_NAME = "pirho-message-box";
		public const string IMAGE_USS_CLASS_NAME = USS_CLASS_NAME + "__image";
		public const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";

		#endregion

		#region Defaults

		public const MessageBoxType DEFAULT_TYPE = MessageBoxType.Info;

		#endregion

		#region Members

		private readonly Image _image;
		private readonly TextElement _label;

		private MessageBoxType _type = DEFAULT_TYPE;

		#endregion

		#region Public Interface

#if UNITY_2023_1_OR_NEWER
        [UxmlAttribute("message-type")]
#endif
		public MessageBoxType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
				_image.image = GetIcon(_type);
			}
		}

#if UNITY_2023_1_OR_NEWER
        [UxmlAttribute("message")]
#endif
		public string Message
		{
			get	{ return _label.text; }
			set	{ ((INotifyValueChanged<string>)_label).SetValueWithoutNotify(value); }
		}

		public MessageBox() : this(DEFAULT_TYPE, string.Empty)
		{
		}

		public MessageBox(MessageBoxType type, string message)
		{
			_image = new Image();
			_image.AddToClassList(IMAGE_USS_CLASS_NAME);

			_label = new TextElement();
			_label.AddToClassList(LABEL_USS_CLASS_NAME);

			Type = type;
			Message = message;
			
			Add(_image);
			Add(_label);

			this.AddStyleSheet(STYLESHEET);
			AddToClassList(USS_CLASS_NAME);
		}

		#endregion

		#region Icon Management

		private Texture GetIcon(MessageBoxType type)
		{
			switch (type)
			{
				case MessageBoxType.Info: return Icon.Info.Texture;
				case MessageBoxType.Warning: return Icon.Warning.Texture;
				case MessageBoxType.Error: return Icon.Error.Texture;
				default: return null;
			}
		}

		#endregion

		#region UXML Support

#if !UNITY_2023_1_OR_NEWER
		public new class UxmlFactory : UxmlFactory<MessageBox, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _messageType = new UxmlStringAttributeDescription { name = "message-type" };
			private readonly UxmlStringAttributeDescription _message = new UxmlStringAttributeDescription { name = "message" };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(element, bag, cc);

				var field = element as MessageBox;

				field.Message = _message.GetValueFromBag(bag, cc);

				var messageType = _messageType.GetValueFromBag(bag, cc);

				if (!string.IsNullOrEmpty(messageType))
                {
                    field.Type = ParseValue(messageType);
                }
            }

			private MessageBoxType ParseValue(string valueName)
			{
				try
				{
					return (MessageBoxType)Enum.Parse(typeof(MessageBoxType), valueName);
				}
				catch
				{
					Debug.LogWarningFormat(INVALID_TYPE_WARNING, valueName);
					return DEFAULT_TYPE;
				}
			}
		}
#endif

		#endregion
	}
}
