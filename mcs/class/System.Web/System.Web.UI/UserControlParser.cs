//
// System.Web.UI.UserControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	internal sealed class UserControlParser : TemplateControlParser
	{
		internal UserControlParser (string virtualPath, string inputFile, HttpContext context)
		{
			Context = context;
			BaseVirtualDir = UrlUtils.GetDirectory (virtualPath);
			InputFile = inputFile;
			AddApplicationAssembly ();
		}
		
		public static Type GetCompiledType (string virtualPath, string inputFile, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (virtualPath, inputFile, context);
			return ucp.CompileIntoType ();
		}

		protected override Type CompileIntoType ()
		{
			AspGenerator generator = new AspGenerator (this);
			return generator.GetCompiledType ();
		}

		internal override void ProcessMainAttributes (Hashtable atts)
		{
			SetBaseType (PagesConfig.UserControlBaseType);
			base.ProcessMainAttributes (atts);
		}
		
		internal override Type DefaultBaseType {
			get { return typeof (UserControl); }
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.UI.UserControl"; }
		}

		internal override string DefaultDirectiveName {
			get { return "control"; }
		}
	}
}

