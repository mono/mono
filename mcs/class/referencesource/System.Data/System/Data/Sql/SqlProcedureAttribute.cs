//------------------------------------------------------------------------------
//  <copyright file="SqlProcedureAttribute.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="true">daltudov</owner>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">beysims</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">vadimt</owner>
//------------------------------------------------------------------------------

using System; 

namespace Microsoft.SqlServer.Server {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false), Serializable]
    public sealed class SqlProcedureAttribute : System.Attribute {
    
        private string m_fName;

        public SqlProcedureAttribute() {
            // default values
            m_fName = null;
        } 
        
        public string Name {
            get {
                return m_fName;
            }
            set {
                m_fName = value;
            }
        }
    }
}
