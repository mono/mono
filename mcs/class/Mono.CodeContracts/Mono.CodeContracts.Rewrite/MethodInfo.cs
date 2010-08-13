//
// MethodInfo.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mono.CodeContracts.Rewrite {
	class MethodInfo {

		public MethodInfo (ModuleDefinition module, MethodDefinition method)
		{
			this.Method = method;
			this.Module = module;

			this.typeVoid = new Lazy<TypeReference> (() => this.Module.Import (typeof (void)));
			this.typeObject = new Lazy<TypeReference> (() => this.Module.Import (typeof (object)));
			this.typeInt32 = new Lazy<TypeReference> (() => this.Module.Import (typeof (int)));
			this.typeInt64 = new Lazy<TypeReference> (() => this.Module.Import (typeof (long)));
			this.typeUInt32 = new Lazy<TypeReference> (() => this.Module.Import (typeof (uint)));
			this.typeBoolean = new Lazy<TypeReference> (() => this.Module.Import (typeof (bool)));
			this.typeString = new Lazy<TypeReference> (() => this.Module.Import (typeof (string)));
		}

		public MethodDefinition Method { get; private set; }
		public ModuleDefinition Module { get; private set; }

		private Lazy<TypeReference> typeVoid;
		private Lazy<TypeReference> typeObject;
		private Lazy<TypeReference> typeInt32;
		private Lazy<TypeReference> typeInt64;
		private Lazy<TypeReference> typeUInt32;
		private Lazy<TypeReference> typeBoolean;
		private Lazy<TypeReference> typeString;

		public TypeReference TypeVoid {
			get { return this.typeVoid.Value; }
		}

		public TypeReference TypeObject {
			get { return this.typeObject.Value; }
		}

		public TypeReference TypeInt32 {
			get { return this.typeInt32.Value; }
		}

		public TypeReference TypeInt64 {
			get { return this.typeInt64.Value; }
		}

		public TypeReference TypeUInt32 {
			get { return this.typeUInt32.Value; }
		}

		public TypeReference TypeBoolean {
			get { return this.typeBoolean.Value; }
		}

		public TypeReference TypeString {
			get { return this.typeString.Value; }
		}

	}
}
