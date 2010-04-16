//
// System.Func.cs
//
// Authors:
//      Alejandro Serrano "Serras" (trupill@yahoo.es)
//	Marek Safar (marek.safar@gmail.com)
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
// MERCHANTABILITY, FITNESS FOR TArg PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//

using System.Runtime.CompilerServices;

namespace System {

#if NET_4_0 || BOOTSTRAP_NET_4_0
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]	
	public delegate TResult Func<out TResult> ();
	
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
	public delegate TResult Func<in T, out TResult> (T arg1);
	
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
	public delegate TResult Func<in T1, in T2, out TResult> (T1 arg1, T2 arg2);
	
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
	public delegate TResult Func<in T1, in T2, in T3, out TResult> (T1 arg1, T2 arg2, T3 arg3);
	
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
	public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	
	public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
	public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
	public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
#elif MOONLIGHT
	public delegate TResult Func<T1, T2, T3, T4, T5, TResult> (
		T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

	public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult> (
		T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult> (
		T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> (
		T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
#endif
}
