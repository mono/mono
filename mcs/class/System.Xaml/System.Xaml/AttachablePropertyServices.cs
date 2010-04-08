using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xaml
{
	public static class AttachablePropertyServices
	{
		public static void CopyPropertiesTo (object instance, KeyValuePair<AttachableMemberIdentifier,object> [] array, int index)
		{
			throw new NotImplementedException ();
		}

		public static int GetAttachedPropertyCount (object instance)
		{
			throw new NotImplementedException ();
		}

		public static bool RemoveProperty (object instance, AttachableMemberIdentifier name)
		{
			throw new NotImplementedException ();
		}

		public static void SetProperty (object instance, AttachableMemberIdentifier name, object value)
		{
			throw new NotImplementedException ();
		}

		public static bool TryGetProperty (object instance, AttachableMemberIdentifier name, out object value)
		{
			throw new NotImplementedException ();
		}

		public static bool TryGetProperty<T> (object instance, AttachableMemberIdentifier name, out T value)
		{
			throw new NotImplementedException ();
		}
	}
}
