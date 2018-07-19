//------------------------------------------------------------------------------
// <copyright file="IValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI {

    /// <devdoc>
    ///    <para>Defines the contract that the validation controls must implement.</para>
    /// </devdoc>
    public interface IValidator {    
                

        /// <devdoc>
        ///    <para>Indicates whether the content entered in a control is valid.</para>
        /// </devdoc>
        bool IsValid {
            get;
            set;
        }
        

        /// <devdoc>
        ///    <para>Indicates the error message text generated when the control's content is not 
        ///       valid.</para>
        /// </devdoc>
        string ErrorMessage { 
            get;
            set;
        }
                

        /// <devdoc>
        ///    <para>Compares the entered content with the valid parameters provided by the 
        ///       validation control.</para>
        /// </devdoc>
        void Validate();
    }              
}


