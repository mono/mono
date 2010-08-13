//
// RewriterResults.cs
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

namespace Mono.CodeContracts.Rewrite {

	public class RewriterResults {

		internal static RewriterResults Warning (string warning)
		{
			return new RewriterResults (new [] { warning }, null);
		}

		internal static RewriterResults Error (string error)
		{
			return new RewriterResults (null, new [] { error });
		}

		internal RewriterResults (ICollection<string> warnings, ICollection<string> errors)
		{
			this.warnings = warnings;
			this.errors = errors;
		}

		private ICollection<string> warnings, errors;

		public bool AnyWarnings {
			get {
				return this.warnings != null && this.warnings.Count > 0;
			}
		}

		public bool AnyErrors {
			get {
				return this.errors != null && this.errors.Count > 0;
			}
		}

		public IEnumerable<string> Warnings {
			get {
				return this.warnings ?? Enumerable.Empty<string> ();
			}
		}

		public IEnumerable<string> Errors {
			get {
				return this.errors ?? Enumerable.Empty<string> ();
			}
		}

	}

}
