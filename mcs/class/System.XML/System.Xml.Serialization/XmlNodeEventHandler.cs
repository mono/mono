//
// XmlNodeEventHandler.cs: 
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

namespace System.Xml.Serialization {
	
	[Serializable]
	public delegate void XmlNodeEventHandler (object sender, XmlNodeEventArgs e);
}

