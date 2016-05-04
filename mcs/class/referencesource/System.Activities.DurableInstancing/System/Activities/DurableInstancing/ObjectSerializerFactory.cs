//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime;

    static class ObjectSerializerFactory
    {
        public static IObjectSerializer GetObjectSerializer(InstanceEncodingOption instanceEncodingOption)
        {
            IObjectSerializer result = null;

            switch (instanceEncodingOption)
            {
                case InstanceEncodingOption.None:
                    result = new DefaultObjectSerializer();
                    break;
                case InstanceEncodingOption.GZip:
                    result = new GZipObjectSerializer();
                    break;
                default:
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        SR.UnknownCompressionOption(instanceEncodingOption)));
            }

            return result;
        }

        public static IObjectSerializer GetDefaultObjectSerializer()
        {
            return new DefaultObjectSerializer();
        }
    }
}
