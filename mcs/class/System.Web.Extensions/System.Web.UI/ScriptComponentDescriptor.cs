//
// ScriptComponentDescriptor.cs
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
using System.Web.Script.Serialization;

namespace System.Web.UI
{
	public class ScriptComponentDescriptor : ScriptDescriptor
	{
		string _elementID;
		string _type;
		string _id;
		IDictionary<string, string> _properties;
		IDictionary<string, string> _events;
		IDictionary<string, string> _references;

		public ScriptComponentDescriptor (string type) {
			if (String.IsNullOrEmpty (type))
				throw new ArgumentException ("Value cannot be null or empty.", "type");
			_type = type;
		}

		public virtual string ClientID {
			get {
				return ID;
			}
		}

		internal string ElementIDInternal {
			get {
				return _elementID;
			}
			set {
				_elementID = value;
			}
		}

		public virtual string ID {
			get {
				if (_id == null)
					return String.Empty;
				return _id;
			}
			set {
				_id = value;
			}
		}

		public string Type {
			get {
				return _type;
			}
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("Value cannot be null or empty.", "value");
				_type = value;
			}
		}

		public void AddComponentProperty (string name, string componentID) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (componentID == null)
				throw new ArgumentException ("Value cannot be null or empty.", "componentID");

			AddEntry (ref _references, String.Format ("\"{0}\"", name), String.Format ("\"{0}\"", componentID));
		}

		public void AddElementProperty (string name, string elementID) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (elementID == null)
				throw new ArgumentException ("Value cannot be null or empty.", "elementID");

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), String.Format ("$get(\"{0}\")", elementID));
		}

		public void AddEvent (string name, string handler) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (handler == null)
				throw new ArgumentException ("Value cannot be null or empty.", "handler");

			AddEntry (ref _events, String.Format ("\"{0}\"", name), handler);
		}

		public void AddProperty (string name, object value) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");

			string valueString;
			if (value == null)
				valueString = "null";
			else
				valueString = JavaScriptSerializer.DefaultSerializer.Serialize (value);

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), valueString);
		}

		public void AddScriptProperty (string name, string script) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (script == null)
				throw new ArgumentException ("Value cannot be null or empty.", "script");

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), script);
		}
		
		void AddEntry (ref IDictionary<string, string> dictionary, string key, string value) {
			if (dictionary == null)
				dictionary = new SortedDictionary<string, string> ();
			if (!dictionary.ContainsKey (key))
				dictionary.Add (key, value);
			else
				dictionary [key] = value;
		}

		protected internal override string GetScript ()
		{
			string id = ID;
			if (id != String.Empty)
				AddProperty ("id", id);
			
			bool haveFormID = String.IsNullOrEmpty (FormID) == false;
			bool haveElementID = String.IsNullOrEmpty (ElementIDInternal) == false;
			var sb = new StringBuilder ("$create(");

			if (haveFormID)
				sb.Append ("$get(\"");
			sb.Append (Type);
			if (haveFormID)
				sb.Append ("\")");

			WriteSerializedProperties (sb);
			WriteSerializedEvents (sb);
			WriteSerializedReferences (sb);

			if (haveElementID)
				sb.AppendFormat (", $get(\"{0}\")", ElementIDInternal);

			sb.Append (");");

			return sb.ToString ();
		}

		internal static string SerializeDictionary (IDictionary<string, string> dictionary)
		{
			if (dictionary == null || dictionary.Count == 0)
				return "null";
			StringBuilder sb = new StringBuilder ("{");
			foreach (string key in dictionary.Keys)
				sb.AppendFormat ("{0}:{1},", key, dictionary [key]);
			sb.Length--;
			sb.Append ("}");
			return sb.ToString ();
		}

		void WriteSerializedProperties (StringBuilder sb)
		{
			sb.Append (", ");
			sb.Append (SerializeDictionary (_properties));
		}

		void WriteSerializedEvents (StringBuilder sb)
		{
			sb.Append (", ");
			sb.Append (SerializeDictionary (_events));
		}

		void WriteSerializedReferences (StringBuilder sb)
		{
			sb.Append (", ");
			sb.Append (SerializeDictionary (_references));
		}
	}
}