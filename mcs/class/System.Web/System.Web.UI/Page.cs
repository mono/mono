//
// System.Web.UI.Page.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.Security.Principal;
using System.Web;

namespace System.Web.UI {

	public class Page : TemplateControl, IHttpHandler
	{

		#region Constructor
		public Page ()
		{
		}

		#endregion

		#region Fields
// 		protected const string postEventArgumentID;
// 		protected const string postEventSourceID;

		#endregion

		#region Properties

		public HttpApplicationState Application {
			get { throw new NotImplementedException (); }
		}

		bool AspCompatMode {
			set { throw new NotImplementedException (); }
		}

		bool Buffer {
			set { throw new NotImplementedException (); }
		}

// 		public Cache Cache {
// 			get { throw new NotImplementedException (); }
// 		}

		public string ClientTarget {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		int CodePage {
			set { throw new NotImplementedException (); }
		}

		string ContentType {
			set { throw new NotImplementedException (); }
		}

		protected override HttpContext Context {
			get { throw new NotImplementedException (); }
		}

		string Culture {
			set { throw new NotImplementedException (); }
		}

		public override bool EnableViewState {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

// 		public override bool EnableViewStateMac {
// 			get { throw new NotImplementedException (); }
// 			set { throw new NotImplementedException (); }
// 		}

		public string ErrorPage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		ArrayList FileDependencies {
			set { throw new NotImplementedException (); }
		}

		public override string ID {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool IsPostBack {
			get { throw new NotImplementedException (); }
		}

		public bool IsReusable {
			get { throw new NotImplementedException (); }
		}

		public bool IsValid {
			get { throw new NotImplementedException (); }
		}

		int LCID {
			set { throw new NotImplementedException (); }
		}

		public HttpRequest Request {
			get { throw new NotImplementedException (); }
		}

		public HttpResponse Response {
			get { throw new NotImplementedException (); }
		}

		string ResponseEncoding {
			set { throw new NotImplementedException (); }
		}

		public HttpServerUtility Server {
			get { throw new NotImplementedException (); }
		}

		public virtual HttpSessionState Session {
			get { throw new NotImplementedException (); }
		}

		public bool SmartNavigation {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public TraceContext Trace {
			get { throw new NotImplementedException (); }
		}

		bool TraceEnabled {
			set { throw new NotImplementedException (); }
		}

		TraceMode TraceModeEncoding {
			set { throw new NotImplementedException (); }
		}

		int TransactionMode {
			set { throw new NotImplementedException (); }
		}

		string UICulture {
			set { throw new NotImplementedException (); }
		}

		public IPrincipal User {
			get { throw new NotImplementedException (); }
		}

		public ValidatorCollection Validators {
			get { throw new NotImplementedException (); }
		}

		public override bool Visible {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		public string GetPostBackClientEvent (Control control, string argument)
		{
			throw new NotImplementedException ();
		}

		public string GetPostBackClientHyperlink (Control control, string argument)
		{
			throw new NotImplementedException ();
		}

		public string GetPostBackEventReference (Control control)
		{
			throw new NotImplementedException ();
		}
		
		public string GetPostBackEventReference (Control control, string argument)
		{
			throw new NotImplementedException ();
		}

		public void ProcessRequest (HttpContext context)
		{
			throw new NotImplementedException ();
		}

		//
		// Wacky temporary API for making it to compile
		//
		public void RegisterClientScriptFile (string a, string b, string c)
		{
			throw new NotImplementedException ();
		}

		public void RegisterRequiresPostBack (Control control)
		{
		}

		public void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
		{
		}

		public void RegisterViewStateHandler ()
		{
		}

		public virtual void Validate ()
		{
		}

		public virtual void VerifyRenderingInServerForm (Control control)
		{
		}

		#endregion
	}
}
