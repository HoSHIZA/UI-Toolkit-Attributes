namespace PiRhoSoft.Utilities
{
	public enum MessageBoxType
	{
		Info,
		Warning,
		Error
	}

	public class MessageBoxAttribute : PropertyTraitAttribute
	{
		public string Message { get; private set; }
		public MessageBoxType Type { get; private set; }
		public TraitLocation Location { get; set; }

		public MessageBoxAttribute(string message, MessageBoxType type) :
#if UNITY_2021_1_OR_NEWER
            base(PER_CONTAINER_PHASE, 20)
#else
            base(PER_CONTAINER_PHASE, 10)
#endif
		{
			Message = message;
			Type = type;
		}
	}
}