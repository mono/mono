//
// System.Web.UI.ControlBuilderAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ControlBuilderAttribute : Attribute
	{
		Type builderType;
		public static readonly ControlBuilderAttribute Default;
		
		public ControlBuilderAttribute (Type builderType)
		{
			this.builderType = builderType;
		}

		public Type BuilderType {
			get { return builderType; }
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return false;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		[MonoTODO]
		public override bool IsDefaultAttribute ()
		{
			return false;
		}
	}
}
