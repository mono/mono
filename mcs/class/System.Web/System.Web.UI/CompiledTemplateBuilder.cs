//
// System.Web.UI.CompiledTemplateBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
// This is used in the generated C# code from MS and xsp does the same.
// It just seems to be a container implementing an ITemplate interface.

namespace System.Web.UI {

public sealed class CompiledTemplateBuilder : ITemplate
{
	private BuildTemplateMethod templateMethod;

	public CompiledTemplateBuilder (BuildTemplateMethod templateMethod)
	{
		this.templateMethod = templateMethod;
	}

	public void InstantiateIn (Control ctrl)
	{
		templateMethod (ctrl);
	}
}
}

