//
// System.Runtime.CompilerServices.RequiredAttributeAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
			AttributeTargets.Enum | AttributeTargets.Interface)]
	[Serializable]
	public sealed class RequiredAttributeAttribute : Attribute
	{
		public RequiredAttributeAttribute (Type requiredContract)
		{
		}

		public Type RequiredContract {
			get { throw new NotSupportedException (); }
		}
	}
}
