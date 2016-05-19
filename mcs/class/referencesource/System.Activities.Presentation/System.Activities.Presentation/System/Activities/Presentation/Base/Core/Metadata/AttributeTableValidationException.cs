//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Metadata 
{

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Activities.Presentation;

    // <summary>
    // This exception may be thrown from the ValidateTable method on
    // AttributeTable.  It will be thrown if the metadata provided in
    // the table does not match properties, methods and events on real
    // types.
    // </summary>
    [Serializable]
    public class AttributeTableValidationException : Exception 
    {
        private string[] _validationErrors;

        // <summary>
        // Creates a new AttributeTableValidationException.
        // </summary>
        public AttributeTableValidationException() : base() 
        {
        }

        // <summary>
        // Creates a new AttributeTableValidationException.
        // </summary>
        // <param name="message"></param>
        public AttributeTableValidationException(string message)
            : base(message) 
        {
        }

        // <summary>
        // Creates a new AttributeTableValidationException.
        // </summary>
        // <param name="message">The message provided to the user.</param>
        // <param name="inner">An optional inner exception.</param>
        public AttributeTableValidationException(string message, Exception inner)
            : base(message, inner) 
        {
        }

        // <summary>
        // Creates a new AttributeTableValidationException.
        // </summary>
        // <param name="message">The message provided to the user.</param>
        // <param name="validationErrors">Zero or more errors that occurred during validation.</param>
        public AttributeTableValidationException(string message, IEnumerable<string> validationErrors)
            : base(message) 
        {
            _validationErrors = CreateArray(validationErrors);
        }

        // <summary>
        // Creates a new AttributeTableValidationException.
        // </summary>
        // <param name="message">The message provided to the user.</param>
        // <param name="inner">An optional inner exception.</param>
        // <param name="validationErrors">Zero or more errors that occurred during validation.</param>
        public AttributeTableValidationException(string message, Exception inner, IEnumerable<string> validationErrors)
            : base(message, inner) 
        {
            _validationErrors = CreateArray(validationErrors);
        }

        // <summary>
        // Used during serialization to deserialize an exception.
        // </summary>
        // <param name="info">The serialization store.</param>
        // <param name="context">The serialization context.</param>
        protected AttributeTableValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        {
            if (info == null) 
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            _validationErrors = (string[])info.GetValue("ValidationErrors", typeof(string[]));
        }

        // <summary>
        // Returns an enumeration of validation errors.
        // </summary>
        public IEnumerable<string> ValidationErrors 
        {
            get { return _validationErrors; }
        }

        //
        // Helper method to create an array from an enumeration.
        //
        private static string[] CreateArray(IEnumerable<string> validationErrors) {

            string[] array;

            if (validationErrors != null) 
            {
                int cnt = 0;
                IEnumerator<string> e = validationErrors.GetEnumerator();
                while (e.MoveNext()) 
                {
                    cnt++;
                }

                e.Reset();

                array = new string[cnt];

                cnt = 0;

                while (e.MoveNext()) 
                {
                    array[cnt++] = e.Current;
                }
            }
            else 
            {
                array = new string[0];
            }

            return array;
        }

        // <summary>
        // Override of Exception's GetObjectData that is used to perform serialization.
        // </summary>
        // <param name="info">The serialization store.</param>
        // <param name="context">The serialization context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) 
        {
            if (info == null) 
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ValidationErrors", _validationErrors);
        }
    }
}
