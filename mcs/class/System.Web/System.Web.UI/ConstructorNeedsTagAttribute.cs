//
// System.Web.UI.ConstructorNeedsTagAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ConstructorNeedsTagAttribute : Attribute
	{
		bool needsTag;

		public ConstructorNeedsTagAttribute ()
		{
			needsTag = false;
		}

		public ConstructorNeedsTagAttribute (bool needsTag)
		{
			this.needsTag = needsTag;
		}

		public bool NeedsTag {
			get { return needsTag; }
		}
	}
}
