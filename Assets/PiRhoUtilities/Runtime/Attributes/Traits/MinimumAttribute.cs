namespace PiRhoSoft.Utilities
{
	public class MinimumAttribute : PropertyTraitAttribute
	{
		public const int ORDER = 1;

		public float Minimum { get; private set; }
		public string MinimumSource { get; private set; }

		public MinimumAttribute(float minimum) : base(VALIDATE_PHASE, ORDER)
		{
			Minimum = minimum;
		}

		public MinimumAttribute(int minimum) : base(VALIDATE_PHASE, ORDER)
		{
			Minimum = minimum;
		}

		public MinimumAttribute(string maximumSource) : base(VALIDATE_PHASE, ORDER)
		{
			MinimumSource = maximumSource;
		}
	}
}