//
// System.Web.UI.CodeRenderBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Web.UI
{
	sealed class CodeRenderBuilder : CodeBuilder
	{
		public CodeRenderBuilder (string code, bool isAssign, string fileName, int line)
			: base (code, isAssign, fileName, line)
		{
		}
	}
}

