//
// System.Web.UI.CodeBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Web.Compilation;

namespace System.Web.UI
{
	abstract class CodeBuilder : ControlBuilder
	{
		string code;
		bool isAssign;
		
		public CodeBuilder (string code, bool isAssign, ILocation location)
		{
			this.code = code;
			this.isAssign = isAssign;
			this.line = location.BeginLine;
			this.fileName = location.Filename;
			this.location = location;
		}

		internal override object CreateInstance ()
		{
			return null;
		}

		internal string Code {
			get { return code; }
			set { code = value; }
		}

		internal bool IsAssign {
			get { return isAssign; }
		}
	}
}

