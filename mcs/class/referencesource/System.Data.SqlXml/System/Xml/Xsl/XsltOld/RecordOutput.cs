//------------------------------------------------------------------------------
// <copyright file="RecordOutput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">sdub</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;

    internal interface RecordOutput {
        Processor.OutputResult RecordDone(RecordBuilder record);
        void TheEnd();
    }

}
