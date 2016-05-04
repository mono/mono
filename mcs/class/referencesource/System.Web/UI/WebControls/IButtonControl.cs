//------------------------------------------------------------------------------
// <copyright file="IButtonControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {
    
    public interface IButtonControl {


        /// <devdoc>
        /// Gets or sets whether pressing the button causes page validation to fire. 
        /// </devdoc>
        bool CausesValidation { get; set; }


        /// <devdoc>
        /// Gets or sets an optional argument that is propogated in
        /// the command event with the associated CommandName
        /// property.
        /// </devdoc>
        string CommandArgument { get; set; }
        

        /// <devdoc>
        /// Gets or sets the command associated with the button control that is propogated 
        /// in the command event along with the CommandArgument property.
        /// </devdoc>
        string CommandName { get; set; }
        

        /// <devdoc>
        /// Represents the method that will handle the Click event of a button control.
        /// </devdoc>
        event EventHandler Click;
        

        /// <devdoc>
        /// Represents the method that will handle the Command event of a button control.
        /// </devdoc>
        event CommandEventHandler Command;


        /// <devdoc>
        /// Gets or sets the target url associated with the button control. 
        /// </devdoc>
        string PostBackUrl { get; set; }


        /// <devdoc>
        /// The text for the button.
        /// </devdoc>
        string Text { get; set; }


        /// <devdoc>
        /// The name of the validation group for the button.
        /// </devdoc>
        string ValidationGroup { get; set; }
    }
}
