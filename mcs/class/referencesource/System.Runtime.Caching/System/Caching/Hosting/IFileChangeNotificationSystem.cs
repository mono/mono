// <copyright file="IFileChangeNotificationSystem.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching.Hosting {
    public interface IFileChangeNotificationSystem {
        void StartMonitoring(string filePath, OnChangedCallback onChangedCallback, out Object state, out DateTimeOffset lastWriteTime, out long fileSize);

        void StopMonitoring(string filePath, Object state);
    }
}
