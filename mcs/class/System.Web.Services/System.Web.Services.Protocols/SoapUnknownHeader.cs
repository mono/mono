// 
// System.Web.Services.Protocols.SoapUnknownHeader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Web.Services.Protocols {
	public sealed class SoapUnknownHeader : SoapHeader {

		#region Fields

		XmlElement element;

		#endregion // Fields

		#region Constructors

		public SoapUnknownHeader ()
		{
			element = null;
		}

		#endregion // Constructors

		#region Properties

		public XmlElement Element {
			get { return element; }
			set { element = value; }
		}

		#endregion // Properties
	}
}
