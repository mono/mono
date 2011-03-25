//
// System.Web.UI.HtmlControls.HtmlForm.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Web.SessionState;

namespace System.Web.UI.HtmlControls 
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HtmlForm : HtmlContainerControl 
	{
		bool inited;
		string _defaultfocus;
		string _defaultbutton;
		bool submitdisabledcontrols = false;
		bool? isUplevel;
		
		public HtmlForm () : base ("form")
		{
		}

		// LAMESPEC: This is undocumented on MSDN, but apparently it does exist on MS.NET.
		// See https://bugzilla.novell.com/show_bug.cgi?id=442104
		public string Action {
			get {
				string action = Attributes ["action"];
				if (String.IsNullOrEmpty (action))
					return String.Empty;

				return action;
			}

			set {
				if (String.IsNullOrEmpty (value))
					Attributes ["action"] = null;
				else
					Attributes ["action"] = value;
			}
		}		

		[DefaultValue ("")]
		public string DefaultButton {
			get {
				return _defaultbutton ?? String.Empty;
			}
			set {
				_defaultbutton = value;
			}
		}

		[DefaultValue ("")]
		public string DefaultFocus {
			get {
				return _defaultfocus ?? String.Empty;
			}
			set {
				_defaultfocus = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Enctype {
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
		public string Method {
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
		public virtual string Name {
			get {
				return UniqueID;
			}
			set {
				/* why am i here? I do nothing. */
			}
		}

		[DefaultValue (false)]
		public virtual bool SubmitDisabledControls {
			get {
				return submitdisabledcontrols;
			}
			set {
				submitdisabledcontrols = value;
			}
		}
			
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Target {
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
				Control container = NamingContainer;
				if (container == Page)
					return ID;
				return "aspnetForm";
			}
		}

		[MonoTODO ("why override?")]
		protected override ControlCollection CreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}

		protected internal override void OnInit (EventArgs e)
		{
			inited = true;
			Page page = Page;
			if (page != null) {
				page.RegisterViewStateHandler ();
				page.RegisterForm (this);
			}
			
			base.OnInit (e);
		}

		internal bool DetermineRenderUplevel ()
		{
#if TARGET_J2EE
			if (HttpContext.Current == null)
				return false;

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
#else
			if (isUplevel != null)
				return (bool) isUplevel;
			
			isUplevel = UplevelHelper.IsUplevel (
				System.Web.Configuration.HttpCapabilitiesBase.GetUserAgentForDetection (HttpContext.Current.Request));
			return (bool) isUplevel;
#endif
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}

		protected override void RenderAttributes (HtmlTextWriter w)
		{
			/* Need to always render: method, action and id
			 */
			
			string action;
			string customAction = Attributes ["action"];
			Page page = Page;
			HttpRequest req = page != null ? page.RequestInternal : null;
#if !TARGET_J2EE
			if (String.IsNullOrEmpty (customAction)) {
				string file_path = req != null ? req.ClientFilePath : null;
				string current_path = req != null ? req.CurrentExecutionFilePath : null;

				if (file_path == null)
					action = Action;
				else if (file_path == current_path) {
					// Just the filename will do
					action = UrlUtils.GetFile (file_path);
				} else {
					// Fun. We need to make cookieless sessions work, so no
					// absolute paths here.
					bool cookieless;
					SessionStateSection sec = WebConfigurationManager.GetSection ("system.web/sessionState") as SessionStateSection;
					cookieless = sec != null ? sec.Cookieless == HttpCookieMode.UseUri: false;
					string appVPath = HttpRuntime.AppDomainAppVirtualPath;
					int appVPathLen = appVPath.Length;
						
					if (appVPathLen > 1) {
						if (cookieless) {
							if (StrUtils.StartsWith (file_path, appVPath, true))
								file_path = file_path.Substring (appVPathLen);
						} else if (StrUtils.StartsWith (current_path, appVPath, true))
							current_path = current_path.Substring (appVPathLen);
					}
					
					if (cookieless) {
						Uri current_uri = new Uri ("http://host" + current_path);
						Uri fp_uri = new Uri ("http://host" + file_path);
						action = fp_uri.MakeRelative (current_uri);
					} else
						action = current_path;
				}
			} else
				action = customAction;
			if (req != null)
				action += req.QueryStringRaw;
#else
			// Allow the page to transform action to a portlet action url
			if (String.IsNullOrEmpty (customAction)) {
				string queryString = req.QueryStringRaw;
				action = CreateActionUrl (VirtualPathUtility.ToAppRelative (req.CurrentExecutionFilePath) +
					(string.IsNullOrEmpty (queryString) ? string.Empty : "?" + queryString));
			}
			else
				action = customAction;

#endif
			if (req != null) {
				XhtmlConformanceSection xhtml = WebConfigurationManager.GetSection ("system.web/xhtmlConformance") as XhtmlConformanceSection;
				if (xhtml == null || xhtml.Mode != XhtmlConformanceMode.Strict)
#if NET_4_0
					if (RenderingCompatibilityLessThan40)
#endif
						// LAMESPEC: MSDN says the 'name' attribute is rendered only in
						// Legacy mode, this is not true.
						w.WriteAttribute ("name", Name);
			}
			
			w.WriteAttribute ("method", Method);
			if (String.IsNullOrEmpty (customAction))
				w.WriteAttribute ("action", action, true);

			/*
			 * This is a hack that guarantees the ID is set properly for HtmlControl to
			 * render it later on. As ugly as it is, we use it here because of the way
			 * the ID, ClientID and UniqueID properties work internally in our Control
			 * code.
			 *
			 * Fixes bug #82596
			 */
			if (ID == null) {
#pragma warning disable 219
				string client = ClientID;
#pragma warning restore 219
			}
			
			string submit = page != null ? page.GetSubmitStatements () : null;
			if (!String.IsNullOrEmpty (submit)) {
				Attributes.Remove ("onsubmit");
				w.WriteAttribute ("onsubmit", submit);
			}
			
			/* enctype and target should not be written if
			 * they are empty
			 */
			string enctype = Enctype;
			if (!String.IsNullOrEmpty (enctype))
				w.WriteAttribute ("enctype", enctype);

			string target = Target;
			if (!String.IsNullOrEmpty (target))
				w.WriteAttribute ("target", target);

			string defaultbutton = DefaultButton;
			if (!String.IsNullOrEmpty (defaultbutton)) {
				Control c = FindControl (defaultbutton);

				if (c == null || !(c is IButtonControl))
					throw new InvalidOperationException(String.Format ("The DefaultButton of '{0}' must be the ID of a control of type IButtonControl.",
											   ID));

				if (page != null && DetermineRenderUplevel ()) {
					w.WriteAttribute (
						"onkeypress",
						"javascript:return " + page.WebFormScriptReference + ".WebForm_FireDefaultButton(event, '" + c.ClientID + "')");
				}
			}

			/* Now remove them from the hash so the base
			 * RenderAttributes can do all the rest
			 */
			Attributes.Remove ("method");
			Attributes.Remove ("enctype");
			Attributes.Remove ("target");

			base.RenderAttributes (w);
		}

		protected internal override void RenderChildren (HtmlTextWriter w)
		{
			Page page = Page;
			
			if (!inited && page != null) {
				page.RegisterViewStateHandler ();
				page.RegisterForm (this);
			}
			if (page != null)
				page.OnFormRender (w, ClientID);
			base.RenderChildren (w);
			if (page != null)
				page.OnFormPostRender (w, ClientID);
		}

		/* According to corcompare */
		[MonoTODO ("why override?")]
		public override void RenderControl (HtmlTextWriter w)
		{
			base.RenderControl (w);
		}

		protected internal override void Render (HtmlTextWriter w)
		{
			base.Render (w);
		}
	}
}
