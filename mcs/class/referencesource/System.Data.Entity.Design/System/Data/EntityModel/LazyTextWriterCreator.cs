//---------------------------------------------------------------------
// <copyright file="LazyTextWriterCreator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// This class is responsible for abstracting the knowledge
    /// of whether the user provided a TextWriter or a FilePath.
    /// 
    /// If the user gave us a filePath we try not to create the TextWriter
    /// till we absolutely need it in order to prevent the file from being created
    /// in error cases.
    /// </summary>
    internal class LazyTextWriterCreator : IDisposable
    {
        private bool _ownTextWriter;
        private TextWriter _writer = null;
        private string _targetFilePath = null;

        internal LazyTextWriterCreator(TextWriter writer)
        {
            Debug.Assert(writer != null, "writer parameter is null");

            _ownTextWriter = false;
            _writer = writer;
        }

        [ResourceExposure(ResourceScope.Machine)] //The target file path is used to open a stream which is a machine resource.
        internal LazyTextWriterCreator(string targetFilePath)
        {
            Debug.Assert(targetFilePath != null, "targetFilePath parameter is null");

            _ownTextWriter = true;
            _targetFilePath = targetFilePath;
        }

        [ResourceExposure(ResourceScope.None)] //The resource( target file path) is not exposed to the callers of this method
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For StreamWriter constructor and we pick 
                                                                            //the target file path from class variable.
        internal TextWriter GetOrCreateTextWriter()
        {
            if (_writer == null)
            {
                // lazy creating the writer
                _writer = new StreamWriter(_targetFilePath);
            }
            return _writer;
        }

        internal string TargetFilePath
        {
            get { return _targetFilePath; }
        }

        internal bool IsUserSuppliedTextWriter
        {
            get { return !_ownTextWriter; }
        }

        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            if (_ownTextWriter && _writer != null)
            {
                _writer.Dispose();
            }
        }
    }
}
