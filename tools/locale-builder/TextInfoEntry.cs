using System;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Tools.LocaleBuilder {

        public class TextInfoEntry : Entry {
		
		string ansi = "0";
		string ebcdic = "0";
		string mac = "0";
		string oem = "0";
		string listsep = ",";

		public TextInfoEntry (int lcid, XPathDocument d)
		{
			string q = "/textinfos/textinfo [@lcid=" + lcid + "]";
			XPathNodeIterator ni = (XPathNodeIterator) d.CreateNavigator ().Evaluate (q);
			// no info, move along
			if (! ni.MoveNext ())
				throw new Exception ();		
			
			ansi = ni.Current.GetAttribute ("ansi", String.Empty);
			ebcdic = ni.Current.GetAttribute ("ebcdic", String.Empty);
			mac = ni.Current.GetAttribute ("mac", String.Empty);
			oem = ni.Current.GetAttribute ("oem", String.Empty);
			listsep = ni.Current.GetAttribute ("listsep", String.Empty);
		}
		
                public override string ToString ()
                {
			StringBuilder b = new StringBuilder ();
			b.Append ("{ ");
			b.Append (ansi);
			b.Append (", ");
			b.Append (ebcdic);
			b.Append (", ");
			b.Append (mac );
			b.Append (", ");
			b.Append (oem);
			b.Append (", '");
			b.Append (listsep);
			b.Append ("' }");
			
			return b.ToString ();
		}
	}
}