//------------------------------------------------------------------------------
// <copyright file="IHierarchicalDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    public interface IHierarchicalDataSource {


        // events
        event EventHandler DataSourceChanged;


        // methods
        HierarchicalDataSourceView GetHierarchicalView(string viewPath);
    }
}


