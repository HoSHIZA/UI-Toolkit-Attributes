using UnityEngine;

namespace PiRhoSoft.Utilities
{
	public enum TraitLocation
	{
		Above,
		Below,
		Left,
		Right
	}

	public abstract class PropertyTraitAttribute : PropertyAttribute
	{
		public const int TEST_PHASE = 1;
		public const int PER_CONTAINER_PHASE = 2;
		public const int CONTAINER_PHASE = 3;
		public const int FIELD_PHASE = 4;
		public const int VALIDATE_PHASE = 5;
		public const int CONTROL_PHASE = 6;

		protected PropertyTraitAttribute(int drawPhase, int drawOrder)
		{
			order = int.MaxValue - (drawPhase * 1000 + drawOrder);
		}
	}
}