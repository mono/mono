// created on 24/04/2003 at 15:37
//
//	System.Runtime.Serialization.Formatters.Soap.ISoapReader
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal interface ISoapReader {
		event ElementReadEventHandler ElementReadEvent;
		ISoapMessage TopObject {
			get; set;
		}
	}
}
