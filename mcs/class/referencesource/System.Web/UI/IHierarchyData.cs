//------------------------------------------------------------------------------
// <copyright file="IHierarchyData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;

    public interface IHierarchyData {


        // properties
        bool HasChildren { get; }


        string Path { get; }


        object Item { get; }


        string Type { get; }


        // methods
        IHierarchicalEnumerable GetChildren();


        IHierarchyData GetParent();
    }
}


