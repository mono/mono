//
// XmlElementEventHandler.cs: 
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

namespace System.Xml.Serialization {
	
	[Serializable]
	public delegate void XmlElementEventHandler (object sender, XmlElementEventArgs e);
}

