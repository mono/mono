#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
// 
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DbLinq.Util
{
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        private MethodInfo objectGetHashCode;
        /// <summary>
        /// This is the raw GetHashCode, not inherited
        /// </summary>
        protected MethodInfo ObjectGetHashCode
        {
            get
            {
                if (objectGetHashCode == null)
                {
                    objectGetHashCode = typeof(object).GetMethod("GetHashCode");
                }
                return objectGetHashCode;
            }
        }

        private object[] noArgs = new object[0];

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            var hash = (int)ObjectGetHashCode.Invoke(obj, noArgs);
            return hash;
        }
    }
}
