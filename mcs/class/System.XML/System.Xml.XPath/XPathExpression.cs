//
// System.Xml.XPath.XPathExpression
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System.Collections;

namespace System.Xml.XPath
{
	public abstract class XPathExpression
	{
		#region Constructor

		internal XPathExpression ()
		{
		}

		#endregion

		#region Properties

		public abstract string Expression { get; }

		public abstract XPathResultType ReturnType { get; }

		#endregion

		#region Methods

		public abstract void AddSort (object expr, IComparer comparer);

		public abstract void AddSort (
			object expr,
			XmlSortOrder order,
			XmlCaseOrder caseOrder,
			string lang,
			XmlDataType dataType
		);

		public abstract XPathExpression Clone ();

		public abstract void SetContext (XmlNamespaceManager nsManager);
		
		#endregion
	}
}
