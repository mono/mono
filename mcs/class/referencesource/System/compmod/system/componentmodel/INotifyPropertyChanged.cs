//------------------------------------------------------------------------------
// <copyright file="INotifyPropertyChanged.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.ComponentModel
{
    public interface INotifyPropertyChanged
    {
        /// <devdoc>
        /// </devdoc>
        event PropertyChangedEventHandler PropertyChanged;
    }
}
