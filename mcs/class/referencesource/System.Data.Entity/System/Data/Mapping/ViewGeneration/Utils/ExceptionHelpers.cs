//---------------------------------------------------------------------
// <copyright file="ExceptionHelpers.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Text;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Diagnostics;
using System.Data.Common.Utils;

namespace System.Data.Mapping.ViewGeneration.Utils
{

    // Miscellaneous helper routines for generating mapping exceptions
    internal static class ExceptionHelpers
    {
        internal static void ThrowMappingException(ErrorLog.Record errorRecord, ConfigViewGenerator config)
        {
            InternalMappingException exception = new InternalMappingException(errorRecord.ToUserString(), errorRecord);
            if (config.IsNormalTracing)
            {
                exception.ErrorLog.PrintTrace();
            }
            throw exception;
        }

        internal static void ThrowMappingException(ErrorLog errorLog, ConfigViewGenerator config)
        {
            InternalMappingException exception = new InternalMappingException(errorLog.ToUserString(), errorLog);
            if (config.IsNormalTracing)
            {
                exception.ErrorLog.PrintTrace();
            }
            throw exception;
        }
    }
}
