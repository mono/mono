//
// System.UnitySerializationHolder.cs
//
// Author:
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2003 Lluis Sanchez Gual

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	internal class UnitySerializationHolder : IObjectReference, ISerializable
	{
		string _data;
		UnityType _unityType;
		string _assemblyName;

		// FIXME: there must be other types that use UnitySerializationHolder for
		// serialization, but I don't know yet which ones.

		enum UnityType: byte
		{
			DBNull = 2,
			Type = 4,
			Module = 5,
			Assembly = 6
		}

		UnitySerializationHolder (SerializationInfo info, StreamingContext ctx)
		{
			_data = info.GetString ("Data");
			_unityType = (UnityType) info.GetInt32 ("UnityType");
			_assemblyName = info.GetString ("AssemblyName");
		}

		public static void GetTypeData (Type instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", instance.FullName);
			info.AddValue ("UnityType", (int) UnityType.Type);
			info.AddValue ("AssemblyName", instance.Assembly.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public static void GetDBNullData (DBNull instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", null);
			info.AddValue ("UnityType", (int) UnityType.DBNull);
			info.AddValue ("AssemblyName", instance.GetType().Assembly.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public static void GetAssemblyData (Assembly instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", instance.FullName);
			info.AddValue ("UnityType", (int) UnityType.Assembly);
			info.AddValue ("AssemblyName", instance.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public static void GetModuleData (Module instance, SerializationInfo info, StreamingContext ctx)
		{
			info.AddValue ("Data", instance.ScopeName);
			info.AddValue ("UnityType", (int) UnityType.Module);
			info.AddValue ("AssemblyName", instance.Assembly.FullName);
			info.SetType (typeof (UnitySerializationHolder));
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// Not needed.
			throw new NotSupportedException();
		}

		public virtual object GetRealObject (StreamingContext context)
		{
			switch (_unityType) {
			case UnityType.Type: {
				Assembly assembly = Assembly.Load (_assemblyName);
				return assembly.GetType (_data);
			}
			case UnityType.DBNull:
				return DBNull.Value;
			case UnityType.Module: {
				Assembly assembly = Assembly.Load (_assemblyName);
				return assembly.GetModule (_data);
			}
			case UnityType.Assembly:
				return Assembly.Load (_data);
			default:
				throw new NotSupportedException (Locale.GetText
					("UnitySerializationHolder does not support this type."));
			}
		}
	}
}
