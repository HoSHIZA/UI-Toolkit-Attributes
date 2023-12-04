using UnityEngine;

namespace PiRhoSoft.Utilities
{
	public class SnapAttribute : PropertyTraitAttribute
	{
		public const int ORDER = 0;

		public float Number { get; private set; }
		public Vector4 Vector { get; private set; }
		public Bounds Bounds { get; private set; }
		public string SnapSource { get; private set; }

		public SnapAttribute(string snapSource) : base(VALIDATE_PHASE, ORDER)
		{
			SnapSource = snapSource;
		}

		public SnapAttribute(float snap) : base(VALIDATE_PHASE, ORDER)
		{
			Number = snap;
		}

		public SnapAttribute(int snap) : base(VALIDATE_PHASE, ORDER)
		{
			Number = snap;
		}

		public SnapAttribute(float x, float y) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector2(x, y);
		}

		public SnapAttribute(int x, int y) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector2(x, y);
		}

		public SnapAttribute(float x, float y, float z) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector3(x, y, z);
		}

		public SnapAttribute(int x, int y, int z) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector3(x, y, z);
		}

		public SnapAttribute(float x, float y, float z, float w) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector4(x, y, z, w);
		}

		public SnapAttribute(int x, int y, int width, int height) : base(VALIDATE_PHASE, ORDER)
		{
			Vector = new Vector4(x, y, width, height);
		}

		public SnapAttribute(float x, float y, float z, float width, float height, float depth) : base(VALIDATE_PHASE, ORDER)
		{
			Bounds = new Bounds(new Vector3(x, y, z), new Vector3(width, height, depth));
		}

		public SnapAttribute(int x, int y, int z, int width, int height, int depth) : base(VALIDATE_PHASE, ORDER)
		{
			Bounds = new Bounds(new Vector3(x, y, z), new Vector3(width, height, depth));
		}
	}
}