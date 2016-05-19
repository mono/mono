//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Debug
{
    using System;
    using System.Collections.Generic;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Debug;
    using System.Activities.Debugger;
    using System.Activities.Presentation.Model;


    public interface IDesignerDebugView
    {
        SourceLocation CurrentContext
        {
            get; set;
        }
        SourceLocation CurrentLocation
        {
            get; set;
        }

        bool IsDebugging
        {
            get; set;
        }

        // Hide source file name from the xaml.
        bool HideSourceFileName
        {
            get;
            set;
        }

        SourceLocation SelectedLocation
        {
            get;
        }

        IDictionary<SourceLocation, BreakpointTypes> GetBreakpointLocations();

        void ResetBreakpoints();
        void DeleteBreakpoint(SourceLocation sourceLocation);
        SourceLocation GetExactLocation(SourceLocation approximateLocation);
        void InsertBreakpoint(SourceLocation sourceLocation, BreakpointTypes breakpointType);
        void UpdateBreakpoint(SourceLocation sourceLocation, BreakpointTypes breakpointType);
        void EnsureVisible(SourceLocation sourceLocation);
    }
}
