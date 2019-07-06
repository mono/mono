// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices
{
	static class JitHelpers
	{
		[Intrinsic]
		// should be implemented as an intrinsic for Interpreter too: https://github.com/mono/mono/issues/15596
		public static bool EnumEquals<T> (T x, T y) where T : struct, Enum => x.Equals(y);

		[Intrinsic]
		public static int EnumCompareTo<T> (T x, T y) where T : struct, Enum  => x.CompareTo(y);
	}
}
