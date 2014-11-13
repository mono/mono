//---------------------------------------------------------------------
// <copyright file="PropertyConstraintException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------


namespace System.Data
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Property constraint exception class. Note that this class has state - so if you change even
    /// its internals, it can be a breaking change
    /// </summary>
    /// 
    [Serializable]
    public sealed class PropertyConstraintException : ConstraintException
    {
        private string _propertyName;

        /// <summary>
        /// constructor with default message
        /// </summary>
        public PropertyConstraintException() // required ctor
            : base()
        {
        }

        /// <summary>
        /// costructor with supplied message
        /// </summary>
        /// <param name="message">localized error message</param>
        public PropertyConstraintException(string message)  // required ctor
            : base(message)
        {
        }

        /// <summary>
        /// costructor with supplied message and inner exception
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        public PropertyConstraintException(string message, Exception innerException)  // required ctor
            : base(message, innerException)
        {
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        public PropertyConstraintException(string message, string propertyName) // required ctor
            : base(message)
        {
            EntityUtil.CheckStringArgument(propertyName, "propertyName");
            _propertyName = propertyName;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        public PropertyConstraintException(string message, string propertyName, Exception innerException) // required ctor
            : base(message, innerException)
        {
            EntityUtil.CheckStringArgument(propertyName, "propertyName");
            _propertyName = propertyName;
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private PropertyConstraintException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                _propertyName = info.GetString("PropertyName");
            }
        }

        /// <summary>
        /// sets the System.Runtime.Serialization.SerializationInfo
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context"></param>
        [SecurityCritical]
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("PropertyName", _propertyName);
        }

        /// <summary>
        /// Gets the name of the property that violated the constraint.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }
    }
}
