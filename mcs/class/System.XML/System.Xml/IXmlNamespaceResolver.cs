//
// IXmlNamespaceResolver.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
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
using System.Collections;
using System.Security.Policy;
using System.Xml.XPath;

namespace System.Xml
{
#if NET_2_0
	public interface IXmlNamespaceResolver
#else
	internal interface IXmlNamespaceResolver
#endif
	{
		[Obsolete]
		XmlNameTable NameTable { get; } 

		IDictionary GetNamespacesInScope (XmlNamespaceScope scope);

		string LookupNamespace (string prefix);  

		[Obsolete]
		string LookupNamespace (string prefix, bool atomizedName);  

		string LookupPrefix (string ns);  

		[Obsolete]
		string LookupPrefix (string ns, bool atomizedName);  
	}
}
