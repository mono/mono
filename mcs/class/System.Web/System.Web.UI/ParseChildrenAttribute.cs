//
// System.Web.UI.ParseChildrenAttribute.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ParseChildrenAttribute : Attribute
	{
		bool childrenAsProperties;
		string defaultProperty;
		public static readonly ParseChildrenAttribute Default = new ParseChildrenAttribute ();

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
			if (childrenAsProperties)
				this.defaultProperty = defaultProperty;
		}

		public bool ChildrenAsProperties {

			get { return childrenAsProperties; }

			set { childrenAsProperties = value; }
		}

		public string DefaultProperty {
			get { return defaultProperty; }

			set { defaultProperty = value; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ParseChildrenAttribute))
				return false;

			ParseChildrenAttribute o = (ParseChildrenAttribute) obj;
			if (childrenAsProperties == o.childrenAsProperties){
				if (childrenAsProperties == false)
					return true;
				return (defaultProperty == o.DefaultProperty);
			}
			return false;
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
