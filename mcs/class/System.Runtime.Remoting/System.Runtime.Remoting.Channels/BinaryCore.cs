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
		
		public BinaryCore (object owner, IDictionary properties)
		{
			bool includeVersions = true;
			bool strictBinding = false;
			
			foreach(DictionaryEntry property in properties)
			{
				switch((string)property.Key)
				{
					case "includeVersions": 
						includeVersions = Convert.ToBoolean (property.Value);
						break;
						
					case "strictBinding":
						strictBinding = Convert.ToBoolean (property.Value);
						break;
						
					default:
						throw new RemotingException (owner.GetType().Name + " does not recognize '" + property.Key + "' configuration property");
				}
			}
			
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

