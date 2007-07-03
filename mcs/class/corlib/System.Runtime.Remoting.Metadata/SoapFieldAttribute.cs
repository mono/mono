//
// System.Runtime.Remoting.Metadata.SoapFieldAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;
using System.Reflection;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Field)]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapFieldAttribute : SoapAttribute
	{
		int _order;
		string _elementName;
		bool _isElement = false;
		
		public SoapFieldAttribute ()
		{
		}

		public int Order {
			get {
				return _order;
			}

			set {
				_order = value;
			}
		}
		
		public string XmlElementName {
			get {
				return _elementName;
			}

			set {
				_isElement = value != null;
				_elementName = value;
			}
		}

		public bool IsInteropXmlElement ()
		{
			return _isElement;
		}
		
		internal override void SetReflectionObject (object reflectionObject)
		{
			FieldInfo f = (FieldInfo) reflectionObject;
			if (_elementName == null) _elementName = f.Name;
		}
	}
}
