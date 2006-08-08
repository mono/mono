//
// System.Security.Principal.GenericIdentity.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Principal {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public class GenericIdentity : IIdentity {

		// field names are serialization compatible with .net
		private string m_name;
		private string m_type;
		
		public GenericIdentity (string name, string type)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (type == null)
				throw new ArgumentNullException ("type");

			m_name = name;
			m_type = type;
		}

		public GenericIdentity (string name)
			: this (name, String.Empty)
		{
		}

		public virtual string AuthenticationType {
			get {
				return m_type;
			}
		}

		public virtual string Name {
			get {
				return m_name;
			}
		}

		public virtual bool IsAuthenticated {
			get {
				return (m_name.Length > 0);
			}
		}
	}
}
