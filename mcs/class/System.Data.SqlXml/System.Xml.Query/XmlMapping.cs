//
// System.Xml.Query.XmlMapping
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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
