namespace PiRhoSoft.Utilities
{
	public class DictionaryAttribute : PropertyTraitAttribute
	{
		public const string ALWAYS = "";
		public const string NEVER = null;

		public string AllowAdd = ALWAYS;
		public string AllowRemove = ALWAYS;

		public bool AllowReorder = true;
		public bool IsCollapsable = true;

		public string AddPlaceholder = null;
		public string EmptyLabel = null;

		public string AddCallback = null;
		public string RemoveCallback = null;
		public string ReorderCallback = null;
		public string ChangeCallback = null;

		public DictionaryAttribute() : base(CONTAINER_PHASE, 0)
		{
		}
	}
}
