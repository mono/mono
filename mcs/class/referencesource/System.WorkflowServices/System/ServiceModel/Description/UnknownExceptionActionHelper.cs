//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;

    static class UnknownExceptionActionHelper
    {
        public static bool IsDefined(UnknownExceptionAction action)
        {
            return action == UnknownExceptionAction.AbortInstance ||
                action == UnknownExceptionAction.TerminateInstance;
        }
    }
}
