// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using Microsoft.Internal;

#if !SILVERLIGHT
using System.Runtime.Serialization;
using Microsoft.Internal.Runtime.Serialization;
#endif

namespace System.ComponentModel.Composition
{
    /// <summary>
    ///     Represents an error that occurs during composition in a <see cref="CompositionContainer"/>.
    /// </summary>
    [Serializable]
    [DebuggerTypeProxy(typeof(CompositionErrorDebuggerProxy))]
    public class CompositionError : ICompositionError
#if !SILVERLIGHT
        , ISerializable
#endif
    {
        private readonly CompositionErrorId _id;
        private readonly string _description;
        private readonly Exception _exception;
        private readonly ICompositionElement _element;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionError"/> class
        ///     with the specified error message.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set the 
        ///     <see cref="Description"/> property to an empty string ("").
        /// </param>
        public CompositionError(string message)
            : this(CompositionErrorId.Unknown, message, (ICompositionElement)null, (Exception)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionError"/> class
        ///     with the specified error message and composition element that is the
        ///     cause of the composition error.
        /// </summary>
        /// <param name="element">
        ///     The <see cref="ICompositionElement"/> that is the cause of the
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set
        ///     the <see cref="CompositionError.Element"/> property to 
        ///     <see langword="null"/>.
        /// </param>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set the 
        ///     <see cref="Description"/> property to an empty string ("").
        /// </param>
        public CompositionError(string message, ICompositionElement element)
            : this(CompositionErrorId.Unknown, message, element, (Exception)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionError"/> class 
        ///     with the specified error message and exception that is the cause of the  
        ///     composition error.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set the 
        ///     <see cref="Description"/> property to an empty string ("").
        /// </param>
        /// <param name="exception">
        ///     The <see cref="Exception"/> that is the underlying cause of the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set
        ///     the <see cref="CompositionError.Exception"/> property to <see langword="null"/>.
        /// </param>
        public CompositionError(string message, Exception exception)
            : this(CompositionErrorId.Unknown, message, (ICompositionElement)null, exception)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionError"/> class 
        ///     with the specified error message, and composition element and exception that 
        ///     is the cause of the composition error.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set the 
        ///     <see cref="Description"/> property to an empty string ("").
        /// </param>
        /// <param name="element">
        ///     The <see cref="ICompositionElement"/> that is the cause of the
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set
        ///     the <see cref="CompositionError.Element"/> property to 
        ///     <see langword="null"/>.
        /// </param>
        /// <param name="exception">
        ///     The <see cref="Exception"/> that is the underlying cause of the 
        ///     <see cref="CompositionError"/>; or <see langword="null"/> to set
        ///     the <see cref="CompositionError.Exception"/> property to <see langword="null"/>.
        /// </param>
        public CompositionError(string message, ICompositionElement element, Exception exception)
            : this(CompositionErrorId.Unknown, message, element, exception)
        {
        }

        internal CompositionError(CompositionErrorId id, string description, ICompositionElement element, Exception exception)
        {
            _id = id;
            _description = description ?? string.Empty;
            _element = element;
            _exception = exception;            
        }

#if !SILVERLIGHT

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionError"/> class 
        ///     with the specified serialization data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        ///     <see cref="CompositionError"/>.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the 
        ///     source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SerializationException">
        ///     <paramref name="info"/> is missing a required value.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     <paramref name="info"/> contains a value that cannot be cast to the correct type.
        /// </exception>
        [System.Security.SecuritySafeCritical]
        protected CompositionError(SerializationInfo info, StreamingContext context)
        {
            Requires.NotNull(info, "info");

            _id = info.GetValue<CompositionErrorId>("Id");
            _element = info.GetValue<ICompositionElement>("Element");
            _exception = info.GetValue<Exception>("Exception");
            _description = info.GetString("Description");
        }

#endif

        /// <summary>
        ///     Gets the composition element that is the cause of the error.
        /// </summary>
        /// <value>
        ///     The <see cref="ICompositionElement"/> that is the cause of the
        ///     <see cref="CompositionError"/>. The default is <see langword="null"/>.
        /// </value>
        public ICompositionElement Element
        {
            get { return _element; }
        }

        /// <summary>
        ///     Gets the message that describes the composition error.
        /// </summary>
        /// <value>
        ///     A <see cref="String"/> containing a message that describes the
        ///     <see cref="CompositionError"/>.
        /// </value>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        ///     Gets the exception that is the underlying cause of the composition error.
        /// </summary>
        /// <value>
        ///     The <see cref="Exception"/> that is the underlying cause of the 
        ///     <see cref="CompositionError"/>. The default is <see langword="null"/>.
        /// </value>
        public Exception Exception
        {
            get { return _exception; }
        }

        CompositionErrorId ICompositionError.Id
        {
            get { return _id; }
        }

        Exception ICompositionError.InnerException
        {
            get { return Exception; }
        }

        /// <summary>
        ///     Returns a string representation of the composition error.
        /// </summary>
        /// <returns>
        ///     A <see cref="String"/> containing the <see cref="Description"/> property.
        /// </returns>
        [System.Security.SecuritySafeCritical]
        public override string ToString()
        {
            return this.Description;
        }

#if !SILVERLIGHT

        /// <summary>
        ///     Gets the serialization data of the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        ///     <see cref="ComposablePartException"/>.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the 
        ///     source or destination.
        /// </param>
        [System.Security.SecurityCritical]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Element", _element.ToSerializableElement());
            info.AddValue("Id", _id);
            info.AddValue("Exception", _exception); 
            info.AddValue("Description", _description);
        }

        /// <summary>
        ///     Gets the serialization data of the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        ///     <see cref="ComposablePartException"/>.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the 
        ///     source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        [System.Security.SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Requires.NotNull(info, "info");

            GetObjectData(info, context);
        }

#endif

        internal static CompositionError Create(CompositionErrorId id, string format, params object[] parameters)
        {
            return Create(id, (ICompositionElement)null, (Exception)null, format, parameters);
        }

        internal static CompositionError Create(CompositionErrorId id, ICompositionElement element, string format, params object[] parameters)
        {
            return Create(id, element, (Exception)null, format, parameters);
        }

        internal static CompositionError Create(CompositionErrorId id, ICompositionElement element, Exception exception, string format, params object[] parameters)
        {
            return new CompositionError(id, string.Format(CultureInfo.CurrentCulture, format, parameters), element, exception);
        }
    }
}