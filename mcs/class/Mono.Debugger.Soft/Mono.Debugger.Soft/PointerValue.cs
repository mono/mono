// 
// PointerValue.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
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
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	/*
	 * Represents a value of a pointer type in the debuggee
	 */
	public class PointerValue : Value {
		TypeMirror type;
		long addr;

		public PointerValue (VirtualMachine vm, TypeMirror type, long addr) : base (vm, 0) {
			this.type = type;
			this.addr = addr;
		}

		public long Address {
			get { return addr; }
		}

		public TypeMirror Type {
			get { return type; }
		}

		// Since protocol version 2.46
		public Value Value {
			get {
				ValueImpl value;
				if (Address == 0)
					return null;
				try {
					value = vm.conn.Pointer_GetValue (Address, Type);
				}
				catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("Invalid pointer address.");
				else
					throw;
				}
				return vm.DecodeValue (value);
			}
		}

		public override bool Equals (object obj) {
			if (obj != null && obj is PointerValue)
				return addr == (obj as PointerValue).addr;
			return base.Equals (obj);
		}

		public override int GetHashCode () {
			return base.GetHashCode ();
		}

		public override string ToString () {
			return string.Format ("PointerValue<({0}) 0x{1:x}>", type.CSharpName, addr);
		}
	}
}
