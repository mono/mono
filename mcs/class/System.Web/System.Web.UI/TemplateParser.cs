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
using System.Collections;
using System.Web;

namespace System.Web.UI
{
	internal delegate Type TagMapper (string tag);

	public abstract class TemplateParser : BaseParser
	{
		string inputFile;
		string text;
		Hashtable options;
		TagMapper mapper;

		protected abstract Type CompileIntoType ();

		protected virtual void HandleOptions (object obj)
		{
		}

		internal void SetTagMapper (TagMapper mapper)
		{
			this.mapper = mapper;
		}

		internal Type GetTypeFromTag (string tag)
		{
			if (mapper == null)
				throw new HttpException ("No tag mapper set!");

			return mapper (tag);
		}

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
		
		internal Hashtable Options {
			get { return options; }
			set { options = value; }
		}
	}
}

