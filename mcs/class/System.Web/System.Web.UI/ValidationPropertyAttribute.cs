//
// System.Web.UI.ValidationPropertyAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {
	[AttributeUsage (AttributeTarget.Class)]
	public sealed class ValidationPropertyAttribute : Attribute
	{
		string name;

		public ValidationPropertyAttribute (string name)
		{
			this.name = name;
		}

		public override string Name {
			get { return name; }
		}
	}
}
 
