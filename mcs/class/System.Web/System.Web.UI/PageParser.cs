//	
// System.Web.UI.PageParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;
using System.IO;

namespace System.Web.UI
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class PageParser : TemplateControlParser
	{
#if NET_4_0
		static Type defaultPageBaseType;
		static Type defaultApplicationBaseType;
		static Type defaultPageParserFilterType;
		static Type defaultUserControlBaseType;
		static bool enableLongStringsAsResources = true;
#endif
		PagesEnableSessionState enableSessionState = PagesEnableSessionState.True;
		bool enableViewStateMac;
		bool enableViewStateMacSet;
		bool smartNavigation;
		bool haveTrace;
		bool trace;
		bool notBuffer;
		TraceMode tracemode = TraceMode.Default;
		string contentType;
		MainDirectiveAttribute <int> codepage;
		MainDirectiveAttribute <string> responseEncoding;
		MainDirectiveAttribute <int> lcid;
		MainDirectiveAttribute <string> clientTarget;
		MainDirectiveAttribute <string> masterPage;
		MainDirectiveAttribute <string> title;
		MainDirectiveAttribute <string> theme;
#if NET_4_0
		MainDirectiveAttribute <string> metaDescription;
		MainDirectiveAttribute <string> metaKeywords;
#endif
		string culture;
		string uiculture;
		string errorPage;
		bool validateRequest;
		bool async;
		int asyncTimeout = -1;
		Type masterType;
		string masterVirtualPath;
		string styleSheetTheme;
		bool enable_event_validation;
		bool maintainScrollPositionOnPostBack;
		int maxPageStateFieldLength = -1;
		Type previousPageType;
		string previousPageVirtualPath;
#if NET_4_0
		public static bool EnableLongStringsAsResources {
			get { return enableLongStringsAsResources; }
			set {
				BuildManager.AssertPreStartMethodsRunning ();
				enableLongStringsAsResources = value;
			}
		}
		
		public static Type DefaultPageBaseType {
			get { return defaultPageBaseType; }
			set {
				BuildManager.AssertPreStartMethodsRunning ();
				if (value != null && !typeof (Page).IsAssignableFrom (value))
					throw new ArgumentException (String.Format ("The value assigned to property '{0}' is invalid.", "DefaultPageBaseType"));
				
				defaultPageBaseType = value;
			}
		}

		public static Type DefaultApplicationBaseType {
			get { return defaultApplicationBaseType; }
			set {
				BuildManager.AssertPreStartMethodsRunning ();
				if (value != null && !typeof (HttpApplication).IsAssignableFrom (value))
					throw new ArgumentException (String.Format ("The value assigned to property '{0}' is invalid.", "DefaultApplicationBaseType"));
				defaultApplicationBaseType = value;
			}
		}

		public static Type DefaultPageParserFilterType {
			get { return defaultPageParserFilterType; }
			set {
				BuildManager.AssertPreStartMethodsRunning ();
				if (value != null && !typeof (PageParserFilter).IsAssignableFrom (value))
					throw new ArgumentException (String.Format ("The value assigned to property '{0}' is invalid.", "DefaultPageParserFilterType"));
				defaultPageParserFilterType = value;
			}
		}

		public static Type DefaultUserControlBaseType {
			get { return defaultUserControlBaseType; }
			set {
				if (value != null && !typeof (UserControl).IsAssignableFrom (value))
					throw new ArgumentException (String.Format ("The value assigned to property '{0}' is invalid.", "DefaultUserControlBaseType"));
				BuildManager.AssertPreStartMethodsRunning ();
				defaultUserControlBaseType = value;
			}
		}
#endif
		public PageParser ()
		{
			LoadConfigDefaults ();
		}
		
		internal PageParser (string virtualPath, string inputFile, HttpContext context)
		{
			this.VirtualPath = new VirtualPath (virtualPath);
			Context = context;
			BaseVirtualDir = VirtualPathUtility.GetDirectory (virtualPath, false);
			InputFile = inputFile;
			SetBaseType (null);
			AddApplicationAssembly ();
			LoadConfigDefaults ();
		}

		internal PageParser (VirtualPath virtualPath, TextReader reader, HttpContext context)
			: this (virtualPath, null, reader, context)
		{
		}
		
		internal PageParser (VirtualPath virtualPath, string inputFile, TextReader reader, HttpContext context)
		{
			this.VirtualPath = virtualPath;
			Context = context;
			BaseVirtualDir = virtualPath.DirectoryNoNormalize;
			Reader = reader;
			if (String.IsNullOrEmpty (inputFile))
				InputFile = virtualPath.PhysicalPath;
			else
				InputFile = inputFile;
			SetBaseType (null);
			AddApplicationAssembly ();
			LoadConfigDefaults ();
		}

		internal override void LoadConfigDefaults ()
		{
			base.LoadConfigDefaults ();
			PagesSection ps = PagesConfig;

			notBuffer = !ps.Buffer;
			enableSessionState = ps.EnableSessionState;
			enableViewStateMac = ps.EnableViewStateMac;
			smartNavigation = ps.SmartNavigation;
			validateRequest = ps.ValidateRequest;

			string value = ps.MasterPageFile;
			if (value.Length > 0)
				masterPage = new MainDirectiveAttribute <string> (value, true);
			
			enable_event_validation = ps.EnableEventValidation;
			maxPageStateFieldLength = ps.MaxPageStateFieldLength;
			value = ps.Theme;
			if (value.Length > 0)
				theme = new MainDirectiveAttribute <string> (value, true);
			
			styleSheetTheme = ps.StyleSheetTheme;
			if (styleSheetTheme.Length == 0)
				styleSheetTheme = null;
			maintainScrollPositionOnPostBack = ps.MaintainScrollPositionOnPostBack;
		}
		
		public static IHttpHandler GetCompiledPageInstance (string virtualPath, string inputFile, HttpContext context)
		{
			bool isFake = false;

			if (!String.IsNullOrEmpty (inputFile))
				isFake = !inputFile.StartsWith (HttpRuntime.AppDomainAppPath);
			
			return BuildManager.CreateInstanceFromVirtualPath (new VirtualPath (virtualPath, inputFile, isFake), typeof (IHttpHandler)) as IHttpHandler;
		}

		internal override void ProcessMainAttributes (IDictionary atts)
		{
			// note: the 'enableSessionState' configuration property is
			// processed in a case-sensitive manner while the page-level
			// attribute is processed case-insensitive
			string enabless = GetString (atts, "EnableSessionState", null);
			if (enabless != null) {
				if (String.Compare (enabless, "readonly", true, Helpers.InvariantCulture) == 0)
					enableSessionState = PagesEnableSessionState.ReadOnly;
				else if (String.Compare (enabless, "true", true, Helpers.InvariantCulture) == 0)
					enableSessionState = PagesEnableSessionState.True;
				else if (String.Compare (enabless, "false", true, Helpers.InvariantCulture) == 0)
					enableSessionState = PagesEnableSessionState.False;
				else
					ThrowParseException ("Invalid value for enableSessionState: " + enabless);
			}

			string value = GetString (atts, "CodePage", null);
			if (value != null) {
				if (responseEncoding != null)
					ThrowParseException ("CodePage and ResponseEncoding are mutually exclusive.");
				
				if (!BaseParser.IsExpression (value)) {
					int cpval = -1;

					try {
						cpval = (int) UInt32.Parse (value);
					} catch {
						ThrowParseException ("Invalid value for CodePage: " + value);
					}

					try {
						Encoding.GetEncoding (cpval);
					} catch {
						ThrowParseException ("Unsupported codepage: " + value);
					}
					codepage = new MainDirectiveAttribute <int> (cpval, true);
				} else
					codepage = new MainDirectiveAttribute <int> (value);
			}
			
			value = GetString (atts, "ResponseEncoding", null);
			if (value != null) {
				if (codepage != null)
					ThrowParseException ("CodePage and ResponseEncoding are mutually exclusive.");

				if (!BaseParser.IsExpression (value)) {
					try {
						Encoding.GetEncoding (value);
					} catch {
						ThrowParseException ("Unsupported encoding: " + value);
					}
					responseEncoding = new MainDirectiveAttribute <string> (value, true);
				} else
					responseEncoding = new MainDirectiveAttribute <string> (value);
			}
			
			contentType = GetString (atts, "ContentType", null);

			value = GetString (atts, "LCID", null);
			if (value != null) {
				if (!BaseParser.IsExpression (value)) {
					int parsedLcid = -1;
					try {
						parsedLcid = (int) UInt32.Parse (value);
					} catch {
						ThrowParseException ("Invalid value for LCID: " + value);
					}

					CultureInfo ci = null;
					try {
						ci = new CultureInfo (parsedLcid);
					} catch {
						ThrowParseException ("Unsupported LCID: " + value);
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
					lcid = new MainDirectiveAttribute <int> (parsedLcid, true);
				} else
					lcid = new MainDirectiveAttribute <int> (value);
			}

			culture = GetString (atts, "Culture", null);
			if (culture != null) {
				if (lcid != null) 
					ThrowParseException ("Culture and LCID are mutually exclusive.");
				
				CultureInfo ci = null;
				try {
					if (!culture.StartsWith ("auto"))
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
					if (!uiculture.StartsWith ("auto"))
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
			value = GetString (atts, "ClientTarget", null);
			if (value != null) {				
				if (!BaseParser.IsExpression (value)) {
					value = value.Trim ();
					
					ClientTargetSection sec = GetConfigSection <ClientTargetSection> ("system.web/clientTarget");
					ClientTarget ct = null;
				
					if ((ct = sec.ClientTargets [value]) == null)
						value = value.ToLowerInvariant ();
				
					if (ct == null && (ct = sec.ClientTargets [value]) == null) {
						ThrowParseException (String.Format (
									     "ClientTarget '{0}' is an invalid alias. See the " +
									     "documentation for <clientTarget> config. section.",
									     clientTarget));
					}
					value = ct.UserAgent;
					clientTarget = new MainDirectiveAttribute <string> (value, true);
				} else
					clientTarget = new MainDirectiveAttribute <string> (value);
			}

			notBuffer = !GetBool (atts, "Buffer", true);
			async = GetBool (atts, "Async", false);
			string asyncTimeoutVal = GetString (atts, "AsyncTimeout", null);
			if (asyncTimeoutVal != null) {
				try {
					asyncTimeout = Int32.Parse (asyncTimeoutVal);
				} catch (Exception) {
					ThrowParseException ("AsyncTimeout must be an integer value");
				}
			}
			
			value = GetString (atts, "MasterPageFile", masterPage != null ? masterPage.Value : null);
			if (!String.IsNullOrEmpty (value)) {
				if (!BaseParser.IsExpression (value)) {
					if (!HostingEnvironment.VirtualPathProvider.FileExists (value))
						ThrowParseFileNotFound (value);
					AddDependency (value);
					masterPage = new MainDirectiveAttribute <string> (value, true);
				} else
					masterPage = new MainDirectiveAttribute <string> (value);
			}
			
			value = GetString(atts, "Title", null);
			if (value != null) {
				if (!BaseParser.IsExpression (value))
					title = new MainDirectiveAttribute <string> (value, true);
				else
					title = new MainDirectiveAttribute <string> (value);
			}
			
			value = GetString (atts, "Theme", theme != null ? theme.Value : null);
			if (value != null) {
				if (!BaseParser.IsExpression (value))
					theme = new MainDirectiveAttribute <string> (value, true);
				else
					theme = new MainDirectiveAttribute <string> (value);
			}
			
			styleSheetTheme = GetString (atts, "StyleSheetTheme", styleSheetTheme);
			enable_event_validation = GetBool (atts, "EnableEventValidation", enable_event_validation);
			maintainScrollPositionOnPostBack = GetBool (atts, "MaintainScrollPositionOnPostBack", maintainScrollPositionOnPostBack);

			if (atts.Contains ("EnableViewState")) {
				enableViewStateMac = GetBool (atts, "EnableViewStateMac", enableViewStateMac);
				enableViewStateMacSet = true;
			}
#if NET_4_0
			value = GetString (atts, "MetaDescription", null);
			if (value != null) {
				if (!BaseParser.IsExpression (value))
					metaDescription = new MainDirectiveAttribute <string> (value, true);
				else
					metaDescription = new MainDirectiveAttribute <string> (value);
			}

			value = GetString (atts, "MetaKeywords", null);
			if (value != null) {
				if (!BaseParser.IsExpression (value))
					metaKeywords = new MainDirectiveAttribute <string> (value, true);
				else
					metaKeywords = new MainDirectiveAttribute <string> (value);
			}
#endif
			// Ignored by now
			GetString (atts, "SmartNavigation", null);

			base.ProcessMainAttributes (atts);
		}
		
		internal override void AddDirective (string directive, IDictionary atts)
		{
			bool isMasterType = String.Compare ("MasterType", directive, StringComparison.OrdinalIgnoreCase) == 0;
			bool isPreviousPageType = isMasterType ? false : String.Compare ("PreviousPageType", directive,
											 StringComparison.OrdinalIgnoreCase) == 0;

			string typeName = null;
			string virtualPath = null;
			Type type = null;
			
			if (isMasterType || isPreviousPageType) {
				PageParserFilter pfilter = PageParserFilter;
				if (pfilter != null)
					pfilter.PreprocessDirective (directive.ToLowerInvariant (), atts);
				
				typeName = GetString (atts, "TypeName", null);
				virtualPath = GetString (atts, "VirtualPath", null);

				if (typeName != null && virtualPath != null)
					ThrowParseException (
						String.Format ("The '{0}' directive must have exactly one attribute: TypeName or VirtualPath", directive));
				if (typeName != null) {
					type = LoadType (typeName);
					if (type == null)
						ThrowParseException (String.Format ("Could not load type '{0}'.", typeName));
					if (isMasterType)
						masterType = type;
					else
						previousPageType = type;
				} else if (!String.IsNullOrEmpty (virtualPath)) {
					if (!HostingEnvironment.VirtualPathProvider.FileExists (virtualPath))
						ThrowParseFileNotFound (virtualPath);

					AddDependency (virtualPath);
					if (isMasterType)
						masterVirtualPath = virtualPath;
					else
						previousPageVirtualPath = virtualPath;
				} else
					ThrowParseException (String.Format ("The {0} directive must have either a TypeName or a VirtualPath attribute.", directive));

				if (type != null)
					AddAssembly (type.Assembly, true);
			} else
				base.AddDirective (directive, atts);
		}
		
		static string SuggestCulture (string culture)
		{
			string retval = null;
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.SpecificCultures)) {
				if (ci.Name.StartsWith (culture))
					retval += ci.Name + " ";
			}
			return retval;
		}

		internal Type GetCompiledPageType (string virtualPath, string inputFile, HttpContext context)
		{
			return BuildManager.GetCompiledType (virtualPath);
		}
		
		internal override Type CompileIntoType ()
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

		internal bool EnableViewStateMacSet {
			get { return enableViewStateMacSet; }
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
#if NET_4_0
		internal override Type DefaultBaseType {
			get {
				Type ret = DefaultPageBaseType;
				if (ret == null)
					return base.DefaultBaseType;

				return ret;
			}
		}
#endif
		internal override string DefaultBaseTypeName {
			get { return PagesConfig.PageBaseType; }
		}
		
		internal override string DefaultDirectiveName {
			get { return "page"; }
		}

		internal string ContentType {
			get { return contentType; }
		}

		internal MainDirectiveAttribute <string> ResponseEncoding {
			get { return responseEncoding; }
		}
		
		internal MainDirectiveAttribute <int> CodePage {
			get { return codepage; }
		}

		internal MainDirectiveAttribute <int> LCID {
			get { return lcid; }
		}

		internal MainDirectiveAttribute <string> ClientTarget {
			get { return clientTarget; }
		}

		internal MainDirectiveAttribute <string> MasterPageFile {
			get { return masterPage; }
		}

		internal MainDirectiveAttribute <string> Title {
			get { return title; }
		}

		internal MainDirectiveAttribute <string> Theme {
			get { return theme; }
		}
#if NET_4_0
		internal MainDirectiveAttribute <string> MetaDescription {
			get { return metaDescription; }
		}

		internal MainDirectiveAttribute <string> MetaKeywords {
			get { return metaKeywords; }
		}
#endif
		internal string Culture {
			get { return culture; }
		}

		internal string UICulture {
			get { return uiculture; }
		}

		internal string ErrorPage {
			get { return errorPage; }
		}

		internal bool ValidateRequest {
			get { return validateRequest; }
		}

		internal bool NotBuffer {
			get { return notBuffer; }
		}

		internal bool Async {
			get { return async; }
		}

		internal int AsyncTimeout {
			get { return asyncTimeout; }
		}

		internal string StyleSheetTheme {
			get { return styleSheetTheme; }
		}
		
		internal Type MasterType {
			get {
				if (masterType == null && !String.IsNullOrEmpty (masterVirtualPath))
					masterType = BuildManager.GetCompiledType (masterVirtualPath);
				
				return masterType;
			}
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

		internal Type PreviousPageType {
			get {
				if (previousPageType == null && !String.IsNullOrEmpty (previousPageVirtualPath)) {
					string mappedPath = MapPath (previousPageVirtualPath);
					previousPageType = GetCompiledPageType (previousPageVirtualPath, mappedPath, HttpContext.Current);
				}
				
				return previousPageType;
			}
		}
	}
}

