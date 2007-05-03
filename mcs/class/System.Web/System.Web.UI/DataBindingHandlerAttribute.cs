//
// System.Web.UI.WebControls.DataBindingHandlerAttribute class
//
// Author: Duncan Mak (duncan@novell.com)
//
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
	public sealed class DataBindingHandlerAttribute : Attribute
	{
		string name;		

		static DataBindingHandlerAttribute ()
		{
			Default = new DataBindingHandlerAttribute ();
		}

		public DataBindingHandlerAttribute ()
			: this (String.Empty)
		{
		}

		public DataBindingHandlerAttribute (string name)
		{
			this.name = (name != null) ? name : String.Empty;
		}

		public DataBindingHandlerAttribute (Type type)
		{
			this.name = type.AssemblyQualifiedName;
		}

		public static readonly DataBindingHandlerAttribute Default;

		public override bool Equals (object obj) 
		{
			DataBindingHandlerAttribute other = obj as DataBindingHandlerAttribute;
			if (other == null) {
				return false;
			}

			return HandlerTypeName.Equals (other.HandlerTypeName);
		}

		public override int GetHashCode () 
		{
			return HandlerTypeName.GetHashCode ();
		}

		public string HandlerTypeName {
			get { return name; }
		}
	}
}
