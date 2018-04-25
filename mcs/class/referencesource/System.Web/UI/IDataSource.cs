//------------------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;

    public interface IDataSource {

        event EventHandler DataSourceChanged;


        DataSourceView GetView(string viewName);


        ICollection GetViewNames();
    }
}


