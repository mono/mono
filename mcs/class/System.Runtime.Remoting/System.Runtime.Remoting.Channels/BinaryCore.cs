//
// System.Runtime.Remoting.Channels.BinaryCore.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels
{
	internal class BinaryCore
	{
		BinaryFormatter _serializationFormatter;
		BinaryFormatter _deserializationFormatter;
		
		public static BinaryCore DefaultInstance = new BinaryCore (true, false);
		
		public BinaryCore (IDictionary properties)
		{
			bool includeVersions = true;
			bool strictBinding = false;
			
			object val = properties ["includeVersions"];
			if (val != null) includeVersions = Convert.ToBoolean (val);
			
			val = properties ["strictBinding"];
			if (val != null) strictBinding = Convert.ToBoolean (val);
			
			Init (includeVersions, strictBinding);
		}
		
		public BinaryCore (bool includeVersions, bool stringBinding)
		{
			Init (includeVersions, stringBinding);
		}
		
		public void Init (bool includeVersions, bool stringBinding)
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
			
			if (!includeVersions)
			{
				_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
				_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			}

			if (!stringBinding)
			{
				_serializationFormatter.Binder = ChannelCore.SimpleBinder;
				_deserializationFormatter.Binder = ChannelCore.SimpleBinder;
			}
		}
		
		public BinaryFormatter Serializer
		{
			get { return _serializationFormatter; }
		}
		
		public BinaryFormatter Deserializer
		{
			get { return _deserializationFormatter; }
		}
	}
}

