//
// System.Security.SecurityElement.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Ximian, Inc. http://www.ximian.com

using System.Globalization;
using System.Collections;
using System.Text;

namespace System.Security 
{
	[Serializable]
	public sealed class SecurityElement 
	{
		string text;
		string tag;
		Hashtable attributes;
		ArrayList children;
		
		// these values are determined by a simple test program against the MS.Net implementation:
		//	for (int i = 0; i < 256; i++) {
		//		if (!SecurityElement.IsValidTag ("" + ((char) i))) {
		//			System.Console.WriteLine ("TAG: " + i);
		//		}
		//	}		
		// note: this is actually an incorrect implementation of MS, as for example the &
		// character is not a valid character in tag names.
		private static char [] invalid_tag_chars = new char [] { ' ', '<', '>' };
		private static char [] invalid_text_chars = new char [] { '<', '>' };
		private static char [] invalid_attr_name_chars = new char [] { ' ', '<', '>' };
		private static char [] invalid_attr_value_chars = new char [] { '"', '<', '>' };
		private static char [] invalid_chars = new char [] { '<', '>', '"', '\'', '&' };
		
		public SecurityElement (string tag) : this (tag, null)
		{
		}
		
		public SecurityElement (string tag, string text)
		{
			this.Tag = tag;
			this.Text = text;
		}
		
		public Hashtable Attributes {
			get {
				if (attributes == null) 
					return null;
					
				Hashtable result = new Hashtable ();
				IDictionaryEnumerator e = attributes.GetEnumerator ();
				while (e.MoveNext ())
					result.Add (e.Key, e.Value);
				return result;
			}

			set {				
				if (value == null || value.Count == 0) {
					attributes = null;
					return;
				}
				
				Hashtable result = new Hashtable ();
				IDictionaryEnumerator e = value.GetEnumerator ();
				while (e.MoveNext ()) {
					string key = (string) e.Key;
					string val = (string) e.Value;
					if (!IsValidAttributeName (key))
						throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + key);

					if (!IsValidAttributeValue (val))
						throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + key);

					result.Add (key, val);
				}
				attributes = result;
			}
		}

		public ArrayList Children {
			get {
				return children;
			}

			set {
				if (value != null) {
					foreach (object o in value) {
						if (o == null)
							throw new ArgumentNullException ();
						// shouldn't we also throw an exception 
						// when o isn't an instance of SecurityElement?
					}
				}
				children = value;
			}
		}

		public string Tag {
			get {
				return tag;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ();
				if (!IsValidTag (value))
					throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + value);
				tag = value;
			}
		}

		public string Text {
			get {
				return text;
			}

			set {
				if (!IsValidText (value))
					throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + text);				
				text = value;
			}
		}

		public void AddAttribute (string name, string value)
		{
			if (name == null || value == null)
				throw new ArgumentNullException ();

			if (attributes == null)
				attributes = new Hashtable ();

			//
			// The hashtable will throw ArgumentException if name is already there
			//

			if (!IsValidAttributeName (name))
				throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + name);

			if (!IsValidAttributeValue (value))
				throw new ArgumentException (Locale.GetText ("Invalid XML string") + ": " + value);
			
			attributes.Add (name, value);
		}

		public void AddChild (SecurityElement child)
		{
			if (child == null)
				throw new ArgumentNullException ();

			if (children == null)
				children = new ArrayList ();

			children.Add (child);
		}

		public string Attribute (string name)
		{
			if (name == null)
				throw new ArgumentNullException ();

			if (attributes != null)
				return (string) attributes [name];
			else
				return null;
		}

		public bool Equal (SecurityElement other)
		{
			if (other == null)
				return false;
				
			if (this == other)
				return true;

			if (this.text != other.text)
				return false;

			if (this.tag != other.tag)
				return false;

			if (this.attributes == null && other.attributes != null && other.attributes.Count != 0)
				return false;
				
			if (other.attributes == null && this.attributes != null && this.attributes.Count != 0)
				return false;

			if (this.attributes != null && other.attributes != null) {
				if (this.attributes.Count != other.attributes.Count) 
					return false;
				IDictionaryEnumerator e = attributes.GetEnumerator ();
				while (e.MoveNext ()) 
					if (other.attributes [e.Key] != e.Value)
						return false;
			}
			
			if (this.children == null && other.children != null && other.children.Count != 0)
				return false;
					
			if (other.children == null && this.children != null && this.children.Count != 0)
				return false;
				
			if (this.children != null && other.children != null) {
				if (this.children.Count != other.children.Count)
					return false;
				for (int i = 0; i < this.children.Count; i++) 
					if (!((SecurityElement) this.children [i]).Equal ((SecurityElement) other.children [i]))
						return false;
			}
			
			return true;
		}

		public static string Escape (string str)
		{
			StringBuilder sb;
			
			if (str.IndexOfAny (invalid_chars) == -1)
				return str;

			sb = new StringBuilder ();
			int len = str.Length;
			
			for (int i = 0; i < len; i++) {
				char c = str [i];

				switch (c) {
				case '<':  sb.Append ("&lt;"); break;
				case '>':  sb.Append ("&gt;"); break;
				case '"':  sb.Append ("&quot;"); break;
				case '\'': sb.Append ("&apos;"); break;
				case '&':  sb.Append ("&amp;"); break;
				default:   sb.Append (c); break;
				}
			}

			return sb.ToString ();
		}

		public static bool IsValidAttributeName (string name)
		{
			return name != null && name.IndexOfAny (invalid_attr_name_chars) == -1;
		}

		public static bool IsValidAttributeValue (string value)
		{
			return value != null && value.IndexOfAny (invalid_attr_value_chars) == -1;
		}

		public static bool IsValidTag (string value)
		{
			return value != null && value.IndexOfAny (invalid_tag_chars) == -1;
		}

		public static bool IsValidText (string value)
		{
			if (value == null)
				return true;
			return value.IndexOfAny (invalid_text_chars) == -1;
		}

		public SecurityElement SearchForChildByTag (string tag) 
		{
			if (tag == null)
				throw new ArgumentNullException ("tag");
				
			if (this.children == null)
				return null;
				
			for (int i = 0; i < children.Count; i++) {
				SecurityElement elem = (SecurityElement) children [i];
				if (elem.tag == tag)
					return elem;
			}
			return null;
		}			

		public string SearchForTextOfTag (string tag) 
		{
			if (tag == null)
				throw new ArgumentNullException ("tag");
				
			if (this.tag == tag)
				return this.text;
				
			if (this.children == null)
				return null;
			
			for (int i = 0; i < children.Count; i++) {
				string result = ((SecurityElement) children [i]).SearchForTextOfTag (tag);
				if (result != null) 
					return result;
			}

			return null;			
		}
		
		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();
			ToXml (ref s, 0);
			return s.ToString ();
		}
		
		private void ToXml(ref StringBuilder s, int level)
		{
			s.Append (' ', level * 3 );
			s.Append ("<");
			s.Append (tag);
			
			if (attributes != null) {
				IDictionaryEnumerator e = attributes.GetEnumerator ();				
				while (e.MoveNext ()) {
					s.Append (" ")
					 .Append (e.Key)
					 .Append ("=\"")
					 .Append (e.Value)
					 .Append ("\"");
				}
			}
			
			if ((text == null || text == String.Empty) && 
			    (children == null || children.Count == 0))
				s.Append ("/>").Append (Environment.NewLine);
			else {
				s.Append (">").Append (text);
				if (children != null) {
					s.Append (Environment.NewLine);
					foreach (SecurityElement child in children) {
						child.ToXml (ref s, level + 1);
					}
				}
				s.Append ("</")
				 .Append (tag)
				 .Append (">")
				 .Append (Environment.NewLine);
			}
		}
	}
}
