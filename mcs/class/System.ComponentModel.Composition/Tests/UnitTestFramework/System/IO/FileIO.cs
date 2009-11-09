// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Reflection;
using System.UnitTesting;

namespace System.IO 
{
    public static class FileIO
    {
        public const string RootTemporaryDirectoryName = "RootTempDirectory";

        private static string _temporaryDirectory;
        public static string GetRootTemporaryDirectory()
        {
            if (_temporaryDirectory == null)
            {
#if SILVERLIGHT
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), RootTemporaryDirectoryName);
#else
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RootTemporaryDirectoryName);
#endif

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                _temporaryDirectory = path;
            }

            return _temporaryDirectory;
        }

        public static string GetNewTemporaryDirectory()
        {
            string path = Path.Combine(GetRootTemporaryDirectory(), TestServices.GenerateRandomString());

            Directory.CreateDirectory(path);

            return path;
        }

        public static string GetTemporaryFileName(string extension)
        {
            return Path.Combine(GetRootTemporaryDirectory(), TestServices.GenerateRandomString() + "." + extension);
        }
    }
}
