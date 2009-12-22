//
// System.Web.UI.HtmlControls.HtmlForm.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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

		public HtmlForm () : base ("form")
		{
		}

#if NET_2_0
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
		
		string _defaultbutton;
		[DefaultValue ("")]
		public string DefaultButton
		{
			get {
				return _defaultbutton ?? String.Empty;
			}
			set {
				_defaultbutton = value;
			}
		}

		string _defaultfocus;
		[DefaultValue ("")]
		public string DefaultFocus
		{
			get {
				return _defaultfocus ?? String.Empty;
			}
			set {
				_defaultfocus = value;
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
		bool? isUplevel;
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
#if NET_2_0
			string customAction = Attributes ["action"];
#endif
			Page p = Page;
			HttpRequest req = p != null ? p.Request : null;
			if (req == null)
				throw new HttpException ("No current request, cannot continue rendering.");
#if !TARGET_J2EE
#if NET_2_0
			if (String.IsNullOrEmpty (customAction)) {
#endif
				string file_path = req.ClientFilePath;
				string current_path = req.CurrentExecutionFilePath;
				if (file_path == current_path) {
					// Just the filename will do
					action = UrlUtils.GetFile (file_path);
				} else {
					// Fun. We need to make cookieless sessions work, so no
					// absolute paths here.
					bool cookieless;

#if NET_2_0
					SessionStateSection sec = WebConfigurationManager.GetSection ("system.web/sessionState") as SessionStateSection;
					cookieless = sec != null ? sec.Cookieless == HttpCookieMode.UseUri: false;
#else
					SessionConfig sec = HttpContext.GetAppConfig ("system.web/sessionState") as SessionConfig;
					cookieless = sec != null ? sec.CookieLess : false;
#endif
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
#if NET_2_0
			} else
				action = customAction;
#endif
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

#if NET_2_0
			XhtmlConformanceSection xhtml = WebConfigurationManager.GetSection ("system.web/xhtmlConformance") as
				XhtmlConformanceSection;
			
			if (xhtml != null && xhtml.Mode == XhtmlConformanceMode.Legacy)
#endif
				w.WriteAttribute ("name", Name);

			w.WriteAttribute ("method", Method);
#if NET_2_0
			if (String.IsNullOrEmpty (customAction))
#endif
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
			if (!String.IsNullOrEmpty (defaultbutton)) {
				Control c = FindControl (defaultbutton);

				if (c == null || !(c is IButtonControl))
					throw new InvalidOperationException(String.Format ("The DefaultButton of '{0}' must be the ID of a control of type IButtonControl.",
											   ID));

				if (DetermineRenderUplevel ()) {
					w.WriteAttribute (
						"onkeypress",
						"javascript:return " + Page.WebFormScriptReference + ".WebForm_FireDefaultButton(event, '" + c.ClientID + "')");
				}
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
			Page page = Page;
			
			if (!inited && page != null) {
				page.RegisterViewStateHandler ();
#if NET_2_0
				page.RegisterForm (this);
#endif
			}
			if (page != null)
				page.OnFormRender (w, ClientID);
			base.RenderChildren (w);
			if (page != null)
				page.OnFormPostRender (w, ClientID);
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

	
