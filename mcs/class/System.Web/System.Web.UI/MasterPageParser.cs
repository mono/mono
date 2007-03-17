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
		Type masterType;

		internal MasterPageParser (string virtualPath, string inputFile, HttpContext context)
		: base (virtualPath, inputFile, context, "System.Web.UI.MasterPage")
		{
		}
		
		public static MasterPage GetCompiledMasterInstance (string virtualPath, string inputFile, HttpContext context)
		{
			MasterPageParser mpp = new MasterPageParser (virtualPath, inputFile, context);
			return (MasterPage) mpp.GetCompiledInstance ();
		}

		public static Type GetCompiledMasterType (string virtualPath, string inputFile, HttpContext context)
		{
			MasterPageParser mpp = new MasterPageParser (virtualPath, inputFile, context);
			return mpp.CompileIntoType ();
		}
		internal override void HandleOptions (object obj)
		{
			base.HandleOptions (obj);

			MasterPage mp = (MasterPage)obj;
			mp.MasterPageFile = MasterPageFile;
		}


		internal override void AddDirective (string directive, Hashtable atts)
		{
			if (String.Compare ("MasterType", directive, true) == 0) {
				string type = GetString (atts, "TypeName", null);
				if (type != null) {
					masterType = LoadType (type);
					if (masterType == null)
						ThrowParseException ("Could not load type '" + type + "'.");
				} else {
					string path = GetString (atts, "VirtualPath", null);
					if (path != null)
						masterType = MasterPageParser.GetCompiledMasterType (path, MapPath (path), HttpContext.Current);
					else
						ThrowParseException ("The MasterType directive must have either a TypeName or a VirtualPath attribute.");				}
				AddAssembly (masterType.Assembly, true);
			}
			else
				base.AddDirective (directive, atts);
		}

		internal Type MasterType {
			get { return masterType; }
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
