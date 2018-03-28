//------------------------------------------------------------------------------
// <copyright file="HierarchicalDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;

    public abstract class HierarchicalDataSourceView {


        public abstract IHierarchicalEnumerable Select();
    }
}
