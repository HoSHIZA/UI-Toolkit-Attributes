namespace PiRhoSoft.Utilities
{
	public class ReferenceAttribute : PropertyTraitAttribute
	{
		public bool IsCollapsable = true;

		public ReferenceAttribute() : base(CONTROL_PHASE, 0)
		{
		}
	}
}
