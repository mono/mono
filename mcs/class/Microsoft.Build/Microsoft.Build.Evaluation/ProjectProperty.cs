// ProjectProperty.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	// In .NET 4.0 MSDN says it is non-abstract, but some of those
	// members are abstract and had been there since 4.0.
	// I take this as doc bug, as non-abstract to abstract is a
	// breaking change and I'd rather believe API designer's sanity.
        public abstract class ProjectProperty
        {
		internal ProjectProperty () // hide default ctor
		{
		}

                public string EvaluatedValue {
                        get {
                                throw new NotImplementedException ();
                        }
                }

		public abstract bool IsEnvironmentProperty { get; }
		public abstract bool IsGlobalProperty { get; }
		public abstract bool IsImported { get; }
		public abstract bool IsReservedProperty { get; }

		public string Name {
			get {
                                throw new NotImplementedException ();
			}
		}

		public abstract ProjectProperty Predecessor { get; }

		public Project Project {
			get {
                                throw new NotImplementedException ();
			}
		}

		public abstract string UnevaluatedValue { get; set; }
		public abstract ProjectPropertyElement Xml { get; }
        }
}

