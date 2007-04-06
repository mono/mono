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
	public sealed
	#else
	internal
	#endif
		class ClientScriptManager
	{
		Hashtable registeredArrayDeclares;
		ScriptEntry clientScriptBlocks;
		ScriptEntry startupScriptBlocks;
		internal Hashtable hiddenFields;
		ScriptEntry submitStatements;
		ScriptEntry scriptIncludes;
		Page page;
#if NET_2_0
		List <int> eventValidationValues;
		Hashtable expandoAttributes;
		bool _hasRegisteredForEventValidationOnCallback;
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
#if NET_2_0
			return String.Format ("{0}.__doPostBack('{1}','{2}')", page.theForm, control.UniqueID, argument);
#else
			return String.Format ("__doPostBack('{0}','{1}')", control.UniqueID, argument);
#endif
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
#if TARGET_J2EE
			// Allow the page to transform ActionUrl to a portlet action url
			if (actionUrl != null && page.PortletNamespace != null) {
				actionUrl = page.CreateActionUrl(actionUrl);
				prefix += "Portal";
			}
#endif

			return String.Format ("{0}WebForm_DoPostback({1},{2},{3},{4},{5},{6},{7},{8},{9})", 
					prefix,
					ClientScriptManager.GetScriptLiteral (options.TargetControl.UniqueID), 
					ClientScriptManager.GetScriptLiteral (options.Argument),
					ClientScriptManager.GetScriptLiteral (actionUrl),
					ClientScriptManager.GetScriptLiteral (options.AutoPostBack),
					ClientScriptManager.GetScriptLiteral (options.PerformValidation),
					ClientScriptManager.GetScriptLiteral (options.TrackFocus),
					ClientScriptManager.GetScriptLiteral (options.ClientSubmit),
					ClientScriptManager.GetScriptLiteral (options.ValidationGroup),
					page.theForm
				);
		}

		internal void RegisterWebFormClientScript ()
		{
			if (IsClientScriptIncludeRegistered (typeof (Page), "webform"))
				return;

			RegisterClientScriptInclude (typeof (Page), "webform", GetWebResourceUrl (typeof (Page), "webform.js"));
			page.RequiresPostBackScript ();
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

			return GetCallbackEventReference (control.UniqueID, argument, clientCallback, context, clientErrorCallback, useAsync);
		}

		public string GetCallbackEventReference (string target, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
		{
			RegisterWebFormClientScript ();
			
			return string.Format ("WebForm_DoCallback('{0}',{1},{2},{3},{4},{5},{6})", target, argument, clientCallback, context, ((clientErrorCallback == null) ? "null" : clientErrorCallback), (useAsync ? "true" : "false"), page.theForm);
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
			return IsScriptRegistered (scriptIncludes, GetType(), key);
		}
	
		public bool IsClientScriptIncludeRegistered (Type type, string key)
		{
			return IsScriptRegistered (scriptIncludes, type, key);
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
			ScriptEntry last = null;
			ScriptEntry entry = scriptList;

			while (entry != null) {
				if (entry.Type == type && entry.Key == key)
					return;
				last = entry;
				entry = entry.Next;
			}
			
			if (addScriptTags) {
				script = "<script type=\"text/javascript\"" +
#if !NET_2_0
					"language=\"javascript\"" +
#endif
					">\n<!--\n" + script + "\n// -->\n</script>";
			}

			entry = new ScriptEntry (type, key, script);
			
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

			RegisterScript (ref scriptIncludes, type, key, url, false);
		}

#if NET_2_0
		public void RegisterClientScriptResource (Type type, string resourceName)
		{
			RegisterScript (ref scriptIncludes, type, "resource-" + resourceName, GetWebResourceUrl (type, resourceName), false);
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
			else if (page.LifeCycle < PageLifeCycle.Render)
				throw new InvalidOperationException ("RegisterForEventValidation may only be called from the Render method");
			if (eventValidationValues == null)
				eventValidationValues = new List <int> ();

			
			int hash = CalculateEventHash (uniqueId, argument);
			if (eventValidationValues.BinarySearch (hash) < 0)
				eventValidationValues.Add (hash);
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
			if (eventValidationValues.BinarySearch (hash) < 0)
				goto bad;
			return;
			
			bad:
			throw new ArgumentException ("Invalid postback or callback argument. Event validation is enabled using <pages enableEventValidation=\"true\"/> in configuration or <%@ Page EnableEventValidation=\"true\" %> in a page. For security purposes, this feature verifies that arguments to postback or callback events originate from the server control that originally rendered them. If the data is valid and expected, use the ClientScriptManager.RegisterForEventValidation method in order to register the postback or callback data for validation.");
		}
#endif
		void WriteScripts (HtmlTextWriter writer, ScriptEntry scriptList)
		{
			while (scriptList != null) {
				writer.WriteLine (scriptList.Script);
				scriptList = scriptList.Next;
			}
		}

#if NET_2_0
		internal void RestoreEventValidationState (string fieldValue)
		{
			if (!page.EnableEventValidation || fieldValue == null || fieldValue.Length == 0)
				return;
			IStateFormatter fmt = page.GetFormatter ();
			int [] eventValues = (int []) fmt.Deserialize (fieldValue);
#if TARGET_JVM // FIXME: No support yet for passing 'int[]' as 'T[]'
			eventValidationValues = new List<int> (eventValues.Length);
			for (int i = 0; i < eventValues.Length; i++)
				eventValidationValues.Add(eventValues[i]);
#else
			eventValidationValues = new List<int> (eventValues);
#endif
			eventValidationValues.Sort ();
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
			if (eventValidationValues == null || eventValidationValues.Count == 0)
				return null;

			if(page.IsCallback && !_hasRegisteredForEventValidationOnCallback)
				return null;

			IStateFormatter fmt = page.GetFormatter ();
			int [] array = new int [eventValidationValues.Count];
#if TARGET_JVM // FIXME: No support yet for passing 'int[]' as 'T[]'
			((ICollection)eventValidationValues).CopyTo (array, 0);
#else
			eventValidationValues.CopyTo (array);
#endif
			return fmt.Serialize (array);
		}

		internal string EventStateFieldName
		{
			get { return "__EVENTVALIDATION"; }
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
		internal void WriteBeginScriptBlock (HtmlTextWriter writer)
		{
			writer.WriteLine ("<script"+
#if !NET_2_0
				" language=\"javascript\""+
#endif
				" type=\"text/javascript\">");
			writer.WriteLine ("<!--");
		}

		internal void WriteEndScriptBlock (HtmlTextWriter writer)
		{
			writer.WriteLine ("// -->");
			writer.WriteLine ("</script>");
		}
		
		internal void WriteHiddenFields (HtmlTextWriter writer)
		{
			if (hiddenFields == null)
				return;
	
			foreach (string key in hiddenFields.Keys) {
				string value = hiddenFields [key] as string;
				writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"{1}\" />", key, value);
			}
	
			hiddenFields = null;
		}
		
		internal void WriteClientScriptIncludes (HtmlTextWriter writer)
		{
			ScriptEntry entry = scriptIncludes;
			while (entry != null) {
				if (!entry.Rendered) {
#if TARGET_J2EE
					if (!page.IsPortletRender)
#endif
						writer.WriteLine ("\n<script src=\"{0}\" type=\"text/javascript\"></script>", entry.Script);
#if TARGET_J2EE
					else {
						string scriptKey = "inc_" + entry.Key.GetHashCode ().ToString ("X");
						writer.WriteLine ("\n<script type=\"text/javascript\">");
						writer.WriteLine ("<!--");
						writer.WriteLine ("if (document.{0} == null) {{", scriptKey);
						writer.WriteLine ("\tdocument.{0} = true", scriptKey);
						writer.WriteLine ("\tdocument.write('<script src=\"{0}\" type=\"text/javascript\"><\\/script>'); }}", entry.Script);
						writer.WriteLine ("// -->");
						writer.WriteLine ("</script>");
					}
#endif
					entry.Rendered = true;
				}
				entry = entry.Next;
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
					writer.Write("\tvar ");
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
#if TARGET_J2EE
					// in addition, add a form array declaration
					if (page.IsPortletRender) {
						writer.Write ("\t" + page.theForm + ".");
						writer.Write (arrayEnum.Key);
						writer.Write (" = ");
						writer.Write (arrayEnum.Key);
						writer.WriteLine (";");
					}
#endif
				}
				WriteEndScriptBlock (writer);
				writer.WriteLine ();
			}
		}

#if NET_2_0
		internal string GetClientValidationEvent (string validationGroup) {
			string eventScript = "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate('" + validationGroup + "');";
#if TARGET_J2EE
			if (page.IsPortletRender)
				return "if (typeof(SetValidatorContext) == 'function') SetValidatorContext ('" + page.theForm + "'); " + eventScript;
#endif
			return eventScript;
		}
#endif

		internal string GetClientValidationEvent ()
		{
			string eventScript = "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate();";
#if TARGET_J2EE
			if (page.IsPortletRender)
				return "if (typeof(SetValidatorContext) == 'function') SetValidatorContext ('" + page.theForm + "'); " + eventScript;
#endif
			return eventScript;
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
			RegisterClientScriptBlock ("HtmlForm-OnSubmitStatemen",
@"<script type=""text/javascript"">
<!--
" + page.theForm + @".WebForm_OnSubmit = function () {
" + sb.ToString () + @"
return true;
}
// -->
</script>");
			return "javascript:return this.WebForm_OnSubmit();";

#else
			return sb.ToString ();
#endif
		}
		
		[MonoTODO ("optimize s.Replace")]
		internal static string GetScriptLiteral (object ob)
		{
			if (ob == null)
				return "null";
			else if (ob is string) {
				string s = (string)ob;
				s = s.Replace ("\\", "\\\\");
				s = s.Replace ("\"", "\\\"");
				return "\"" + s + "\"";
			} else if (ob is bool) {
				return ob.ToString ().ToLower (CultureInfo.InvariantCulture);
			} else {
				return ob.ToString ();
			}
		}
		
		class ScriptEntry
		{
			public Type Type;
			public string Key;
			public string Script;
			public ScriptEntry Next;
			public bool Rendered;
			 
			public ScriptEntry (Type type, string key, string script)
			{
				Key = key;
				Type = type;
				Script = script;
			}
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
