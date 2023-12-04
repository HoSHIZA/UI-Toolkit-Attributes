namespace PiRhoSoft.Utilities
{
	public class CustomLabelAttribute : PropertyTraitAttribute
	{
		public string Label { get; private set; }
		public string LabelSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public CustomLabelAttribute(string label) : base(PER_CONTAINER_PHASE, 0)
		{
			Label = label;
		}

		public CustomLabelAttribute(string labelSource, bool autoUpdate) : base(PER_CONTAINER_PHASE, 0)
		{
			LabelSource = labelSource;
			AutoUpdate = autoUpdate;
		}
	}
}