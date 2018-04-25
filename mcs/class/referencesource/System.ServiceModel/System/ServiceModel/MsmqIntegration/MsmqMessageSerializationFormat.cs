//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    public enum MsmqMessageSerializationFormat
    {
        Xml,
        Binary,
        ActiveX,
        ByteArray,
        Stream
    }

    static class MsmqMessageSerializationFormatHelper
    {
        internal static bool IsDefined(MsmqMessageSerializationFormat value)
        {
            return
                value == MsmqMessageSerializationFormat.ActiveX ||
                value == MsmqMessageSerializationFormat.Binary ||
                value == MsmqMessageSerializationFormat.ByteArray ||
                value == MsmqMessageSerializationFormat.Stream ||
                value == MsmqMessageSerializationFormat.Xml;
        }
    }
}

