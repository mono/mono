//
// System.Web.UI.ClientScriptManager.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Lluis Sanchez (lluis@novell.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Text;
using System.Collections.Specialized;
using System.Web.Util;
using System.Globalization;

namespace System.Web.UI
{
	#if NET_2_0
	public sealed partial
	#else
	internal
	#endif
	class ClientScriptManager
	{
		internal const string EventStateFieldName = "__EVENTVALIDATION";
		
		Hashtable registeredArrayDeclares;
		ScriptEntry clientScriptBlocks;
		ScriptEntry startupScriptBlocks;
		internal Hashtable hiddenFields;
		ScriptEntry submitStatements;
		Page page;
#if NET_2_0
		int [] eventValidationValues;
		int eventValidationPos = 0;
		Hashtable expandoAttributes;
		bool _hasRegisteredForEventValidationOnCallback;
		bool _pageInRender;
		bool _initCallBackRegistered;
#endif
		
		internal ClientScriptManager (Page page)
		{
			this.page = page;
		}

#if !NET_2_0
		public string GetPostBackClientEvent (Control control, string argument)
		{
			return GetPostBackEventReference (control, argument);
		}
#endif

		public string GetPostBackClientHyperlink (Control control, string argument)
		{
			return "javascript:" + GetPostBackEventReference (control, argument);
		}
	
#if NET_2_0
		public string GetPostBackClientHyperlink (Control control, string argument, bool registerForEventValidation)
		{
			if (registerForEventValidation)
				RegisterForEventValidation (control.UniqueID, argument);
			return "javascript:" + GetPostBackEventReference (control, argument);
		}
#endif		

#if !NET_2_0
		internal
#else
		public
#endif
		string GetPostBackEventReference (Control control, string argument)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			
			page.RequiresPostBackScript ();
			if(page.IsMultiForm)
				return page.theForm + ".__doPostBack('" + control.UniqueID + "','" + argument + "')";
			else
				return "__doPostBack('" + control.UniqueID + "','" + argument + "')";
		}

#if NET_2_0
		public string GetPostBackEventReference (Control control, string argument, bool registerForEventValidation)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			
			if (registerForEventValidation)
				RegisterForEventValidation (control.UniqueID, argument);
			return GetPostBackEventReference (control, argument);
		}
		
		public string GetPostBackEventReference (PostBackOptions options, bool registerForEventValidation)
		{
			if (options == null)
				throw new ArgumentNullException ("options");
			if (registerForEventValidation)
				RegisterForEventValidation (options);
			return GetPostBackEventReference (options);
		}
		
		public string GetPostBackEventReference (PostBackOptions options)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (options.ActionUrl == null && options.ValidationGroup == null && !options.TrackFocus && 
				!options.AutoPostBack && !options.PerformValidation)
			{
				if (!options.ClientSubmit)
					return null;

				if (options.RequiresJavaScriptProtocol)
					return GetPostBackClientHyperlink (options.TargetControl, options.Argument);
				else
					return GetPostBackEventReference (options.TargetControl, options.Argument);
			}

			RegisterWebFormClientScript ();

			string actionUrl = options.ActionUrl;
			if (actionUrl != null)
				RegisterHiddenField (Page.PreviousPageID, page.Request.FilePath);

			if(options.TrackFocus)
				RegisterHiddenField (Page.LastFocusID, String.Empty);

			string prefix = options.RequiresJavaScriptProtocol ? "javascript:" : "";
			if (page.IsMultiForm)
				prefix += page.theForm + ".";

			return prefix + "WebForm_DoPostback(" +
				ClientScriptManager.GetScriptLiteral (options.TargetControl.UniqueID) + "," +
				ClientScriptManager.GetScriptLiteral (options.Argument) + "," +
				ClientScriptManager.GetScriptLiteral (actionUrl) + "," +
				ClientScriptManager.GetScriptLiteral (options.AutoPostBack) + "," +
				ClientScriptManager.GetScriptLiteral (options.PerformValidation) + "," +
				ClientScriptManager.GetScriptLiteral (options.TrackFocus) + "," +
				ClientScriptManager.GetScriptLiteral (options.ClientSubmit) + "," +
				ClientScriptManager.GetScriptLiteral (options.ValidationGroup) + ")";
		}

		internal void RegisterWebFormClientScript ()
		{
			if (_webFormClientScriptRequired)
				return;

			page.RequiresPostBackScript ();
			_webFormClientScriptRequired = true;
		}

		bool _webFormClientScriptRendered;
		bool _webFormClientScriptRequired;

		internal void WriteWebFormClientScript (HtmlTextWriter writer) {
			if (!_webFormClientScriptRendered && _webFormClientScriptRequired) {
				writer.WriteLine ();
				WriteClientScriptInclude (writer, GetWebResourceUrl (typeof (Page), "webform.js"), typeof (Page), "webform.js");
				WriteBeginScriptBlock (writer);
				writer.WriteLine ("WebForm_Initialize({0});", page.IsMultiForm ? page.theForm : "window");
				WriteEndScriptBlock (writer);
				_webFormClientScriptRendered = true;
			}
		}
		
		public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context)
		{
			return GetCallbackEventReference (control, argument, clientCallback, context, null, false);
		}

		public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context, bool useAsync)
		{
			return GetCallbackEventReference (control, argument, clientCallback, context, null, useAsync);
		}

		public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			if(!(control is ICallbackEventHandler))
				throw new InvalidOperationException ("The control must implement the ICallbackEventHandler interface and provide a RaiseCallbackEvent method.");

			return GetCallbackEventReference ("'" + control.UniqueID + "'", argument, clientCallback, context, clientErrorCallback, useAsync);
		}

		public string GetCallbackEventReference (string target, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
		{
			RegisterWebFormClientScript ();

			if (!_initCallBackRegistered) {
				_initCallBackRegistered = true;
				RegisterStartupScript (typeof (Page), "WebForm_InitCallback", page.WebFormScriptReference + ".WebForm_InitCallback();", true);
			}
			return page.WebFormScriptReference + ".WebForm_DoCallback(" +
				target + "," +
				(argument ?? "null") + "," +
				clientCallback + "," +
				(context ?? "null") + "," +
				(clientErrorCallback ?? "null") + "," +
				(useAsync ? "true" : "false") + ")";
		}
#endif
		
#if NET_2_0
		public
#else
		internal
#endif
		string GetWebResourceUrl(Type type, string resourceName)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
		
			if (resourceName == null || resourceName.Length == 0)
				throw new ArgumentNullException ("type");
		
			return System.Web.Handlers.AssemblyResourceLoader.GetResourceUrl (type, resourceName); 
		}
		

		public bool IsClientScriptBlockRegistered (string key)
		{
			return IsScriptRegistered (clientScriptBlocks, GetType(), key);
		}
	
		public bool IsClientScriptBlockRegistered (Type type, string key)
		{
			return IsScriptRegistered (clientScriptBlocks, type, key);
		}
	
		public bool IsStartupScriptRegistered (string key)
		{
			return IsScriptRegistered (startupScriptBlocks, GetType(), key);
		}
	
		public bool IsStartupScriptRegistered (Type type, string key)
		{
			return IsScriptRegistered (startupScriptBlocks, type, key);
		}
		
		public bool IsOnSubmitStatementRegistered (string key)
		{
			return IsScriptRegistered (submitStatements, GetType(), key);
		}
	
		public bool IsOnSubmitStatementRegistered (Type type, string key)
		{
			return IsScriptRegistered (submitStatements, type, key);
		}
		
		public bool IsClientScriptIncludeRegistered (string key)
		{
			return IsClientScriptIncludeRegistered (GetType (), key);
		}
	
		public bool IsClientScriptIncludeRegistered (Type type, string key)
		{
			return IsScriptRegistered (clientScriptBlocks, type, "include-" + key);
		}
		
		bool IsScriptRegistered (ScriptEntry scriptList, Type type, string key)
		{
			while (scriptList != null) {
				if (scriptList.Type == type && scriptList.Key == key)
					return true;
				scriptList = scriptList.Next;
			}
			return false;
		}
		
		public void RegisterArrayDeclaration (string arrayName, string arrayValue)
		{
			if (registeredArrayDeclares == null)
				registeredArrayDeclares = new Hashtable();
	
			if (!registeredArrayDeclares.ContainsKey (arrayName))
				registeredArrayDeclares.Add (arrayName, new ArrayList());
	
			((ArrayList) registeredArrayDeclares[arrayName]).Add(arrayValue);
		}

		void RegisterScript (ref ScriptEntry scriptList, Type type, string key, string script, bool addScriptTags)
		{
			RegisterScript (ref scriptList, type, key, script, addScriptTags ? ScriptEntryFormat.AddScriptTag : ScriptEntryFormat.None);
		}

		void RegisterScript (ref ScriptEntry scriptList, Type type, string key, string script, ScriptEntryFormat format)
		{
			ScriptEntry last = null;
			ScriptEntry entry = scriptList;

			while (entry != null) {
				if (entry.Type == type && entry.Key == key)
					return;
				last = entry;
				entry = entry.Next;
			}

			entry = new ScriptEntry (type, key, script, format);
			
			if (last != null) last.Next = entry;
			else scriptList = entry;
		}
	
		internal void RegisterClientScriptBlock (string key, string script)
		{
			RegisterScript (ref clientScriptBlocks, GetType(), key, script, false);
		}
	
		public void RegisterClientScriptBlock (Type type, string key, string script)
		{
			RegisterClientScriptBlock (type, key, script, false);
		}
	
		public void RegisterClientScriptBlock (Type type, string key, string script, bool addScriptTags)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			RegisterScript (ref clientScriptBlocks, type, key, script, addScriptTags);
		}
	
		public void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
		{
			if (hiddenFields == null)
				hiddenFields = new Hashtable ();

			if (!hiddenFields.ContainsKey (hiddenFieldName))
				hiddenFields.Add (hiddenFieldName, hiddenFieldInitialValue);
		}
	
		internal void RegisterOnSubmitStatement (string key, string script)
		{
			RegisterScript (ref submitStatements, GetType (), key, script, false);
		}
	
		public void RegisterOnSubmitStatement (Type type, string key, string script)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			
			RegisterScript (ref submitStatements, type, key, script, false);
		}
	
		internal void RegisterStartupScript (string key, string script)
		{
			RegisterScript (ref startupScriptBlocks, GetType(), key, script, false);
		}
		
		public void RegisterStartupScript (Type type, string key, string script)
		{
			RegisterStartupScript (type, key, script, false);
		}
		
		public void RegisterStartupScript (Type type, string key, string script, bool addScriptTags)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			RegisterScript (ref startupScriptBlocks, type, key, script, addScriptTags);
		}

		public void RegisterClientScriptInclude (string key, string url)
		{
			RegisterClientScriptInclude (GetType (), key, url);
		}
		
		public void RegisterClientScriptInclude (Type type, string key, string url)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (url == null || url.Length == 0)
				throw new ArgumentException ("url");

			RegisterScript (ref clientScriptBlocks, type, "include-" + key, url, ScriptEntryFormat.Include);
		}

#if NET_2_0
		public void RegisterClientScriptResource (Type type, string resourceName)
		{
			RegisterScript (ref clientScriptBlocks, type, "resource-" + resourceName, GetWebResourceUrl (type, resourceName), ScriptEntryFormat.Include);
		}

		public void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue)
		{
			RegisterExpandoAttribute (controlId, attributeName, attributeValue, true);
		}

		public void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue, bool encode)
		{
			if (controlId == null)
				throw new ArgumentNullException ("controlId");

			if (attributeName == null)
				throw new ArgumentNullException ("attributeName");
			
			if (expandoAttributes == null)
				expandoAttributes = new Hashtable ();

			ListDictionary list = (ListDictionary)expandoAttributes [controlId];
			if (list == null) {
				list = new ListDictionary ();
				expandoAttributes [controlId] = list;
			}

			list.Add (attributeName, encode ? StrUtils.EscapeQuotesAndBackslashes (attributeValue) : attributeValue);
		}

		private void EnsureEventValidationArray ()
		{
			if (eventValidationValues == null || eventValidationValues.Length == 0)
				eventValidationValues = new int [64];

			int len = eventValidationValues.Length;

			if (eventValidationPos >= len) {
				int [] tmp = new int [len * 2];
				Array.Copy (eventValidationValues, tmp, len);
				eventValidationValues = tmp;
			}
		}

		internal void ResetEventValidationState ()
		{
			_pageInRender = true;
			eventValidationPos = 0;
		}

		// Implemented following the description in http://odetocode.com/Blogs/scott/archive/2006/03/20/3145.aspx
		private int CalculateEventHash (string uniqueId, string argument)
		{
			int uniqueIdHash = uniqueId.GetHashCode ();
			int argumentHash = String.IsNullOrEmpty (argument) ? 0 : argument.GetHashCode ();
			return (uniqueIdHash ^ argumentHash);
		}
		
		public void RegisterForEventValidation (PostBackOptions options)
		{
			// MS.NET does not check for options == null, so we won't too...
			RegisterForEventValidation (options.TargetControl.UniqueID, options.Argument);
		}
		
		public void RegisterForEventValidation (string uniqueId)
		{
			RegisterForEventValidation (uniqueId, null);
		}
		
		public void RegisterForEventValidation (string uniqueId, string argument)
		{
			if (!page.EnableEventValidation)
				return;
			if (uniqueId == null || uniqueId.Length == 0)
				return;
			if (page.IsCallback)
				_hasRegisteredForEventValidationOnCallback = true;
			else if (!_pageInRender)
				throw new InvalidOperationException ("RegisterForEventValidation may only be called from the Render method");

			EnsureEventValidationArray ();
			
			int hash = CalculateEventHash (uniqueId, argument);
			for (int i = 0; i < eventValidationPos; i++)
				if (eventValidationValues [i] == hash)
					return;
			eventValidationValues [eventValidationPos++] = hash;
		}

		public void ValidateEvent (string uniqueId)
		{
			ValidateEvent (uniqueId, null);
		}

		public void ValidateEvent (string uniqueId, string argument)
		{
			if (uniqueId == null || uniqueId.Length == 0)
				throw new ArgumentException ("must not be null or empty", "uniqueId");
			if (!page.EnableEventValidation)
				return;
			if (eventValidationValues == null)
				goto bad;
			
			int hash = CalculateEventHash (uniqueId, argument);
			for (int i = 0; i < eventValidationValues.Length; i++)
				if (eventValidationValues [i] == hash)
					return;
			
			bad:
			throw new ArgumentException ("Invalid postback or callback argument. Event validation is enabled using <pages enableEventValidation=\"true\"/> in configuration or <%@ Page EnableEventValidation=\"true\" %> in a page. For security purposes, this feature verifies that arguments to postback or callback events originate from the server control that originally rendered them. If the data is valid and expected, use the ClientScriptManager.RegisterForEventValidation method in order to register the postback or callback data for validation.");
		}
#endif
		void WriteScripts (HtmlTextWriter writer, ScriptEntry scriptList)
		{
			if (scriptList == null)
				return;

			writer.WriteLine ();

			while (scriptList != null) {
				switch (scriptList.Format) {
				case ScriptEntryFormat.AddScriptTag:
					EnsureBeginScriptBlock (writer);
					writer.Write (scriptList.Script);
					break;
				case ScriptEntryFormat.Include:
					EnsureEndScriptBlock (writer);
					WriteClientScriptInclude (writer, scriptList.Script, scriptList.Type, scriptList.Key);
					break;
				default:
					EnsureEndScriptBlock (writer);
					writer.WriteLine (scriptList.Script);
					break;
				}
				scriptList = scriptList.Next;
			}
			EnsureEndScriptBlock (writer);
		}

		bool _scriptTagOpened;

		void EnsureBeginScriptBlock (HtmlTextWriter writer) {
			if (!_scriptTagOpened) {
				WriteBeginScriptBlock (writer);
				_scriptTagOpened = true;
			}
		}

		void EnsureEndScriptBlock (HtmlTextWriter writer) {
			if (_scriptTagOpened) {
				WriteEndScriptBlock (writer);
				_scriptTagOpened = false;
			}
		}

#if NET_2_0
		internal void RestoreEventValidationState (string fieldValue)
		{
			if (!page.EnableEventValidation || fieldValue == null || fieldValue.Length == 0)
				return;
			IStateFormatter fmt = page.GetFormatter ();
			eventValidationValues = (int []) fmt.Deserialize (fieldValue);
			eventValidationPos = eventValidationValues.Length;
		}
		
		internal void SaveEventValidationState ()
		{
			if (!page.EnableEventValidation)
				return;

			string eventValidation = GetEventValidationStateFormatted ();
			if (eventValidation == null)
				return;

			RegisterHiddenField (EventStateFieldName, eventValidation);
		}

		internal string GetEventValidationStateFormatted ()
		{
			if (eventValidationValues == null || eventValidationValues.Length == 0)
				return null;

			if(page.IsCallback && !_hasRegisteredForEventValidationOnCallback)
				return null;

			IStateFormatter fmt = page.GetFormatter ();
			int [] array = new int [eventValidationPos];
			Array.Copy (eventValidationValues, array, eventValidationPos);
			return fmt.Serialize (array);
		}

		internal void WriteExpandoAttributes (HtmlTextWriter writer)
		{
			if (expandoAttributes == null)
				return;

			writer.WriteLine ();
			WriteBeginScriptBlock (writer);

			foreach (string controlId in expandoAttributes.Keys) {
				writer.WriteLine ("var {0} = document.all ? document.all [\"{0}\"] : document.getElementById (\"{0}\");", controlId);
				ListDictionary attrs = (ListDictionary) expandoAttributes [controlId];
				foreach (string attributeName in attrs.Keys) {
					writer.WriteLine ("{0}.{1} = \"{2}\";", controlId, attributeName, attrs [attributeName]);
				}
			}
			WriteEndScriptBlock (writer);
			writer.WriteLine ();
		}

#endif
		
#if NET_2_0
		internal const string SCRIPT_BLOCK_START = "//<![CDATA[";
		internal const string SCRIPT_BLOCK_END = "//]]>";
#else
		internal const string SCRIPT_BLOCK_START = "<!--";
		internal const string SCRIPT_BLOCK_END ="// -->";
#endif
		
		internal static void WriteBeginScriptBlock (HtmlTextWriter writer)
		{
			writer.WriteLine ("<script"+
#if !NET_2_0
				" language=\"javascript\""+
#endif
				" type=\"text/javascript\">");
			writer.WriteLine (SCRIPT_BLOCK_START);
		}

		internal static void WriteEndScriptBlock (HtmlTextWriter writer)
		{
			writer.WriteLine (SCRIPT_BLOCK_END);
			writer.WriteLine ("</script>");
		}
		
		internal void WriteHiddenFields (HtmlTextWriter writer)
		{
			if (hiddenFields == null)
				return;

#if NET_2_0
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
#endif
			foreach (string key in hiddenFields.Keys) {
				string value = hiddenFields [key] as string;
				writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"{1}\" />", key, HttpUtility.HtmlAttributeEncode (value));
			}
#if NET_2_0
			writer.RenderEndTag (); // DIV
#endif
			hiddenFields = null;
		}
		
		internal void WriteClientScriptInclude (HtmlTextWriter writer, string path, Type type, string key) {
					if (!page.IsMultiForm)
						writer.WriteLine ("<script src=\"{0}\" type=\"text/javascript\"></script>", path);
					else {
						string scriptKey = "inc_" + (type.FullName + key).GetHashCode ().ToString ("X");
						writer.WriteLine ("<script type=\"text/javascript\">");
						writer.WriteLine (SCRIPT_BLOCK_START);
						writer.WriteLine ("if (!window.{0}) {{", scriptKey);
						writer.WriteLine ("\twindow.{0} = true", scriptKey);
						writer.WriteLine ("\tdocument.write('<script src=\"{0}\" type=\"text/javascript\"><\\/script>'); }}", path);
						writer.WriteLine (SCRIPT_BLOCK_END);
						writer.WriteLine ("</script>");
					}
		}
		
		internal void WriteClientScriptBlocks (HtmlTextWriter writer)
		{
			WriteScripts (writer, clientScriptBlocks);
		}
	
		internal void WriteStartupScriptBlocks (HtmlTextWriter writer)
		{
			WriteScripts (writer, startupScriptBlocks);
		}
	
		internal void WriteArrayDeclares (HtmlTextWriter writer)
		{
			if (registeredArrayDeclares != null) {
				writer.WriteLine();
				WriteBeginScriptBlock (writer);
				IDictionaryEnumerator arrayEnum = registeredArrayDeclares.GetEnumerator();
				while (arrayEnum.MoveNext()) {
					if (page.IsMultiForm)
						writer.Write ("\t" + page.theForm + ".");
					else
						writer.Write ("\tvar ");
					writer.Write(arrayEnum.Key);
					writer.Write(" =  new Array(");
					IEnumerator arrayListEnum = ((ArrayList) arrayEnum.Value).GetEnumerator();
					bool isFirst = true;
					while (arrayListEnum.MoveNext()) {
						if (isFirst)
							isFirst = false;
						else
							writer.Write(", ");
						writer.Write(arrayListEnum.Current);
					}
					writer.WriteLine(");");
				}
				WriteEndScriptBlock (writer);
				writer.WriteLine ();
			}
		}

#if NET_2_0
		internal string GetClientValidationEvent (string validationGroup) {
			if (page.IsMultiForm)
				return "if (typeof(" + page.theForm + ".Page_ClientValidate) == 'function') " + page.theForm + ".Page_ClientValidate('" + validationGroup + "');";
			return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate('" + validationGroup + "');";
		}
#endif

		internal string GetClientValidationEvent ()
		{
			if (page.IsMultiForm)
				return "if (typeof(" + page.theForm + ".Page_ClientValidate) == 'function') " + page.theForm + ".Page_ClientValidate();";
			return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate();";
		}


		internal string WriteSubmitStatements ()
		{
			if (submitStatements == null) return null;
			
			StringBuilder sb = new StringBuilder ();
			ScriptEntry entry = submitStatements;
			while (entry != null) {
#if NET_2_0
				sb.Append (EnsureEndsWithSemicolon (entry.Script));
#else
				sb.Append (entry.Script);
#endif
				entry = entry.Next;
			}
#if NET_2_0
			RegisterClientScriptBlock (GetType(), "HtmlForm-OnSubmitStatemen",
@"
" + page.WebFormScriptReference + @".WebForm_OnSubmit = function () {
" + sb.ToString () + @"
return true;
}
", true);
			return "javascript:return " + page.WebFormScriptReference + ".WebForm_OnSubmit();";

#else
			return sb.ToString ();
#endif
		}
		
		internal static string GetScriptLiteral (object ob)
		{
			if (ob == null)
				return "null";
			else if (ob is string) {
				string s = (string)ob;
				bool escape = false;
				int len = s.Length;

				for (int i = 0; i < len; i++)
					if (s [i] == '\\' || s [i] == '\"') {
						escape = true;
						break;
					}

				if (!escape)
					return string.Concat ("\"", s, "\"");

				StringBuilder sb = new StringBuilder (len + 10);

				sb.Append ('\"');
				for (int si = 0; si < len; si++) {
					if (s [si] == '\"')
						sb.Append ("\\\"");
					else if (s [si] == '\\')
						sb.Append ("\\\\");
					else
						sb.Append (s [si]);
				}
				sb.Append ('\"');

				return sb.ToString ();
			} else if (ob is bool) {
				return ob.ToString ().ToLower (CultureInfo.InvariantCulture);
			} else {
				return ob.ToString ();
			}
		}

		sealed class ScriptEntry
		{
			public readonly Type Type;
			public readonly string Key;
			public readonly string Script;
			public readonly ScriptEntryFormat Format;
			public ScriptEntry Next;

			public ScriptEntry (Type type, string key, string script, ScriptEntryFormat format) {
				Key = key;
				Type = type;
				Script = script;
				Format = format;
			}
		}

		enum ScriptEntryFormat
		{
			None,
			AddScriptTag,
			Include,
		}

#if NET_2_0
		// helper method
		internal static string EnsureEndsWithSemicolon (string value) {
			if (value != null && value.Length > 0 && value [value.Length - 1] != ';')
				return value += ";";
			return value;
		}
#endif
	}
}
