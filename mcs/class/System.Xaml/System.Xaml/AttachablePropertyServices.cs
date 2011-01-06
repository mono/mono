using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xaml
{
	public static class AttachablePropertyServices
	{
		class Table : Dictionary<AttachableMemberIdentifier,object>
		{
		}

		static Dictionary<object,Table> props = new Dictionary<object,Table> ();

		public static void CopyPropertiesTo (object instance, KeyValuePair<AttachableMemberIdentifier,object> [] array, int index)
		{
			Table t;
			if (instance == null || !props.TryGetValue (instance, out t))
				return;
			((ICollection<KeyValuePair<AttachableMemberIdentifier,object>>) t).CopyTo (array, index);
		}

		public static int GetAttachedPropertyCount (object instance)
		{
			Table t;
			return instance != null && props.TryGetValue (instance, out t) ? t.Count : 0;
		}

		public static bool RemoveProperty (object instance, AttachableMemberIdentifier name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Table t;
			return instance != null && props.TryGetValue (instance, out t) ? t.Remove (name) : false;
		}

		public static void SetProperty (object instance, AttachableMemberIdentifier name, object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Table t;
			if (!props.TryGetValue (instance, out t)) {
				t = new Table ();
				props [instance] = t;
			}
			t [name] = value;
		}

		public static bool TryGetProperty (object instance, AttachableMemberIdentifier name, out object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Table t;
			value = null;
			return instance != null && props.TryGetValue (instance, out t) ? t.TryGetValue (name, out value) : false;
		}

		public static bool TryGetProperty<T> (object instance, AttachableMemberIdentifier name, out T value)
		{
			object ret;
			if (!TryGetProperty (instance, name, out ret)) {
				value = default (T);
				return false;
			}
			value = (T) ret;
			return true;
		}
	}
}
