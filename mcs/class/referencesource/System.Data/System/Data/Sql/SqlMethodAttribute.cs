//------------------------------------------------------------------------------
//  <copyright file="SqlMethodAttribute.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="true">daltudov</owner>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">beysims</owner>
// <owner current="true" primary="false">junfang</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">vadimt</owner>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.SqlServer.Server {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false), Serializable]
    public sealed class SqlMethodAttribute : SqlFunctionAttribute {
        private bool m_fCallOnNullInputs;
        private bool m_fMutator;
        private bool m_fInvokeIfReceiverIsNull;

        public SqlMethodAttribute() {
            // default values
            m_fCallOnNullInputs = true;
            m_fMutator = false;
            m_fInvokeIfReceiverIsNull = false;

        } // SqlMethodAttribute

        public bool OnNullCall {
            get {
                return m_fCallOnNullInputs;
            }
            set {
                m_fCallOnNullInputs = value;
            }
        } // CallOnNullInputs

        public bool IsMutator {
            get {
                return m_fMutator;
            }
            set {
                m_fMutator = value;
            }
        } // IsMutator

        public bool InvokeIfReceiverIsNull {
            get {
                return m_fInvokeIfReceiverIsNull;
            }
            set {
                m_fInvokeIfReceiverIsNull = value;
            }
        } // InvokeIfReceiverIsNull
    } // class SqlMethodAttribute
}
