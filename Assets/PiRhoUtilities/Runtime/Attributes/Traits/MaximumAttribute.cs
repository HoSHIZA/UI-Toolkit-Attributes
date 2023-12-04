namespace PiRhoSoft.Utilities
{
	public class MaximumAttribute : PropertyTraitAttribute
	{
		public const int ORDER = 1;

		public float Maximum { get; private set; }
		public string MaximumSource { get; private set; }

		public MaximumAttribute(float maximum) : base(VALIDATE_PHASE, ORDER)
		{
			Maximum = maximum;
		}

		public MaximumAttribute(int maximum) : base(VALIDATE_PHASE, ORDER)
		{
			Maximum = maximum;
		}

		public MaximumAttribute(string maximumSource) : base(VALIDATE_PHASE, ORDER)
		{
			MaximumSource = maximumSource;
		}
	}
}