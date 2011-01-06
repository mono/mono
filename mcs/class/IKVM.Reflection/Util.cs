/*
  Copyright (C) 2008-2010 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	public interface ICustomAttributeProvider
	{
		bool IsDefined(Type attributeType, bool inherit);
		IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit);
	}

	[Serializable]
	public sealed class FileFormatLimitationExceededException : InvalidOperationException
	{
		public const int META_E_STRINGSPACE_FULL = unchecked((int)0x80131198);

		public FileFormatLimitationExceededException(string message, int hresult)
			: base(message)
		{
			this.HResult = hresult;
		}

		private FileFormatLimitationExceededException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		public int ErrorCode
		{
			get { return this.HResult; }
		}
	}

	[Serializable]
	public sealed class Missing : ISerializable
	{
		public static readonly Missing Value = new Missing();

		private Missing() { }

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof(SingletonSerializationHelper));
		}

		[Serializable]
		private sealed class SingletonSerializationHelper : IObjectReference
		{
			public object GetRealObject(StreamingContext context)
			{
				return Value;
			}
		}
	}

	static class Empty<T>
	{
		internal static readonly T[] Array = new T[0];
	}

	static class Util
	{
		internal static Type[] Copy(Type[] array)
		{
			if (array == null || array.Length == 0)
			{
				return Type.EmptyTypes;
			}
			Type[] copy = new Type[array.Length];
			Array.Copy(array, copy, array.Length);
			return copy;
		}

		internal static Type[][] Copy(Type[][] types)
		{
			if (types == null || types.Length == 0)
			{
				return types;
			}
			Type[][] newArray = new Type[types.Length][];
			for (int i = 0; i < newArray.Length; i++)
			{
				newArray[i] = Copy(types[i]);
			}
			return newArray;
		}

		internal static T[] ToArray<T, V>(List<V> list, T[] empty) where V : T
		{
			if (list == null || list.Count == 0)
			{
				return empty;
			}
			T[] array = new T[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = list[i];
			}
			return array;
		}

		// note that an empty array matches a null reference
		internal static bool ArrayEquals(Type[] t1, Type[] t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return t2.Length == 0;
			}
			else if (t2 == null)
			{
				return t1.Length == 0;
			}
			if (t1.Length == t2.Length)
			{
				for (int i = 0; i < t1.Length; i++)
				{
					if (!TypeEquals(t1[i], t2[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		internal static bool ArrayEquals(Type[][] t1, Type[][] t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return t2.Length == 0;
			}
			else if (t2 == null)
			{
				return t1.Length == 0;
			}
			if (t1.Length == t2.Length)
			{
				for (int i = 0; i < t1.Length; i++)
				{
					if (!ArrayEquals(t1[i], t2[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		internal static bool ArrayEquals(Type[][][] t1, Type[][][] t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return t2.Length == 0;
			}
			else if (t2 == null)
			{
				return t1.Length == 0;
			}
			if (t1.Length == t2.Length)
			{
				for (int i = 0; i < t1.Length; i++)
				{
					if (!ArrayEquals(t1[i], t2[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		internal static bool TypeEquals(Type t1, Type t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return false;
			}
			return t1.Equals(t2);
		}

		internal static int GetHashCode(Type[] types)
		{
			if (types == null)
			{
				return 0;
			}
			int h = 0;
			foreach (Type t in types)
			{
				if (t != null)
				{
					h *= 3;
					h ^= t.GetHashCode();
				}
			}
			return h;
		}

		internal static int GetHashCode(Type[][] types)
		{
			int h = 0;
			if (types != null)
			{
				foreach (Type[] array in types)
				{
					h ^= GetHashCode(array);
				}
			}
			return h;
		}

		internal static int GetHashCode(Type[][][] types)
		{
			int h = 0;
			if (types != null)
			{
				foreach (Type[][] array in types)
				{
					h ^= GetHashCode(array);
				}
			}
			return h;
		}
	}
}
