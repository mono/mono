//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum DeadLetterQueue
    {
        None,
        System,
        Custom
    }

    static class DeadLetterQueueHelper
    {
        public static bool IsDefined(DeadLetterQueue mode)
        {
            return mode >= DeadLetterQueue.None && mode <= DeadLetterQueue.Custom;
        }
    }
}
