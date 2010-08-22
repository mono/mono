// BinaryFormatter.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//  Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Formatters.Binary {

	[ComVisible (true)]
	public sealed class BinaryFormatter : IRemotingFormatter, IFormatter 
	{
		private FormatterAssemblyStyle assembly_format = FormatterAssemblyStyle.Simple;
		private SerializationBinder binder;
		private StreamingContext context;
		private ISurrogateSelector surrogate_selector;
		private FormatterTypeStyle type_format = FormatterTypeStyle.TypesAlways;
		private TypeFilterLevel filter_level = TypeFilterLevel.Full;
		
		public BinaryFormatter()
		{
			surrogate_selector=null;
			context=new StreamingContext(StreamingContextStates.All);
		}
		
		public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
		{
			surrogate_selector=selector;
			this.context=context;
		}

		public FormatterAssemblyStyle AssemblyFormat
		{
			get {
				return(assembly_format);
			}
			set {
				assembly_format=value;
			}
		}

		public SerializationBinder Binder
		{
			get {
				return(binder);
			}
			set {
				binder=value;
			}
		}

		public StreamingContext Context 
		{
			get {
				return(context);
			}
			set {
				context=value;
			}
		}
		
		public ISurrogateSelector SurrogateSelector 
		{
			get {
				return(surrogate_selector);
			}
			set {
				surrogate_selector=value;
			}
		}
		
		public FormatterTypeStyle TypeFormat 
		{
			get {
				return(type_format);
			}
			set {
				type_format=value;
			}
		}

		public TypeFilterLevel FilterLevel 
		{
			get { return filter_level; }
			set { filter_level = value; }
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public object Deserialize (Stream serializationStream)
		{
			return NoCheckDeserialize (serializationStream, null);
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public object Deserialize (Stream serializationStream, HeaderHandler handler) 
		{
			return NoCheckDeserialize (serializationStream, handler);
		}

		// shared by Deserialize and UnsafeDeserialize which both involve different security checks
		private object NoCheckDeserialize (Stream serializationStream, HeaderHandler handler)
		{
			if(serializationStream==null) 
			{
				throw new ArgumentNullException("serializationStream");
			}
			if(serializationStream.CanSeek &&
				serializationStream.Length==0) 
			{
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}

			BinaryReader reader = new BinaryReader (serializationStream);

			bool hasHeader;
			ReadBinaryHeader (reader, out hasHeader);

			// Messages are read using a special static method, which does not use ObjectReader
			// if it is not needed. This saves time and memory.

			BinaryElement elem = (BinaryElement) reader.Read ();

			if (elem == BinaryElement.MethodCall) {
				return MessageFormatter.ReadMethodCall (elem, reader, hasHeader, handler, this);
			}
			else if (elem == BinaryElement.MethodResponse) {
				return MessageFormatter.ReadMethodResponse (elem, reader, hasHeader, handler, null, this);
			}
			else {
				ObjectReader serializer = new ObjectReader (this);

				object result;
				Header[] headers;
				serializer.ReadObjectGraph (elem, reader, hasHeader, out result, out headers);
				if (handler != null) handler(headers);
				return result;
			}
		}
		
		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public object DeserializeMethodResponse (Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
		{
			return NoCheckDeserializeMethodResponse (serializationStream, handler, methodCallMessage);
		}

		// shared by DeserializeMethodResponse and UnsafeDeserializeMethodResponse which both involve different security checks
		private object NoCheckDeserializeMethodResponse (Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException("serializationStream");
			}
			if(serializationStream.CanSeek &&
			   serializationStream.Length==0) {
				throw new SerializationException("serializationStream supports seeking, but its length is 0");
			}

			BinaryReader reader = new BinaryReader (serializationStream);

			bool hasHeader;
			ReadBinaryHeader (reader, out hasHeader);
			return MessageFormatter.ReadMethodResponse (reader, hasHeader, handler, methodCallMessage, this);
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			Serialize (serializationStream, graph, null);
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public void Serialize(Stream serializationStream, object graph, Header[] headers)
		{
			if(serializationStream==null) {
				throw new ArgumentNullException ("serializationStream");
			}

			BinaryWriter writer = new BinaryWriter (serializationStream);
			WriteBinaryHeader (writer, headers!=null);

			if (graph is IMethodCallMessage) {
				MessageFormatter.WriteMethodCall (writer, graph, headers, this);
			}
			else if (graph is IMethodReturnMessage)  {
				MessageFormatter.WriteMethodResponse (writer, graph, headers, this);
			}
			else {
				ObjectWriter serializer = new ObjectWriter (this);
				serializer.WriteObjectGraph (writer, graph, headers);
			}
			writer.Flush();
		}

		// faster version (under CAS) as this requires a LinkDemand versus full Demand (i.e. a stack-walk)
		// shouldn't be called unless the code is intended to be executed at full-trust
		[ComVisible (false)]
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public object UnsafeDeserialize (Stream serializationStream, HeaderHandler handler) 
		{
			return NoCheckDeserialize (serializationStream, handler);
		}
		
		// faster version (under CAS) as this requires a LinkDemand versus full Demand (i.e. a stack-walk)
		// shouldn't be called unless the code is intended to be executed at full-trust
		[ComVisible (false)]
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public object UnsafeDeserializeMethodResponse (Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
		{
			return NoCheckDeserializeMethodResponse (serializationStream, handler, methodCallMessage);
		}
		
		private void WriteBinaryHeader (BinaryWriter writer, bool hasHeaders)
		{
			writer.Write ((byte)BinaryElement.Header);
			writer.Write ((int)1);
			if (hasHeaders) writer.Write ((int)2);
			else writer.Write ((int)-1);
			writer.Write ((int)1);
			writer.Write ((int)0);
		}

		private void ReadBinaryHeader (BinaryReader reader, out bool hasHeaders)
		{
			reader.ReadByte();
			reader.ReadInt32();
			int val = reader.ReadInt32();
			hasHeaders = (val==2);
			reader.ReadInt32();
			reader.ReadInt32();
		}
	}
}
