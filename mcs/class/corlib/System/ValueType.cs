//
// System.ValueType.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Novell, Inc.  http://www.novell.com
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

//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Files:
//  - mscorlib/system/valuetype.cs
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	public abstract class ValueType
	{
		/*
		 * Caution: Fields added to ValueType can mess with sub class layouts.
		 * Causing bugs that appear completely unrelated as #30060
		 */
		private static class Internal
		{
			public static int hash_code_of_ptr_seed = 0;
		}

		protected ValueType ()
		{
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool InternalEquals (object o1, object o2, out object[] fields);

		// This is also used by RuntimeHelpers
		internal static bool DefaultEquals (object o1, object o2)
		{
			if (o1 == null && o2 == null)
				return true;
			if (o1 == null || o2 == null)
				return false;

			RuntimeType o1_type = (RuntimeType) o1.GetType ();
			RuntimeType o2_type = (RuntimeType) o2.GetType ();

			if (o1_type != o2_type)
				return false;

			object[] fields;
			bool res = InternalEquals (o1, o2, out fields);
			if (fields == null)
				return res;

			for (int i = 0; i < fields.Length; i += 2) {
				object meVal = fields [i];
				object youVal = fields [i + 1];
				if (meVal == null) {
					if (youVal == null)
						continue;

					return false;
				}

				if (!meVal.Equals (youVal))
					return false;
			}

			return true;
		}

		// <summary>
		//   True if this instance and o represent the same type
		//   and have the same value.
		// </summary>
		public override bool Equals (object obj)
		{
			return DefaultEquals (this, obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static int InternalGetHashCode (object o, out object[] fields);

		// <summary>
		//   Gets a hashcode for this value type using the
		//   bits in the structure
		// </summary>
		public override int GetHashCode ()
		{
			object[] fields;
			int result = InternalGetHashCode (this, out fields);

			if (fields != null)
				for (int i = 0; i < fields.Length; ++i)
					if (fields [i] != null)
						result ^= fields [i].GetHashCode ();
				
			return result;
		}

		internal static int GetHashCodeOfPtr (IntPtr ptr)
		{
			int hash_code = (int) ptr;
			int seed = Internal.hash_code_of_ptr_seed;

			if (seed == 0) {
				/* We use the first non-0 pointer as the seed, all hashcodes will be
				 * based off that. This is to make sure that we only reveal relative
				 * memory addresses and never absolute ones. */
				seed = hash_code;
				Interlocked.CompareExchange (ref Internal.hash_code_of_ptr_seed, seed, 0);
				seed = Internal.hash_code_of_ptr_seed;
			}

			return hash_code - seed;
		}

		// <summary>
		//   Stringified representation of this ValueType.
		//   Must be overriden for better results, by default
		//   it just returns the Type name.
		// </summary>
		public override string ToString ()
		{
			return GetType ().FullName;
		}
	}
}
