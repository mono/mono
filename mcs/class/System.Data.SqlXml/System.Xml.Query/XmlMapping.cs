//
// System.Xml.Query.XmlMapping
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

#if NET_2_0

namespace System.Xml.Query
{
	public sealed class XmlMapping
	{
		private ArrayList dataSourceNames;
		private XmlExpression expression;
		private XmlSchema resultsSchema;

		[MonoTODO]
		public XmlMapping (string mappingUrl)
		{
			using (XmlReader xr = new XmlTextReader (mappingUrl)) {
				Initialize (xr);
			}
		}

		[MonoTODO]
		public XmlMapping (XmlReader reader)
		{
			Initialize (reader);
		}

		private void Initialize (XmlReader reader)
		{
			dataSourceNames = new ArrayList ();

			throw new NotImplementedException ();
		}

		[MonoTODO ("Should return clone list?")]
		public ArrayList DataSourceNames {
			get { return dataSourceNames; }
		}

		public XmlExpression Expression {
			get { return expression; }
		}

		public XmlSchema ResultsSchema {
			get { return resultsSchema; }
		}
	}
}

#endif // NET_2_0
