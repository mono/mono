//
// Microsoft.Web.ScriptControl
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

namespace Microsoft.Web
{
	public abstract class ScriptControl : ScriptComponent
	{
		protected ScriptControl ()
		{
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);

			if (CssClass != "")
				writer.WriteAttributeString ("cssClass", CssClass);

			if (ClientID != null && ClientID != "")
				writer.WriteAttributeString ("targetElement", ClientID);
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			string[] args = new string[1];
			args[0] = "className";

			typeDescriptor.AddMethod (new ScriptMethodDescriptor ("addCssClass", args));
			typeDescriptor.AddMethod (new ScriptMethodDescriptor ("focus"));
			typeDescriptor.AddMethod (new ScriptMethodDescriptor ("scrollIntoView"));
			typeDescriptor.AddMethod (new ScriptMethodDescriptor ("removeCssClass", args));
			typeDescriptor.AddMethod (new ScriptMethodDescriptor ("toggleCssClass", args));

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("associatedElement", ScriptType.Object, true, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("behaviors", ScriptType.Array, true, "Behaviors"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("cssClass", ScriptType.String, false, "CssClass"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("enabled", ScriptType.Boolean, false, "Enabled"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("style", ScriptType.Object, true, ""));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("visible", ScriptType.Boolean, false, "Visible"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("visibilityMode", ScriptType.Enum, false, "VisibilityMode"));
		}

		protected override void RenderScriptTagContents (ScriptTextWriter writer)
		{
			base.RenderScriptTagContents (writer);

			if (behaviors != null && behaviors.Count > 0) {
				writer.WriteStartElement ("behaviors");
				foreach (Behavior b in behaviors) {
					b.RenderScript (writer);
				}
				writer.WriteEndElement (); // behaviors
			}
		}

		BehaviorCollection behaviors;
		public BehaviorCollection Behaviors {
			get {
				if (behaviors == null)
					behaviors = new BehaviorCollection (this);

				return behaviors;
			}
		}

		public string CssClass {
			get {
				object o = ViewState["CssClass"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				ViewState["CssClass"] = value;
			}
		}

		public bool Enabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public VisibilityMode VisibilityMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public new bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
