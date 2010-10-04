//
// DataContractResolver.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Xml;

namespace System.Runtime.Serialization
{
	// See http://msdn.microsoft.com/en-us/library/ee358759.aspx
#if NET_4_0
	public
#else
	internal
#endif
	abstract class DataContractResolver
	{
		public abstract Type ResolveName (string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver);

		public abstract bool TryResolveType (Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace);
	}

	internal class DefaultDataContractResolver : DataContractResolver
	{
		public DefaultDataContractResolver (DataContractSerializer serializer)
		{
			this.serializer = serializer;
		}

		DataContractSerializer serializer;
		XmlDictionary dictionary;

		public override Type ResolveName (string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			var map = serializer.InternalKnownTypes.FindUserMap (new XmlQualifiedName (typeName, typeNamespace));
			if (map == null)
				serializer.InternalKnownTypes.Add (declaredType);
			if (map != null)
				return map.RuntimeType;
			return null;
		}

		public override bool TryResolveType (Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			var map = serializer.InternalKnownTypes.FindUserMap (type);
			if (map == null) {
				typeName = null;
				typeNamespace = null;
				return false;
			} else {
				dictionary = dictionary ?? new XmlDictionary ();
				typeName = dictionary.Add (map.XmlName.Name);
				typeNamespace = dictionary.Add (map.XmlName.Namespace);
				return true;
			}
		}
	}
}
