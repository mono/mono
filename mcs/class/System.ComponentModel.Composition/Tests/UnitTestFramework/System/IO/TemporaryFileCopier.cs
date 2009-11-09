// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;

namespace System.IO 
{
    public class TemporaryFileCopier : TemporaryDirectory
    {
        public TemporaryFileCopier(params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                File.Copy(fileName, Path.Combine(DirectoryPath, Path.GetFileName(fileName)));
            }
        }
    }
}

#endif