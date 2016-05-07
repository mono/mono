//------------------------------------------------------------------------------
// <copyright file="INotifyPropertyChanging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.ComponentModel
{
    public interface INotifyPropertyChanging
    {
        /// <devdoc>
        /// </devdoc>
        event PropertyChangingEventHandler PropertyChanging;
    }
}
