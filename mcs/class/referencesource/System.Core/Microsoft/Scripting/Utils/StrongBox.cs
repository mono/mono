/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace System.Runtime.CompilerServices {

    /// <summary>
    /// Holds a reference to a value.
    /// </summary>
    /// <typeparam name="T">The type of the value that the <see cref = "StrongBox{T}"></see> references.</typeparam>
    public class StrongBox<T> : IStrongBox {
        /// <summary>
        /// Gets the strongly typed value associated with the <see cref = "StrongBox{T}"></see>
        /// <remarks>This is explicitly exposed as a field instead of a property to enable loading the address of the field.</remarks>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public T Value;

        /// <summary>
        /// Initializes a new StrongBox which can receive a value when used in a reference call.
        /// </summary>
        public StrongBox() {
        }

        /// <summary>
        /// Initializes a new <see cref = "StrongBox{T}"></see> with the specified value.
        /// </summary>
        /// <param name="value">A value that the <see cref = "StrongBox{T}"></see> will reference.</param>
        public StrongBox(T value) {
            Value = value;
        }

        object IStrongBox.Value {
            get {
                return Value;
            }
            set {
                Value = (T)value;
            }
        }
    }

    /// <summary>
    /// Defines a property for accessing the value that an object references.
    /// </summary>
    public interface IStrongBox {
        /// <summary>
        /// Gets or sets the value the object references.
        /// </summary>
        object Value { get; set; }
    }
}
