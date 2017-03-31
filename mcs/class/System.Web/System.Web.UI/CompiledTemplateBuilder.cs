//
// System.Web.UI.CompiledTemplateBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// This is used in the generated C# code from MS and xsp does the same.
// It just seems to be a container implementing an ITemplate interface.

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;

namespace System.Web.UI {

// CAS - no InheritanceDemand here as the class is sealed
[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
public sealed class CompiledTemplateBuilder : ITemplate
{
	BuildTemplateMethod templateMethod;

	public CompiledTemplateBuilder (BuildTemplateMethod buildTemplateMethod)
	{
		this.templateMethod = buildTemplateMethod;
	}

	public void InstantiateIn (Control container)
	{
		templateMethod (container);
	}
}
}

