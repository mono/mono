//
// System.Runtime.Serialization.SerializationCallbacks.cs
//
// Author:
//   Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.Reflection;
       
namespace System.Runtime.Serialization {

	internal sealed class SerializationCallbacks
	{
		public delegate void CallbackHandler (StreamingContext context);

		readonly ArrayList onSerializingList;
		readonly ArrayList onSerializedList;
		readonly ArrayList onDeserializingList;
		readonly ArrayList onDeserializedList;

		public bool HasSerializingCallbacks {
			get {return onSerializingList != null;}
		}

		public bool HasSerializedCallbacks {
			get {return onSerializedList != null;}
		}

		public bool HasDeserializingCallbacks {
			get {return onDeserializingList != null;}
		}

		public bool HasDeserializedCallbacks {
			get {return onDeserializedList != null;}
		}

		public SerializationCallbacks (Type type)
		{
			onSerializingList   = GetMethodsByAttribute (type, typeof (OnSerializingAttribute));
			onSerializedList    = GetMethodsByAttribute (type, typeof (OnSerializedAttribute));
			onDeserializingList = GetMethodsByAttribute (type, typeof (OnDeserializingAttribute));
			onDeserializedList  = GetMethodsByAttribute (type, typeof (OnDeserializedAttribute));
		}

		const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.DeclaredOnly;


		static ArrayList GetMethodsByAttribute (Type type, Type attr)
		{
			ArrayList list = new ArrayList ();

			Type t = type;
			while (t != typeof (object)) {
				int count = 0;

				foreach (MethodInfo mi in t.GetMethods (DefaultBindingFlags)) {
					if (mi.IsDefined (attr, false)) {
						list.Add (mi);
						count++;
					}
				}

				// FIXME: MS.NET is checking for this with the verifier at assembly load time.
				if (count > 1)
					throw new TypeLoadException (
						String.Format ("Type '{0}' has more than one method with the following attribute: '{1}'.", type.AssemblyQualifiedName, attr.FullName));

				t = t.BaseType;
			}

			// optimize memory usage
			return list.Count == 0 ? null : list;
		}

		static void Invoke (ArrayList list, object target, StreamingContext context)
		{
			if (list == null)
				return;

			CallbackHandler handler = null;

			// construct a delegate from the specified list
			foreach (MethodInfo mi in list) {
				handler = (CallbackHandler)
					Delegate.Combine (
						Delegate.CreateDelegate (typeof (CallbackHandler), target, mi),
						handler);
			}

			handler (context);
		}

		public void RaiseOnSerializing (object target, StreamingContext contex)
		{
			Invoke (onSerializingList, target, contex);
		}

		public void RaiseOnSerialized (object target, StreamingContext contex)
		{
			Invoke (onSerializedList, target, contex);
		}

		public void RaiseOnDeserializing (object target, StreamingContext contex)
		{
			Invoke (onDeserializingList, target, contex);
		}

		public void RaiseOnDeserialized (object target, StreamingContext contex)
		{
			Invoke (onDeserializedList, target, contex);
		}

		static Hashtable cache = new Hashtable ();
		static object cache_lock = new object ();
		
		public static SerializationCallbacks GetSerializationCallbacks (Type t)
		{
			SerializationCallbacks sc = (SerializationCallbacks) cache [t];
			if (sc != null)
				return sc;

			// Slow path, new entry, we need to copy
			lock (cache_lock){
				sc = (SerializationCallbacks)  cache [t];
				if (sc == null) {
					Hashtable copy = (Hashtable) cache.Clone ();
				
					sc = new SerializationCallbacks (t);
					copy [t] = sc;
					cache = copy;
				}
				return sc;
			}
		}
	}
}

#endif
