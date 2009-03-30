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
        ///  Registers <paramref name="implementationType" /> as an 
        ///  implementation for all implemented interfaces.
        /// </summary>
        /// <param name="implementationType">
        ///  A <see cref="T:System.Type" /> which is the implementation type
        ///  to register.
        /// </param>
        /// <remarks>
        ///  Once this method has been called, 
        ///  <paramref name="implementationType" /> may be used in future
        ///  <see cref="M:Create(Type)" /> and <see cref="M:Get(Type)" />
        ///  invocations.
        /// </remarks>
        void Register(Type implementationType);

        /// <summary>
        ///  Unregisters <paramref name="implementationType" /> as an 
        ///  implementation for all implemented interfaces.
        /// </summary>
        /// <param name="implementationType">
        ///  A <see cref="T:System.Type" /> which is the implementation type
        ///  to unregister.
        /// </param>
        /// <remarks>
        ///  Once this method has been called, 
        ///  <paramref name="implementationType" /> will no longer be used by
        ///  subsequent <see cref="M:Create(Type)" /> and 
        ///  <see cref="M:Get(Type)" /> invocations.
        /// </remarks>
        void Unregister(Type implementationType);

        /// <summary>
        /// Returns an instance of a stateless class (may be a singleton)
        /// </summary>
        /// <returns>
        ///  An instance of type <paramref name="interfaceType" />.
        ///  This instance will be shared by other invocations of 
        ///  <c>Get()</c> with the same <param name="interfaceType" /> value.
        /// </returns>
        object Get(Type interfaceType);

        /// <summary>
        /// Returns a new instance of the specified class (can not be a singleton)
        /// </summary>
        /// <returns>
        ///  A new instance of type <paramref name="interfaceType" />.
        ///  This instance will not be shared by other invocations of 
        ///  <c>Create()</c> with the same <param name="interfaceType" /> 
        ///  value.
        /// </returns>
        object Create(Type interfaceType);

        /// <summary>
        /// Returns a list of types implementing the required interface
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns>
        ///  An <see cref="T:IEnumerable{Type}" /> containing all registered 
        ///  implementations for the interface 
        ///  <paramref name="interfaceType" />.
        /// </returns>
        IEnumerable<Type> GetImplementations(Type interfaceType);
    }

    /// <summary>
    ///  Extension methods for <see cref="T:IObjectFactory" />.
    /// </summary>
#if !MONO_STRICT
    public
#endif
    static class ObjectFactoryExtensions
    {
        /// <summary>
        ///  Creates a new instance of <typeparamref name="T" /> from 
        ///  <paramref name="self" />.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="self">
        ///  An <see cref="T:IObjectFactory" /> to use to create a new instance
        ///  of type <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///  A newly created instance of type <typeparamref name="T" />.
        /// </returns>
        /// <seealso cref="M:IObjectFactory.Create(Type)"/>
        public static T Create<T>(this IObjectFactory self)
        {
            return (T) self.Create(typeof(T));
        }

        /// <summary>
        ///  Gets a (possibly pre-existing) instance of 
        ///  <typeparamref name="T" /> from <paramref name="self" />.
        /// </summary>
        /// <typeparam name="T">The type to get.</typeparam>
        /// <param name="self">
        ///  An <see cref="T:IObjectFactory" /> to use to get an instance
        ///  of type <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///  A (possibly pre-existing) instance of type 
        ///  <typeparamref name="T" />.
        /// </returns>
        /// <seealso cref="M:IObjectFactory.Get(Type)"/>
        public static T Get<T>(this IObjectFactory self)
        {
            return (T) self.Get(typeof(T));
        }
    }
}
