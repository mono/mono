//
// System.Runtime.Serialization.Formatter.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;

namespace System.Runtime.Serialization
{
[CLSCompliant (false)]
[Serializable]
#if NET_2_0
[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
public abstract class Formatter : IFormatter
{
	protected Formatter ()
	{
	}
	
	protected ObjectIDGenerator m_idGenerator = new ObjectIDGenerator ();
	protected Queue m_objectQueue = new Queue ();

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

	protected virtual object GetNext (out long objID)
	{
		if (m_objectQueue.Count == 0)
		{
			// set the out field to 0
			objID = 0L;
			return null;
		}

		Object o = m_objectQueue.Dequeue ();
		bool FirstTime;
		objID = m_idGenerator.HasId (o, out FirstTime);

		return o;
	}

	protected virtual long Schedule (object obj)
	{
		if (obj == null)
			return 0L;

		bool FirstTime;
		long ID = m_idGenerator.GetId (obj, out FirstTime);
		if (FirstTime)
			m_objectQueue.Enqueue (obj);

		return ID;
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

	protected virtual void WriteMember (string memberName, object data)
	{
		if (data == null)
			WriteObjectRef (data, memberName, typeof(Object));

		Type dataType = data.GetType ();
		if (dataType.IsArray)
			WriteArray (data, memberName, dataType);
		else if (dataType == typeof(bool))
			WriteBoolean ((bool)data, memberName);
		else if (dataType == typeof(byte))
			WriteByte ((byte)data, memberName);
		else if (dataType == typeof(char))
			WriteChar ((char)data, memberName);
		else if (dataType == typeof(DateTime))
			WriteDateTime ((DateTime)data, memberName);
		else if (dataType == typeof(decimal))
			WriteDecimal ((decimal)data, memberName);
		else if (dataType == typeof(double))
			WriteDouble ((double)data, memberName);
		else if (dataType == typeof(Int16))
			WriteInt16 ((Int16)data, memberName);
		else if (dataType == typeof(Int32))
			WriteInt32 ((Int32)data, memberName);
		else if (dataType == typeof(Int64))
			WriteInt64 ((Int64)data, memberName);
		else if (dataType == typeof(sbyte))
			WriteSByte ((sbyte)data, memberName);
		else if (dataType == typeof(float))
			WriteSingle ((float)data, memberName);
		else if (dataType == typeof(TimeSpan))
			WriteTimeSpan ((TimeSpan)data, memberName);
		else if (dataType == typeof(UInt16))
			WriteUInt16 ((UInt16)data, memberName);
		else if (dataType == typeof(UInt32))
			WriteUInt32 ((UInt32)data, memberName);
		else if (dataType == typeof(UInt64))
			WriteUInt64 ((UInt64)data, memberName);
		else if (dataType.IsValueType)
			WriteValueType (data, memberName, dataType);

		WriteObjectRef (data, memberName, dataType);
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
