// 
// System.Web.Services.Description.Message.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description 
{
#if NET_2_0
	[XmlFormatExtensionPoint ("Extensions")]
#endif
	public sealed class Message :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{
		#region Fields

#if !NET_2_0
		string name;
#endif
		MessagePartCollection parts;
		ServiceDescription serviceDescription;
#if NET_2_0
		ServiceDescriptionFormatExtensionCollection extensions;
#endif

		#endregion // Fields

		#region Constructors
		
		public Message ()
		{
#if !NET_2_0
			name = String.Empty;
#endif
#if NET_2_0
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
#endif
			parts = new MessagePartCollection (this);
			serviceDescription = null;
		}
		
		#endregion // Constructors

		#region Properties

#if !NET_2_0
		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlElement ("part")]
		public MessagePartCollection Parts {
			get { return parts; }
		}

//		[XmlIgnore]
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

#if NET_2_0
		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}
#endif

		#endregion // Properties

		#region Methods

		public MessagePart FindPartByName (string partName)
		{
			return parts [partName];
		}

		public MessagePart[] FindPartsByName (string[] partNames) 
		{
			ArrayList searchResults = new ArrayList ();

			foreach (string partName in partNames)
				searchResults.Add (FindPartByName (partName));

			int count = searchResults.Count;

			if (count == 0)
				throw new ArgumentException ();

			MessagePart[] returnValue = new MessagePart[count];
			searchResults.CopyTo (returnValue);
			return returnValue;
		}

		internal void SetParent (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion
	}
}
