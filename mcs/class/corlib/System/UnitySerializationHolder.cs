// UnitySerializationHolder.cs
//
// Author:
//  Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2003 Lluis Sanchez Gual

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	internal class UnitySerializationHolder: IObjectReference, ISerializable
	{
		string _data;
		UnityType _unityType;
		string _assemblyName;

		// FIXME: there must be other types that use UnitySerializationHolder for
		// serialization, but I don't know yet which ones.

		enum UnityType: byte { DBNull = 2, Type = 4, Assembly = 6 }

		UnitySerializationHolder(SerializationInfo info, StreamingContext ctx)
		{
			_data = info.GetString("Data");
			_unityType = (UnityType) info.GetInt32("UnityType");
			_assemblyName = info.GetString("AssemblyName");
		}

		public static void GetTypeData(Type instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", instance.FullName);
			info.AddValue ("UnityType", (int) UnityType.Type);
			info.AddValue ("AssemblyName", instance.Assembly.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public static void GetDBNullData(DBNull instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", null);
			info.AddValue ("UnityType", (int) UnityType.DBNull);
			info.AddValue ("AssemblyName", instance.GetType().Assembly.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public static void GetAssemblyData(Assembly instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", instance.FullName);
			info.AddValue ("UnityType", (int) UnityType.Assembly);
			info.AddValue ("AssemblyName", instance.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Not needed.
			throw new NotSupportedException();
		}

		public virtual object GetRealObject(StreamingContext context)
		{
			switch (_unityType)
			{
				case UnityType.Type:
					Assembly assembly = Assembly.Load (_assemblyName);
					return assembly.GetType (_data);

				case UnityType.DBNull:
					return DBNull.Value;

				case UnityType.Assembly:
					return Assembly.Load (_data);

				default:
					throw new NotSupportedException ("UnitySerializationHolder does not support this type");
			}
		}
	}
}
