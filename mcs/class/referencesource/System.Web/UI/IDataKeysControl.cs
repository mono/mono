//------------------------------------------------------------------------------
// <copyright file="IDataKeysControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI.WebControls;

    public interface IDataKeysControl {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
                         Justification="Required by ASP.NET Parser.")]
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        string[] ClientIDRowSuffix { get; }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        DataKeyArray ClientIDRowSuffixDataKeys { get; }
    }
}
