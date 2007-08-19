//
// System.Web.UI.PageTheme.cs
//
// Authors:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;
using System.Xml;

namespace System.Web.UI {

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public abstract class PageTheme
	{
		Page _page = null;

		protected PageTheme ()
		{
		}

		public static object CreateSkinKey (Type controlType, string skinID)
		{
			return string.Concat (skinID, ":", controlType);
		}

		protected object Eval (string expression)
		{
			return Page.Eval (expression);
		}

		protected string Eval (string expression, string format)
		{
			return Page.Eval (expression, format);
		}

		[MonoTODO("Not implemented")]
		public bool TestDeviceFilter (string deviceFilterName)
		{
			throw new NotImplementedException ();
		}

		protected object XPath (string xPathExpression)
		{
			return Page.XPath (xPathExpression);
		}

		protected object XPath (string xPathExpression, IXmlNamespaceResolver resolver)
		{
			return Page.XPath (xPathExpression, resolver);
		}

		protected string XPath (string xPathExpression, string format)
		{
			return Page.XPath (xPathExpression, format);
		}

		protected string XPath (string xPathExpression, string format, IXmlNamespaceResolver resolver)
		{
			return Page.XPath (xPathExpression, format, resolver);
		}

		protected IEnumerable XPathSelect (string xPathExpression)
		{
			return Page.XPathSelect (xPathExpression);
		}

		protected IEnumerable XPathSelect (string xPathExpression, IXmlNamespaceResolver resolver)
		{
			return Page.XPathSelect (xPathExpression, resolver);
		}

		protected abstract string AppRelativeTemplateSourceDirectory { get; }
		protected abstract IDictionary ControlSkins { get; }
		protected abstract string[] LinkedStyleSheets { get; }

		protected Page Page {
			get { return _page; }
		}

		internal void SetPage (Page page)
		{
			_page = page;
		}

		internal ControlSkin GetControlSkin (Type controlType, string skinID)
		{
			object key = PageTheme.CreateSkinKey (controlType, skinID);
			return ControlSkins[key] as ControlSkin;
		}

		internal string [] GetStyleSheets () {
			return LinkedStyleSheets;
		}
	}
}

#endif
