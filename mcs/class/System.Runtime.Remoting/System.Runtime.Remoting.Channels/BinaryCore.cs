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
		bool _includeVersions = true;
		bool _strictBinding = false;
		
#if NET_1_1
		TypeFilterLevel _filterLevel = TypeFilterLevel.Low;
#endif
		
		public static BinaryCore DefaultInstance = new BinaryCore ();
		
		public BinaryCore (object owner, IDictionary properties, string[] allowedProperties)
		{
			foreach(DictionaryEntry property in properties)
			{
				string key = (string) property.Key;
				if (Array.IndexOf (allowedProperties, key) == -1)
					throw new RemotingException (owner.GetType().Name + " does not recognize '" + key + "' configuration property");
				
				switch (key)
				{
					case "includeVersions": 
						_includeVersions = Convert.ToBoolean (property.Value);
						break;
						
					case "strictBinding":
						_strictBinding = Convert.ToBoolean (property.Value);
						break;
						
#if NET_1_1
					case "typeFilterLevel":
						if (property.Value is TypeFilterLevel)
							_filterLevel = (TypeFilterLevel) property.Value;
						else {
							string s = (string) property.Value;
							_filterLevel = (TypeFilterLevel) Enum.Parse (typeof(TypeFilterLevel), s);
						}
						break;
#endif
						
				}
			}
			
			Init ();
		}
		
		public BinaryCore ()
		{
			Init ();
		}
		
		public void Init ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
			
#if NET_1_1
			_serializationFormatter.FilterLevel = _filterLevel;
			_deserializationFormatter.FilterLevel = _filterLevel;
#endif
			
			if (!_includeVersions)
			{
				_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
				_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			}

			if (!_strictBinding)
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

