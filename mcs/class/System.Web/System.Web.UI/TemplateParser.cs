//
// System.Web.UI.TemplateParser.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	public abstract class TemplateParser : BaseParser
	{
		protected abstract Type CompileIntoType ();
	}
}
