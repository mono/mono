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
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	public sealed class PageParser : TemplateControlParser
	{
		bool enableSessionState = true;
		bool readonlySessionState;
		string responseEncoding;
		string contentType;
		int codepage = -1;

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

			// Ignored by now
			GetString (atts, "Buffer", null);
			GetString (atts, "ClientTarget", null);
			GetString (atts, "Culture", null);
			GetString (atts, "EnableViewStateMac", null);
			GetString (atts, "ErrorPage", null);
			GetString (atts, "LCID", null);
			GetString (atts, "Trace", null);
			GetString (atts, "TraceMode", null);
			GetString (atts, "UICulture", null);
			GetBool (atts, "ValidateRequest", true);

			base.ProcessMainAttributes (atts);
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
		
		internal override Type DefaultBaseType
		{
			get {
				return typeof (Page);
			}
		}

		internal override string DefaultDirectiveName
		{
			get {
				return "page";
			}
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
	}
}

