//
// System.Web.UI.ParseChildrenAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ParseChildrenAttribute : Attribute
	{
		bool childrenAsProperties;
		string defaultProperty;

		// LAMESPEC
		public ParseChildrenAttribute ()
		{
			childrenAsProperties = false;
			defaultProperty = "";
		}

		public ParseChildrenAttribute (bool childrenAsProperties)
		{
			this.childrenAsProperties = childrenAsProperties;
			this.defaultProperty = "";
		}

		public ParseChildrenAttribute (bool childrenAsProperties,
					       string defaultProperty)
		{
			this.childrenAsProperties = childrenAsProperties;
			this.defaultProperty = defaultProperty;
		}

		public static readonly ParseChildrenAttribute Default;

		public bool ChildrenAsProperties {

			get { return childrenAsProperties; }

			set { childrenAsProperties = value; }
		}

		public string DefaultProperty {
			get { return defaultProperty; }

			set { defaultProperty = value; }
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
