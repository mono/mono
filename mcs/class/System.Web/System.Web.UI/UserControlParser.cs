//
// System.Web.UI.UserControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	public sealed class UserControlParser : TemplateControlParser
	{
		internal UserControlParser (string virtualPath, string inputFile, HttpContext context)
		{
			Context = context;
			CurrentVirtualPath = virtualPath;
			InputFile = Path.Combine (context.Request.MapPath (virtualPath), inputFile);
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

		protected override Type DefaultBaseType
		{
			get {
				return typeof (UserControl);
			}
		}

		protected internal override string DefaultDirectiveName
		{
			get {
				return "control";
			}
		}
	}
}

