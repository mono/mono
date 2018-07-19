//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    public enum MessageDirection
    {
        Input,
        Output
    }

    static class MessageDirectionHelper
    {
        internal static bool IsDefined(MessageDirection value)
        {
            return (value == MessageDirection.Input || value == MessageDirection.Output);
        }

        internal static MessageDirection Opposite(MessageDirection d)
        {
            return d == MessageDirection.Input ? MessageDirection.Output : MessageDirection.Input;
        }
    }
}
