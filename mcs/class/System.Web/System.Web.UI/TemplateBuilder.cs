//
// System.Web.UI.TemplateBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System;
using System.Collections;
using System.Reflection;

namespace System.Web.UI
{
	public class TemplateBuilder : ControlBuilder, ITemplate
	{
		string text;
		TemplateContainerAttribute containerAttribute;

		public TemplateBuilder ()
		{
		}

		internal TemplateBuilder (ICustomAttributeProvider prov)
		{
			object[] ats = prov.GetCustomAttributes (typeof(TemplateContainerAttribute), true);
			if (ats.Length > 0) {
				containerAttribute = (TemplateContainerAttribute) ats [0];
			}
		}

		public virtual string Text {
			get { return text; }
			set { text = value; }
		}
		
		public Type ContainerType {
			get { return containerAttribute != null ? containerAttribute.ContainerType : null; }
		}

		public override void Init (TemplateParser parser,
					  ControlBuilder parentBuilder,
					  Type type,
					  string tagName,
					  string ID,
					  IDictionary attribs)
		{
			// enough?
			base.Init (parser, parentBuilder, type, tagName, ID, attribs);
		}
		
		public virtual void InstantiateIn (Control container)
		{
			CreateChildren (container);
		}

		public override bool NeedsTagInnerText ()
		{
			return false;
		}

		public override void SetTagInnerText (string text)
		{
			this.text = text;
		}
	}
}

