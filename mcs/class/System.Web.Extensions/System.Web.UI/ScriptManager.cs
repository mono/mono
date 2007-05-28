//
// ScriptManager.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Specialized;

namespace System.Web.UI
{
	[ParseChildrenAttribute (true)]
	[DefaultPropertyAttribute ("Scripts")]
	[DesignerAttribute ("System.Web.UI.Design.ScriptManagerDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[NonVisualControlAttribute]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ScriptManager : Control, IPostBackDataHandler
	{
		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool AllowCustomErrorsRedirect {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue ("")]
		public string AsyncPostBackErrorMessage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public string AsyncPostBackSourceElementID {
			get {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (90)]
		[Category ("Behavior")]
		public int AsyncPostBackTimeout {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[MergableProperty (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public AuthenticationServiceManager AuthenticationService {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnablePageMethods {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool EnablePartialRendering {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (false)]
		[Category ("Behavior")]
		public bool EnableScriptGlobalization {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableScriptLocalization {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public bool IsDebuggingEnabled {
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public bool IsInAsyncPostBack {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool LoadScriptsBeforeUI {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ProfileServiceManager ProfileService {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		public ScriptMode ScriptMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[Category ("Behavior")]
		public string ScriptPath {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ScriptReferenceCollection Scripts {
			get {
				throw new NotImplementedException ();
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[MergableProperty (false)]
		[Category ("Behavior")]
		public ServiceReferenceCollection Services {
			get {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[Browsable (false)]
		public bool SupportsPartialRendering {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Visible {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Action")]
		public event EventHandler<AsyncPostBackErrorEventArgs> AsyncPostBackError;

		[Category ("Action")]
		public event EventHandler<ScriptReferenceEventArgs> ResolveScriptReference;

		public static ScriptManager GetCurrent (Page page)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void OnAsyncPostBackError (AsyncPostBackErrorEventArgs e)
		{
			if (AsyncPostBackError != null)
				AsyncPostBackError (this, e);
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnResolveScriptReference (ScriptReferenceEventArgs e)
		{
			if (ResolveScriptReference != null)
				ResolveScriptReference (this, e);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}

		public static void RegisterArrayDeclaration (Control control, string arrayName, string arrayValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterArrayDeclaration (Page page, string arrayName, string arrayValue)
		{
			throw new NotImplementedException ();
		}

		public void RegisterAsyncPostBackControl (Control control)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptBlock (Control control, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptBlock (Page page, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptInclude (Control control, Type type, string key, string url)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptInclude (Page page, Type type, string key, string url)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptResource (Control control, Type type, string resourceName)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptResource (Page page, Type type, string resourceName)
		{
			throw new NotImplementedException ();
		}

		public void RegisterDataItem (Control control, string dataItem)
		{
			throw new NotImplementedException ();
		}

		public void RegisterDataItem (Control control, string dataItem, bool isJsonSerialized)
		{
			throw new NotImplementedException ();
		}

		public void RegisterDispose (Control control, string disposeScript)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterExpandoAttribute (Control control, string controlId, string attributeName, string attributeValue, bool encode)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterHiddenField (Control control, string hiddenFieldName, string hiddenFieldInitialValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterHiddenField (Page page, string hiddenFieldName, string hiddenFieldInitialValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterOnSubmitStatement (Control control, Type type, string key, string script)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterOnSubmitStatement (Page page, Type type, string key, string script)
		{
			throw new NotImplementedException ();
		}

		public void RegisterPostBackControl (Control control)
		{
			throw new NotImplementedException ();
		}

		public void RegisterScriptDescriptors (IExtenderControl extenderControl)
		{
			throw new NotImplementedException ();
		}

		public void RegisterScriptDescriptors (IScriptControl scriptControl)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterStartupScript (Control control, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterStartupScript (Page page, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		protected override void Render (HtmlTextWriter writer)
		{
			throw new NotImplementedException ();
		}

		public void SetFocus (Control control)
		{
			throw new NotImplementedException ();
		}

		public void SetFocus (string clientID)
		{
			throw new NotImplementedException ();
		}

		#region IPostBackDataHandler Members

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		#endregion
	}
}
