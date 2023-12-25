namespace PiRhoSoft.Utilities
{
	public class ReadOnlyAttribute : PropertyTraitAttribute
	{
		public ReadOnlyAttribute() : 
#if UNITY_2021_1_OR_NEWER
            base(PER_CONTAINER_PHASE, 10)
#else
            base(PER_CONTAINER_PHASE, 0)
#endif
		{
		}
	}
}