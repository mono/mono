// System.Runtime.Remoting.Channels.Simple.SimpleWireFormat.cs
//
// Author:
//	DietmarMaurer (dietmar@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com

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

using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Simple {

	public sealed class SimpleWireFormat 
	{
		enum TypeId : byte {
			Boolean,
			Byte,
			Char,
			Decimal,
			Double,
			Int16,
			Int32,
			Int64,
			SByte,
			String,
			Single,
			UInt16,
			UInt32,
			UInt64,
			NULL
		}

		public SimpleWireFormat ()
		{
		}		

		void SerializeObject (BinaryWriter writer, object obj)
		{
			if (obj == null) {
				writer.Write ((byte)TypeId.NULL);
				return;
			}
			
			Type type = obj.GetType ();

			if (type == typeof (String)) 
			{
				writer.Write ((byte)TypeId.String);
				writer.Write ((String)obj);
				return;
			}
			
			if (type == typeof (int)) {
				writer.Write ((byte)TypeId.Int32);
				writer.Write ((int)obj);
				return;
			}		

			if (type == typeof (long)) {
				writer.Write ((byte)TypeId.Int64);
				writer.Write ((long)obj);
				return;
			}		

			if (type == typeof (uint)) {
				writer.Write ((byte)TypeId.UInt32);
				writer.Write ((uint)obj);
				return;
			}		

			if (type == typeof (ulong)) {
				writer.Write ((byte)TypeId.UInt64);
				writer.Write ((ulong)obj);
				return;
			}		

			if (type == typeof (bool)) {
				writer.Write ((byte)TypeId.Boolean);
				writer.Write ((bool)obj);
				return;
			}		

			if (type == typeof (byte)) {
				writer.Write ((byte)TypeId.Byte);
				writer.Write ((byte)obj);
				return;
			}		

			if (type == typeof (sbyte)) {
				writer.Write ((byte)TypeId.SByte);
				writer.Write ((sbyte)obj);
				return;
			}		

			if (type == typeof (char)) {
				writer.Write ((byte)TypeId.Char);
				writer.Write ((char)obj);
				return;
			}		

			if (type == typeof (double)) {
				writer.Write ((byte)TypeId.Double);
				writer.Write ((double)obj);
				return;
			}		

			if (type == typeof (Single)) {
				writer.Write ((byte)TypeId.Single);
				writer.Write ((Single)obj);
				return;
			}
			
			if (type == typeof (Int16)) {
				writer.Write ((byte)TypeId.Int16);
				writer.Write ((Int16)obj);
				return;
			}		

			if (type == typeof (UInt16)) {
				writer.Write ((byte)TypeId.UInt16);
				writer.Write ((UInt16)obj);
				return;
			}		

			if (type == typeof (Decimal)) {
				writer.Write ((byte)TypeId.Decimal);
				writer.Write ((Decimal)obj);
				return;
			}

			throw new NotSupportedException (); 
		}

		object DeserializeObject (BinaryReader reader)
		{
			TypeId tid = (TypeId)reader.ReadByte ();

			if (tid == TypeId.NULL)
				return null;

			if (tid == TypeId.String) {
				return reader.ReadString ();
			}
			
			if (tid == TypeId.Int32) {
				return reader.ReadInt32 ();
			}
			
			if (tid == TypeId.Int64) {
				return reader.ReadInt64 ();
			}
			
			if (tid == TypeId.UInt32) {
				return reader.ReadUInt32 ();
			}
			
			if (tid == TypeId.UInt64) {
				return reader.ReadUInt64 ();
			}
			
			if (tid == TypeId.Boolean) {
				return reader.ReadBoolean ();
			}
			
			if (tid == TypeId.Byte) {
				return reader.ReadByte ();
			}
			
			if (tid == TypeId.SByte) {
				return reader.ReadSByte ();
			}
			
			if (tid == TypeId.Char) {
				return reader.ReadChar ();
			}
			
			if (tid == TypeId.Double) {
				return reader.ReadDouble ();
			}
			
			if (tid == TypeId.Single) {
				return reader.ReadSingle ();
			}
			
			if (tid == TypeId.Byte) {
				return reader.ReadByte ();
			}
			
			if (tid == TypeId.Int16) {
				return reader.ReadInt16 ();
			}
			
			if (tid == TypeId.UInt16) {
				return reader.ReadUInt16 ();
			}
			
			if (tid == TypeId.Decimal) {
				return reader.ReadDecimal ();
			}
			
			throw new NotSupportedException (); 
		}
		
		public IMethodCallMessage DeserializeRequest (Stream serializationStream, string uri)
		{
			if (serializationStream == null) {
				throw new ArgumentNullException ("serializationStream is null");
			}

			Type svr_type = RemotingServices.GetServerTypeForUri (uri);
			if (svr_type == null)
				throw new RemotingException ("no registered server for uri " + uri); 

			BinaryReader reader = new BinaryReader (serializationStream);
			
			string method_name = reader.ReadString ();
			int arg_count = reader.ReadInt32 ();

			object [] args = new object [arg_count];
			for (int i = 0; i < arg_count; i++) {
				args [i] = DeserializeObject (reader);
			}
			
			MonoMethodMessage msg = new MonoMethodMessage (svr_type, method_name, args);
			msg.Uri = uri;
			
			return msg;
		}

		public IMethodReturnMessage DeserializeResponse (Stream serializationStream,
								 IMethodCallMessage request) 
		{

			BinaryReader reader = new BinaryReader (serializationStream);

			object return_value = DeserializeObject (reader);
			
			int arg_count = reader.ReadInt32 ();
			object [] out_args = new object [arg_count];
			for (int i = 0; i < arg_count; i++)
				out_args [i] = DeserializeObject (reader);
			
			return new ReturnMessage (return_value, out_args, arg_count, null, request);
		}
		
		public void SerializeRequest (Stream serializationStream, object graph)
		{
			if (serializationStream == null) {
				throw new ArgumentNullException ("serializationStream is null");
			}

			BinaryWriter writer = new BinaryWriter (serializationStream);

			IMethodCallMessage msg = graph as IMethodCallMessage;
			if (msg != null) {			
				writer.Write (msg.MethodName);
				writer.Write ((int)msg.InArgCount);
				for (int i = 0; i < msg.InArgCount; i++)
					SerializeObject (writer, msg.GetInArg (i));
				return;
			}

			throw new NotSupportedException ();
		}

		public void SerializeResponse (Stream serializationStream, object graph)
		{
			if (serializationStream == null) {
				throw new ArgumentNullException ("serializationStream is null");
			}

			BinaryWriter writer = new BinaryWriter (serializationStream);

			IMethodReturnMessage res = graph as IMethodReturnMessage;
			if (res != null) {

				// this channel does not support serialization of exception,
				// so we simply let the transport decide what to do
				if (res.Exception != null)
					return;
				
				SerializeObject (writer, res.ReturnValue);
				writer.Write (res.OutArgCount);
			
				for (int i = 0; i < res.OutArgCount; i++)
					SerializeObject (writer, res.GetOutArg (i));

				return;
			}

			throw new NotSupportedException ();
		}
	}
}
