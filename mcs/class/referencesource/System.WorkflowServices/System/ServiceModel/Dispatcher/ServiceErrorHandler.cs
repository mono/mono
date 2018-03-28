//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections;

    class ServiceErrorHandler : DurableErrorHandler
    {
        const string dataKey = "System.ServiceModel.Dispatcher.ServiceErrorHandler.MarkExeption";

        public ServiceErrorHandler(bool debug)
            : base(debug)
        {
        }

        public static void MarkException(Exception toMark)
        {
            // From MSDN: The OutOfMemoryException, StackOverflowException and ThreadAbortException
            // classes always return a null reference for the value of the Data property.
            // These are fatal exceptions and therefore we don't care that we can't mark them.
            IDictionary data = toMark.Data;
            if (data != null && !data.IsReadOnly && !data.IsFixedSize)
            {
                data.Add(dataKey, true);
            }
        }

        protected override bool IsUserCodeException(Exception error)
        {
            IDictionary data = error.Data;

            if (data != null && data.Contains(dataKey))
            {
                return true;
            }

            return false;
        }
    }
}
