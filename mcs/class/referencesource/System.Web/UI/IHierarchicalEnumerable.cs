//------------------------------------------------------------------------------
// <copyright file="IHierarchicalEnumerable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;

    public interface IHierarchicalEnumerable : IEnumerable {


        IHierarchyData GetHierarchyData(object enumeratedItem);
    }
}
