//
// System.Web.UI.ApplicationFileParser.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004-2010 Novell, Inc. (http://www.novell.com)
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
using System.Web.Util;

namespace System.Web.UI
{
	sealed class ApplicationFileParser : TemplateParser
	{
		static List <string> dependencies;
		TextReader reader;
		
		public ApplicationFileParser (string fname, HttpContext context)
		{
			InputFile = fname;
			Context = context;
			VirtualPath = new VirtualPath ("/" + Path.GetFileName (fname));
			LoadConfigDefaults ();
		}

		internal ApplicationFileParser (VirtualPath virtualPath, TextReader reader, HttpContext context)
			: this (virtualPath, null, reader, context)
		{
		}
		
		internal ApplicationFileParser (VirtualPath virtualPath, string inputFile, TextReader reader, HttpContext context)
		{
			VirtualPath = virtualPath;
			Context = context;
			Reader = reader;

			if (String.IsNullOrEmpty (inputFile))
				InputFile = virtualPath.PhysicalPath;
			else
				InputFile = inputFile;
			
			SetBaseType (null);
			LoadConfigDefaults ();
		}
		
		internal override Type CompileIntoType ()
		{
			return GlobalAsaxCompiler.CompileApplicationType (this);
		}

		internal static Type GetCompiledApplicationType (string inputFile, HttpContext context)
		{
			ApplicationFileParser parser = new ApplicationFileParser (inputFile, context);
			AspGenerator generator = new AspGenerator (parser);
			Type type = generator.GetCompiledType ();
			dependencies = parser.Dependencies;
			return type;
		}

		internal override void AddDirective (string directive, IDictionary atts)
		{
			if (String.Compare (directive, "application", true, Helpers.InvariantCulture) != 0 &&
			    String.Compare (directive, "Import", true, Helpers.InvariantCulture) != 0 &&
			    String.Compare (directive, "Assembly", true, Helpers.InvariantCulture) != 0)
				ThrowParseException ("Invalid directive: " + directive);

			base.AddDirective (directive, atts);
		}

		internal static List <string> FileDependencies {
			get { return dependencies; }
		}		
#if NET_4_0
		internal override Type DefaultBaseType {
			get {
				Type ret = PageParser.DefaultApplicationBaseType;
				if (ret == null)
					return base.DefaultBaseType;

				return ret;
			}
		}
#endif
		internal override string DefaultBaseTypeName {
			get { return "System.Web.HttpApplication"; }
		}

		internal override string DefaultDirectiveName {
			get { return "application"; }
		}

		internal override string BaseVirtualDir {
			get { return Context.Request.ApplicationPath; }
		}
		
		internal override TextReader Reader {
                        get { return reader; }
                        set { reader = value; }
                }
	}

}

