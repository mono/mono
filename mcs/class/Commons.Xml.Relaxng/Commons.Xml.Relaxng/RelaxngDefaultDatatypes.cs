//
// RelaxngDefaultDatatypes.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.Xml;

namespace Commons.Xml.Relaxng
{
	public class RelaxngString : RelaxngDatatype
	{
		static RelaxngString instance;
		static RelaxngString ()
		{
			instance = new RelaxngString ();
		}

		internal static RelaxngString Instance {
			get { return instance; }
		}

		public override string Name { get { return "string"; } }
		public override string NamespaceURI { get { return String.Empty; } }

		public override bool IsValid (string text, XmlReader reader)
		{
			return true;
		}

		public override object Parse (string text, XmlReader reader)
		{
			return text;
		}

		public override bool Compare (object o1, object o2)
		{
			return (string) o1 == (string) o2;
		}
	}

	public class RelaxngToken : RelaxngDatatype
	{
		static RelaxngToken instance;
		static RelaxngToken ()
		{
			instance = new RelaxngToken ();
		}

		internal static RelaxngToken Instance {
			get { return instance; }
		}

		public override string Name { get { return "token"; } }
		public override string NamespaceURI { get { return String.Empty; } }

		public override bool IsValid (string text, XmlReader reader)
		{
			return true;
		}

		public override object Parse (string text, XmlReader reader)
		{
			return Util.NormalizeWhitespace (text);
		}

		public override bool Compare (object o1, object o2)
		{
			return Util.NormalizeWhitespace ((string) o1) == 
				Util.NormalizeWhitespace ((string) o2);
		}
	}
}
