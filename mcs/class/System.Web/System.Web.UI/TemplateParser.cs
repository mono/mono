//
// System.Web.UI.TemplateParser
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Web;

namespace System.Web.UI
{
	public abstract class TemplateParser : BaseParser
	{
		private string inputFile;
		private string text;

		protected abstract Type CompileIntoType ();

		protected abstract Type DefaultBaseType { get; }

		protected abstract string DefaultDirectiveName { get; }

		internal string InputFile
		{
			get { return inputFile; }
			set { inputFile = value; }
		}

		internal string Text
		{
			get { return text; }
			set { text = value; }
		}

		internal Type BaseType
		{
			get { return DefaultBaseType; }
		}
	}
}

