namespace PiRhoSoft.Utilities
{
	public class EnumButtonsAttribute : PropertyTraitAttribute
	{
		public bool? Flags { get; private set; }

		public EnumButtonsAttribute() : base(CONTROL_PHASE, 0) => Flags = null;
		public EnumButtonsAttribute(bool flags) : base(CONTROL_PHASE, 0) => Flags = flags;
	}
}