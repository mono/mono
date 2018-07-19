// <copyright file="FileChangeMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections.ObjectModel;

namespace System.Runtime.Caching {
    public abstract class FileChangeMonitor : ChangeMonitor {
        public abstract ReadOnlyCollection<string> FilePaths { get; }
        public abstract DateTimeOffset LastModified { get; }
    }
}
