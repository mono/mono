// 
// IAbstractDomain.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
//

using System.IO;

namespace Mono.CodeContracts.Static.Lattices {
        /// <summary>
        /// Represents abstraction of concrete value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IAbstractDomain<T> {
                /// <summary>
                /// Represents universe set (which holds every value)
                /// </summary>
                T Top { get; }

                /// <summary>
                /// Represents empty set (which holds nothing)
                /// </summary>
                T Bottom { get; }

                /// <summary>
                /// Is this value a universe set
                /// </summary>
                bool IsTop { get; }

                /// <summary>
                /// Is this value an empty set
                /// </summary>
                bool IsBottom { get; }

                /// <summary>
                /// Returns a union of this and that
                /// </summary>
                /// <param name="that"></param>
                /// <returns></returns>
                T Join (T that);

                T Join (T that, bool widen, out bool weaker);

                T Widen (T that);

                /// <summary>
                /// Returns an intersection of this and that
                /// </summary>
                /// <param name="that"></param>
                /// <returns></returns>
                T Meet (T that);

                bool LessEqual (T that);

                T ImmutableVersion ();
                T Clone ();

                void Dump (TextWriter tw);
        }
}