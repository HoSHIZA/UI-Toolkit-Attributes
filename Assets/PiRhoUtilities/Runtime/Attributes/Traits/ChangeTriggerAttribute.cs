namespace PiRhoSoft.Utilities
{
	public class ChangeTriggerAttribute : PropertyTraitAttribute
	{
		public string Method { get; private set; }

		public ChangeTriggerAttribute(string method) : base(FIELD_PHASE, 1)
		{
			Method = method;
		}
	}
}