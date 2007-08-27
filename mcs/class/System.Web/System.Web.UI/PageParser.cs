//	
// System.Web.UI.PageParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Util;
using System.IO;

namespace System.Web.UI
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class PageParser : TemplateControlParser
	{
		PagesEnableSessionState enableSessionState = PagesEnableSessionState.True;
		bool enableViewStateMac = true;
		bool smartNavigation;
		bool haveTrace;
		bool trace;
		bool notBuffer;
		TraceMode tracemode;
		string responseEncoding;
		string contentType;
		int codepage = -1;
		int lcid = -1;
		string culture;
		string uiculture;
		string errorPage;
		bool validateRequest;
		string clientTarget;
		Type baseType = typeof (Page);

#if NET_2_0
		string masterPage;
		Type masterType;
		string title;
		string theme;
		string styleSheetTheme;
		bool enable_event_validation;
		bool maintainScrollPositionOnPostBack;
		int maxPageStateFieldLength = -1;
		string pageParserFilter = String.Empty;
		Type previousPageType;
#endif

		public PageParser ()
		{
			LoadPagesConfigDefaults ();
		}
		
		internal PageParser (string virtualPath, string inputFile, HttpContext context)
		{
			Context = context;
			BaseVirtualDir = UrlUtils.GetDirectory (virtualPath);
			InputFile = inputFile;
			SetBaseType (PagesConfig.PageBaseType);
			AddApplicationAssembly ();
			LoadPagesConfigDefaults ();
		}

#if NET_2_0
		internal PageParser (string virtualPath, TextReader reader, HttpContext context)
		{
			Context = context;
			BaseVirtualDir = UrlUtils.GetDirectory (virtualPath);
			Reader = reader;
			SetBaseType (PagesConfig.PageBaseType);
			AddApplicationAssembly ();
			LoadPagesConfigDefaults ();
		}
#endif

		internal override void LoadPagesConfigDefaults ()
		{
			base.LoadPagesConfigDefaults ();
#if NET_2_0
			PagesSection ps = PagesConfig;
#else
			PagesConfiguration ps = PagesConfig;
#endif

			notBuffer = !ps.Buffer;
			enableSessionState = ps.EnableSessionState;
			enableViewStateMac = ps.EnableViewStateMac;
			smartNavigation = ps.SmartNavigation;
			validateRequest = ps.ValidateRequest;
#if NET_2_0
			masterPage = ps.MasterPageFile;
			if (masterPage.Length == 0)
				masterPage = null;
			
			enable_event_validation = ps.EnableEventValidation;
			maxPageStateFieldLength = ps.MaxPageStateFieldLength;
			pageParserFilter = ps.PageParserFilterType;
			theme = ps.Theme;
			if (theme.Length == 0)
				theme = null;
			styleSheetTheme = ps.StyleSheetTheme;
			if (styleSheetTheme.Length == 0)
				styleSheetTheme = null;
			maintainScrollPositionOnPostBack = ps.MaintainScrollPositionOnPostBack;
#endif
		}
		
		public static IHttpHandler GetCompiledPageInstance (string virtualPath,
								    string inputFile, 
								    HttpContext context)
		{
			PageParser pp = new PageParser (virtualPath, inputFile, context);
			IHttpHandler h = (IHttpHandler) pp.GetCompiledInstance ();
			return h;
		}
		
		internal override void ProcessMainAttributes (Hashtable atts)
		{
			// note: the 'enableSessionState' configuration property is
			// processed in a case-sensitive manner while the page-level
			// attribute is processed case-insensitive
			string enabless = GetString (atts, "EnableSessionState", null);
			if (enabless != null) {
				if (String.Compare (enabless, "readonly", true) == 0)
					enableSessionState = PagesEnableSessionState.ReadOnly;
				else if (String.Compare (enabless, "true", true) == 0)
					enableSessionState = PagesEnableSessionState.True;
				else if (String.Compare (enabless, "false", true) == 0)
					enableSessionState = PagesEnableSessionState.False;
				else
					ThrowParseException ("Invalid value for enableSessionState: " + enabless);
			}

			string cp = GetString (atts, "CodePage", null);
			if (cp != null) {
				if (responseEncoding != null)
					ThrowParseException ("CodePage and ResponseEncoding are " +
						"mutually exclusive.");

				int codepage = 0;
				try {
					codepage = (int) UInt32.Parse (cp);
				} catch {
					ThrowParseException ("Invalid value for CodePage: " + cp);
				}

				try {
					Encoding.GetEncoding (codepage);
				} catch {
					ThrowParseException ("Unsupported codepage: " + cp);
				}
			}
			
			responseEncoding = GetString (atts, "ResponseEncoding", null);
			if (responseEncoding != null) {
				if (codepage != -1)
					ThrowParseException ("CodePage and ResponseEncoding are " +
							     "mutually exclusive.");

				try {
					Encoding.GetEncoding (responseEncoding);
				} catch {
					ThrowParseException ("Unsupported encoding: " + responseEncoding);
				}
			}
			
			contentType = GetString (atts, "ContentType", null);

			string lcidStr = GetString (atts, "LCID", null);
			if (lcidStr != null) {
				try {
					lcid = (int) UInt32.Parse (lcidStr);
				} catch {
					ThrowParseException ("Invalid value for LCID: " + lcid);
				}

				CultureInfo ci = null;
				try {
					ci = new CultureInfo (lcid);
				} catch {
					ThrowParseException ("Unsupported LCID: " + lcid);
				}

				if (ci.IsNeutralCulture) {
					string suggestedCulture = SuggestCulture (ci.Name);
					string fmt = "LCID attribute must be set to a non-neutral Culture.";
					if (suggestedCulture != null) {
						ThrowParseException (fmt + " Please try one of these: " +
								     suggestedCulture);
					} else {
						ThrowParseException (fmt);
					}
				}
			}

			culture = GetString (atts, "Culture", null);
			if (culture != null) {
				if (lcidStr != null) 
					ThrowParseException ("Culture and LCID are mutually exclusive.");
				
				CultureInfo ci = null;
				try {
#if NET_2_0
					if (!culture.StartsWith ("auto"))
#endif
						ci = new CultureInfo (culture);
				} catch {
					ThrowParseException ("Unsupported Culture: " + culture);
				}

				if (ci != null && ci.IsNeutralCulture) {
					string suggestedCulture = SuggestCulture (culture);
					string fmt = "Culture attribute must be set to a non-neutral Culture.";
					if (suggestedCulture != null)
						ThrowParseException (fmt +
								" Please try one of these: " + suggestedCulture);
					else
						ThrowParseException (fmt);
				}
			}

			uiculture = GetString (atts, "UICulture", null);
			if (uiculture != null) {
				CultureInfo ci = null;
				try {
#if NET_2_0
					if (!uiculture.StartsWith ("auto"))
#endif
						ci = new CultureInfo (uiculture);
				} catch {
					ThrowParseException ("Unsupported Culture: " + uiculture);
				}

				if (ci != null && ci.IsNeutralCulture) {
					string suggestedCulture = SuggestCulture (uiculture);
					string fmt = "UICulture attribute must be set to a non-neutral Culture.";
					if (suggestedCulture != null)
						ThrowParseException (fmt +
								" Please try one of these: " + suggestedCulture);
					else
						ThrowParseException (fmt);
				}
			}

			string tracestr = GetString (atts, "Trace", null);
			if (tracestr != null) {
				haveTrace = true;
				atts ["Trace"] = tracestr;
				trace = GetBool (atts, "Trace", false);
			}

			string tracemodes = GetString (atts, "TraceMode", null);
			if (tracemodes != null) {
				bool valid = true;
				try {
					tracemode = (TraceMode) Enum.Parse (typeof (TraceMode), tracemodes, false);
				} catch {
					valid = false;
				}

				if (!valid || tracemode == TraceMode.Default)
					ThrowParseException ("The 'tracemode' attribute is case sensitive and must be " +
							"one of the following values: SortByTime, SortByCategory.");
			}

			errorPage = GetString (atts, "ErrorPage", null);
			validateRequest = GetBool (atts, "ValidateRequest", validateRequest);
			clientTarget = GetString (atts, "ClientTarget", null);
			if (clientTarget != null) {
				clientTarget = clientTarget.Trim ();
#if NET_2_0
				ClientTargetSection sec = (ClientTargetSection)WebConfigurationManager.GetSection ("system.web/clientTarget");
				ClientTarget ct = null;
				
				if ((ct = sec.ClientTargets [clientTarget]) == null)
					clientTarget = clientTarget.ToLower (CultureInfo.InvariantCulture);
				
				if (ct == null && (ct = sec.ClientTargets [clientTarget]) == null) {
					ThrowParseException (String.Format (
							"ClientTarget '{0}' is an invalid alias. See the " +
							"documentation for <clientTarget> config. section.",
							clientTarget));
				}
				clientTarget = ct.UserAgent;
#else
				NameValueCollection coll;
				coll = (NameValueCollection) Context.GetConfig ("system.web/clientTarget");
				object ct = null;
				
				if (coll != null) {
					ct = coll [clientTarget];
					if (ct == null)
						ct = coll [clientTarget.ToLower ()];
				}
				
				if (ct == null) {
					ThrowParseException (String.Format (
							"ClientTarget '{0}' is an invalid alias. See the " +
							"documentation for <clientTarget> config. section.",
							clientTarget));
				}
				clientTarget = (string) ct;
#endif
			}

			notBuffer = !GetBool (atts, "Buffer", true);
			
#if NET_2_0
			masterPage = GetString (atts, "MasterPageFile", masterPage);
			
			// Make sure the page exists
			if (!String.IsNullOrEmpty (masterPage))
				MasterPageParser.GetCompiledMasterType (masterPage, MapPath (masterPage), HttpContext.Current);

			title = GetString(atts, "Title", null);

			theme = GetString (atts, "Theme", theme);
			styleSheetTheme = GetString (atts, "StyleSheetTheme", styleSheetTheme);
			enable_event_validation = GetBool (atts, "EnableEventValidation", true);
			maintainScrollPositionOnPostBack = GetBool (atts, "MaintainScrollPositionOnPostBack", maintainScrollPositionOnPostBack);
#endif
			// Ignored by now
			GetString (atts, "EnableViewStateMac", null);
			GetString (atts, "SmartNavigation", null);

			base.ProcessMainAttributes (atts);
		}
		
#if NET_2_0
		internal override void AddDirective (string directive, Hashtable atts)
		{			
			bool isMasterType = String.Compare ("MasterType", directive, StringComparison.OrdinalIgnoreCase) == 0;
			bool isPreviousPageType = isMasterType ? false : String.Compare ("PreviousPageType", directive,
											 StringComparison.OrdinalIgnoreCase) == 0;
			
			string typeName = null;
			string virtualPath = null;
			Type type = null;
			
			if (isMasterType || isPreviousPageType) {
				typeName = GetString (atts, "TypeName", null);
				virtualPath = GetString (atts, "VirtualPath", null);

				if (typeName != null && virtualPath != null)
					ThrowParseException (
						String.Format ("The '{0}' directive must have exactly one attribute: TypeName or VirtualPath", directive));
				if (typeName != null) {
					type = LoadType (typeName);
					if (type == null)
						ThrowParseException (String.Format ("Could not load type '{0}'.", typeName));
				} else if (virtualPath != null) {
					string mappedPath = MapPath (virtualPath);
					if (isMasterType)
						type = masterType = MasterPageParser.GetCompiledMasterType (virtualPath,
												     mappedPath,
												     HttpContext.Current);
					else
						type = previousPageType = GetCompiledPageType (virtualPath, mappedPath,
											       HttpContext.Current);
				} else
					ThrowParseException (
						String.Format ("The {0} directive must have either a TypeName or a VirtualPath attribute.", directive));

				AddAssembly (type.Assembly, true);
			} else
				base.AddDirective (directive, atts);
		}
#endif
		
		static string SuggestCulture (string culture)
		{
			string retval = null;
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.SpecificCultures)) {
				if (ci.Name.StartsWith (culture))
					retval += ci.Name + " ";
			}
			return retval;
		}

		public static Type GetCompiledPageType (string virtualPath, string inputFile, HttpContext context)
		{
			PageParser pp = new PageParser (virtualPath, inputFile, context);
			return pp.CompileIntoType ();
		}
		
		protected override Type CompileIntoType ()
		{
			AspGenerator generator = new AspGenerator (this);
			return generator.GetCompiledType ();
		}

		internal bool EnableSessionState {
			get {
				return enableSessionState == PagesEnableSessionState.True ||
					ReadOnlySessionState;
			}
		}

		internal bool EnableViewStateMac {
			get { return enableViewStateMac; }
		}
		
		internal bool SmartNavigation {
			get { return smartNavigation; }
		}
		
		internal bool ReadOnlySessionState {
			get {
				return enableSessionState == PagesEnableSessionState.ReadOnly;
			}
		}

		internal bool HaveTrace {
			get { return haveTrace; }
		}

		internal bool Trace {
			get { return trace; }
		}

		internal TraceMode TraceMode {
			get { return tracemode; }
		}
		
		internal override Type DefaultBaseType {
			get { return baseType; }
		}

		internal override string DefaultBaseTypeName {
			get { return "System.Web.UI.Page"; }
		}

		internal override string DefaultDirectiveName {
			get { return "page"; }
		}

		internal string ResponseEncoding {
			get { return responseEncoding; }
		}

		internal string ContentType {
			get { return contentType; }
		}

		internal int CodePage {
			get { return codepage; }
		}

		internal string Culture {
			get { return culture; }
		}

		internal string UICulture {
			get { return uiculture; }
		}

		internal int LCID {
			get { return lcid; }
		}

		internal string ErrorPage {
			get { return errorPage; }
		}

		internal bool ValidateRequest {
			get { return validateRequest; }
		}

		internal string ClientTarget {
			get { return clientTarget; }
		}

		internal bool NotBuffer {
			get { return notBuffer; }
		}

#if NET_2_0
		internal string Theme {
			get { return theme; }
		}

		internal string StyleSheetTheme {
			get { return styleSheetTheme; }
		}

		internal string MasterPageFile {
			get { return masterPage; }
		}
		
		internal Type MasterType {
			get { return masterType; }
		}

		internal string Title {
			get { return title; }
		}

		internal bool EnableEventValidation {
			get { return enable_event_validation; }
		}

		internal bool MaintainScrollPositionOnPostBack {
			get { return maintainScrollPositionOnPostBack; }
		}

		internal int MaxPageStateFieldLength {
			get { return maxPageStateFieldLength; }
		}

		internal string PageParserFilterType {
			get { return pageParserFilter; }
		}

		internal Type PreviousPageType {
			get { return previousPageType; }
		}
#endif
	}
}

