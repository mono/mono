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
		bool _includeVersions;
		bool _strictBinding;
		
		public static SoapCore DefaultInstance = new SoapCore (true, false);
		
		public SoapCore (object owner, IDictionary properties)
		{
			_includeVersions = true;
			_strictBinding = false;
			
			foreach(DictionaryEntry property in properties)
			{
				switch((string)property.Key)
				{
					case "includeVersions": 
						_includeVersions = Convert.ToBoolean (property.Value);
						break;
						
					case "strictBinding":
						_strictBinding = Convert.ToBoolean (property.Value);
						break;
						
					default:
						throw new RemotingException (owner.GetType().Name + " does not recognize '" + property.Key + "' configuration property");
				}
			}
			
			Init ();
		}
		
		public SoapCore (bool includeVersions, bool strictBinding)
		{
			_includeVersions = includeVersions;
			_strictBinding = strictBinding;
			Init ();
		}
		
		public void Init ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

			_serializationFormatter = CreateFormatter (surrogateSelector, context);
			_deserializationFormatter = CreateFormatter (null, context);
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

