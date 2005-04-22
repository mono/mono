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
using System.Text;

namespace System.Web.UI
{
	#if NET_2_0
	public
	#else
	internal
	#endif
		class ClientScriptManager
	{
		Hashtable registeredArrayDeclares;
		ScriptEntry clientScriptBlocks;
		ScriptEntry startupScriptBlocks;
		Hashtable hiddenFields;
		ScriptEntry submitStatements;
		ScriptEntry scriptIncludes;
		Page page;
	
		internal ClientScriptManager (Page page)
		{
			this.page = page;
		}
	
		public string GetPostBackClientEvent (Control control, string argument)
		{
			return GetPostBackEventReference (control, argument);
		}
	
		public string GetPostBackClientHyperlink (Control control, string argument)
		{
			return "javascript:" + GetPostBackEventReference (control, argument);
		}
	
		public string GetPostBackEventReference (Control control)
		{
			return GetPostBackEventReference (control, "");
		}
	
		public string GetPostBackEventReference (Control control, string argument)
		{
			page.RequiresPostBackScript ();
			return String.Format ("__doPostBack('{0}','{1}')", control.UniqueID, argument);
		}
		
#if NET_2_0
		public string GetPostBackEventReference (PostBackOptions options)
		{
			if (options.ActionUrl == null && options.ValidationGroup == null && !options.TrackFocus && 
				!options.AutoPostBack && !options.PerformValidation)
			{
				if (options.RequiresJavaScriptProtocol)
					return GetPostBackClientHyperlink (options.TargetControl, options.Argument);
				else
					return GetPostBackEventReference (options.TargetControl, options.Argument);
			}
			
			if (!IsClientScriptIncludeRegistered (typeof(Page), "webform")) {
				RegisterClientScriptInclude (typeof(Page), "webform", GetWebResourceUrl (typeof(Page), "webform.js"));
			}
			
			if (options.ActionUrl != null)
				RegisterHiddenField (Page.PreviousPageID, page.Request.FilePath);
			
			if (options.ClientSubmit || options.ActionUrl != null)
				page.RequiresPostBackScript ();
			
			return String.Format ("{0}WebForm_DoPostback({1},{2},{3},{4},{5},{6},{7},{8})", 
					options.RequiresJavaScriptProtocol ? "javascript:" : "",
					ClientScriptManager.GetScriptLiteral (options.TargetControl.UniqueID), 
					ClientScriptManager.GetScriptLiteral (options.Argument),
					ClientScriptManager.GetScriptLiteral (options.ActionUrl),
					ClientScriptManager.GetScriptLiteral (options.AutoPostBack),
					ClientScriptManager.GetScriptLiteral (options.PerformValidation),
					ClientScriptManager.GetScriptLiteral (options.TrackFocus),
					ClientScriptManager.GetScriptLiteral (options.ClientSubmit),
					ClientScriptManager.GetScriptLiteral (options.ValidationGroup)
				);
		}
		
		public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context)
		{
			return GetCallbackEventReference (control, argument, clientCallback, context, null);
		}
		
		public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context, string clientErrorCallback)
		{
			if (!IsClientScriptIncludeRegistered (typeof(Page), "callback"))
				RegisterClientScriptInclude (typeof(Page), "callback", GetWebResourceUrl (typeof(Page), "callback.js"));
			
			return string.Format ("WebForm_DoCallback ('{0}', {1}, {2}, {3}, {4})", control.UniqueID, argument, clientCallback, context, clientErrorCallback);
		}
		
		public string GetWebResourceUrl(Type type, string resourceName)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
		
			if (resourceName == null || resourceName.Length == 0)
				throw new ArgumentNullException ("type");
		
			return System.Web.Handlers.AssemblyResourceLoader.GetResourceUrl (type, resourceName); 
		}
		
#endif

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
			
			if (addScriptTags)
				script = "<script language=javascript>\n<!--\n" + script + "\n// -->\n</script>";

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
			RegisterScript (ref clientScriptBlocks, type, key, script, false);
		}
	
		public void RegisterClientScriptBlock (Type type, string key, string script, bool addScriptTags)
		{
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
			RegisterScript (ref submitStatements, type, key, script, false);
		}
	
		internal void RegisterStartupScript (string key, string script)
		{
			RegisterScript (ref startupScriptBlocks, GetType(), key, script, false);
		}
		
		public void RegisterStartupScript (Type type, string key, string script)
		{
			RegisterScript (ref startupScriptBlocks, type, key, script, false);
		}
		
		public void RegisterStartupScript (Type type, string key, string script, bool addScriptTags)
		{
			RegisterScript (ref startupScriptBlocks, type, key, script, addScriptTags);
		}

		public void RegisterClientScriptInclude (string key, string url)
		{
			RegisterScript (ref scriptIncludes, GetType(), key, url, false);
		}
		
		public void RegisterClientScriptInclude (Type type, string key, string url)
		{
			RegisterScript (ref scriptIncludes, type, key, url, false);
		}
		
		void WriteScripts (HtmlTextWriter writer, ScriptEntry scriptList)
		{
			while (scriptList != null) {
				writer.WriteLine (scriptList.Script);
				scriptList = scriptList.Next;
			}
		}
		
		internal void WriteHiddenFields (HtmlTextWriter writer)
		{
			if (hiddenFields == null)
				return;
	
			foreach (string key in hiddenFields.Keys) {
				string value = hiddenFields [key] as string;
				writer.WriteLine ("\n<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, value);
			}
	
			hiddenFields = null;
		}
		
		internal void WriteClientScriptIncludes (HtmlTextWriter writer)
		{
			ScriptEntry entry = scriptIncludes;
			while (entry != null) {
				writer.WriteLine ("\n<script src=\"{0}\" type=\"text/javascript\"></script>", entry.Script);
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
				writer.WriteLine("<script language=\"javascript\">");
				writer.WriteLine("<!--");
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
				}
				writer.WriteLine("// -->");
				writer.WriteLine("</script>");
				writer.WriteLine();
			}
		}
		
		internal string WriteSubmitStatements ()
		{
			if (submitStatements == null) return null;
			
			StringBuilder sb = new StringBuilder ();
			ScriptEntry entry = submitStatements;
			while (entry != null) {
				sb.Append (entry.Script);
				entry = entry.Next;
			}
			return sb.ToString ();
		}
		
		internal static string GetScriptLiteral (object ob)
		{
			if (ob == null)
				return "null";
			else if (ob is string) {
				string s = (string)ob;
				s = s.Replace ("\"", "\\\"");
				return "\"" + s + "\"";
			} else if (ob is bool) {
				return ob.ToString().ToLower();
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
			 
			public ScriptEntry (Type type, string key, string script)
			{
				Key = key;
				Type = type;
				Script = script;
			}
		}
	}
}
