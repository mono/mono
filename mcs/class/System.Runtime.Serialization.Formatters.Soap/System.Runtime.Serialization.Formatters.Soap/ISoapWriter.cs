// created on 07/04/2003 at 17:49
//
//	System.Runtime.Serialization.Formatters.Soap.ISoapWriter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters.Soap {
	interface ISoapWriter {
		void WriteRoot(object objValue, Type objType, bool getIntoFields);
		void WriteFields(SerializationInfo info);
		
		void WriteArrayItem(Type itemType, object itemValue);
		object TopObject {
			set;
		}
		void Run();
		event DoneWithElementEventHandler DoneWithElementEvent;
		event DoneWithElementEventHandler DoneWithArray;
		event DoneWithElementEventHandler GetRootInfo;
		ObjectWriter Writer {
			get; set;
		}
		
		Stack CurrentArrayType {
			get;
		}
	}
}
