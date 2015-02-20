// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    /// <summary>
    /// An enumeration of values that represent states of activity validation.
    /// </summary>
    public enum ValidationState
    {
        // The numeric values of each of the enum values indicate the severity of the error.
        // The higher the number is, the more severe the error. See the MarkError method in the
        // ValidationService class for example of usage.

        /// <summary>
        /// A value that indicates that an error occurred during the validation of an activity. 
        /// The numeric values of each of the enumeration values indicate the severity of the error. The value associated with the error state is 3
        /// </summary>
        Error = 3,

        /// <summary>
        /// A value that indicates that a warning occurred during the validation of an activity. 
        /// The numeric values of each of the enumeration values indicate the severity of the error. The value associated with the warning state is 2.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// A value that represents that validation found a child activity to be invalid. 
        /// The numeric values of each of the enumeration values indicate the severity of the error. The value associated with the invalid child activity is 1.
        /// </summary>
        ChildInvalid = 1,

        /// <summary>
        /// A value that indicates that an activity is valid. 
        /// The numeric values of each of the enumeration values indicate the severity of the error. The value associated with the valid state is 0.
        /// </summary>
        Valid = 0
    }
}
