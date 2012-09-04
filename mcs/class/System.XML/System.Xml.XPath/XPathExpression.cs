//
// System.Xml.XPath.XPathExpression
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
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

using System.Collections;
using Mono.Xml.XPath;
using System.Xml.Xsl;

using NSResolver = System.Xml.IXmlNamespaceResolver;

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

		public static XPathExpression Compile (string xpath)
		{
			return Compile (xpath, null, null);
		}

		public static XPathExpression Compile (string xpath, NSResolver nsResolver)
		{
			return Compile (xpath, nsResolver, null);
		}

		internal static XPathExpression Compile (string xpath,
			NSResolver nsResolver, IStaticXsltContext ctx)
		{
			XPathParser parser = new XPathParser (ctx);
			CompiledExpression x = new CompiledExpression (xpath, parser.Compile (xpath));
			x.SetContext (nsResolver);
			return x;
		}

		public abstract void SetContext (IXmlNamespaceResolver nsResolver);
		#endregion
	}
}
