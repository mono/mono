//------------------------------------------------------------------------------
// <copyright file="Action.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;
    using System.Xml.Xsl.XsltOld.Debugger;

    internal abstract class Action {
        internal const int Initialized  =  0;
        internal const int Finished     = -1;

        internal abstract void Execute(Processor processor, ActionFrame frame);

        internal virtual void ReplaceNamespaceAlias(Compiler compiler){}

        // -------------- Debugger related stuff ---------
        // We have to pass ActionFrame to GetNavigator and GetVariables
        // because CopyCodeAction can't implement them without frame.count

        internal virtual DbgData GetDbgData(ActionFrame frame) { return DbgData.Empty; }
    }
}
