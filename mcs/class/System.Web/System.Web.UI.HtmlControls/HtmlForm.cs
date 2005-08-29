//
// System.Web.UI.HtmlControls.HtmlForm.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
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

using System.ComponentModel;
using System.Collections.Specialized;
using System.Web.UI.WebControls;

namespace System.Web.UI.HtmlControls 
{
	public class HtmlForm : HtmlContainerControl 
	{
		public HtmlForm () : base ("form")
		{
		}

#if NET_2_0
		[DefaultValue ("")]
		public string DefaultButton
		{
			get {
				string defaultbutton = Attributes["defaultbutton"];

				if (defaultbutton == null) {
					return (String.Empty);
				}

				return (defaultbutton);
			}
			set {
				if (value == null) {
					Attributes.Remove ("defaultbutton");
				} else {
					Attributes["defaultbutton"] = value;
				}
			}
		}

		[DefaultValue ("")]
		public string DefaultFocus
		{
			get {
				string defaultfocus = Attributes["defaultfocus"];

				if (defaultfocus == null) {
					return (String.Empty);
				}

				return (defaultfocus);
			}
			set {
				if (value == null) {
					Attributes.Remove ("defaultfocus");
				} else {
					Attributes["defaultfocus"] = value;
				}
			}
		}
#endif		

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Enctype 
		{
			get {
				string enc = Attributes["enctype"];

				if (enc == null) {
					return (String.Empty);
				}

				return (enc);
			}
			set {
				if (value == null) {
					Attributes.Remove ("enctype");
				} else {
					Attributes["enctype"] = value;
				}
			}
		}
		
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Method 
		{
			get {
				string method = Attributes["method"];

				if (method == null) {
					return ("post");
				}
				
				return (method);
			}
			set {
				if (value == null) {
					Attributes.Remove ("method");
				} else {
					Attributes["method"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		public virtual
#else		
		public
#endif		
		string Name 
		{
			get {
				string name = Attributes["name"];

				if (name == null) {
					return (UniqueID);
				}
				
				return (name);
			}
			set {
				if (value == null) {
					Attributes.Remove ("name");
				} else {
					Attributes["name"] = value;
				}
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[MonoTODO]
		public virtual bool SubmitDisabledControls 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif
			
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Target 
		{
			get {
				string target = Attributes["target"];

				if (target == null) {
					return (String.Empty);
				}
				
				return (target);
			}
			set {
				if (value == null) {
					Attributes.Remove ("target");
				} else {
					Attributes["target"] = value;
				}
			}
		}

#if NET_2_0
		public override
#else		
		// New in NET1.1 sp1
		public new
#endif		
		string UniqueID
		{
			get {
				return base.UniqueID;
			}
		}

#if NET_2_0		
		[MonoTODO]
		protected override ControlCollection CreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnInit (EventArgs e)
		{
			Page.RegisterViewStateHandler ();

			base.OnInit (e);
		}

#if NET_2_0
		protected internal override void OnPreRender (EventArgs e)
		{
			string focus_id = null;
			bool render_uplevel = false;
			bool need_script_block = false;

			base.OnPreRender(e);

			/* this bit is c&p'ed from BaseValidator.DetermineRenderUplevel */
			try {
				if (Page != null && Page.Request != null)
					render_uplevel = (
						/* From someplace on the web: "JavaScript 1.2
						 * and later (also known as ECMAScript) has
						 * built-in support for regular
						 * expressions" */
						((Page.Request.Browser.EcmaScriptVersion.Major == 1
						  && Page.Request.Browser.EcmaScriptVersion.Minor >= 2)
						 || (Page.Request.Browser.EcmaScriptVersion.Major > 1))

						/* document.getElementById, .getAttribute,
						 * etc, are all DOM level 1.  I don't think we
						 * use anything in level 2.. */
						&& Page.Request.Browser.W3CDomVersion.Major >= 1);
			}
			catch {
				/* this can happen with a fake Page in nunit
				 * tests, since Page.Context == null */
				;
			}


			/* figure out if we have some control we're going to focus */
			if (DefaultFocus != null && DefaultFocus != "")
				focus_id = DefaultFocus;
			else if (DefaultButton != null && DefaultButton != "")
				focus_id = DefaultButton;

			/* presumably there are other conditions to
			 * this test, not just whether or not we have
			 * a default focus/button */
			need_script_block = (focus_id != null);

			if (render_uplevel) {
				Page.RequiresPostBackScript();

				if (need_script_block && !Page.ClientScript.IsClientScriptBlockRegistered ("Mono-System.Web-HtmlScriptBlock")) {
					Page.ClientScript.RegisterClientScriptBlock ("Mono-System.Web-HtmlScriptBlock",
										     String.Format ("<script language=\"JavaScript\" src=\"{0}\"></script>",
												    Page.ClientScript.GetWebResourceUrl (GetType(),
																	 "webform.js")));
				}


				if (focus_id != null) {
					Page.ClientScript.RegisterStartupScript ("HtmlForm-DefaultButton-StartupScript",
										 String.Format ("<script language=\"JavaScript\">\n" + 
												"<!--\n" + 
												"WebForm_AutoFocus('{0}');// -->\n" + 
												"</script>\n", focus_id));
				}
			}
		}
#endif		

		protected override void RenderAttributes (HtmlTextWriter w)
		{
			/* Need to always render: name, method, action
			 * and id
			 */

			string action = Page.Request.FilePath;
			string query = Page.Request.QueryStringRaw;
			if (query != null && query.Length > 0) {
				action += "?" + query;
			}
			
			w.WriteAttribute ("name", Name);
			w.WriteAttribute ("method", Method);
			w.WriteAttribute ("action", action);

			if (ID == null) {
				/* If ID != null then HtmlControl will
				 * write the id attribute
				 */
				w.WriteAttribute ("id", ClientID);
				Attributes.Remove ("id");
			}

			string submit = Page.GetSubmitStatements ();
			if (submit != null && submit != "")
				w.WriteAttribute ("onsubmit", submit);
			
			/* enctype and target should not be written if
			 * they are empty
			 */
			string enctype = Enctype;
			if (enctype != null && enctype != "") {
				w.WriteAttribute ("enctype", enctype);
			}

			string target = Target;
			if (target != null && target != "") {
				w.WriteAttribute ("target", target);
			}

#if NET_2_0
			string defaultbutton = DefaultButton;
			if (defaultbutton != null && defaultbutton != "") {
				Control c = FindControl (defaultbutton);

				if (c == null || !(c is IButtonControl))
					throw new InvalidOperationException(String.Format ("The DefaultButton of '{0}' must be the ID of a control of type IButtonControl.",
											   ID));
			}
#endif

			/* Now remove them from the hash so the base
			 * RenderAttributes can do all the rest
			 */
			Attributes.Remove ("name");
			Attributes.Remove ("method");
			Attributes.Remove ("enctype");
			Attributes.Remove ("target");
#if NET_2_0
			Attributes.Remove ("defaultfocus");
			Attributes.Remove ("defaultbutton");
#endif

			base.RenderAttributes (w);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderChildren (HtmlTextWriter w)
		{
			Page.OnFormRender (w, ClientID);
			base.RenderChildren (w);
			Page.OnFormPostRender (w, ClientID);
		}

#if NET_2_0
		/* According to corcompare */
		[MonoTODO]
		public override void RenderControl (HtmlTextWriter w)
		{
			base.RenderControl (w);
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter w)
		{
			base.Render (w);
		}
	}
}

	
