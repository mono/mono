//
// System.Runtime.Serialization.FormatterServices
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.Serialization
{
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

				FieldInfo fi = member as FieldInfo; //FIXME: Can fi be null?
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

			if (!type.IsSerializable) {
				string msg = String.Format ("Type {0} in assembly {1} is not marked as serializable.",
							    type, type.Assembly.FullName);
				throw new SerializationException (msg);
			}

			//FIXME: context?
			ArrayList fields = new ArrayList ();
			Type t = type;
			while (t != null) {
				GetFields (t, fields);
				t = t.BaseType;
			}

			MemberInfo [] result = new MemberInfo [fields.Count];
			fields.CopyTo (result);
			return result;
		}

		private static void GetFields (Type type, ArrayList fields)
		{
			FieldInfo [] fs = type.GetFields (fieldFlags);
			foreach (FieldInfo field in fs)
				if (!(field.IsNotSerialized))
					fields.Add (field);
		}

		public static Type GetTypeFromAssembly (Assembly assem, string name)
		{
			if (assem == null)
				throw new ArgumentNullException ("assem");

			if (name == null)
				throw new ArgumentNullException ("name");

			return assem.GetType (name);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern object GetUninitializedObjectInternal (Type type);

		public static object GetUninitializedObject (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (string))
				throw new ArgumentException ("Uninitialized Strings cannot be created.");

			return GetUninitializedObjectInternal (type);
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

				FieldInfo fi = member as FieldInfo; //FIXME: can fi be null?
				fi.SetValue (obj, data [i]);
			}

			return obj;
		}
	}
}

