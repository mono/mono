//
// System.Web.UI.XPathBinder
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml.XPath;
using System.Xml;

namespace System.Web.UI {
	public sealed class XPathBinder {
		private XPathBinder ()
		{
		}
		
		public static object Eval (object container, string xpath)
		{
			if (xpath == null || xpath.Length == 0)
				throw new ArgumentNullException ("xpath");
			
			IXPathNavigable factory = container as IXPathNavigable;
			
			if (factory == null)
				throw new ArgumentException ("container");
			
			object result = factory.CreateNavigator ().Evaluate (xpath);
			
			XPathNodeIterator itr = result as XPathNodeIterator;
			if (itr != null) {
				if (itr.MoveNext())
					return itr.Current.Value;
				else
					return null;
			}
			return result;
		}
		
		public static string Eval (object container, string xpath, string format)
		{
			object result = Eval (container, xpath);
			
			if (result == null)
				return String.Empty;
			if (format == null || format.Length == 0)
				return result.ToString ();
			
			return String.Format (format, result);
		}

		public static IEnumerable Select (object container, string xpath)
		{
			if (xpath == null || xpath.Length == 0)
				throw new ArgumentNullException ("xpath");
			
			IXPathNavigable factory = container as IXPathNavigable;
			
			if (factory == null)
				throw new ArgumentException ("container");
			
			XPathNodeIterator itr = factory.CreateNavigator ().Select (xpath);
			ArrayList ret = new ArrayList ();
			
			while (itr.MoveNext ()) {
				IHasXmlNode nodeAccessor = itr.Current as IHasXmlNode;
				if (nodeAccessor == null)
					throw new InvalidOperationException ();
				ret.Add (nodeAccessor.GetNode ());
			}
			return ret;
		}
		
	}
}
#endif

