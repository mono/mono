//
// System.Web.UI.PageParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	public sealed class PageParser : TemplateControlParser
	{
		bool enableSessionState = true;
		bool trace;
		TraceMode tracemode;
		bool readonlySessionState;
		string responseEncoding;
		string contentType;
		int codepage = -1;
		int lcid = -1;
		string culture;
		string uiculture;

		// FIXME: this is here just for DesignTimeTemplateParser. Anything to do?
		internal PageParser ()
		{
		}
		
		internal PageParser (string virtualPath, string inputFile, HttpContext context)
		{
			Context = context;
			BaseVirtualDir = UrlUtils.GetDirectory (virtualPath);
			InputFile = inputFile;
			AddApplicationAssembly ();
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
			string enabless = GetString (atts, "EnableSessionState", null);
			if (enabless != null) {
				readonlySessionState = (String.Compare (enabless, "readonly", true) == 0);
				if (readonlySessionState == true || String.Compare (enabless, "true", true) == 0) {
					enableSessionState = true;
				} else if (String.Compare (enabless, "false", true) == 0) {
					enableSessionState = false;
				} else {
					ThrowParseException ("Invalid value for EnableSessionState: " + enabless);
				}
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
					ci = new CultureInfo (culture);					
				} catch {
					ThrowParseException ("Unsupported Culture: " + culture);
				}

				if (ci.IsNeutralCulture) {
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
					ci = new CultureInfo (uiculture);					
				} catch {
					ThrowParseException ("Unsupported Culture: " + uiculture);
				}

				if (ci.IsNeutralCulture) {
					string suggestedCulture = SuggestCulture (uiculture);
					string fmt = "UICulture attribute must be set to a non-neutral Culture.";
					if (suggestedCulture != null)
						ThrowParseException (fmt +
								" Please try one of these: " + suggestedCulture);
					else
						ThrowParseException (fmt);
				}
			}

			trace = GetBool (atts, "Trace", false);

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
			
			// Ignored by now
			GetString (atts, "Buffer", null);
			GetString (atts, "ClientTarget", null);
			GetString (atts, "EnableViewStateMac", null);
			GetString (atts, "ErrorPage", null);
			GetString (atts, "Trace", null);
			GetString (atts, "TraceMode", null);
			GetString (atts, "SmartNavigation", null);
			GetBool (atts, "ValidateRequest", true);

			base.ProcessMainAttributes (atts);
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

		protected override Type CompileIntoType ()
		{
			AspGenerator generator = new AspGenerator (this);
			return generator.GetCompiledType ();
		}

		internal bool EnableSessionState {
			get { return enableSessionState; }
		}
		
		internal bool ReadOnlySessionState {
			get { return readonlySessionState; }
		}

		internal bool Trace {
			get { return trace; } 
		}

		internal TraceMode TraceMode {
			get { return tracemode; }
		}
		
		internal override Type DefaultBaseType {
			get { return typeof (Page); }
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
	}
}

