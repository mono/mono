// 
// TypeCache.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct TypeCache {
		private readonly string full_name;
		private TypeNode cache;
		private bool cache_is_valid;
		private bool have_type;

		public TypeCache (string fullName)
		{
			this.cache_is_valid = false;
			this.have_type = false;
			this.cache = default(TypeNode);
			this.full_name = fullName;
		}

		public bool TryGet (IMetaDataProvider mdProvider, out TypeNode type)
		{
			if (!this.cache_is_valid) {
				this.cache_is_valid = true;
				this.have_type = mdProvider.TryGetSystemType (this.full_name, out this.cache);
			}

			type = this.cache;
			return this.have_type;
		}
	}
}
