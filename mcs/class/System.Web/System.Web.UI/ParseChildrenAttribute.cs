//
// System.Web.UI.ParseChildrenAttribute.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ParseChildrenAttribute : Attribute
	{
		bool childrenAsProperties;
		string defaultProperty;
		public static readonly ParseChildrenAttribute Default = new ParseChildrenAttribute ();
		
#if NET_2_0
		public static readonly ParseChildrenAttribute ParseAsChildren = new ParseChildrenAttribute (false);
		public static readonly ParseChildrenAttribute ParseAsProperties = new ParseChildrenAttribute (true);

		Type childType = typeof(System.Web.UI.Control);
#endif

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
#if NET_2_0
		public ParseChildrenAttribute (Type childControlType)
		{
			childType = childControlType;
			defaultProperty = "";
		}
#endif

		public bool ChildrenAsProperties {

			get { return childrenAsProperties; }

			set { childrenAsProperties = value; }
		}

		public string DefaultProperty {
			get { return defaultProperty; }

			set { defaultProperty = value; }
		}

#if NET_2_0
		public Type ChildControlType {
			get { return childType; }
		}
#endif

		public override bool Equals (object obj)
		{
			ParseChildrenAttribute o = (obj as ParseChildrenAttribute);
			if (o == null)
				return false;

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
