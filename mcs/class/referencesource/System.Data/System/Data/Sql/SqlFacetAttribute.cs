//------------------------------------------------------------------------------
//  <copyright file="SqlFacetAttribute.cs" company="Microsoft Corporation">
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

    [ AttributeUsage( AttributeTargets.Field | AttributeTargets.Property |
                      AttributeTargets.ReturnValue | AttributeTargets.Parameter,
                      AllowMultiple = false,
                      Inherited = false ) ]
    public class SqlFacetAttribute: Attribute {
        private bool    m_IsFixedLength;
        private int     m_MaxSize;
        private int     m_Scale;
        private int     m_Precision;
        private bool    m_IsNullable;

        // Is this a fixed size field?
        public bool IsFixedLength {
            get {
                return this.m_IsFixedLength;
            }
            set {
                this.m_IsFixedLength = value;
            }
        }

        // The maximum size of the field (in bytes or characters depending on the field type)
        //  or -1 if the size can be unlimited.
        public int MaxSize {
            get {
                return this.m_MaxSize;
            }
            set {
                this.m_MaxSize = value;
            }
        }

        // Precision, only valid for numeric types.
        public int Precision {
            get {
                return this.m_Precision;
            }
            set {
                this.m_Precision = value;
            }
        }

        // Scale, only valid for numeric types.
        public int Scale {
            get {
                return this.m_Scale;
            }
            set {
                this.m_Scale = value;
            }
        }

        // Is this field nullable?
        public bool IsNullable {
            get {
                return this.m_IsNullable;
            }
            set {
                this.m_IsNullable = value;
            }
        }
    }
}
