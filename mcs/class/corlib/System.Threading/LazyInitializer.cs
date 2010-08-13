// 
// LazyInitializer.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;

namespace System.Threading
{
	public static class LazyInitializer
	{
		public static T EnsureInitialized<T> (ref T target) where T : class
		{
			return EnsureInitialized (ref target, GetDefaultCtorValue<T>);
		}
		
		public static T EnsureInitialized<T> (ref T target, Func<T> initFunc) where T : class
		{
			Interlocked.CompareExchange (ref target, initFunc (), null);
			
			return target;
		}

		public static T EnsureInitialized<T> (ref T target, ref bool initialized, ref object syncRoot)
		{
			return EnsureInitialized (ref target, ref initialized, ref syncRoot, GetDefaultCtorValue<T>);
		}
		
		public static T EnsureInitialized<T> (ref T target, ref bool initialized, ref object syncRoot, Func<T> initFunc)
		{
			lock (syncRoot) {
				if (initialized)
					return target;
				
				initialized = true;
				return target = initFunc ();
			}
		}
		
		internal static T GetDefaultCtorValue<T> ()
		{
			try { 
				return Activator.CreateInstance<T> ();
			} catch { 
				throw new MissingMemberException ("The type being lazily initialized does not have a "
				                                  + "public, parameterless constructor.");
			}
		}
	}
}
#endif
