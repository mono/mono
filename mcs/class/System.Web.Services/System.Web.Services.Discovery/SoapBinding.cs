// 
// System.Web.Services.Discovery.SoapBinding.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("soap", Namespace="http://schemas/xmlsoap.org/disco/schema/soap/", IsNullable=true)]
	public sealed class SoapBinding  {

		#region Fields
		
		public const string Namespace = "http://schemas/xmlsoap.org/disco/schema/soap/";

		private string address;
		private XmlQualifiedName binding;
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public SoapBinding () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties
		
		[XmlAttribute("address")]
		public string Address {
			get { return address; }
			set { address = value; }
		}
		
		[XmlAttribute("binding")]
		public XmlQualifiedName Binding {
			get { return binding; }
			set { binding = value; }
		}
		
		#endregion // Properties

	}
}
