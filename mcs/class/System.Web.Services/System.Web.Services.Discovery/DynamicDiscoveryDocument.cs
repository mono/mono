// 
// System.Web.Services.Discovery.DynamicDiscoveryDocument.cs
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

using System.IO;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("dynamicDiscovery", Namespace="urn:schemas-dynamicdiscovery:disco.2000-03-17", IsNullable=true)]
	public sealed class DynamicDiscoveryDocument {

		#region Fields
		
		public const string Namespace = "urn:schemas-dynamicdiscovery:disco.2000-03-17";
		
		ExcludePathInfo[] excludes;
		
		#endregion // Fields
		
		#region Constructors

		public DynamicDiscoveryDocument () 
		{
		}
		
		#endregion // Constructors

		#region Properties
		
		[XmlElement("exclude", typeof(ExcludePathInfo))]
		public ExcludePathInfo[] ExcludePaths {
			get { return excludes; }
			set { excludes = value; }
		}
		
		#endregion // Properties

		#region Methods

		public static DynamicDiscoveryDocument Load (Stream stream)
		{
			XmlSerializer ser = new XmlSerializer (typeof(DynamicDiscoveryDocument));
			return (DynamicDiscoveryDocument) ser.Deserialize (stream);
		}

		public void Write (Stream stream)
		{
			XmlSerializer ser = new XmlSerializer (typeof(DynamicDiscoveryDocument));
			ser.Serialize (stream, this);
		}
		
		internal bool IsExcluded (string path)
		{
			if (excludes == null) return false;
			
			foreach (ExcludePathInfo ex in excludes)
				if (ex.Path == path) return true;
				
			return false;
		}

		#endregion // Methods
	}
}
