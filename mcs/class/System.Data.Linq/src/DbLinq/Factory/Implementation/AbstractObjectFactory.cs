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

namespace DbLinq.Factory.Implementation
{
    /// <summary>
    /// Brings base mechanisms for the factory
    /// </summary>
    internal abstract class AbstractObjectFactory: IObjectFactory
    {
        /// <summary>
        /// Returns an instance of a stateless class (may be a singleton)
        /// </summary>
        /// <typeparam name="T">class or interface</typeparam>
        /// <returns></returns>
        public virtual T Get<T>()
        {
            return (T)GetInstance(typeof(T), false);
        }

        /// <summary>
        /// Returns a new instance of the specified class (can not be a singleton)
        /// </summary>
        /// <typeparam name="T">class or interface</typeparam>
        /// <returns></returns>
        public virtual T Create<T>()
        {
            return (T)GetInstance(typeof(T), true);
        }

        /// <summary>
        /// Underlying method for Get&lt;T&gt; and Create&lt;T&gt;
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newInstanceRequired"></param>
        /// <returns></returns>
        public abstract object GetInstance(Type t, bool newInstanceRequired);

        /// <summary>
        /// Returns a list of types implementing the required interface
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public abstract IEnumerable<Type> GetImplementations(Type interfaceType);
    }
}
