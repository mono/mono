//
// System.Security.SecurityElement.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com

using System.Globalization;
using System.Collections;
using System.Text;

namespace System.Security {

	[MonoTODO ("See bottom of the class for missing methods")]
	public sealed class SecurityElement {
		string text;
		string tag;
		
		public SecurityElement (string tag, string text)
		{
			if (tag.IndexOfAny (invalid_chars) != -1)
				throw new ArgumentException (Locale.GetText ("Invalid XML string"));
			if (text.IndexOfAny (invalid_chars) != -1 ||
			    tag.IndexOfAny (invalid_chars) != -1)
				throw new ArgumentException (Locale.GetText ("Invalid XML string"));
			
			this.tag = tag;
			this.text = text;
		}

		public SecurityElement (string tag)
		{
			if (tag.IndexOfAny (invalid_chars) != -1)
				throw new ArgumentException (Locale.GetText ("Invalid XML string"));

			this.tag = tag;
		}

		Hashtable attributes;
		public Hashtable Attributes {
			get {
				return attributes;
			}

			set {
				attributes = value;
			}
		}

		ArrayList children;
		public ArrayList Children {
			get {
				return children;
			}

			set {
				children = value;
			}
		}

		public string Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

		public string Text {
			get {
				return text;
			}

			set {
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

			if (name.IndexOfAny (invalid_chars) != -1)
				throw new ArgumentException (Locale.GetText ("Invalid XML string"));

			if (value.IndexOfAny (invalid_chars) != -1)
				throw new ArgumentException (Locale.GetText ("Invalid XML string"));
			
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

		public bool Equals (SecurityElement other)
		{
			if (other == null)
				return false;

			if (text != other.text)
				return false;

			if (tag != other.tag)
				return false;

			throw new Exception ("IMPLEMENT ME: Compare attributes and children");
		}

		static char [] invalid_chars = new char [] { '<', '>', '"', '\'', '&' };
		
		public static string Escape (string str)
		{
			StringBuilder sb;
			
			if (str.IndexOfAny (invalid_chars) == -1)
				return str;

			sb = new StringBuilder ();
			int len = str.Length;
			
			for (int i = 0; i < len; i++){
				char c = str [i];

				switch (c){
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

		public static bool IsInvalidAttributeName (string name)
		{
			return name.IndexOfAny (invalid_chars) != -1;
		}

		public static bool IsInvalidAttributeValue (string value)
		{
			return value.IndexOfAny (invalid_chars) != -1;
		}

		public static bool IsInvalidTag (string value)
		{
			return value.IndexOfAny (invalid_chars) != -1;
		}

		public static bool IsInvalidText (string value)
		{
			return value.IndexOfAny (invalid_chars) != -1;
		}

		//
		// TODO:
		//
		// SearchForChildByTag
		// SearchForTextOfTag
		// ToString
	}
}
