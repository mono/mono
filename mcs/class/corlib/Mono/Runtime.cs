//
// Mono Runtime gateway functions
//
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

using System;
using System.Runtime.CompilerServices;

namespace Mono {

	internal class Runtime
	{
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void mono_runtime_install_handlers ();
		
		static internal void InstallSignalHandlers ()
		{
			mono_runtime_install_handlers ();
		}

		// Should not be removed intended for external use
		// Safe to be called using reflection
		// Format is undefined only for use as a string for reporting
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string GetDisplayName ();

		/*
		Create a object without calling any of it's constructors.
		@h is a pointer to the runtime type handle of that object.		
		Recomended usage is to emit the following code sequence:
		ldtoken [mscorlib]System.Object
		call object [mscorlib]Mono.Runtime::NewObject(intptr)

		This is the only well understood sequence known by the JIT
		which produces faster code.
		*/
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object NewObject (RuntimeTypeHandle h);
	}
	
}
