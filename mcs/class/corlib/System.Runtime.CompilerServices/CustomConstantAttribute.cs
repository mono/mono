//
// System.Runtime.CompilerServices.CustomConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[AttributeUsage (AttributeTargets.Class)] [Serializable]
	public abstract class CustomConstantAttribute : Attribute
	{
		protected CustomConstantAttribute ()
		{
		}

		public abstract object Value { get; }
	}
}
