//
// System.Runtime.CompilerServices.CompilationRelaxationsAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[AttributeUsage (AttributeTargets.Module)] [Serializable]
	public class CompilationRelaxationsAttribute : Attribute
	{
		int relax;
		public CompilationRelaxationsAttribute (int relaxations)
		{
			relax = relaxations;
		}

		public int CompilationRelaxations {
			get { return relax; }
		}
	}
}
