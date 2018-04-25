#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
using System.Web.Resources;
#else
using Microsoft.VSDesigner.WCF.Resources;
#endif

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// Map file loader
    /// </summary>
    /// <remarks>
    /// MapFileLoader instance and MapFile instance should be 1:1 mapping.
    /// </remarks>
#if WEB_EXTENSIONS_CODE
    internal abstract class MapFileLoader
#else
    [CLSCompliant(true)]
    public abstract class MapFileLoader
#endif
    {
        /// <summary>
        /// Save the given map file.
        /// </summary>
        /// <param name="mapFile">The map file to be saved</param>
        public void SaveMapFile(MapFile mapFile)
        {
            Debug.Assert(mapFile != null, "mapFile is null!");

            SaveExternalFiles(mapFile);

            using (var mapFileWriter = GetMapFileWriter())
            {
                GetMapFileSerializer().Serialize(mapFileWriter, Unwrap(mapFile));
            }
        }

        /// <summary>
        /// Load the map file.
        /// </summary>        
        /// <returns>Concrete map file instance.</returns>
        public MapFile LoadMapFile()
        {
            MapFile mapFile = null;

            using (var mapFileReader = GetMapFileReader())
            {
                var proxyGenerationErrors = new List<ProxyGenerationError>();

                ValidationEventHandler handler =
                    (sender, e) =>
                    {
                        bool isError = (e.Severity == XmlSeverityType.Error);
                        proxyGenerationErrors.Add(
                            new ProxyGenerationError(ProxyGenerationError.GeneratorState.LoadMetadata,
                                                     MapFileName,
                                                     e.Exception,
                                                     !isError));

                        if (isError)
                        {
                            throw e.Exception;
                        }
                    };

                var readerSettings = new XmlReaderSettings()
                {
                    Schemas = GetMapFileSchemaSet(),
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
                };

                using (XmlReader reader = XmlReader.Create(mapFileReader, readerSettings, string.Empty))
                {
                    try
                    {
                        readerSettings.ValidationEventHandler += handler;

                        mapFile = ReadMapFile(reader);

                        SetMapFileLoadErrors(mapFile, proxyGenerationErrors);
                    }
                    finally
                    {
                        readerSettings.ValidationEventHandler -= handler;
                    }
                }
            }

            if (mapFile != null)
            {
                LoadExternalFiles(mapFile);
            }

            return mapFile;
        }

        /// <summary>
        /// Load metadata file from file system
        /// </summary>
        public void LoadMetadataFile(MetadataFile metadataFile)
        {
            try
            {
                metadataFile.CleanUpContent();
                metadataFile.LoadContent(ReadMetadataFile(metadataFile.FileName));
            }
            catch (Exception ex)
            {
                metadataFile.ErrorInLoading = ex;
            }
        }

        /// <summary>
        /// Load extension file
        /// </summary>
        public void LoadExtensionFile(ExtensionFile extensionFile)
        {
            try
            {
                extensionFile.CleanUpContent();
                extensionFile.ContentBuffer = ReadExtensionFile(extensionFile.FileName);
            }
            catch (Exception ex)
            {
                extensionFile.ErrorInLoading = ex;
            }
        }

        #region protected abstract methods

        /// <summary>
        /// The name of the file where the MapFile instance is loaded.
        /// </summary>
        protected abstract string MapFileName { get; }

        /// <summary>
        /// Wrap the map file impl.
        /// </summary>
        protected abstract MapFile Wrap(object mapFileImpl);

        /// <summary>
        /// Unwrap the map file.
        /// </summary>
        protected abstract object Unwrap(MapFile mapFile);

        /// <summary>
        /// Get the map file schema set
        /// </summary>
        /// <return>Xml schema set of the map file</return>
        protected abstract XmlSchemaSet GetMapFileSchemaSet();

        /// <summary>
        /// Get the map file serializer
        /// </summary>
        /// <returns>Xml serializer of the map file</returns>
        protected abstract XmlSerializer GetMapFileSerializer();

        /// <summary>
        /// Get access to a text reader that gets access to the map file byte stream
        /// </summary>
        /// <returns>Text reader of the map file</returns>
        protected virtual TextReader GetMapFileReader()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get access to a text writer that writes the map file byte stream.
        /// </summary>
        /// <returns>Text writer of the map file</returns>
        protected virtual TextWriter GetMapFileWriter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get access to a byte array that contain the contents of the given metadata
        /// file
        /// </summary>
        /// <param name="name">
        /// Name of the metadata file. Could be a path relative to the svcmap file location
        /// or the name of an item in a metadata storage.
        /// </param>
        /// <returns>Content of the metadata file</returns>
        protected virtual byte[] ReadMetadataFile(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write the metadata file.
        /// </summary>
        /// <param name="file">The metadata file to be written</param>
        protected virtual void WriteMetadataFile(MetadataFile file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get access to a byte array that contain the contents of the given extension
        /// file
        /// </summary>
        /// <param name="name">
        /// Name of the extension file. Could be a path relative to the svcmap file location
        /// or the name of an item in a metadata storage.
        /// </param>
        /// <returns>Content of the extension file</returns>
        protected virtual byte[] ReadExtensionFile(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write the extension file.
        /// </summary>
        /// <param name="file">The extension file to be written</param>
        protected virtual void WriteExtensionFile(ExtensionFile file)
        {
            throw new NotImplementedException();
        }

        #endregion protected abstract methods

        #region private methods

        private MapFile ReadMapFile(XmlReader reader)
        {
            try
            {
                return Wrap(GetMapFileSerializer().Deserialize(reader));
            }
            catch (InvalidOperationException ex)
            {
                XmlException xmlException = ex.InnerException as XmlException;
                if (xmlException != null)
                {
                    // the innerException contains detail error message
                    throw xmlException;
                }

                XmlSchemaException schemaException = ex.InnerException as XmlSchemaException;
                if (schemaException != null)
                {
                    if (schemaException.LineNumber > 0)
                    {
                        // append line/position to the message
                        throw new XmlSchemaException(String.Format(CultureInfo.CurrentCulture,
                                                                   WCFModelStrings.ReferenceGroup_AppendLinePosition,
                                                                   schemaException.Message,
                                                                   schemaException.LineNumber,
                                                                   schemaException.LinePosition),
                                                     schemaException,
                                                     schemaException.LineNumber,
                                                     schemaException.LinePosition);
                    }
                    else
                    {
                        throw schemaException;
                    }
                }

                // It's something we can't handle, throw it.
                throw;
            }
        }

        private void SaveExternalFiles(MapFile mapFile)
        {
            // KEEP the order! The name of metadata files could be adjusted when we save them.

            foreach (MetadataFile metadataFile in mapFile.MetadataList)
            {
                if (metadataFile.ErrorInLoading == null)
                {
                    WriteMetadataFile(metadataFile);
                }
            }

            foreach (ExtensionFile extensionFile in mapFile.Extensions)
            {
                if (extensionFile.ErrorInLoading == null)
                {
                    WriteExtensionFile(extensionFile);
                }
            }
        }

        private void LoadExternalFiles(MapFile mapFile)
        {
            // Do basic check for metadata files and extension files.
            ValidateMapFile(mapFile);

            foreach (MetadataFile metadataFile in mapFile.MetadataList)
            {
                metadataFile.IsExistingFile = true;
                LoadMetadataFile(metadataFile);
            }

            foreach (ExtensionFile extensionFile in mapFile.Extensions)
            {
                extensionFile.IsExistingFile = true;
                LoadExtensionFile(extensionFile);
            }
        }

        private void ValidateMapFile(MapFile mapFile)
        {
            var metadataFileNames = mapFile.MetadataList.Select(p => p.FileName).Where(p => !string.IsNullOrEmpty(p));
            var extensionFileNames = mapFile.Extensions.Select(p => p.FileName).Where(p => !string.IsNullOrEmpty(p));

            var fileNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string fileName in metadataFileNames.Concat(extensionFileNames))
            {
                if (!fileNameSet.Contains(fileName))
                {
                    fileNameSet.Add(fileName);
                }
                else
                {
                    throw new FormatException(String.Format(CultureInfo.CurrentCulture,
                                                            WCFModelStrings.ReferenceGroup_TwoExternalFilesWithSameName,
                                                            fileName));
                }
            }
        }

        private void SetMapFileLoadErrors(MapFile mapFile, IEnumerable<ProxyGenerationError> proxyGenerationErrors)
        {
            mapFile.LoadErrors = proxyGenerationErrors;
        }

        #endregion private methods
    }
}
