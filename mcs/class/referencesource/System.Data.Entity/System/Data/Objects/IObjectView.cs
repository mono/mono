//---------------------------------------------------------------------
// <copyright file="IObjectView.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.ComponentModel;

namespace System.Data.Objects
{
    internal interface IObjectView
    {
        void EntityPropertyChanged(object sender, PropertyChangedEventArgs e);
        void CollectionChanged(object sender, CollectionChangeEventArgs e);
    }
}
