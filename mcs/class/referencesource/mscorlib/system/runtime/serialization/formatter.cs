// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Formatter
**
**
** Purpose: The abstract base class for all COM+ Runtime 
**          Serialization Formatters.
**
**
===========================================================*/


namespace System.Runtime.Serialization {
    using System.Threading;
    using System.Runtime.Remoting;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.IO;
    using System.Globalization;
    // This abstract class provides some helper methods for implementing
    // IFormatter.  It will manage queueing objects for serialization
    // (the functionality formerly provided by the IGraphWalker interface)
    // and generating ids on a per-object basis.
[Serializable]
[CLSCompliant(false)]
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class Formatter : IFormatter {
    
        protected ObjectIDGenerator m_idGenerator;
        protected Queue m_objectQueue;

        // The default constructor instantiates the queue for objects 
        // to be serialized and creates a new instance of the 
        // ObjectIDGenerator.
        protected Formatter() {
                m_objectQueue = new Queue();
                m_idGenerator = new ObjectIDGenerator();
        }
    
        public abstract Object Deserialize(Stream serializationStream);
    
        // This gives back the next object to be serialized.  Objects
        // are returned in a FIFO order based on how they were passed
        // to Schedule.  The id of the object is put into the objID parameter
        // and the Object itself is returned from the function.
        protected virtual Object GetNext(out long objID) {
            bool isNew;
    
            if (m_objectQueue.Count==0) {
                objID=0;
                return null;
            }

            Object obj = m_objectQueue.Dequeue();
            objID = m_idGenerator.HasId(obj, out isNew);
            if (isNew) {
                throw new SerializationException(Environment.GetResourceString("Serialization_NoID"));
            }
            
            return obj;
        }
    
        // Schedules an object for later serialization if it hasn't already been scheduled.
        // We get an ID for obj and put it on the queue for later serialization
        // if this is a new object id.
        protected virtual long Schedule(Object obj) {
            bool isNew;
            long id;
    
            if (obj==null) {
                return 0;
            }
    
            id = m_idGenerator.GetId(obj, out isNew);

            if (isNew) {
                m_objectQueue.Enqueue(obj);
            }
            return id;
        }
    
    
        public abstract void Serialize(Stream serializationStream, Object graph);
    
        // Writes an array to the stream
        protected abstract void WriteArray(Object obj, String name, Type memberType);
        
        // Writes a boolean to the stream.
         protected abstract void WriteBoolean(bool val, String name);
    
        // Writes a byte to the stream.
        protected abstract void WriteByte(byte val, String name);
    
        // Writes a character to the stream.
        protected abstract void WriteChar(char val, String name);
             
        // Writes an instance of DateTime to the stream.
        protected abstract void WriteDateTime(DateTime val, String name);
    
        // Writes an instance of Decimal to the stream.
        protected abstract void WriteDecimal(Decimal val, String name);
    
        // Writes an instance of Double to the stream.
        protected abstract void WriteDouble(double val, String name);
    
        // Writes an instance of Int16 to the stream.
        protected abstract void WriteInt16(short val, String name);
    
        // Writes an instance of Int32 to the stream.
    
        protected abstract void WriteInt32(int val, String name);
    
        // Writes an instance of Int64 to the stream.
        protected abstract void WriteInt64(long val, String name);
    
        // Writes an object reference to the stream.  Schedules the object with the graph walker
        // to handle the work.
        protected abstract void WriteObjectRef(Object obj, String name, Type memberType);
    
        // Switches on the type of the member to determine which of the Write* methods
        // to call in order to write this particular member to the stream.
        protected virtual void WriteMember(String memberName, Object data) {

            BCLDebug.Trace("SER", "[Formatter.WriteMember]data: ", data);

            if (data==null) {
                WriteObjectRef(data, memberName, typeof(Object));
                return;
            }

            Type varType = data.GetType();
    
            BCLDebug.Trace("SER", "[Formatter.WriteMember]data is of type: " , varType);

            if (varType==typeof(Boolean)) {
                WriteBoolean(Convert.ToBoolean(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Char)) {
                WriteChar(Convert.ToChar(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(SByte)) {
                WriteSByte(Convert.ToSByte(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Byte)) {
                WriteByte(Convert.ToByte(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Int16)) {
                WriteInt16(Convert.ToInt16(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Int32)) {
                WriteInt32(Convert.ToInt32(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Int64)) {
                WriteInt64(Convert.ToInt64(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Single)) {
                WriteSingle(Convert.ToSingle(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Double)) {
                WriteDouble(Convert.ToDouble(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(DateTime)) {
                WriteDateTime(Convert.ToDateTime(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(Decimal)) {
                WriteDecimal(Convert.ToDecimal(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(UInt16)) {
                WriteUInt16(Convert.ToUInt16(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(UInt32)) {
                WriteUInt32(Convert.ToUInt32(data, CultureInfo.InvariantCulture), memberName);
            } else if (varType==typeof(UInt64)) {
                WriteUInt64(Convert.ToUInt64(data, CultureInfo.InvariantCulture), memberName);
            } else {
                if (varType.IsArray) {
                    WriteArray(data, memberName, varType);
                } else if (varType.IsValueType) {
                    WriteValueType(data, memberName, varType);
                } else {
                    WriteObjectRef(data, memberName, varType);
                }
            }
        }
            
        // Writes an instance of SByte to the stream.
        [CLSCompliant(false)]
        protected abstract void WriteSByte(sbyte val, String name);
    
        // Writes an instance of Single to the stream.
        protected abstract void WriteSingle(float val, String name);
    
        // Writes an instance of TimeSpan to the stream.
        protected abstract void WriteTimeSpan(TimeSpan val, String name);
    
    
        // Writes an instance of an ushort to the stream.
        [CLSCompliant(false)]
        protected abstract void WriteUInt16(ushort val, String name);
    
        // Writes an instance of an uint to the stream.
        [CLSCompliant(false)]
        protected abstract void WriteUInt32(uint val, String name);
    
        // Writes an instance of a ulong to the stream.
        [CLSCompliant(false)]
        protected abstract void WriteUInt64(ulong val, String name);
    
    
        // Writes a valuetype out to the stream.
        protected abstract void WriteValueType(Object obj, String name, Type memberType);


        public abstract ISurrogateSelector SurrogateSelector {
            get;
            set;
        }

        public abstract SerializationBinder Binder {
            get;
            set;
        }

        public abstract StreamingContext Context {
            get;
            set;
        }
    }
    
}
