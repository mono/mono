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

using System;
using System.Diagnostics;
using System.Dynamic.Utils;

#if SILVERLIGHT // Stubs

// This is needed so we can build Silverlight version on Codeplex
// where System.Core namespace is not defined.
namespace System.Core 
{ 
    class Dummy { } 
}

namespace System {

    /// <summary>
    /// An application exception.
    /// </summary>
    public class ApplicationException : Exception {
        private const int error = unchecked((int)0x80131600);
        /// <summary>
        /// The constructor.
        /// </summary>
        public ApplicationException()
            : base("Application Exception") {
            HResult = error;
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="message">The message.</param>
        public ApplicationException(string message)
            : base(message) {
            HResult = error;
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ApplicationException(string message, Exception innerException)
            : base(message, innerException) {
            HResult = error;
        }
    }

    namespace Runtime.InteropServices {
        /// <summary>
        /// The Default Parameter Value Attribute.
        /// </summary>
        public sealed class DefaultParameterValueAttribute : Attribute {
            /// <summary>
            /// The constructor
            /// </summary>
            /// <param name="value">The value.</param>
            public DefaultParameterValueAttribute(object value) { }
        }
    }

    // We reference these namespaces via "using"
    // We don't actually use them because the code is #if !SILVERLIGHT
    // Rather than fix the usings all over the place, just define these here
    namespace Runtime.Remoting { class Dummy {} }
    namespace Security.Policy { class Dummy {} }
    namespace Xml.XPath { class Dummy {} }

    namespace Reflection {
        /// <summary>
        /// PortableExecutableKinds enum.
        /// </summary>
        public enum PortableExecutableKinds {
            /// <summary>
            /// ILOnly
            /// </summary>
            ILOnly = 0
        }

        /// <summary>
        /// ImageFileMachine enum.
        /// </summary>
        public enum ImageFileMachine {
            /// <summary>
            /// I386
            /// </summary>
            I386 = 1
        }
    }

    namespace ComponentModel {

        /// <summary>
        /// The Warning exception.
        /// </summary>
        public class WarningException : SystemException {
            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="message">The message.</param>
            public WarningException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// The serializable attribute.
    /// </summary>
    public class SerializableAttribute : Attribute {
    }

    /// <summary>
    /// Non serializable attribute.
    /// </summary>
    public class NonSerializedAttribute : Attribute {
    }

    namespace Runtime.Serialization {
        /// <summary>
        /// ISerializable interface.
        /// </summary>
        public interface ISerializable {
        }
    }

    /// <summary>
    /// The ConsoleColor enum.
    /// </summary>
    public enum ConsoleColor {
        /// <summary>
        /// Black.
        /// </summary>
        Black = 0,
        /// <summary>
        /// DarkBlue.
        /// </summary>
        DarkBlue = 1,
        /// <summary>
        /// DarkGreen.
        /// </summary>
        DarkGreen = 2,
        /// <summary>
        /// DaryCyan.
        /// </summary>
        DarkCyan = 3,
        /// <summary>
        /// DarkRed
        /// </summary>
        DarkRed = 4,
        /// <summary>
        /// DarkMagenta
        /// </summary>
        DarkMagenta = 5,
        /// <summary>
        /// DarkYellow
        /// </summary>
        DarkYellow = 6,
        /// <summary>
        /// Gray
        /// </summary>
        Gray = 7,
        /// <summary>
        /// DarkGray
        /// </summary>
        DarkGray = 8,
        /// <summary>
        /// Blue
        /// </summary>
        Blue = 9,
        /// <summary>
        /// Green
        /// </summary>
        Green = 10,
        /// <summary>
        /// Cyan
        /// </summary>
        Cyan = 11,
        /// <summary>
        /// Red
        /// </summary>
        Red = 12,
        /// <summary>
        /// Magenta
        /// </summary>
        Magenta = 13,
        /// <summary>
        /// Yellow
        /// </summary>
        Yellow = 14,
        /// <summary>
        /// White
        /// </summary>
        White = 15,
    }

}

#endif