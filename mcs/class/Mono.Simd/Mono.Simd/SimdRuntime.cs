// SimdRuntime.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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


namespace Mono.Simd
{
	public static class SimdRuntime
	{
		//
		// Mono replaces the method below at JIT time on platforms that support
		// SIMD with the actual acceleration mode
		//
		public static AccelMode AccelMode
		{
			get
			{
				return AccelMode.None;
			}
		}

		public static AccelMode MethodAccelerationMode (MethodInfo method)
		{
			if (method == null)
				return AccelMode.None;
			object[] attr = method.GetCustomAttributes (typeof (AccelerationAttribute), false);
			if (attr.Length == 0)
				return AccelMode.None;
			return ((AccelerationAttribute) attr [0]).Mode;
		}

		public static AccelMode MethodAccelerationMode (Type type, String method)
		{
			return MethodAccelerationMode (type, method, null);
		}

		public static AccelMode MethodAccelerationMode (Type type, String method, Type[] signature)
		{
			MethodInfo mi = signature == null ? type.GetMethod (method) : type.GetMethod (method, signature);
			return MethodAccelerationMode (mi);
		}

		public static bool IsMethodAccelerated (Type type, String method)
		{			
			return (MethodAccelerationMode (type, method, null) & AccelMode) != 0;
		}

		public static bool IsMethodAccelerated (Type type, String method, Type[] signature)
		{			
			return (MethodAccelerationMode (type, method, signature) & AccelMode) != 0;
		}
	}
}
