// 
// AbstractDomainExtensions.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Lattices {
        public static class AbstractDomainExtensions {
                public static bool IsNormal<T> (this IAbstractDomain<T> domain)
                {
                        return !domain.IsTop && !domain.IsBottom;
                }

                public static string BottomSymbolIfAny<T> (this IAbstractDomain<T> domain)
                {
                        return domain.IsBottom ? "_|_" : string.Empty;
                }

                public static bool TryTrivialLessEqual<T> (this T left, T right, out bool result)
                        where T : IAbstractDomain<T>
                {
                        if (ReferenceEquals (left, right))
                                return true.With (true, out result);

                        if (left.IsBottom)
                                return true.With (true, out result);

                        if (left.IsTop)
                                return true.With (right.IsTop, out result);

                        if (right.IsBottom)
                                return true.With (false, out result);

                        if (right.IsTop)
                                return true.With (true, out result);

                        return false.Without (out result);
                }

                public static bool TryTrivialJoin<T> (this T left, T right, out T result)
                        where T : IAbstractDomain<T>
                {
                        if (ReferenceEquals (left, right))
                                return true.With (left, out result);

                        if (left.IsBottom)
                                return true.With (right, out result);

                        if (left.IsTop)
                                return true.With (left, out result);

                        if (right.IsBottom)
                                return true.With (left, out result);

                        if (right.IsTop)
                                return true.With (right, out result);

                        return false.Without (out result);
                }

                public static bool TryTrivialMeet<T> (this T left, T right, out T result)
                        where T : IAbstractDomain<T>
                {
                        if (ReferenceEquals (left, right))
                                return true.With (left, out result);

                        if (left.IsBottom)
                                return true.With (left, out result);

                        if (left.IsTop)
                                return true.With (right, out result);

                        if (right.IsBottom)
                                return true.With (right, out result);

                        if (right.IsTop)
                                return true.With (left, out result);

                        return false.Without (out result);
                }
        }
}