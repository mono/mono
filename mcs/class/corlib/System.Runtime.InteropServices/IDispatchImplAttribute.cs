//
// System.Runtime.InteropServices.IDispatchImplAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class IDispatchImplAttribute : Attribute
	{
		private IDispatchImplType Impl;

		public IDispatchImplAttribute (IDispatchImplType implType)
		{
			Impl = implType;
		}

		public IDispatchImplAttribute (short implType)
		{
			Impl = (IDispatchImplType)implType;
		}

		public IDispatchImplType Value {
			get { return Impl; } 
		}
	}
}
