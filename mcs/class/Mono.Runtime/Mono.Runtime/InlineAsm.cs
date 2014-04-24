//
// InlineAsm.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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

namespace Mono.Runtime {

	[Flags]
	public enum InlineAsmFlags {
		None = 0,
		ClobbersFlags  = 1 << 1,
		ClobbersMemory = 1 << 2,
		Volatile       = 1 << 3,
	}

	// architecture constraints
	public enum Amd64Asm {
		Rax,
		Rcx,
		Rdx,
		Rbx,
		Rsp,
		Rbp,
		Rsi,
		Rdi,
		R8,
		R9,
		R10,
		R11,
		R12,
		R13,
		R14,
		R15,
		Xmm0,
		Xmm1,
		Xmm2,
		Xmm3,
		Xmm4,
		Xmm5,
		Xmm6,
		Xmm7,
		Xmm8,
		Xmm9,
		Xmm10,
		Xmm11,
		Xmm12,
		Xmm13,
		Xmm14,
		Xmm15,
		Input  = 1 << 24,
		Output = 1 << 25,
		ArrayStart = 1 << 26,
	}

	public enum X86Asm {
		Eax,
		Ecx,
		Edx,
		Ebx,
		Esp,
		Ebp,
		Esi,
		Edi,
		Xmm0,
		Xmm1,
		Xmm2,
		Xmm3,
		Xmm4,
		Xmm5,
		Xmm6,
		Xmm7,
		Input  = 1 << 24,
		Output = 1 << 25,
		ArrayStart = 1 << 26,
	}

	public enum ArmAsm {
		R0,
		R1,
		R2,
		R3,
		R4,
		R5,
		R6,
		R7,
		R8,
		R9,
		R10,
		R11,
		R12,
		R13,
		R14,
		R15,
		F0,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		F13,
		F14,
		F15,
		F16,
		F17,
		F18,
		F19,
		F20,
		F21,
		F22,
		F23,
		F24,
		F25,
		F26,
		F27,
		F28,
		F29,
		F30,
		F31,
		Input  = 1 << 24,
		Output = 1 << 25,
		ArrayStart = 1 << 26,
	}

/*
	public class ArmXXX
	{
		public enum Descriptor
		{
			R0,
			R1,
			R2,
			R3,
			R4,
			R5,
			R6,
			R7,
			R8,
			R9,
			R10,
			R11,
			R12,
			R13,
			R14,
			R15,			
		}

		public Arm (object value, Descriptor descriptor)
		{			
		}
	}
*/

	public static class InlineAsm {

		// Each of the methods here is in practice an internal call: the JIT will
		// substitute the correct result inline, while on runtimes that don't support the
		// feature, the code executes as is and everything is disabled (the developer
		// should insert a fallback to handle this case)
		public static bool ArmSupported {
			get {
				return false;
			}
		}

		public static bool X86Supported {
			get {
				return false;
			}
		}

		public static bool Amd64Supported {
			get {
				return false;
			}
		}

		public static bool SSE2Supported {
			get {
				return false;
			}
		}

		//
		// The following methods are never seen by the JIT (and even if someone tries to
		// sneak them in they do nothing). Instead, the C# compiler intercepts them and
		// replaces the call with a ldtoken/pop IL instruction sequence: the resulting IL
		// code is valid IL (and would do nothing anyway on a runtime without inline asm support).
		// the token references a field with a data blob that contains the serialized information
		// (flags, compiled assembly code, argument constraints, optionally the original assembly code).
		// this is similar to the ldtoken use for array initialization.
		// args contains pairs of local vars (including method arguments) and their constraints
		// the C# compiler needs to make sure the constraints are of the right type and constants
		// for arguments flagged with Output, the C# compiler should treat the variable like
		// if it was passed as out varname to a method for definitive assignment purposes
		//

		public static void Arm (InlineAsmFlags options, string code, params object[] args)
		{
		}

		public static void X86 (InlineAsmFlags options, string code, params object[] args)
		{
		}

		public static void Amd64 (InlineAsmFlags options, string code, params object[] args)
		{
		}

		// GPU code?
	}
}

