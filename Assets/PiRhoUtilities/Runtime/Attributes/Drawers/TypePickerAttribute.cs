﻿using System;

namespace PiRhoSoft.Utilities
{
	public class TypePickerAttribute : PropertyTraitAttribute
	{
		public Type BaseType { get; private set; }
		public bool ShowAbstract { get; private set; }

		public TypePickerAttribute(Type baseType, bool showAbstract = false) : base(CONTROL_PHASE, 0)
		{
			BaseType = baseType;
			ShowAbstract = showAbstract;
		}
	}
}
