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

namespace DbLinq.Factory
{
    /// <summary>
    /// The object factory is the start point for DbLinq main factory.
    /// See ObjectFactory.Current for details
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
    interface IObjectFactory
    {
        /// <summary>
        /// Returns an instance of a stateless class (may be a singleton)
        /// </summary>
        /// <typeparam name="T">class or interface</typeparam>
        /// <returns></returns>
        T Get<T>();

        /// <summary>
        /// Returns a new instance of the specified class (can not be a singleton)
        /// </summary>
        /// <typeparam name="T">class or interface</typeparam>
        /// <returns></returns>
        T Create<T>();

        /// <summary>
        /// Underlying method for Get&lt;T> and Create&lt;T>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newInstanceRequired"></param>
        /// <returns></returns>
        object GetInstance(Type t, bool newInstanceRequired);

        /// <summary>
        /// allow DbMetal to suggest ConsoleLogger as preferred ILogger
        /// </summary>
        T GetInstance<T>(T suggestedInstance);

        /// <summary>
        /// Returns a list of types implementing the required interface
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        IEnumerable<Type> GetImplementations(Type interfaceType);
    }
}
