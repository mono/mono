// ReflectionSerializationHolder.cs.cs
//
// Author:
//  Patrik Torstensson
//
// (C) 2003 Patrik Torstensson

using System;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal class ReflectionSerializationHolder : IObjectReference, ISerializable
	{
		string		_memberName;
		string		_memberSignature;
		MemberTypes	_memberType;
		Type		_reflectedType;

		ReflectionSerializationHolder(SerializationInfo info, StreamingContext ctx)
		{
			string assemblyName;
			string typeName;

			assemblyName = info.GetString("AssemblyName");
			typeName = info.GetString("ClassName");

			_memberName = info.GetString("Name");
			_memberSignature = info.GetString("Signature");
			_memberType = (MemberTypes) info.GetInt32("MemberType");

			// Load type
			Assembly asm = Assembly.Load(assemblyName);

			_reflectedType = asm.GetType(typeName, true, true);
		}

		public static void Serialize(SerializationInfo info, String name, Type klass, String signature, MemberTypes type)
		{
			info.SetType( typeof(ReflectionSerializationHolder));

			info.AddValue("AssemblyName", klass.Module.Assembly.FullName, typeof(String));
			info.AddValue("ClassName", klass.FullName, typeof(String));

			info.AddValue("Name", name, typeof(String));
			info.AddValue("Signature", signature, typeof(String));
			info.AddValue("MemberType",(int)type);
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

					ctors = _reflectedType.GetConstructors (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
					for (int i = 0; i < ctors.Length; i++) 
						if ( ctors[i].ToString().Equals(_memberSignature)) 
							return ctors[i];

					throw new SerializationException("Failed to find serialized constructor");

				case MemberTypes.Method:
					MethodInfo [] methods;

					methods = _reflectedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
					for (int i = 0; i < methods.Length; i++) 
						if ((methods[i]).ToString().Equals(_memberSignature)) 
							return methods[i];

					throw new SerializationException("Failed to find serialized method");
				default:
					throw new SerializationException("Failed to get object for member type " + _memberType.ToString());
			}
		}
	}
}
