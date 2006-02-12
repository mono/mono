//
// XhtmlTextWriter.cs
//
// Author:
//	Cesar Lopez Nataren <cnataren@novell.com>
//

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Collections;

namespace System.Web.UI {

	public class XhtmlTextWriter : HtmlTextWriter
	{
		Hashtable common_attrs = new Hashtable (DefaultCommonAttributes.Length);
		Hashtable suppress_common_attrs = new Hashtable (DefaultSuppressCommonAttributes.Length);
		Hashtable element_specific_attrs = new Hashtable ();

		Hashtable attr_render = new Hashtable ();

		XhtmlMobileDocType doc_type;

		static string [] DefaultCommonAttributes = {
			"class",
			"id",
			"title",
			"xml:lang"
		};

		//
		// XHTML elements whose CommonAttributes are supressed
		//
		static string [] DefaultSuppressCommonAttributes = {
			"base",
			"meta",
			"br",
			"head",
			"title",
			"html",
			"style"
		};

	        public XhtmlTextWriter (TextWriter writer)
			: this (writer, DefaultTabString)
		{
		}

		public XhtmlTextWriter (TextWriter writer, string tabString)
			: base (writer, tabString)
		{
			SetupCommonAttributes ();
			SetupSuppressCommonAttributes ();
			SetupElementsSpecificAttributes ();
		}

		void SetupHash (Hashtable hash, string [] values) {
			foreach (string str in values)
				hash.Add (str, true);
		}

		//
		// if you need to add a new default common attribute,
		// add the literal as a member of the DefaultCommonAttributes array
		//
		void SetupCommonAttributes ()
		{
			SetupHash (common_attrs, DefaultCommonAttributes);
		}

		//
		// if you need to add a new suppressed common attribute,
		// add the literal as a member of the SuppressCommonAttrs array
		//
		void SetupSuppressCommonAttributes ()
		{
			SetupHash (suppress_common_attrs, DefaultSuppressCommonAttributes);
		}

		//
		// I did not make them static because different instances of XhtmlTextWriter's
		// do not share the changes made to the element's attributes tables,
		// they are not read-only.
		//
		Hashtable a_attrs, base_attrs, blockquote_attrs, br_attrs, form_attrs, head_attrs;
		Hashtable html_attrs, img_attrs, input_attrs, label_attrs, li_attrs, link_attrs;
		Hashtable meta_attrs, object_attrs, ol_attrs, optgroup_attrs, option_attrs, param_attrs;
		Hashtable pre_attrs, q_attrs, select_attrs, style_attrs, table_attrs, textarea_attrs;
		Hashtable td_attrs, th_attrs, title_attrs, tr_attrs;

		void SetupElementsSpecificAttributes ()
		{
			string [] a_attrs_names = {"accesskey", "href", "charset", "hreflang", "rel", "type", "rev", "title", "tabindex"};
			SetupElementSpecificAttributes ("a", a_attrs, a_attrs_names);

			string [] base_attrs_names = {"href"};
			SetupElementSpecificAttributes ("base", base_attrs, base_attrs_names);

			string [] blockquote_attrs_names = {"cite"};
			SetupElementSpecificAttributes ("blockquote", blockquote_attrs, blockquote_attrs_names);

			string [] br_attrs_names = {"id", "class", "title"};
			SetupElementSpecificAttributes ("br", br_attrs, br_attrs_names);

			string [] form_attrs_names = {"action", "method", "enctype"};
			SetupElementSpecificAttributes ("form", form_attrs, form_attrs_names);

			string [] head_attrs_names = {"xml:lang"};
			SetupElementSpecificAttributes ("head", head_attrs, head_attrs_names);

			string [] html_attrs_names = {"version", "xml:lang", "xmlns"};
			SetupElementSpecificAttributes ("html", html_attrs, html_attrs_names);

			string [] img_attrs_names = {"src", "alt", "width", "longdesc", "height"};
			SetupElementSpecificAttributes ("img", img_attrs, img_attrs_names);

			string [] input_attrs_names = {"size", "accesskey", "title", "name", "type", "disabled", 
						       "value", "src", "checked", "maxlength", "tabindex"};
			SetupElementSpecificAttributes ("input", input_attrs, input_attrs_names);

			string [] label_attrs_names = {"accesskey", "for"};
			SetupElementSpecificAttributes ("label", label_attrs, label_attrs_names);

			string [] li_attrs_names = {"value"};
			SetupElementSpecificAttributes ("li", li_attrs, li_attrs_names);

			string [] link_attrs_names = {"hreflang", "rev", "type", "charset", "rel", "href", "media"};
			SetupElementSpecificAttributes ("link", link_attrs, link_attrs_names);

			string [] meta_attrs_names = {"content", "name", "xml:lang", "http-equiv", "scheme"};
			SetupElementSpecificAttributes ("meta", meta_attrs, meta_attrs_names);

			string [] object_attrs_names = {"codebase", "classid", "data", "standby", "name", "type", 
							"height", "archive", "declare", "width", "tabindex", "codetype"};
			SetupElementSpecificAttributes ("object", object_attrs, object_attrs_names);

			string [] ol_attrs_names = {"start"};
			SetupElementSpecificAttributes ("ol", ol_attrs, ol_attrs_names);

			string [] optgroup_attrs_names = {"label", "disabled"};
			SetupElementSpecificAttributes ("optgroup", optgroup_attrs, optgroup_attrs_names);

			string [] option_attrs_names = {"selected", "value"};
			SetupElementSpecificAttributes ("option", option_attrs, option_attrs_names);

			string [] param_attrs_names = {"id", "name", "valuetype", "value", "type"};
			SetupElementSpecificAttributes ("param", param_attrs, param_attrs_names);

			string [] pre_attrs_names = {"xml:space"};
			SetupElementSpecificAttributes ("pre", pre_attrs, pre_attrs_names);

			string [] q_attrs_names = {"cite"};
			SetupElementSpecificAttributes ("q", q_attrs, q_attrs_names);

			string [] select_attrs_names = {"name", "tabindex", "disabled", "multiple", "size"};
			SetupElementSpecificAttributes ("select", select_attrs, select_attrs_names);

			string [] style_attrs_names = {"xml:lang", "xml:space", "type", "title", "media"};
			SetupElementSpecificAttributes ("style", style_attrs, style_attrs_names);

			string [] table_attrs_names = {"width", "summary"};
			SetupElementSpecificAttributes ("table", table_attrs, table_attrs_names);

			string [] textarea_attrs_names = {"name", "cols", "accesskey", "tabindex", "rows"};
			SetupElementSpecificAttributes ("textarea", textarea_attrs, textarea_attrs_names);

			string [] td_and_th_attrs_names = {"headers", "align", "rowspan", "colspan", "axis",
							   "scope", "abbr", "valign"};
			SetupElementSpecificAttributes ("td", td_attrs, td_and_th_attrs_names);
			SetupElementSpecificAttributes ("th", th_attrs, td_and_th_attrs_names);

			string [] title_attrs_names = {"xml:lang"};
			SetupElementSpecificAttributes ("title", title_attrs, title_attrs_names);

			string [] tr_attrs_names = {"align", "valign"};
			SetupElementSpecificAttributes ("tr", tr_attrs, tr_attrs_names);
		}
		
		void SetupElementSpecificAttributes (string elementName, Hashtable attrs, string [] attributesNames)
		{
			attrs = new Hashtable (attributesNames.Length);
			InitElementAttributes (attrs, attributesNames);
			element_specific_attrs.Add (elementName, attrs);
		}

		void InitElementAttributes (Hashtable attrs, string [] attributesNames)
		{
			SetupHash (attrs, attributesNames);
		}

		protected Hashtable CommonAttributes {
			get { return common_attrs; }
		}

		protected Hashtable ElementSpecificAttributes {
			get { return element_specific_attrs; }
		}

		protected Hashtable  SuppressCommonAttributes {
			get { return suppress_common_attrs; }
		}

		public virtual void AddRecognizedAttribute (string elementName, string attributeName)
		{
			Hashtable elem_attrs = (Hashtable) element_specific_attrs [elementName];

			if (elem_attrs == null) {
				Hashtable attrs = new Hashtable ();
				attrs.Add (attributeName, true);
				element_specific_attrs.Add (elementName, attrs);
			} else
				elem_attrs.Add (attributeName, true);
		}

		public override bool IsValidFormAttribute (string attributeName)
		{
			return attributeName == "action" || attributeName == "method" || attributeName == "enctype";
		}

		public virtual void RemoveRecognizedAttribute (string elementName, string attributeName)
		{
			Hashtable elem_attrs = (Hashtable) element_specific_attrs [elementName];

			if (elem_attrs != null)
				elem_attrs.Remove (attributeName);
		}

		public virtual void SetDocType (XhtmlMobileDocType docType)
		{
			doc_type = docType;
		}

		// writes <br/>
		public override void WriteBreak ()
		{
			string tag = GetTagName (HtmlTextWriterTag.Br);
			WriteBeginTag (tag);
			Write (SlashChar);
			Write (TagRightChar);
		}

		[MonoTODO]
		protected override bool OnAttributeRender (string name, string value, HtmlTextWriterAttribute key)
		{
			// I tested every possible value of HtmlTextWriterAttribute
			// and the MS implementation always throws ArgumentNullException
			return (bool) attr_render [null];
		}

		[MonoTODO]
		protected override bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
			// I tested every possible value of HtmlTextWriterStyle
			// and the MS implementation always returned false. Sigh
			return false;
		}
	}
}

#endif
