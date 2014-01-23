// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Json
{
    /// <summary>
    /// Struct that wraps values which cause JIT compilation at runtime.
    /// This Struct is added to solve the FxCop warning CA908 in JsonObject.cs.
    /// </summary>
    /// <typeparam name="T">Wrapped type.</typeparam>
    internal struct NGenWrapper<T>
    {
        /// <summary>
        /// Value of type T which represents the actual data which is currently in hold.
        /// </summary>
        public T Value;

        /// <summary>
        /// Creates an instance of the <see cref="System.Json.NGenWrapper{T}"/> class
        /// </summary>
        /// <param name="value">The wrapped object of T</param>
        public NGenWrapper(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Cast operator from <see cref="System.Json.NGenWrapper{T}"/> to <typeparamref name="T"/>
        /// </summary>
        /// <param name="value">Object in type <see cref="System.Json.NGenWrapper{T}"/></param>
        /// <returns>Object in type <typeparamref name="T">The wrapped element type</typeparamref></returns>
        /// <typeparamref name="T">The wrapped element type</typeparamref>
        public static implicit operator T(NGenWrapper<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Cast operator from <typeparamref name="T"/> to <see cref="System.Json.NGenWrapper{T}"/>
        /// </summary>
        /// <param name="value">Object in type <typeparamref name="T"/></param>
        /// <returns>Object in type <see cref="System.Json.NGenWrapper{T}"/></returns>
        /// <typeparamref name="T">The wrapped element type</typeparamref>
        public static implicit operator NGenWrapper<T>(T value)
        {
            return new NGenWrapper<T>(value);
        }
    }
}
