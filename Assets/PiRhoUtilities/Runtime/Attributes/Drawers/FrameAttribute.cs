namespace PiRhoSoft.Utilities
{
	public class FrameAttribute : PropertyTraitAttribute
	{
		public bool IsCollapsable = true;

		public FrameAttribute() : base(CONTROL_PHASE, 0)
		{
		}
	}
}
