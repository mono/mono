//
// System.Web.UI.ApplicationfileParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web;

namespace System.Web.UI
{
	internal sealed class ApplicationFileParser : TemplateParser
	{
		[MonoTODO]
		protected override Type CompileIntoType ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static Type GetCompiledApplicationType (string inputFile, 
								 HttpContext context,
								 ref ApplicationFileParser parser)
		{
			throw new NotImplementedException ();
		}

		protected override Type DefaultBaseType
		{
			get {
				return typeof (HttpApplication);
			}
		}

		protected override string DefaultDirectiveName
		{
			get {
				return "application";
			}
		}
	}

}

