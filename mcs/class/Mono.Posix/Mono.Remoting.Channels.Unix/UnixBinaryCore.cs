//
// Mono.Remoting.Channels.Unix.BinaryCore.cs
//
// Author: Lluis Sanchez Gual (lluis@novell.com)
//
// 2005 (C) Copyright, Novell, Inc.
//

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
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixBinaryCore
	{
		BinaryFormatter _serializationFormatter;
		BinaryFormatter _deserializationFormatter;
		bool _includeVersions = true;
		bool _strictBinding = false;
		IDictionary _properties;
		
		public static UnixBinaryCore DefaultInstance = new UnixBinaryCore ();
		
		public UnixBinaryCore (object owner, IDictionary properties, string[] allowedProperties)
		{
			_properties = properties;
			
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
				}
			}
			
			Init ();
		}
		
		public UnixBinaryCore ()
		{
			_properties = new Hashtable ();
			Init ();
		}
		
		public void Init ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
			
			if (!_includeVersions)
			{
				_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
				_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			}

			if (!_strictBinding)
			{
				_serializationFormatter.Binder = SimpleBinder.Instance;
				_deserializationFormatter.Binder = SimpleBinder.Instance;
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
		
		public IDictionary Properties
		{
			get { return _properties; }
		}
	}
	
	
	internal class SimpleBinder: SerializationBinder
	{
		public static SimpleBinder Instance = new SimpleBinder ();
		 
		public override Type BindToType (String assemblyName, string typeName)
		{
			Assembly asm;
			
			if (assemblyName.IndexOf (',') != -1)
			{
				// Try using the full name
				try
				{
					asm = Assembly.Load (assemblyName);
					if (asm == null) return null;
					Type t = asm.GetType (typeName);
					if (t != null) return t;
				}
				catch {}
			}
			
			// Try using the simple name
			asm = Assembly.LoadWithPartialName (assemblyName);
			if (asm == null) return null;
			return asm.GetType (typeName, true);
		}
	}
}

