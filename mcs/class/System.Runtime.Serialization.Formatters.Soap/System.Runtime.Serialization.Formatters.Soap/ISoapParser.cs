// created on 23/04/2003 at 12:05
//
//	System.Runtime.Serialization.Formatters.Soap.ISoapParser
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;
using System.IO;


namespace System.Runtime.Serialization.Formatters.Soap {
	internal interface ISoapParser {
		void Run();
		Stream InStream {
			set;
		}
		event SoapElementReadEventHandler SoapElementReadEvent;
	}
}
