//
// System.Runtime.Serialization.FormatterServices
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

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
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Globalization;

namespace System.Runtime.Serialization
{
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
	public sealed class FormatterServices
	{
		private const BindingFlags fieldFlags = BindingFlags.Public |
							BindingFlags.Instance |
							BindingFlags.NonPublic |
							BindingFlags.DeclaredOnly;

		private FormatterServices ()
		{
		}

		public static object [] GetObjectData (object obj, MemberInfo [] members)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (members == null)
				throw new ArgumentNullException ("members");

			int n = members.Length;
			object [] result = new object [n];
			for (int i = 0; i < n; i++) {
				MemberInfo member = members [i];
				if (member == null)
					throw new ArgumentNullException (String.Format ("members[{0}]", i));

				if (member.MemberType != MemberTypes.Field)
					throw new SerializationException (
							String.Format ("members [{0}] is not a field.", i));

				FieldInfo fi = member as FieldInfo; // members must be fields
				result [i] = fi.GetValue (obj);
			}

			return result;
		}

		public static MemberInfo [] GetSerializableMembers (Type type)
		{
			StreamingContext st = new StreamingContext (StreamingContextStates.All);
			return GetSerializableMembers (type, st);
		}

		public static MemberInfo [] GetSerializableMembers (Type type, StreamingContext context)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			//FIXME: context?
			ArrayList fields = new ArrayList ();
			Type t = type;
			while (t != null) {
				if (!t.IsSerializable) {
					string msg = String.Format ("Type {0} in assembly {1} is not " +
								    "marked as serializable.",
								    t, t.Assembly.FullName);

					throw new SerializationException (msg);
				}

				GetFields (type, t, fields);
				t = t.BaseType;
			}

			MemberInfo [] result = new MemberInfo [fields.Count];
			fields.CopyTo (result);
			return result;
		}

		private static void GetFields (Type reflectedType, Type type, ArrayList fields)
		{
			FieldInfo [] fs = type.GetFields (fieldFlags);
			foreach (FieldInfo field in fs)
				if (!(field.IsNotSerialized)) {
					MonoField mf = field as MonoField;
					if (mf != null && reflectedType != type && !mf.IsPublic) {
						string fname = type.Name + "+" + mf.Name;
						fields.Add (mf.Clone (fname));
					}
					else
						fields.Add (field);
				}
		}

		public static Type GetTypeFromAssembly (Assembly assem, string name)
		{
			if (assem == null)
				throw new ArgumentNullException ("assem");

			if (name == null)
				throw new ArgumentNullException ("name");

			return assem.GetType (name);
		}

		public static object GetUninitializedObject (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (string))
				throw new ArgumentException ("Uninitialized Strings cannot be created.");

			return System.Runtime.Remoting.Activation.ActivationServices.AllocateUninitializedClassInstance (type);
		}

		public static object PopulateObjectMembers (object obj, MemberInfo [] members, object [] data)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (members == null)
				throw new ArgumentNullException ("members");

			if (data == null)
				throw new ArgumentNullException ("data");

			int length = members.Length;
			if (length != data.Length)
				throw new ArgumentException ("different length in members and data");

			for (int i = 0; i < length; i++) {
				MemberInfo member = members [i];
				if (member == null)
					throw new ArgumentNullException (String.Format ("members[{0}]", i));
					
				if (member.MemberType != MemberTypes.Field)
					throw new SerializationException (
							String.Format ("members [{0}] is not a field.", i));

				FieldInfo fi = member as FieldInfo; // members must be fields
				fi.SetValue (obj, data [i]);
			}

			return obj;
		}
		

		public static void CheckTypeSecurity (Type t, TypeFilterLevel securityLevel)
		{
			if (securityLevel == TypeFilterLevel.Full) return;
			CheckNotAssignable (typeof(System.DelegateSerializationHolder), t);
			CheckNotAssignable (typeof(System.Runtime.Remoting.Lifetime.ISponsor), t);
			CheckNotAssignable (typeof(System.Runtime.Remoting.IEnvoyInfo), t);
			CheckNotAssignable (typeof(System.Runtime.Remoting.ObjRef), t);
		}
		
		static void CheckNotAssignable (Type basetype, Type type)
		{
			if (basetype.IsAssignableFrom (type)) {
				string msg = "Type " + basetype + " and the types derived from it";
				msg += " (such as " + type + ") are not permitted to be deserialized at this security level";
				throw new System.Security.SecurityException (msg);
			}
		}

		public static object GetSafeUninitializedObject (Type type)
		{
			// FIXME: MS.NET uses code access permissions to check if the caller is
			// allowed to create an instance of this type. We can't support this
			// because it is not implemented in mono.
			
			// In concrete, the it will request a SecurityPermission of 
			// type "Infrastructure".
			
			return GetUninitializedObject (type);
		}

#if NET_4_0
		// This method was introduced in .Net due to a bug serializing objects with circular references
		// which we don't appear to have, so we just return the same object.
		// See http://support.microsoft.com/kb/927495/en-us/ in case of doubt.
		[ComVisible (false)]
		public static ISerializationSurrogate GetSurrogateForCyclicalReference (ISerializationSurrogate innerSurrogate)
		{
			return innerSurrogate;
		}
#endif
	}
}
