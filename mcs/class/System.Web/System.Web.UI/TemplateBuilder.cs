//
// System.Web.UI.TemplateBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;

namespace System.Web.UI
{
	public class TemplateBuilder : ControlBuilder, ITemplate
	{
		string text;

		public TemplateBuilder ()
		{
		}

		public virtual string Text {
			get { return text; }
			set { text = value; }
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

