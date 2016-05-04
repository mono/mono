//------------------------------------------------------------------------------
//  <copyright file="SqlTriggerAttribute.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">daltudov</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">beysims</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">vadimt</owner>
//------------------------------------------------------------------------------

using System; 

namespace Microsoft.SqlServer.Server {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false), Serializable]
    public sealed class SqlTriggerAttribute : System.Attribute {
        private string m_fName;
        private string m_fTarget;
        private string m_fEvent;

        public SqlTriggerAttribute() {
            // default values
            m_fName = null;
            m_fTarget = null;
            m_fEvent = null;
        } 

        public string Name {
            get {
                return m_fName;
            }
            set {
                m_fName = value;
            }
        }

        public string Target {
            get {
                return m_fTarget;
            }
            set {
                m_fTarget = value;
            }
        }

        public string Event {
            get {
                return m_fEvent;
            }
            set {
                m_fEvent = value;
            }
        }
    }
}

