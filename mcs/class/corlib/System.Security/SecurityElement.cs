//
// System.Security.SecurityElement.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//   Sebastien Pouliot  <spouliot@videotron.ca>
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
		internal class SecurityAttribute {
			
			private string _name;
			private string _value;

			public SecurityAttribute (string name, string value) 
			{
				if (!IsValidAttributeName (name))
					throw new ArgumentException (Locale.GetText ("Invalid XML attribute name") + ": " + name);

				if (!IsValidAttributeValue (value))
					throw new ArgumentException (Locale.GetText ("Invalid XML attribute value") + ": " + value);

				_name = name;
				_value = value;
			}

			public string Name {
				get { return _name; }
			}

			public string Value {
				get { return _value; }
			}
		}

		string text;
		string tag;
		ArrayList attributes;
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
					
				Hashtable result = new Hashtable (attributes.Count);
				foreach (SecurityAttribute sa in attributes) {
					result.Add (sa.Name, sa.Value);
				}
				return result;
			}

			set {				
				if (value == null || value.Count == 0) {
					attributes.Clear ();
					return;
				}
				
				if (attributes == null)
					attributes = new ArrayList ();
				else
					attributes.Clear ();
				IDictionaryEnumerator e = value.GetEnumerator ();
				while (e.MoveNext ()) {
					attributes.Add (new SecurityAttribute ((string) e.Key, (string) e.Value));
				}
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
				int colon = value.IndexOf (':');
				tag = colon < 0 ? value : value.Substring (colon + 1);
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
			if (name == null)
				throw new ArgumentNullException ("name");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (GetAttribute (name) != null)
				throw new ArgumentException (Locale.GetText ("Duplicate attribute : " + name));

			if (attributes == null)
				attributes = new ArrayList ();
			attributes.Add (new SecurityAttribute (name, value));
		}

		public void AddChild (SecurityElement child)
		{
			if (child == null)
				throw new ArgumentNullException ("child");

			if (children == null)
				children = new ArrayList ();

			children.Add (child);
		}

		public string Attribute (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			SecurityAttribute sa = GetAttribute (name);
			return ((sa == null) ? null : sa.Value);
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
				foreach (SecurityAttribute sa1 in attributes) {
					SecurityAttribute sa2 = other.GetAttribute (sa1.Name);
					if ((sa2 == null) || (sa1.Value != sa2.Value))
						return false;
				}
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
		
		private void ToXml (ref StringBuilder s, int level)
		{
			s.Append (' ', level * 3);
			s.Append ("<");
			s.Append (tag);
			
			if (attributes != null) {
				for (int i=0; i < attributes.Count; i++) {
					SecurityAttribute sa = (SecurityAttribute) attributes [i];
					s.Append (" ");
					// all other attributes must align with the first one
					if (i != 0)
						s.Append (' ', (level * 3) + tag.Length + 1);
					s.Append (sa.Name)
					 .Append ("=\"")
					 .Append (sa.Value)
					 .Append ("\"");
					if (i != attributes.Count - 1)
						s.Append (Environment.NewLine);
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
					s.Append (' ', level * 3);
				}
				s.Append ("</")
				 .Append (tag)
				 .Append (">")
				 .Append (Environment.NewLine);
			}
		}

		internal SecurityAttribute GetAttribute (string name) 
		{
			if (attributes != null) {
				foreach (SecurityAttribute sa in attributes) {
					if (sa.Name == name)
						return sa;
				}
			}
			return null;
		}
	}
}
