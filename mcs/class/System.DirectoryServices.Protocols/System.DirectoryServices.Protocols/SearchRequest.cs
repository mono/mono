//
// SearchRequest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
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

using System;
using System.Collections.Specialized;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	[MonoTODO]
	public class SearchRequest : DirectoryRequest
	{
		public SearchRequest ()
		{
			Attributes = new StringCollection ();
		}

		[MonoTODO]
		public SearchRequest (string distinguishedName, string ldapFilter, SearchScope searchScope, params string [] attributeList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SearchRequest (string distinguishedName, XmlDocument filter, SearchScope searchScope, params string [] attributeList)
		{
			throw new NotImplementedException ();
		}

		public DereferenceAlias Aliases { get; set; }
		public StringCollection Attributes { get; private set; }
		public string DistinguishedName { get; set; }
		public object Filter { get; set; }
		public SearchScope Scope { get; set; }
		public int SizeLimit { get; set; }
		public TimeSpan TimeLimit { get; set; }
		public bool TypesOnly { get; set; }

		[MonoTODO]
		protected override XmlElement ToXmlNode (XmlDocument doc)
		{
			throw new NotImplementedException ();
		}
	}
}
