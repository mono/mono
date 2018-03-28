//------------------------------------------------------------------------------
// <copyright file="SqlUserDefinedAggregateAttribute.cs" company="Microsoft Corporation">
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
// <owner current="false" primary="false">venkar</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System; 
    using System.Data;
    using System.Data.Common;
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple=false, Inherited=false)]
    public sealed class SqlUserDefinedAggregateAttribute: Attribute {
        private int m_MaxByteSize;
        private bool m_fInvariantToDup;
        private bool m_fInvariantToNulls;
        private bool m_fInvariantToOrder = true;
        private bool m_fNullIfEmpty;
        private Format m_format;
        private string m_fName;
        
        // The maximum value for the maxbytesize field, in bytes.
        public const int MaxByteSizeValue = 8000;

        // A required attribute on all udaggs, used to indicate that the
        // given type is a udagg, and its storage format.
        public SqlUserDefinedAggregateAttribute(Format format) {
            switch(format) {
            case Format.Unknown:
                throw ADP.NotSupportedUserDefinedTypeSerializationFormat((Microsoft.SqlServer.Server.Format)format, "format");
            case Format.Native:
            case Format.UserDefined:
                this.m_format = format;
                break;
            default:
                throw ADP.InvalidUserDefinedTypeSerializationFormat((Microsoft.SqlServer.Server.Format)format);
            }
        }

        // The maximum size of this instance, in bytes. Does not have to be
        // specified for Native format serialization. The maximum value
        // for this property is specified by MaxByteSizeValue.
        public int MaxByteSize {
            get {
                return this.m_MaxByteSize;
            }
            set {
                // MaxByteSize of -1 means 2GB and is valid, as well as 0 to MaxByteSizeValue
                if (value < -1 || value > MaxByteSizeValue) {
                    throw ADP.ArgumentOutOfRange(Res.GetString(Res.SQLUDT_MaxByteSizeValue), "MaxByteSize", value);
                }
                this.m_MaxByteSize = value;
            }
        }

        public bool IsInvariantToDuplicates {
            get {
                return this.m_fInvariantToDup;
            }
            set {
                this.m_fInvariantToDup = value;
            }
        }

        public bool IsInvariantToNulls {
            get {
                return this.m_fInvariantToNulls;
            }
            set {
                this.m_fInvariantToNulls = value;
            }
        }

        public bool IsInvariantToOrder {
            get {
                return this.m_fInvariantToOrder;
            }
            set {
                this.m_fInvariantToOrder = value;
            }
        }

        public bool IsNullIfEmpty {
            get {
                return this.m_fNullIfEmpty;
            }
            set {
                this.m_fNullIfEmpty = value;
            }
        }

        // The on-disk format for this type.
        public Format Format {
            get {
                return this.m_format;
            }
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

