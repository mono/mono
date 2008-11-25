//
// System.Web.UI.INonBindingContainer
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//

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
namespace System.Web.UI 
{
	// Apparently this attribute is used in place of our BINDING_CONTAINER control mask bit,
	// which makes sense as in certain scenarios (e.g. TemplateControlAttribute naming a
	// container type which cannot be used for two-way binding) the check must be made on type,
	// not on object.
	//
	// This interface is briefly mentioned in:
	//  http://www.developmentnow.com/g/10_2006_6_0_0_776419/DataBinding-to-SubProperties.htm
	//
	// For a sample of why this is needed, see:
	//
	//  http://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.templatepagerfield.pagertemplate.aspx
	//  The C# code on the above page won't work without this interface being implemented and
	//  used in ControlBuilder.BindingContainerType. The reason why the sample wouldn't work is
	//  that the TemplatePagerField.PagerTemplate property carries a custom TemplateContainer
	//  (typeof (DataPagerFieldItem)) attribute and, without INonBindingContainer interface
	//  attached to DataPagerFieldItem, mono would generate code with the Container and
	//  BindingContainer referring to DataPagerFieldItem. In such case, attempting to access
	//  DataPager properties would obviously fail. Looking at the MS.NET generated code,
	//  Container and BindingContainer are of the DataPager type in this instance.
	//
	internal interface INonBindingContainer
	{
	}
}
