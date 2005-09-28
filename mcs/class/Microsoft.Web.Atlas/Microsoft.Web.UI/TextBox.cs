//
// Microsoft.Web.UI.TextBox
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
	public class TextBox : ScriptControl
	{
		public TextBox ()
		{
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("validators", ScriptType.Array, true, "Validators"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("text", ScriptType.String, false, "Text"));
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

			if (AutoCompletionServiceMethod != ""
			    && AutoCompletionServiceUrl != "") {
				writer.WriteStartElement ("behaviors");
				writer.WriteStartElement ("autoComplete");
				writer.WriteAttributeString ("serviceURL", AutoCompletionServiceUrl);
				writer.WriteAttributeString ("serviceMethod", AutoCompletionServiceMethod);
				if (AutoCompletionMinimumPrefixLength != 0)
					writer.WriteAttributeString ("minimumPrefixLength", AutoCompletionMinimumPrefixLength.ToString());

				writer.WriteAttributeString ("completionList", ID + "__autocomplete"); // XXX ?

				writer.WriteEndElement (); // autoComplete
				writer.WriteEndElement (); // behaviors
			}
				
		}

		public int AutoCompletionInterval {
			get {
				object o = ViewState["AutoCompletionInterval"];
				if (o == null)
					return 1000;
				return (int)o;
			}
			set {
				ViewState["AutoCompletionInterval"] = value;
			}
		}

		[MonoTODO]
		public int AutoCompletionMinimumPrefixLength {
			get {
				object o = ViewState["AutoCompletionMinimumPrefixLength"];
				if (o == null)
					return 3;
				return (int)o;
			}
			set {
				ViewState["AutoCompletionMinimumPrefixLength"] = value;
			}
		}

		public string AutoCompletionServiceMethod {
			get {
				return (string)ViewState["AutoCompletionServiceMethod"];
			}
			set {
				ViewState["AutoCompletionServiceMethod"] = value;
			}
		}

		public string AutoCompletionServiceUrl {
			get {
				return (string)ViewState["AutoCompletionServiceUrl"];
			}
			set {
				ViewState["AutoCompletionServiceUrl"] = value;
			}
		}

		public int AutoCompletionSetCount {
			get {
				object o = ViewState["AutoCompletionSetCount"];
				if (o == null)
					return 10;
				return (int)o;
			}
			set {
				ViewState["AutoCompletionSetCount"] = value;
			}
		}

		public int Size {
			get {
				object o = ViewState["Size"];
				if (o == null)
					return 0;
				return (int)o;
			}
			set {
				ViewState["Size"] = value;
			}
		}

		public override string TagName {
			get {
				return "textBox";
			}
		}

		public string Text {
			get {
				object o = ViewState["Text"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["Text"] = value;
			}
		}
	}

}

#endif
