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

		// LAMESPEC: we will default to true for now.
		public ConstructorNeedsTagAttribute ()
		{
			needsTag = true;
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
