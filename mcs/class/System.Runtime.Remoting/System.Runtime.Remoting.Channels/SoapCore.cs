//
// System.Runtime.Remoting.Channels.SoapCore.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;

namespace System.Runtime.Remoting.Channels
{
	internal class SoapCore
	{
		SoapFormatter _serializationFormatter;
		SoapFormatter _deserializationFormatter;
		bool _includeVersions = true;
		bool _strictBinding = false;
		
#if NET_1_1
		TypeFilterLevel _filterLevel = TypeFilterLevel.Low;
#endif

		public static SoapCore DefaultInstance = new SoapCore ();
		
		public SoapCore (object owner, IDictionary properties, string[] allowedProperties)
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
		
		public SoapCore ()
		{
			Init ();
		}
		
		public void Init ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

			_serializationFormatter = CreateFormatter (surrogateSelector, context);
			_deserializationFormatter = CreateFormatter (null, context);

#if NET_1_1
			_serializationFormatter.FilterLevel = _filterLevel;
			_deserializationFormatter.FilterLevel = _filterLevel;
#endif
		}
		
		SoapFormatter CreateFormatter (ISurrogateSelector selector, StreamingContext context)
		{
			SoapFormatter fm = new SoapFormatter (selector, context);
			
			if (!_includeVersions)
				fm.AssemblyFormat = FormatterAssemblyStyle.Simple;
			
			if (!_strictBinding)
				fm.Binder = ChannelCore.SimpleBinder;
				
			return fm;
		}
		
		public SoapFormatter GetSafeDeserializer ()
		{
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);
			return CreateFormatter (null, context);
		}
		
		public SoapFormatter Serializer
		{
			get { return _serializationFormatter; }
		}
		
		public SoapFormatter Deserializer
		{
			get { return _deserializationFormatter; }
		}
	}
}

