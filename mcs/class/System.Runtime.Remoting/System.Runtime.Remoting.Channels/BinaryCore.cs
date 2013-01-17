//
// System.Runtime.Remoting.Channels.BinaryCore.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
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
		IDictionary _properties;
		
		TypeFilterLevel _filterLevel = TypeFilterLevel.Low;
		
		public static BinaryCore DefaultInstance = new BinaryCore ();
		
		public BinaryCore (object owner, IDictionary properties, string[] allowedProperties)
		{
			_properties = properties;
			
			if (_properties == null)
			{
				_properties = new Hashtable(10);
			}
			
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
						
					case "typeFilterLevel":
						if (property.Value is TypeFilterLevel)
							_filterLevel = (TypeFilterLevel) property.Value;
						else {
							string s = (string) property.Value;
							_filterLevel = (TypeFilterLevel) Enum.Parse (typeof(TypeFilterLevel), s);
						}
						break;
						
				}
			}
			
			Init ();
		}
		
		public BinaryCore ()
		{
			_properties = new Hashtable ();
			Init ();
		}
		
		public void Init ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);

#if !TARGET_JVM
			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
#else
			_serializationFormatter = (BinaryFormatter) vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (surrogateSelector, context, false);
			_deserializationFormatter = (BinaryFormatter) vmw.@internal.remoting.BinaryFormatterUtils.CreateBinaryFormatter (null, context, false);
#endif
			
			_serializationFormatter.FilterLevel = _filterLevel;
			_deserializationFormatter.FilterLevel = _filterLevel;
			
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
		
		public IDictionary Properties
		{
			get { return _properties; }
		}
		
		public TypeFilterLevel TypeFilterLevel
		{
			get { return _filterLevel; }
		}
	}
}

