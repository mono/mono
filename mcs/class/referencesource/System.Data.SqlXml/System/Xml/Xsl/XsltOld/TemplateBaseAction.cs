//------------------------------------------------------------------------------
// <copyright file="TemplateBaseAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Xml;
    using System.Xml.XPath;
    using System.Globalization;

    // RootAction and TemplateActions have a litle in common -- they are responsible for variable allocation
    // TemplateBaseAction -- implenemts this shared behavior

    internal abstract class TemplateBaseAction : ContainerAction {
        protected int variableCount;      // space to allocate on frame for variables
        private   int variableFreeSlot;   // compile time counter responsiable for variable placement logic

        public int AllocateVariableSlot() {
            // Variable placement logic. Optimized
            int thisSlot = this.variableFreeSlot;
            this.variableFreeSlot ++;
            if(this.variableCount < this.variableFreeSlot) {
                this.variableCount = this.variableFreeSlot;
            }
            return thisSlot;
        }

        public void ReleaseVariableSlots(int n) {
        // This code does optimisation of variable placement. Comented out for this version
        //      Reuse of the variable disable the check that variable was assigned before the actual use
        //      this check has to be done in compile time n future.
//            this.variableFreeSlot -= n;
        }
    }
}
