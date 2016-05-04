//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System.Collections.Generic;

    //commanding interface - used for integration of workflow designer actions (like context menu) with custom provided
    //implementation - i.e. property browser service, debugging service, etc.
    public interface ICommandService
    {
        //returns if given command id is supported 
        bool IsCommandSupported(int commandId);
        //verifies if given command can be executed, throws NotSupportedExecption if command is not supported
        bool CanExecuteCommand(int commandId);
        //executes command with given id and parameters, throws NotSupportedException if command is not supported
        void ExecuteCommand(int commandId, Dictionary<string, object> parameters);
    }

    public static class CommandValues
    {
        public const int ShowProperties = 5;
        public const int InsertBreakpoint = 6;
        public const int DeleteBreakpoint = 7;
        public const int EnableBreakpoint = 8;
        public const int DisableBreakpoint = 9;
    }
}
