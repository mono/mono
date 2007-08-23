// MemberInfoSerializationHolder.cs.cs
//
// Author:
//  Patrik Torstensson
//  Robert Jordan <robertj@gmx.net>
//
// (C) 2003 Patrik Torstensson

//
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal class MemberInfoSerializationHolder : IObjectReference, ISerializable
	{
		const BindingFlags DefaultBinding = BindingFlags.Instance | BindingFlags.Static |
			BindingFlags.Public | BindingFlags.NonPublic;

		readonly string		_memberName;
		readonly string		_memberSignature;
		readonly MemberTypes	_memberType;
		readonly Type		_reflectedType;
#if NET_2_0
		readonly Type[]          _genericArguments;
#endif
		MemberInfoSerializationHolder(SerializationInfo info, StreamingContext ctx)
		{
			string assemblyName;
			string typeName;

			assemblyName = info.GetString("AssemblyName");
			typeName = info.GetString("ClassName");

			_memberName = info.GetString("Name");
			_memberSignature = info.GetString("Signature");
			_memberType = (MemberTypes) info.GetInt32("MemberType");

#if NET_2_0
			try {
				_genericArguments = null;

				// FIXME: this doesn't work at present. It seems that
				// ObjectManager doesn't cope with nested IObjectReferences.
				// _genericArguments = (Type[]) info.GetValue("GenericArguments", typeof(Type[]));
			} catch (SerializationException) {
				// expected (old NET_1_0 protocol)
			}
#endif
			// Load type
			Assembly asm = Assembly.Load(assemblyName);

			_reflectedType = asm.GetType(typeName, true, true);
		}

		public static void Serialize(SerializationInfo info, String name, Type klass, String signature, MemberTypes type)
		{
			Serialize (info, name, klass, signature, type, null);
		}

#if NET_2_0
		public
#endif
		static void Serialize(SerializationInfo info, String name, Type klass, String signature, MemberTypes type, Type[] genericArguments)
		{
			info.SetType( typeof(MemberInfoSerializationHolder));

			info.AddValue("AssemblyName", klass.Module.Assembly.FullName, typeof(String));
			info.AddValue("ClassName", klass.FullName, typeof(String));

			info.AddValue("Name", name, typeof(String));
			info.AddValue("Signature", signature, typeof(String));
			info.AddValue("MemberType",(int)type);
#if NET_2_0
			info.AddValue("GenericArguments", genericArguments, typeof (Type[]));
#endif
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException();
		}

		public object GetRealObject(StreamingContext context)
		{
			switch (_memberType) 
			{
				case MemberTypes.Constructor:
					ConstructorInfo [] ctors;

					ctors = _reflectedType.GetConstructors (DefaultBinding);
					for (int i = 0; i < ctors.Length; i++) 
						if ( ctors[i].ToString().Equals(_memberSignature)) 
							return ctors[i];

					throw new SerializationException (String.Format ("Could not find constructor '{0}' in type '{1}'", _memberSignature, _reflectedType));

				case MemberTypes.Method:
					MethodInfo [] methods;

					methods = _reflectedType.GetMethods(DefaultBinding);
					for (int i = 0; i < methods.Length; i++) 
						if ((methods[i]).ToString().Equals(_memberSignature)) 
							return methods[i];
#if NET_2_0
						else if (_genericArguments != null &&
							methods[i].IsGenericMethod &&
							methods[i].GetGenericArguments().Length == _genericArguments.Length) {

							MethodInfo mi = methods[i].MakeGenericMethod(_genericArguments);

							if (mi.ToString() == _memberSignature)
								return mi;
						}
#endif

					throw new SerializationException (String.Format ("Could not find method '{0}' in type '{1}'", _memberSignature, _reflectedType));

				case MemberTypes.Field:
					FieldInfo fi = _reflectedType.GetField (_memberName, DefaultBinding);

					if (fi != null)
						return fi;

					throw new SerializationException (String.Format ("Could not find field '{0}' in type '{1}'", _memberName, _reflectedType));

				case MemberTypes.Property:
					PropertyInfo pi = _reflectedType.GetProperty (_memberName, DefaultBinding);

					if (pi != null)
						return pi;

					throw new SerializationException (String.Format ("Could not find property '{0}' in type '{1}'", _memberName, _reflectedType));

#if NET_2_0
				case MemberTypes.Event:
					EventInfo ei = _reflectedType.GetEvent (_memberName, DefaultBinding);

					if (ei != null)
						return ei;

					throw new SerializationException (String.Format ("Could not find event '{0}' in type '{1}'", _memberName, _reflectedType));
#endif

				default:
					throw new SerializationException (String.Format ("Unhandled MemberType {0}",  _memberType));
			}
		}
	}
}
