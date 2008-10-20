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
//
// System.Web.UI.ToolboxDataAttribute.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)

using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ToolboxDataAttribute : Attribute {

		public static readonly ToolboxDataAttribute Default = new ToolboxDataAttribute (String.Empty);

		string data;

		public ToolboxDataAttribute (string data)
		{
			this.data = data;
		}

		public string Data {
			get { return data; }
		}

		public override bool Equals (object obj)
		{
			ToolboxDataAttribute other = obj as ToolboxDataAttribute;
			if (other == null)
				return false;
			return other.Data == data;
		}

		public override int GetHashCode ()
		{
			if (data == null)
				return -1;
			return data.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
#if NET_2_0
			return ((data == null) || (data.Length == 0));
#else
			return ((data != null) && (data.Length == 0));
#endif
		}
	}
}


