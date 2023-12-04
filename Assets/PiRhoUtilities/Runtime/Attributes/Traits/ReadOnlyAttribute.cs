namespace PiRhoSoft.Utilities
{
	public class ReadOnlyAttribute : PropertyTraitAttribute
	{
		public ReadOnlyAttribute() : base(PER_CONTAINER_PHASE, 10)
		{
		}
	}
}