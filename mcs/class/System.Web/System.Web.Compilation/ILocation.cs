//
// System.Web.Compilation.ILocation
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.Compilation
{
	interface ILocation
	{
		string Filename { get; }
		int BeginLine { get; }
		int EndLine { get; }
		int BeginColumn { get; }
		int EndColumn { get; }
		string PlainText { get; }
	}
}

