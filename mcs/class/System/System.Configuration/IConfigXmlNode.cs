//
// System.Configuration.IConfigXmlNode
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Configuration
{
	interface  IConfigXmlNode
	{
		string Filename { get; }
		int LineNumber { get; }
	}
}

