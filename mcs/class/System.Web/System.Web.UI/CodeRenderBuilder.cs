//
// System.Web.UI.CodeRenderBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Web.Compilation;

namespace System.Web.UI
{
	sealed class CodeRenderBuilder : CodeBuilder
	{
		public CodeRenderBuilder (string code, bool isAssign, ILocation location)
			: base (code, isAssign, location)
		{
		}
	}
}

