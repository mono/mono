//
// System.Web.UI.ParseChildrenAttribute.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
//

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

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ParseChildrenAttribute : Attribute
	{
		bool childrenAsProperties;
		string defaultProperty;
		public static readonly ParseChildrenAttribute Default = new ParseChildrenAttribute ();
		
#if NET_2_0
		Type childType;
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
			set { childType = value; }
		}
#endif

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
