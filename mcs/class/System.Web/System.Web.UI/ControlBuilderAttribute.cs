//
// System.Web.UI.ControlBuilderAttribute.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ControlBuilderAttribute : Attribute
	{
		Type builderType;
		public static readonly ControlBuilderAttribute Default = new ControlBuilderAttribute (null);
		
		public ControlBuilderAttribute (Type builderType)
		{
			this.builderType = builderType;
		}

		public Type BuilderType {
			get { return builderType; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ControlBuilderAttribute))
				return false;
			return ((ControlBuilderAttribute) obj).builderType == builderType;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
	}
}

