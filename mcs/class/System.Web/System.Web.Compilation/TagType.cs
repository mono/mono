//
// System.Web.Compilation.TagType
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
namespace System.Web.Compilation
{
	enum TagType
	{
		Text,
		Tag,
		Close,
		SelfClosing,
		Directive,
		ServerComment,
		DataBinding,
		CodeRender,
		CodeRenderExpression,
		Include
	}
}

