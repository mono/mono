// 
// System.Web.Services.Disocvery.DiscoveryClientResult.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
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


using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryClientResult {
	
		#region Fields

		private string filename;
		private string referenceTypeName;
		private string url;

		#endregion // Fields

		#region Constructors

		public DiscoveryClientResult () 
		{
		}
		
		public DiscoveryClientResult (Type referenceType, string url, string filename) : this() 
		{
			this.filename = filename;
			this.url = url;
			this.referenceTypeName = referenceType.FullName;
		}
		
		#endregion // Constructors

		#region Properties	
	
		[XmlAttribute("filename")]
		public string Filename {
			get { return filename; }
			set { filename = value; }
		}
		
		[XmlAttribute("referenceType")]
		public string ReferenceTypeName {
			get { return referenceTypeName; }
			set { referenceTypeName = value; }
		}
		
		[XmlAttribute("url")]
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		#endregion // Properties
	}
}
