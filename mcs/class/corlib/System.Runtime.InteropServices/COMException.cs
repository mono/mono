//
// COMException.cs - COM Exception
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System.Runtime.Serialization;

namespace System.Runtime.InteropServices {

[Serializable]
public class COMException : ExternalException {

	public COMException () 
		: base () {}

	public COMException (string message) 
		: base (message) {}

	public COMException (string message, Exception inner) 
		: base (message, inner) {}

	public COMException (string message, int errorCode) 
		: base (message, errorCode) {}

	protected COMException (SerializationInfo info, StreamingContext context) 
		: base (info, context) {}

	[MonoTODO()]
	public override string ToString ()
	{
		return null;
	}
} 

}
