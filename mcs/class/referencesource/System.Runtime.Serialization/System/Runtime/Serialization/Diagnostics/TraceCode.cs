//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Diagnostics
{
    static class TraceCode
    {
        public const int Serialization = 0X30000; // soft link to System.ServiceModel.Diagnostics.TraceCode since they use ElementIgnored as well (for now)
        public const int WriteObjectBegin = TraceCode.Serialization | 0X0001;
        public const int WriteObjectEnd = TraceCode.Serialization | 0X0002;
        public const int WriteObjectContentBegin = TraceCode.Serialization | 0X0003;
        public const int WriteObjectContentEnd = TraceCode.Serialization | 0X0004;
        public const int ReadObjectBegin = TraceCode.Serialization | 0X0005;
        public const int ReadObjectEnd = TraceCode.Serialization | 0X0006;
        public const int ElementIgnored = TraceCode.Serialization | 0X0007;
        public const int XsdExportBegin = TraceCode.Serialization | 0X0008;
        public const int XsdExportEnd = TraceCode.Serialization | 0X0009;
        public const int XsdImportBegin = TraceCode.Serialization | 0X000A;
        public const int XsdImportEnd = TraceCode.Serialization | 0X000B;
        public const int XsdExportError = TraceCode.Serialization | 0X000C;
        public const int XsdImportError = TraceCode.Serialization | 0X000D;
        public const int XsdExportAnnotationFailed = TraceCode.Serialization | 0X000E;
        public const int XsdImportAnnotationFailed = TraceCode.Serialization | 0X000F;
        public const int XsdExportDupItems = TraceCode.Serialization | 0X0010;
        public const int FactoryTypeNotFound = TraceCode.Serialization | 0X0011;
        public const int ObjectWithLargeDepth = TraceCode.Serialization | 0X0012;
    }
}
