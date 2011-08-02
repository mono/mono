// 
// XmlSerializerImplementation.cs 
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
using System.Collections;

namespace System.Xml.Serialization
{
#if NET_2_0
	public
#else
	internal
#endif
	
	abstract class XmlSerializerImplementation
	{
		public virtual XmlSerializationReader Reader {
			get { throw new NotSupportedException (); }
		}
#if MOONLIGHT
		public virtual IDictionary ReadMethods {
			get { throw new NotSupportedException (); }
		}
		public virtual IDictionary TypedSerializers {
			get { throw new NotSupportedException (); }
		}
		public virtual IDictionary WriteMethods {
			get { throw new NotSupportedException (); }
		}
#else
		public virtual Hashtable ReadMethods {
			get { throw new NotSupportedException (); }
		}
		public virtual Hashtable TypedSerializers {
			get { throw new NotSupportedException (); }
		}
		public virtual Hashtable WriteMethods {
			get { throw new NotSupportedException (); }
		}
#endif
		public virtual XmlSerializationWriter Writer {
			get { throw new NotSupportedException (); }
		}
		public virtual bool CanSerialize (Type type)
		{
			throw new NotSupportedException ();
		}
		public virtual XmlSerializer GetSerializer (Type type)
		{
			throw new NotSupportedException ();
		}
	}
}

