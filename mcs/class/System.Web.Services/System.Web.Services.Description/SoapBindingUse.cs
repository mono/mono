// 
// System.Web.Services.Description.SoapBindingUse.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public enum SoapBindingUse {
		[XmlIgnore]
		Default,
		[XmlEnum ("encoded")]
		Encoded,
		[XmlEnum ("literal")]
		Literal
	}
}
