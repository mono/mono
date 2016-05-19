//------------------------------------------------------------------------------
// <copyright file="IDesignerHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;

    /// <devdoc>
    ///    <para>
    ///       Methods for the Designer host to report on the state of transactions.
    ///    </para>
    /// </devdoc>
    public interface IDesignerHostTransactionState {
        bool IsClosingTransaction { get; }
    }
}

