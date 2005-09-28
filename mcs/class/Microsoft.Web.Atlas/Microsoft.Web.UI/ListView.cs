//
// Microsoft.Web.UI.ListView
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Web.UI;
using Microsoft.Web;

namespace Microsoft.Web.UI
{
	public class ListView : ScriptControl
	{
		ITemplate emptyTemplate;
		ITemplate layoutTemplate;
		
		public ListView ()
		{
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("alternatingItemCssClass", ScriptType.String, false, "AlternatingItemCssClass"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("data", ScriptType.Object, false, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("length", ScriptType.Number, true, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("layoutTemplate", ScriptType.Object, false, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("itemCssClass", ScriptType.String, false, "ItemCssClass"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("itemTemplateParentElementId", ScriptType.String, false, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("separatorTemplate", ScriptType.Object, false, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("emptyTemplate", ScriptType.Object, false, ""));
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			ScriptManager mgr = ScriptManager.GetCurrentScriptManager (Page);
			mgr.RegisterScriptReference ("ScriptLibrary/AtlasUI.js", true);
			mgr.RegisterScriptReference ("ScriptLibrary/AtlasControls.js", true);
		}

		protected override void Render (HtmlTextWriter writer)
		{
		}

		protected override void RenderScriptTagContents (ScriptTextWriter writer)
		{
			base.RenderScriptTagContents (writer);

			writer.WriteStartElement ("layoutTemplate");
			writer.WriteStartElement ("template");
			writer.WriteAttributeString ("layoutElement", ID + "_layoutTemplate"); // XXX ?
			writer.WriteEndElement (); // template
			writer.WriteEndElement (); // layoutTemplate

			writer.WriteStartElement ("itemTemplate");
			writer.WriteEndElement (); // itemTemplate
		}

		public string AlternatingItemCssClass {
			get {
				object o = ViewState["AlternatingItemCssClass"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["AlternatingItemCssClass"] = value;
			}
		}

		public ITemplate EmptyTemplate {
			get {
				return emptyTemplate;
			}
			set {
				emptyTemplate = value;
			}
		}

		public string ItemCssClass {
			get {
				object o = ViewState["ItemCssClass"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["ItemCssClass"] = value;
			}
		}

		public string ItemTemplateControlID {
			get {
				object o = ViewState["ItemTemplateControlID"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["ItemTemplateControlID"] = value;
			}
		}

		public ITemplate LayoutTemplate {
			get {
				return layoutTemplate;
			}
			set {
				layoutTemplate = value;
			}
		}

		public string SeparatorTemplateControlID {
			get {
				object o = ViewState["SeparatorTemplateControlID"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["SeparatorTemplateControlID"] = value;
			}
		}

		public override string TagName {
			get {
				return "listView";
			}
		}
	}

}

#endif
