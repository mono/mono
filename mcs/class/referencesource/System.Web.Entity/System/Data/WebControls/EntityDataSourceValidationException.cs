//---------------------------------------------------------------------
// <copyright file="EntityDataSourceValidationException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Web.DynamicData;

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Represents errors that occur when validating properties of a dynamic data source.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "SerializeObjectState used instead")]
    public sealed class EntityDataSourceValidationException : Exception, IDynamicValidatorException
    {
        /// <summary>
        /// Exception state used to serialize/deserialize the exception in a safe manner.
        /// </summary>
        [NonSerialized]
        private EntityDataSourceValidationExceptionState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataSourceValidationException" /> class.
        /// </summary>
        public EntityDataSourceValidationException()
            : base()
        {
            InitializeExceptionState(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataSourceValidationException" /> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public EntityDataSourceValidationException(string message)
            : base(message)
        {
            InitializeExceptionState(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataSourceValidationException" /> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public EntityDataSourceValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            InitializeExceptionState(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataSourceValidationException" /> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerExceptions">Inner exceptions.</param>
        internal EntityDataSourceValidationException(string message, Dictionary<string, Exception> innerExceptions)
            : base(message)
        {
            InitializeExceptionState(innerExceptions);
        }

        /// <summary>
        /// Initializes internal exception state.
        /// </summary>
        /// <param name="innerExceptions">Inner exceptions.</param>
        private void InitializeExceptionState(Dictionary<string, Exception> innerExceptions)
        {
            _state = new EntityDataSourceValidationExceptionState(innerExceptions);
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        /// Returns inner exceptions.
        /// </summary>
        IDictionary<string, Exception> IDynamicValidatorException.InnerExceptions
        {
            get { return _state.InnerExceptions; }
        }

        /// <summary>
        /// Subscribes the SerializeObjectState event.
        /// </summary>
        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
        }

        /// <summary>
        /// Holds the exception state that will be serialized when the exception is serialized.
        /// </summary>
        [Serializable]
        private class EntityDataSourceValidationExceptionState : ISafeSerializationData
        {
            /// <summary>
            /// Inner exceptions.
            /// </summary>
            private readonly Dictionary<string, Exception> _innerExceptions;

            /// <summary>
            /// Initializes a new instance of the <see cref="EntityDataSourceValidationExceptionState"/> class.
            /// </summary>
            /// <param name="innerExceptions"></param>
            public EntityDataSourceValidationExceptionState(Dictionary<string, Exception> innerExceptions)
            {
                _innerExceptions = innerExceptions ?? new Dictionary<string, Exception>();
            }

            /// <summary>
            /// Returns inner exceptions.
            /// </summary>
            public Dictionary<string, Exception> InnerExceptions
            {
                get
                {
                    return _innerExceptions;
                }
            }

            /// <summary>
            /// Completes the deserialization.
            /// </summary>
            /// <param name="deserialized">The deserialized object.</param>
            public void CompleteDeserialization(object deserialized)
            {
                ((EntityDataSourceValidationException)deserialized)._state = this;
            }
        }
    }
}
