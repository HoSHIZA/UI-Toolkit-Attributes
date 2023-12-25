using UnityEditor;
using UnityEngine.UIElements;

namespace PiRhoSoft.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(MessageBoxAttribute))]
    internal class MessageBoxDrawer : PropertyDrawer
	{
		public const string STYLESHEET = "MessageBoxStyle.uss";
		public const string USS_CLASS_NAME = "pirho-trait-message";
		public const string SIDE_USS_CLASS_NAME = USS_CLASS_NAME + "--side";
		public const string MESSAGE_USS_CLASS_NAME = USS_CLASS_NAME + "__message-box";
		public const string ABOVE_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--above";
		public const string BELOW_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--below";
		public const string LEFT_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--left";
		public const string RIGHT_USS_CLASS_NAME = MESSAGE_USS_CLASS_NAME + "--right";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var messageAttribute = attribute as MessageBoxAttribute;
			var element = this.CreateNextElement(property);
			var container = new VisualElement();
            
			container.AddStyleSheet(STYLESHEET);
			container.AddToClassList(USS_CLASS_NAME);
			container.Add(element);
            
			var message = new MessageBox(messageAttribute.Type, messageAttribute.Message);
			message.AddToClassList(MESSAGE_USS_CLASS_NAME);
            
#if UNITY_2021_1_OR_NEWER
            message.style.marginRight = -2;
#else
            message.style.marginRight = 3;
#endif

			if (messageAttribute.Location == TraitLocation.Above)
			{
				message.AddToClassList(ABOVE_USS_CLASS_NAME);
				container.Insert(0, message);
			}
			if (messageAttribute.Location == TraitLocation.Below)
			{
				message.AddToClassList(BELOW_USS_CLASS_NAME);
				container.Add(message);
			}
			else if (messageAttribute.Location == TraitLocation.Left)
			{
				message.AddToClassList(LEFT_USS_CLASS_NAME);
				container.Insert(0, message);
				container.AddToClassList(SIDE_USS_CLASS_NAME);
			}
			else if (messageAttribute.Location == TraitLocation.Right)
			{
				message.AddToClassList(RIGHT_USS_CLASS_NAME);
				container.Add(message);
				container.AddToClassList(SIDE_USS_CLASS_NAME);
			}

			return container;
		}
	}
}