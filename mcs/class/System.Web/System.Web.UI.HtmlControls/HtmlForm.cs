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
using System.Security.Permissions;
using System.Web.Util;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace System.Web.UI.HtmlControls 
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HtmlForm : HtmlContainerControl 
	{
		bool inited;

		public HtmlForm () : base ("form")
		{
		}

#if NET_2_0
		string defaultbutton = "";
		[DefaultValue ("")]
		public string DefaultButton
		{
			get {
				return defaultbutton;
			}
			set {
				defaultbutton = (value == null ? "" : value);
			}
		}

		string defaultfocus = "";
		[DefaultValue ("")]
		public string DefaultFocus
		{
			get {
				return defaultfocus;
			}
			set {
				defaultfocus = (value == null ? "" : value);
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

				if ((method == null) || (method.Length == 0)) {
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
		public virtual string Name 
		{
			get {
				return UniqueID;
			}
			set {
				/* why am i here? I do nothing. */
			}
		}

#if NET_2_0
		bool submitdisabledcontrols = false;
		[DefaultValue (false)]
		public virtual bool SubmitDisabledControls 
		{
			get {
				return submitdisabledcontrols;
			}
			set {
				submitdisabledcontrols = value;
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

		public override string UniqueID {
			get {
				return base.UniqueID;
			}
		}

#if NET_2_0		
		[MonoTODO ("why override?")]
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
			inited = true;
			Page.RegisterViewStateHandler ();

#if NET_2_0
			Page.RegisterForm (this);
#endif

			base.OnInit (e);
		}

#if NET_2_0
		internal bool DetermineRenderUplevel ()
		{
			/* this bit is c&p'ed from BaseValidator.DetermineRenderUplevel */
			try {
				if (Page != null && Page.Request != null)
					return (
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

			return false;
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}
#endif		

		protected override void RenderAttributes (HtmlTextWriter w)
		{
			/* Need to always render: method, action and id
			 */
			/* The name attribute is rendered _only_ if we're not in
			   2.0 mode or if the xhtml conformance mode is set to
			   Legacy for 2.0 according to http://msdn2.microsoft.com/en-us/library/system.web.ui.htmlcontrols.htmlform.name.aspx
			*/
			
			string action;
			string file_path = Page.Request.FilePath;
			string current_path = Page.Request.CurrentExecutionFilePath;
			if (file_path == current_path) {
				// Just the filename will do
				action = UrlUtils.GetFile (file_path);
			} else {
				// Fun. We need to make cookieless sessions work, so no
				// absolute paths here.
				Uri current_uri = new Uri ("http://host" + current_path);
				Uri fp_uri = new Uri ("http://host" + file_path);
				action = fp_uri.MakeRelative (current_uri);
			}

			action += Page.Request.QueryStringRaw;
#if TARGET_J2EE
			// Allow the page to transform action to a portlet action url
			if (Page.IsPortletRender)
				action = Page.RenderResponse.createActionURL(action);
#endif

#if NET_2_0
			XhtmlConformanceSection xhtml = WebConfigurationManager.GetSection ("system.web/xhtmlConformance") as
				XhtmlConformanceSection;
			
			if (xhtml != null && xhtml.Mode == XhtmlConformanceMode.Legacy)
#endif
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
			if (submit != null && submit != "") {
				Attributes.Remove ("onsubmit");
				w.WriteAttribute ("onsubmit", submit);
			}
			
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
			Attributes.Remove ("method");
			Attributes.Remove ("enctype");
			Attributes.Remove ("target");

			base.RenderAttributes (w);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderChildren (HtmlTextWriter w)
		{
			if (!inited) {
				Page.RegisterViewStateHandler ();
#if NET_2_0
				Page.RegisterForm (this);
#endif
			}
			Page.OnFormRender (w, ClientID);
			base.RenderChildren (w);
			Page.OnFormPostRender (w, ClientID);
		}

#if NET_2_0
		/* According to corcompare */
		[MonoTODO ("why override?")]
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

	
