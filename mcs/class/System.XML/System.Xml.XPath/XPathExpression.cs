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

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

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

#if NET_2_0
		public
#else
		internal
#endif
		static XPathExpression Compile (string xpath)
		{
			return Compile (xpath, null, null);
		}

#if NET_2_0
		public static XPathExpression Compile (
			string xpath, NSResolver nsmgr)
		{
			return Compile (xpath, nsmgr, null);
		}
#endif

		internal static XPathExpression Compile (string xpath,
			NSResolver nsmgr, IStaticXsltContext ctx)
		{
			XPathParser parser = new XPathParser (ctx);
			CompiledExpression x = new CompiledExpression (xpath, parser.Compile (xpath));
			x.SetContext (nsmgr);
			return x;
		}

#if NET_2_0
		public abstract void SetContext (IXmlNamespaceResolver nsResolver);
#endif
		#endregion
	}
}
