//------------------------------------------------------------------------------
// <copyright file="ImageMapEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// <para>Provides data for the ImageMap click event.</para>
    /// </devdoc>
    public class ImageMapEventArgs : EventArgs {

        private string _postBackValue;


        public ImageMapEventArgs(string value) {
            _postBackValue = value;
        }


        /// <devdoc>
        /// <para>Gets the value associated with the clicked area.</para>
        /// </devdoc>
        public string PostBackValue {
            get {
                return _postBackValue;
            }
        }
    }
}
