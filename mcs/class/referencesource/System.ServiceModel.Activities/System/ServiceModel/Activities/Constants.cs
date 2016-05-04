//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.ServiceModel.Channels;

    static class Constants
    {
        public const string CorrelatesWith = "CorrelatesWith";
        public const string EndpointAddress = "EndpointAddress";
        public const string Message = "Message";
        public const string Parameter = "Parameter";
        public const string RequestMessage = "RequestMessage";
        public const string Result = "Result";
        public const string TransactionHandle = "TransactionHandle";
        public const string NoPersistHandle = "noPersistHandle";

        public static readonly Type MessageType = typeof(Message);
        public static readonly Type CorrelationHandleType = typeof(CorrelationHandle);
        public static readonly Type UriType = typeof(Uri);
        public static readonly Type NoPersistHandleType = typeof(NoPersistHandle);

        public static readonly object[] EmptyArray = new object[0];
        public static readonly string[] EmptyStringArray = new string[0];
        public static readonly Type[] EmptyTypeArray = new Type[0];
    }
}
