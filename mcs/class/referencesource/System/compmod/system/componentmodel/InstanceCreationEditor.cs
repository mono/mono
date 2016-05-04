 //------------------------------------------------------------------------------
// <copyright file="TypeConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <devdoc>
    ///     An InstanceCreationEditor allows the user to create an instance of a particular type of property from a dropdown
    ///     Within the PropertyGrid.  Usually, the text specified by InstanceCreationEditor.Text will be displayed on the 
    ///     dropdown from the PropertyGrid as a link or button.  When clicked, the InstanceCreationEditor.CreateInstance
    ///     method will be called with the Type of the object to create.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class InstanceCreationEditor {

        /// <devdoc>
        /// </devdoc>
        public virtual string Text {
            get {
                return SR.GetString(SR.InstanceCreationEditorDefaultText);
            }
        }

        /// <devdoc>
        /// This method is invoked when you user chooses the link displayed by the PropertyGrid for the InstanceCreationEditor.
        /// The object returned from this method must be an instance of the specified type, or null in which case the editor will do nothing.
        ///
        /// </devdoc>
        public abstract object CreateInstance(ITypeDescriptorContext context, Type instanceType);
    }
}





