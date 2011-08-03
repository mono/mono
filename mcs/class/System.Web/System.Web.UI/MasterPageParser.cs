//
// System.Web.UI.MasterPageParser
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.UI
{
	internal sealed class MasterPageParser: UserControlParser
	{
		Type masterType;
		string masterTypeVirtualPath;
		List <string> contentPlaceHolderIds;
		string cacheEntryName;
		
		internal MasterPageParser (VirtualPath virtualPath, string inputFile, HttpContext context)
			: base (virtualPath, inputFile, context, "System.Web.UI.MasterPage")
		{
			this.cacheEntryName = String.Concat ("@@MasterPagePHIDS:", virtualPath, ":", inputFile);
			
			contentPlaceHolderIds = HttpRuntime.InternalCache.Get (this.cacheEntryName) as List <string>;
			LoadConfigDefaults ();
		}

		internal MasterPageParser (VirtualPath virtualPath, TextReader reader, HttpContext context)
			: this (virtualPath, null, reader, context)
		{
		}
		
		internal MasterPageParser (VirtualPath virtualPath, string inputFile, TextReader reader, HttpContext context)
			: base (virtualPath, inputFile, reader, context)
		{
			this.cacheEntryName = String.Concat ("@@MasterPagePHIDS:", virtualPath, ":", InputFile);
			
			contentPlaceHolderIds = HttpRuntime.InternalCache.Get (this.cacheEntryName) as List <string>;
			LoadConfigDefaults ();
		}
		
		public static MasterPage GetCompiledMasterInstance (string virtualPath, string inputFile, HttpContext context)
		{
			return BuildManager.CreateInstanceFromVirtualPath (virtualPath, typeof (MasterPage)) as MasterPage;
		}

		public static Type GetCompiledMasterType (string virtualPath, string inputFile, HttpContext context)
		{
			return BuildManager.GetCompiledType (virtualPath);
		}
		
		internal override void HandleOptions (object obj)
		{
			base.HandleOptions (obj);

			MasterPage mp = (MasterPage)obj;
			mp.MasterPageFile = MasterPageFile;
		}

		internal override void AddDirective (string directive, IDictionary atts)
		{
			if (String.Compare ("MasterType", directive, StringComparison.OrdinalIgnoreCase) == 0) {
				PageParserFilter pfilter = PageParserFilter;
				if (pfilter != null)
					pfilter.PreprocessDirective (directive.ToLowerInvariant (), atts);
				
				string type = GetString (atts, "TypeName", null);
				if (type != null) {
					masterType = LoadType (type);
					if (masterType == null)
						ThrowParseException ("Could not load type '" + type + "'.");
				} else {
					string path = GetString (atts, "VirtualPath", null);
					if (!String.IsNullOrEmpty (path)) {
						var vpp = HostingEnvironment.VirtualPathProvider;
						if (!vpp.FileExists (path))
							ThrowParseFileNotFound (path);

						path = vpp.CombineVirtualPaths (VirtualPath.Absolute, VirtualPathUtility.ToAbsolute (path));
						masterTypeVirtualPath = path;
						AddDependency (path);
					} else
						ThrowParseException ("The MasterType directive must have either a TypeName or a VirtualPath attribute.");
				}
				if (masterType != null)
					AddAssembly (masterType.Assembly, true);
			}
			else
				base.AddDirective (directive, atts);
		}

		internal void AddContentPlaceHolderId (string id)
		{
			if (contentPlaceHolderIds == null) {
				contentPlaceHolderIds = new List <string> (1);
				HttpRuntime.InternalCache.Insert (cacheEntryName, contentPlaceHolderIds);
			}
			
			contentPlaceHolderIds.Add (id);
		}
		
		internal Type MasterType {
			get {
				if (masterType == null && !String.IsNullOrEmpty (masterTypeVirtualPath))
					masterType = BuildManager.GetCompiledType (masterTypeVirtualPath);
				
				return masterType;
			}
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.UI.MasterPage"; }
		}

		internal override string DefaultDirectiveName {
			get { return "master"; }
		}
	}
}
