//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
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

namespace System.Messaging 
{
	public class Trustee 
	{
		[MonoTODO]
		public Trustee()
		{
			this.name = null;
			this.systemName = null;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		[MonoTODO("What about systemName?")]
		public Trustee(string name)
		{
			this.name = name;
			this.systemName = null;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		private string name;
		private string systemName;
		private TrusteeType trusteeType;
		
		public Trustee(string name, string systemName)
		{
			this.name = name;
			this.systemName = systemName;
			this.trusteeType = TrusteeType.Unknown;
		}
		
		public Trustee(string name, string systemName, TrusteeType trusteeType)
		{
			this.name = name;
			this.systemName = systemName;
			this.trusteeType = trusteeType;
		}
		
		public string Name 
		{
			get { return name; }
			set { name = value;}
		}
		
		public string SystemName 
		{
			get { return systemName; }
			set { systemName = value;}
		}

		public TrusteeType TrusteeType
		{
			get { return trusteeType; }
			set { trusteeType = value;}
		}
	}
}
