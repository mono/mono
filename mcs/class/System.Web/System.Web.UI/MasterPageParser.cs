//
// System.Web.UI.MasterPageParser
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc.
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

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	internal sealed class MasterPageParser: UserControlParser
	{
		internal MasterPageParser (string virtualPath, string inputFile, HttpContext context)
		: base (virtualPath, inputFile, context, "System.Web.UI.MasterPage")
		{
		}
		
		public static MasterPage GetCompiledMasterInstance (string virtualPath, string inputFile, HttpContext context)
		{
			MasterPageParser mpp = new MasterPageParser (virtualPath, inputFile, context);
			return (MasterPage) mpp.GetCompiledInstance ();
		}

		internal override Type DefaultBaseType {
			get { return typeof (MasterPage); }
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.UI.MasterPage"; }
		}

		internal override string DefaultDirectiveName {
			get { return "master"; }
		}
	}
}

#endif
