//
// System.Web.UI.TemplateControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Web.Compilation;

namespace System.Web.UI
{
	public abstract class TemplateControlParser : TemplateParser
	{
		internal object GetCompiledInstance (string virtualPath, string inputFile, HttpContext context)
		{
			InputFile = inputFile;
			Type type = CompileIntoType ();
			if (type == null)
				return null;

			object ctrl = Activator.CreateInstance (type);
			if (ctrl == null)
				return null;

			HandleOptions (ctrl);
			return ctrl;
		}

		protected override void HandleOptions (object obj)
		{
			Control ctrl = obj as Control;
			Hashtable options = Options;
			if (options == null)
				return;

			if (options ["AutoEventWireup"] != null)
				ctrl.AutoEventWireup = (bool) options ["AutoEventWireup"];
		}
	}
}

