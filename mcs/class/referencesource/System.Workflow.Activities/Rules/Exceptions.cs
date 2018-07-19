// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation - All Rights Reserved
// ---------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel.Compiler;

namespace System.Workflow.Activities.Rules
{
    #region RuleException
    /// <summary>
    /// Represents the base class for all rule engine exception classes
    /// </summary>
    [Serializable]
    public class RuleException : Exception, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the RuleException class
        /// </summary>
        public RuleException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleException class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public RuleException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleException class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="ex">The inner exception</param>
        public RuleException(string message, Exception ex)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Constructor required by for Serialization - initialize a new instance from serialized data
        /// </summary>
        /// <param name="serializeInfo">Reference to the object that holds the data needed to deserialize the exception</param>
        /// <param name="context">Provides the means for deserializing the exception data</param>
        protected RuleException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
        }
    }
    #endregion

    #region RuleEvaluationException
    /// <summary>
    /// Represents the the exception thrown when an error is encountered during evaluation
    /// </summary>
    [Serializable]
    public class RuleEvaluationException : RuleException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the RuleRuntimeException class
        /// </summary>
        public RuleEvaluationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleRuntimeException class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public RuleEvaluationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleRuntimeException class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="ex">The inner exception</param>
        public RuleEvaluationException(string message, Exception ex)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Constructor required by for Serialization - initialize a new instance from serialized data
        /// </summary>
        /// <param name="serializeInfo">Reference to the object that holds the data needed to deserialize the exception</param>
        /// <param name="context">Provides the means for deserializing the exception data</param>
        protected RuleEvaluationException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
        }
    }
    #endregion

    #region RuleEvaluationIncompatibleTypesException
    /// <summary>
    /// Represents the exception thrown when types are incompatible
    /// </summary>
    [Serializable]
    public class RuleEvaluationIncompatibleTypesException : RuleException, ISerializable
    {
        private Type m_leftType;
        private CodeBinaryOperatorType m_op;
        private Type m_rightType;

        /// <summary>
        /// Type on the left of the operator
        /// </summary>
        public Type Left
        {
            get { return m_leftType; }
            set { m_leftType = value; }
        }

        /// <summary>
        /// Arithmetic operation that failed
        /// </summary>
        public CodeBinaryOperatorType Operator
        {
            get { return m_op; }
            set { m_op = value; }
        }

        /// <summary>
        /// Type on the right of the operator
        /// </summary>
        public Type Right
        {
            get { return m_rightType; }
            set { m_rightType = value; }
        }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationIncompatibleTypesException class
        /// </summary>
        public RuleEvaluationIncompatibleTypesException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationIncompatibleTypesException class
        /// </summary>
        /// <param name="message"></param>
        public RuleEvaluationIncompatibleTypesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationIncompatibleTypesException class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public RuleEvaluationIncompatibleTypesException(string message, Exception ex)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationIncompatibleTypesException class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="left"></param>
        /// <param name="op"></param>
        /// <param name="right"></param>
        public RuleEvaluationIncompatibleTypesException(
            string message,
            Type left,
            CodeBinaryOperatorType op,
            Type right)
            : base(message)
        {
            m_leftType = left;
            m_op = op;
            m_rightType = right;
        }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationIncompatibleTypesException class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="left"></param>
        /// <param name="op"></param>
        /// <param name="right"></param>
        /// <param name="ex"></param>
        public RuleEvaluationIncompatibleTypesException(
            string message,
            Type left,
            CodeBinaryOperatorType op,
            Type right,
            Exception ex)
            : base(message, ex)
        {
            m_leftType = left;
            m_op = op;
            m_rightType = right;
        }

        /// <summary>
        /// Constructor required by for Serialization - initialize a new instance from serialized data
        /// </summary>
        /// <param name="serializeInfo">Reference to the object that holds the data needed to deserialize the exception</param>
        /// <param name="context">Provides the means for deserializing the exception data</param>
        protected RuleEvaluationIncompatibleTypesException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
            if (serializeInfo == null)
                throw new ArgumentNullException("serializeInfo");
            string qualifiedTypeString = serializeInfo.GetString("left");
            if (qualifiedTypeString != "null")
                m_leftType = Type.GetType(qualifiedTypeString);
            m_op = (CodeBinaryOperatorType)serializeInfo.GetValue("op", typeof(CodeBinaryOperatorType));
            qualifiedTypeString = serializeInfo.GetString("right");
            if (qualifiedTypeString != "null")
                m_rightType = Type.GetType(qualifiedTypeString);
        }

        /// <summary>
        /// Implements the ISerializable interface
        /// </summary>
        /// <param name="info">Reference to the object that holds the data needed to serialize/deserialize the exception</param>
        /// <param name="context">Provides the means for serialiing the exception data</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
            info.AddValue("left", (m_leftType != null) ? m_leftType.AssemblyQualifiedName : "null");
            info.AddValue("op", m_op);
            info.AddValue("right", (m_rightType != null) ? m_rightType.AssemblyQualifiedName : "null");
        }
    }
    #endregion

    #region RuleSetValidationException
    /// <summary>
    /// Represents the exception thrown when a ruleset can not be validated
    /// </summary>
    [Serializable]
    public class RuleSetValidationException : RuleException, ISerializable
    {
        private ValidationErrorCollection m_errors;

        /// <summary>
        /// Collection of validation errors that occurred while validating the RuleSet
        /// </summary>
        public ValidationErrorCollection Errors
        {
            get { return m_errors; }
        }

        /// <summary>
        /// Initializes a new instance of the RuleSetValidationException class
        /// </summary>
        public RuleSetValidationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleSetValidationException class
        /// </summary>
        /// <param name="message"></param>
        public RuleSetValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleSetValidationException class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public RuleSetValidationException(string message, Exception ex)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RuleSetValidationException class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errors"></param>
        public RuleSetValidationException(
            string message,
            ValidationErrorCollection errors)
            : base(message)
        {
            m_errors = errors;
        }

        /// <summary>
        /// Constructor required by for Serialization - initialize a new instance from serialized data
        /// </summary>
        /// <param name="serializeInfo">Reference to the object that holds the data needed to deserialize the exception</param>
        /// <param name="context">Provides the means for deserializing the exception data</param>
        protected RuleSetValidationException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
            if (serializeInfo == null)
                throw new ArgumentNullException("serializeInfo");
            m_errors = (ValidationErrorCollection)serializeInfo.GetValue("errors", typeof(ValidationErrorCollection));
        }

        /// <summary>
        /// Implements the ISerializable interface
        /// </summary>
        /// <param name="info">Reference to the object that holds the data needed to serialize/deserialize the exception</param>
        /// <param name="context">Provides the means for serialiing the exception data</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
            info.AddValue("errors", m_errors);
        }
    }
    #endregion
}
