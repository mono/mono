//
// System.Reflection/MethodBody.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	public
	class MethodBody {
		ExceptionHandlingClause[] clauses;
		LocalVariableInfo[] locals;
		byte[] il;
		bool init_locals;
		int sig_token;
		int max_stack;

		protected
		MethodBody () {
		}

		internal MethodBody (ExceptionHandlingClause[] clauses, LocalVariableInfo[] locals,
							 byte[] il, bool init_locals, int sig_token, int max_stack) {
			this.clauses = clauses;
			this.locals = locals;
			this.il = il;
			this.init_locals = init_locals;
			this.sig_token = sig_token;
			this.max_stack = max_stack;
		}

		public
	virtual
		IList<ExceptionHandlingClause> ExceptionHandlingClauses {
			get {
				return Array.AsReadOnly<ExceptionHandlingClause> (clauses);
			}
		}

		public
		virtual
		IList<LocalVariableInfo> LocalVariables {
			get {
				return Array.AsReadOnly<LocalVariableInfo> (locals);
			}
		}

		public
		virtual
		bool InitLocals {
			get {
				return init_locals;
			}
		}

		public
		virtual
		int LocalSignatureMetadataToken {
			get {
				return sig_token;
			}
		}


		public
		virtual
		int MaxStackSize {
			get {
				return max_stack;
			}
		}

		public
		virtual
		byte[] GetILAsByteArray () {
			return il;
		}
	}

}


