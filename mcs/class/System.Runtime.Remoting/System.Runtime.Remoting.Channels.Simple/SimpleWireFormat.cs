// System.Runtime.Remoting.Channels.Simple.SimpleWireFormat.cs
//
// Author:
//	DietmarMaurer (dietmar@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com

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
			Exception,
			NULL
		}

		public SimpleWireFormat ()
		{
		}
		

		void SerializeObject (BinaryWriter writer, object obj)
		{
			Type type = obj.GetType ();

			if (obj == null) {
				writer.Write ((byte)TypeId.NULL);
				return;
			}
			
			if (type == typeof (String)) {
				writer.Write ((byte)TypeId.String);
				writer.Write ((String)obj);
				return;
			}
			
			if (type == typeof (int)) {
				writer.Write ((byte)TypeId.Int32);
				writer.Write ((int)obj);
				return;
			}		

			if (type == typeof (bool)) {
				writer.Write ((byte)TypeId.Boolean);
				writer.Write ((bool)obj);
				return;
			}		

			Exception e = obj as Exception;
			if (e != null) {
				writer.Write ((byte)TypeId.Exception);
				writer.Write (e.Message);
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
			
			if (tid == TypeId.Boolean) {
				return reader.ReadBoolean ();
			}
			
			if (tid == TypeId.Exception) {
				return new RemotingException (reader.ReadString ());
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
			
			Console.WriteLine ("Deserialize " + msg);
			
			return msg;
		}

		public IMethodReturnMessage DeserializeResponse (Stream serializationStream,
								 IMethodCallMessage request) 
		{

			BinaryReader reader = new BinaryReader (serializationStream);
			
		      
			object return_value = DeserializeObject (reader);
			Exception e = return_value as Exception;
			if (e != null) 
				return new ReturnMessage (e, request);
			
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

				if (res.Exception != null) {
					SerializeObject (writer, res.Exception);
					return;
				}
				
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
