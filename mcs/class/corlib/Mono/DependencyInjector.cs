//
// DependencyInjector.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mono
{
	static class DependencyInjector
	{
		/*
		 * Allows us to use code from `System.dll` in `mscorlib.dll`.
		 * 
		 */
		internal static ISystemDependencyProvider SystemProvider {
			get {
				if (systemDependency != null)
					return systemDependency;

				lock (locker) {
					if (systemDependency != null)
						return systemDependency;

					systemDependency = ReflectionLoad ();
					if (systemDependency == null)
						throw new PlatformNotSupportedException ($"Cannot find '{TypeName}' dependency");

					return systemDependency;
				}
			}
		}

		internal static void Register (ISystemDependencyProvider provider)
		{
			lock (locker) {
				if (systemDependency != null && systemDependency != provider)
					throw new InvalidOperationException ();
				systemDependency = provider;
			}
		}

		const string TypeName = "Mono.SystemDependencyProvider, System";

		[PreserveDependency ("get_Instance()", "Mono.SystemDependencyProvider", "System")]
		static ISystemDependencyProvider ReflectionLoad ()
		{
			var type = Type.GetType (TypeName);
			if (type == null)
				return null;

			var prop = type.GetProperty ("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
			if (prop == null)
				return null;

			return (ISystemDependencyProvider) prop.GetValue (null);
		}

		static object locker = new object ();
		static ISystemDependencyProvider systemDependency;
	}
}
