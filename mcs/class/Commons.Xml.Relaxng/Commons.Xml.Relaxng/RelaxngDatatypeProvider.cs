//
// RelaxngDatatypeProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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
using System.Xml;
using System.Xml.Schema;

namespace Commons.Xml.Relaxng
{
	public abstract class RelaxngDatatypeProvider
	{
		public abstract RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters);
	}

	internal class RelaxngNamespaceDatatypeProvider : RelaxngDatatypeProvider
	{
		static RelaxngNamespaceDatatypeProvider instance;
		static RelaxngDatatype stringType = RelaxngString.Instance;
		static RelaxngDatatype tokenType = RelaxngToken.Instance;

		static RelaxngNamespaceDatatypeProvider ()
		{
			instance = new RelaxngNamespaceDatatypeProvider ();
		}

		public static RelaxngNamespaceDatatypeProvider Instance {
			get { return instance; }
		}

		private RelaxngNamespaceDatatypeProvider () {}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters)
		{
			if (ns != String.Empty)
				throw new RelaxngException ("Not supported data type URI");
			if (parameters != null && parameters.Count > 0)
				throw new RelaxngException ("Parameter is not allowed for this datatype: " + name);

			switch (name) {
			case "string":
				return stringType;
			case "token":
				return tokenType;
			}
			return null;
		}
	}
}
