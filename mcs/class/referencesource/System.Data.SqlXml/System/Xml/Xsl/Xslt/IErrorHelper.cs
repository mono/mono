//------------------------------------------------------------------------------
// <copyright file="IErrorHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">antonl</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl {

    internal interface IErrorHelper {

        void ReportError(string res, params string[] args);

        void ReportWarning(string res, params string[] args);
    }
}
