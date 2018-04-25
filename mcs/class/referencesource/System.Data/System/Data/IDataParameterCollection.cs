//------------------------------------------------------------------------------
// <copyright file="IDataParameterCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IDataParameterCollection : System.Collections.IList {

        object this[string parameterName] {
            get;
            set;
        }

        bool Contains(string parameterName);

        int IndexOf(string parameterName);

        void RemoveAt(string parameterName);
    }
}
