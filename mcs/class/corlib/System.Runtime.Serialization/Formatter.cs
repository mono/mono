//
// System.Runtime.Serialization.Formatter.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.IO;

namespace System.Runtime.Serialization
{
[CLSCompliant (false)]
[Serializable]
public abstract class Formatter : IFormatter
{
	protected Formatter ()
	{
	}
	
	protected ObjectIDGenerator m_idGenerator;
	protected Queue m_objectQueue;

	public abstract SerializationBinder Binder {
		get;
		set;
	}

	public abstract StreamingContext Context {
		get;
		set;
	}

	public abstract ISurrogateSelector SurrogateSelector {
		get;
		set;
	}

	public abstract object Deserialize (Stream serializationStream);

	[MonoTODO]
	protected virtual object GetNext (out long objID)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected virtual long Schedule (object obj)
	{
		throw new NotImplementedException ();
	}

	public abstract void Serialize (Stream serializationStream, object graph);
	 
	protected abstract void WriteArray (object obj, string name, Type memberType);
	 
	protected abstract void WriteBoolean (bool val, string name);
	 
	protected abstract void WriteByte (byte val, string name);
	 
	protected abstract void WriteChar (char val, string name);
	 
	protected abstract void WriteDateTime (DateTime val, string name);

	protected abstract void WriteDecimal (Decimal val, string name);

	protected abstract void WriteDouble (double val, string name);

	protected abstract void WriteInt16 (short val, string name);

	protected abstract void WriteInt32 (int val, string name);

	protected abstract void WriteInt64 (long val, string name);

	[MonoTODO]
	protected virtual void WriteMember (string memberName, object data)
	{
		throw new NotImplementedException ();
	}

	protected abstract void WriteObjectRef (object obj, string name, Type memberType);


        [CLSCompliant (false)]
	protected abstract void WriteSByte (sbyte val, string name);


	protected abstract void WriteSingle (float val, string name);
	
	protected abstract void WriteTimeSpan (TimeSpan val, string name);

	[CLSCompliant (false)]
	protected abstract void WriteUInt16 (ushort val, string name);

	[CLSCompliant (false)]
	protected abstract void WriteUInt32 (uint val, string name);

	[CLSCompliant (false)]
	protected abstract void WriteUInt64 (ulong val, string name);

	protected abstract void WriteValueType (object obj, string name, Type memberType);	
}
}
