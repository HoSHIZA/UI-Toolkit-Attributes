﻿namespace PiRhoSoft.Utilities
{
	public class GroupAttribute : PropertyTraitAttribute
	{
		public string Name { get; private set; }

		public GroupAttribute(string name, int drawOrder = 0) : base(CONTAINER_PHASE, drawOrder)
		{
			Name = name;
		}
	}
}
