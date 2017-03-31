//
// System.Web.UI.WebControls.WebParts.ConnectionInterfaceCollection.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace System.Web.UI.WebControls.WebParts
{
	public sealed class ConnectionInterfaceCollection : ReadOnlyCollectionBase
	{
		public static readonly ConnectionInterfaceCollection Empty = new ConnectionInterfaceCollection();

		public ConnectionInterfaceCollection ()
		{
		}

		public ConnectionInterfaceCollection (ICollection connectionInterfaces)
		{
			InnerList.AddRange (connectionInterfaces);
		}

		public ConnectionInterfaceCollection (ConnectionInterfaceCollection existingConnectionInterfaces,
						      ICollection connectionInterfaces)
			: this()
		{
			InnerList.AddRange (existingConnectionInterfaces);
			InnerList.AddRange (connectionInterfaces);
		}

		public bool Contains (Type value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (Type[] array, 
				    int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (Type value)
		{
			return InnerList.IndexOf (value);
		}

		public Type this [ int index ] {
			get {
				return (Type)InnerList [index];
			}
		}
	}
}
