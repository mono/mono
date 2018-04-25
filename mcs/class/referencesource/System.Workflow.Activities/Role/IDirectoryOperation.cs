#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

#endregion

namespace System.Workflow.Activities
{
    internal interface IDirectoryOperation
    {
        void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response);
    }

}
