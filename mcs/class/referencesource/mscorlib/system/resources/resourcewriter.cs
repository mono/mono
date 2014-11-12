// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ResourceWriter
** 
** <OWNER>[....]</OWNER>
**
**
** Purpose: Default way to write strings to a CLR resource 
** file.
**
** 
===========================================================*/
namespace System.Resources {
    using System;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
#if FEATURE_SERIALIZATION
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
#endif // FEATURE_SERIALIZATION
    using System.Globalization;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;
    using System.Security;
    using System.Security.Permissions;

    // Generates a binary .resources file in the system default format 
    // from name and value pairs.  Create one with a unique file name,
    // call AddResource() at least once, then call Generate() to write
    // the .resources file to disk, then call Close() to close the file.
    // 
    // The resources generally aren't written out in the same order 
    // they were added.
    // 
    // See the RuntimeResourceSet overview for details on the system 
    // default file format.
    // 
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class ResourceWriter : IResourceWriter
    {

        private Func<Type, String> typeConverter;

        // Set this delegate to allow multi-targeting for .resources files.
        public Func<Type, String> TypeNameConverter
        {
            get
            {
                return typeConverter;
            }
            set
            {
                typeConverter = value;
            }
        }
        
        // For cases where users can't create an instance of the deserialized 
        // type in memory, and need to pass us serialized blobs instead.
        // LocStudio's managed code parser will do this in some cases.
        private class PrecannedResource
        {
            internal String TypeName;
            internal byte[] Data;

            internal PrecannedResource(String typeName, byte[] data)
            {
                TypeName = typeName;
                Data = data;
            }
        }

        private class StreamWrapper
        {
            internal Stream m_stream;
            internal bool m_closeAfterWrite;

            internal StreamWrapper(Stream s, bool closeAfterWrite)
            {
                m_stream = s;
                m_closeAfterWrite = closeAfterWrite;
            }
        }

        // An initial size for our internal sorted list, to avoid extra resizes.
        private const int _ExpectedNumberOfResources = 1000;
        private const int AverageNameSize = 20 * 2;  // chars in little endian Unicode
        private const int AverageValueSize = 40;

        private Dictionary<String, Object> _resourceList;
        private Stream _output;
        private Dictionary<String, Object> _caseInsensitiveDups;
        private Dictionary<String, PrecannedResource> _preserializedData;
        private const int _DefaultBufferSize = 4096;

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]        
        public ResourceWriter(String fileName)
        {
            if (fileName==null)
                throw new ArgumentNullException("fileName");
            Contract.EndContractBlock();
            _output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            _resourceList = new Dictionary<String, Object>(_ExpectedNumberOfResources, FastResourceComparer.Default);
            _caseInsensitiveDups = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
        }
    
        public ResourceWriter(Stream stream)
        {
            if (stream==null)
                throw new ArgumentNullException("stream");
            if (!stream.CanWrite)
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"));
            Contract.EndContractBlock();
            _output = stream;
            _resourceList = new Dictionary<String, Object>(_ExpectedNumberOfResources, FastResourceComparer.Default);
            _caseInsensitiveDups = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
        }
    
        // Adds a string resource to the list of resources to be written to a file.
        // They aren't written until Generate() is called.
        // 
        public void AddResource(String name, String value)
        {
            if (name==null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            _resourceList.Add(name, value);
        }
        
        // Adds a resource of type Object to the list of resources to be 
        // written to a file.  They aren't written until Generate() is called.
        // 
        public void AddResource(String name, Object value)
        {
            if (name==null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            // needed for binary compat
            if (value != null && value is Stream)
            {
                AddResourceInternal(name, (Stream)value, false);
            }
            else
            {
                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, value);
            }
        }

        // Adds a resource of type Stream to the list of resources to be 
        // written to a file.  They aren't written until Generate() is called.
        // Doesn't close the Stream when done.
        //
        public void AddResource(String name, Stream value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            AddResourceInternal(name, value, false);
        }

        // Adds a resource of type Stream to the list of resources to be 
        // written to a file.  They aren't written until Generate() is called.
        // closeAfterWrite parameter indicates whether to close the stream when done.
        // 
        public void AddResource(String name, Stream value, bool closeAfterWrite)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            AddResourceInternal(name, value, closeAfterWrite);
        }

        private void AddResourceInternal(String name, Stream value, bool closeAfterWrite)
        {
            if (value == null)
            {
                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, value);
            }
            else
            {
                // make sure the Stream is seekable
                if (!value.CanSeek)
                    throw new ArgumentException(Environment.GetResourceString("NotSupported_UnseekableStream"));

                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, new StreamWrapper(value, closeAfterWrite));
            }
        }

        // Adds a named byte array as a resource to the list of resources to 
        // be written to a file. They aren't written until Generate() is called.
        // 
        public void AddResource(String name, byte[] value)
        {
            if (name==null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            _resourceList.Add(name, value);
        }
        
        public void AddResourceData(String name, String typeName, byte[] serializedData)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (serializedData == null)
                throw new ArgumentNullException("serializedData");
            Contract.EndContractBlock();
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            if (_preserializedData == null)
                _preserializedData = new Dictionary<String, PrecannedResource>(FastResourceComparer.Default);

            _preserializedData.Add(name, new PrecannedResource(typeName, serializedData));
        }


        // Closes the output stream.
        public void Close()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) {
                if (_resourceList != null) {
                    Generate();
                }
                if (_output != null) {
                    _output.Close();
                }
            }
            _output = null;
            _caseInsensitiveDups = null;
            // _resourceList is set to null by Generate.
        }

        public void Dispose()
        {
            Dispose(true);
        }

        // After calling AddResource, Generate() writes out all resources to the 
        // output stream in the system default format.
        // If an exception occurs during object serialization or during IO,
        // the .resources file is closed and deleted, since it is most likely
        // invalid.
        [SecuritySafeCritical]  // Asserts permission to create & delete a temp file.
        public void Generate()
        {
            if (_resourceList == null)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));

            BinaryWriter bw = new BinaryWriter(_output, Encoding.UTF8);
            List<String> typeNames = new List<String>();
                
            // Write out the ResourceManager header
            // Write out magic number
            bw.Write(ResourceManager.MagicNumber);
                
            // Write out ResourceManager header version number
            bw.Write(ResourceManager.HeaderVersionNumber);

            MemoryStream resMgrHeaderBlob = new MemoryStream(240);
            BinaryWriter resMgrHeaderPart = new BinaryWriter(resMgrHeaderBlob);

            // Write out class name of IResourceReader capable of handling 
            // this file.
            resMgrHeaderPart.Write(MultitargetingHelpers.GetAssemblyQualifiedName(typeof(ResourceReader),typeConverter));

            // Write out class name of the ResourceSet class best suited to
            // handling this file.
            // This needs to be the same even with multi-targeting. It's the 
            // full name -- not the ----sembly qualified name.
            resMgrHeaderPart.Write(ResourceManager.ResSetTypeName);
            resMgrHeaderPart.Flush();

            // Write number of bytes to skip over to get past ResMgr header
            bw.Write((int)resMgrHeaderBlob.Length);

            // Write the rest of the ResMgr header
            bw.Write(resMgrHeaderBlob.GetBuffer(), 0, (int)resMgrHeaderBlob.Length);
            // End ResourceManager header


            // Write out the RuntimeResourceSet header
            // Version number
            bw.Write(RuntimeResourceSet.Version);
#if RESOURCE_FILE_FORMAT_DEBUG
            // Write out a tag so we know whether to enable or disable 
            // debugging support when reading the file.
            bw.Write("***DEBUG***");
#endif

            // number of resources
            int numResources = _resourceList.Count;
            if (_preserializedData != null)
                numResources += _preserializedData.Count;
            bw.Write(numResources);
                
            // Store values in temporary streams to write at end of file.
            int[] nameHashes = new int[numResources];
            int[] namePositions = new int[numResources];
            int curNameNumber = 0;
            MemoryStream nameSection = new MemoryStream(numResources * AverageNameSize);
            BinaryWriter names = new BinaryWriter(nameSection, Encoding.Unicode);

            // The data section can be very large, and worse yet, we can grow the byte[] used
            // for the data section repeatedly.  When using large resources like ~100 images,
            // this can lead to both a fragmented large object heap as well as allocating about
            // 2-3x of our storage needs in extra overhead.  Using a temp file can avoid this.
            // Assert permission to get a temp file name, which requires two permissions.
            // Additionally, we may be running under an account that doesn't have permission to
            // write to the temp directory (enforced via a Windows ACL).  Fall back to a MemoryStream.
            Stream dataSection = null;  // Either a FileStream or a MemoryStream
            String tempFile = null;

            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            permSet.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            try {
                permSet.Assert();
                tempFile = Path.GetTempFileName();
                File.SetAttributes(tempFile, FileAttributes.Temporary | FileAttributes.NotContentIndexed);
                // Explicitly opening with FileOptions.DeleteOnClose to avoid complicated File.Delete
                // (safe from ----s w/ antivirus software, etc)
                dataSection = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read,
                                             4096, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
            }
            catch (UnauthorizedAccessException) {
                // In case we're running under an account that can't access a temp directory.
                dataSection = new MemoryStream();
            }
            catch (IOException) {
                // In case Path.GetTempFileName fails because no unique file names are available
                dataSection = new MemoryStream();
            }
            finally {
                PermissionSet.RevertAssert();
            }

            using(dataSection) {
                BinaryWriter data = new BinaryWriter(dataSection, Encoding.UTF8);
#if FEATURE_SERIALIZATION
                IFormatter objFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence));
#endif // FEATURE_SERIALIZATION

#if RESOURCE_FILE_FORMAT_DEBUG
                // Write NAMES right before the names section.
                names.Write(new byte[] { (byte) 'N', (byte) 'A', (byte) 'M', (byte) 'E', (byte) 'S', (byte) '-', (byte) '-', (byte) '>'});
            
                // Write DATA at the end of the name table section.
                data.Write(new byte[] { (byte) 'D', (byte) 'A', (byte) 'T', (byte) 'A', (byte) '-', (byte) '-', (byte)'-', (byte)'>'});
#endif

                // We've stored our resources internally in a Hashtable, which 
                // makes no guarantees about the ordering while enumerating.  
                // While we do our own sorting of the resource names based on their
                // hash values, that's only sorting the nameHashes and namePositions
                // arrays.  That's all that is strictly required for correctness,
                // but for ease of generating a patch in the future that 
                // modifies just .resources files, we should re-sort them.

                SortedList sortedResources = new SortedList(_resourceList, FastResourceComparer.Default);
                if (_preserializedData != null) {
                    foreach (KeyValuePair<String, PrecannedResource> entry in _preserializedData)
                        sortedResources.Add(entry.Key, entry.Value);
                }

                IDictionaryEnumerator items = sortedResources.GetEnumerator();
                // Write resource name and position to the file, and the value
                // to our temporary buffer.  Save Type as well.
                while (items.MoveNext()) {
                    nameHashes[curNameNumber] = FastResourceComparer.HashFunction((String)items.Key);
                    namePositions[curNameNumber++] = (int)names.Seek(0, SeekOrigin.Current);
                    names.Write((String)items.Key); // key
                    names.Write((int)data.Seek(0, SeekOrigin.Current)); // virtual offset of value.
#if RESOURCE_FILE_FORMAT_DEBUG
                    names.Write((byte) '*');
#endif
                    Object value = items.Value;
                    ResourceTypeCode typeCode = FindTypeCode(value, typeNames);

                    // Write out type code
                    Write7BitEncodedInt(data, (int)typeCode);

                    // Write out value
                    PrecannedResource userProvidedResource = value as PrecannedResource;
                    if (userProvidedResource != null) {
                        data.Write(userProvidedResource.Data);
                    }
                    else {
#if FEATURE_SERIALIZATION
                        WriteValue(typeCode, value, data, objFormatter);
#else 
                        WriteValue(typeCode, value, data);
#endif
                    }

#if RESOURCE_FILE_FORMAT_DEBUG
                    data.Write(new byte[] { (byte) 'S', (byte) 'T', (byte) 'O', (byte) 'P'});
#endif
                }

                // At this point, the ResourceManager header has been written.
                // Finish RuntimeResourceSet header
                //   Write size & contents of class table
                bw.Write(typeNames.Count);
                for (int i = 0; i < typeNames.Count; i++)
                    bw.Write(typeNames[i]);

                // Write out the name-related items for lookup.
                //  Note that the hash array and the namePositions array must
                //  be sorted in parallel.
                Array.Sort(nameHashes, namePositions);

                //  Prepare to write sorted name hashes (alignment fixup)
                //   Note: For 64-bit machines, these MUST be aligned on 8 byte 
                //   boundaries!  Pointers on IA64 must be aligned!  And we'll
                //   run faster on X86 machines too.
                bw.Flush();
                int alignBytes = ((int)bw.BaseStream.Position) & 7;
                if (alignBytes > 0) {
                    for (int i = 0; i < 8 - alignBytes; i++)
                        bw.Write("PAD"[i % 3]);
                }

                //  Write out sorted name hashes.
                //   Align to 8 bytes.
                Contract.Assert((bw.BaseStream.Position & 7) == 0, "ResourceWriter: Name hashes array won't be 8 byte aligned!  Ack!");
#if RESOURCE_FILE_FORMAT_DEBUG
                bw.Write(new byte[] { (byte) 'H', (byte) 'A', (byte) 'S', (byte) 'H', (byte) 'E', (byte) 'S', (byte) '-', (byte) '>'} );
#endif
                foreach (int hash in nameHashes)
                    bw.Write(hash);
#if RESOURCE_FILE_FORMAT_DEBUG
                Console.Write("Name hashes: ");
                foreach(int hash in nameHashes)
                    Console.Write(hash.ToString("x")+"  ");
                Console.WriteLine();
#endif

                //  Write relative positions of all the names in the file.
                //   Note: this data is 4 byte aligned, occuring immediately 
                //   after the 8 byte aligned name hashes (whose length may 
                //   potentially be odd).
                Contract.Assert((bw.BaseStream.Position & 3) == 0, "ResourceWriter: Name positions array won't be 4 byte aligned!  Ack!");
#if RESOURCE_FILE_FORMAT_DEBUG
                bw.Write(new byte[] { (byte) 'P', (byte) 'O', (byte) 'S', (byte) '-', (byte) '-', (byte) '-', (byte) '-', (byte) '>' } );
#endif
                foreach (int pos in namePositions)
                    bw.Write(pos);
#if RESOURCE_FILE_FORMAT_DEBUG
                Console.Write("Name positions: ");
                foreach(int pos in namePositions)
                    Console.Write(pos.ToString("x")+"  ");
                Console.WriteLine();
#endif

                // Flush all BinaryWriters to their underlying streams.
                bw.Flush();
                names.Flush();
                data.Flush();

                // Write offset to data section
                int startOfDataSection = (int)(bw.Seek(0, SeekOrigin.Current) + nameSection.Length);
                startOfDataSection += 4;  // We're writing an int to store this data, adding more bytes to the header
                BCLDebug.Log("RESMGRFILEFORMAT", "Generate: start of DataSection: 0x" + startOfDataSection.ToString("x", CultureInfo.InvariantCulture) + "  nameSection length: " + nameSection.Length);
                bw.Write(startOfDataSection);

                // Write name section.
                bw.Write(nameSection.GetBuffer(), 0, (int)nameSection.Length);
                names.Close();

                // Write data section.
                Contract.Assert(startOfDataSection == bw.Seek(0, SeekOrigin.Current), "ResourceWriter::Generate - start of data section is wrong!");
                dataSection.Position = 0;
                dataSection.CopyTo(bw.BaseStream);
                data.Close();
            } // using(dataSection)  <--- Closes dataSection, which was opened w/ FileOptions.DeleteOnClose
            bw.Flush();

            // Indicate we've called Generate
            _resourceList = null;
        }

        // Finds the ResourceTypeCode for a type, or adds this type to the
        // types list.
        private ResourceTypeCode FindTypeCode(Object value, List<String> types)
        {
            if (value == null)
                return ResourceTypeCode.Null;

            Type type = value.GetType();
            if (type == typeof(String))
                return ResourceTypeCode.String;
            else if (type == typeof(Int32))
                return ResourceTypeCode.Int32;
            else if (type == typeof(Boolean))
                return ResourceTypeCode.Boolean;
            else if (type == typeof(Char))
                return ResourceTypeCode.Char;
            else if (type == typeof(Byte))
                return ResourceTypeCode.Byte;
            else if (type == typeof(SByte))
                return ResourceTypeCode.SByte;
            else if (type == typeof(Int16))
                return ResourceTypeCode.Int16;
            else if (type == typeof(Int64))
                return ResourceTypeCode.Int64;
            else if (type == typeof(UInt16))
                return ResourceTypeCode.UInt16;
            else if (type == typeof(UInt32))
                return ResourceTypeCode.UInt32;
            else if (type == typeof(UInt64))
                return ResourceTypeCode.UInt64;
            else if (type == typeof(Single))
                return ResourceTypeCode.Single;
            else if (type == typeof(Double))
                return ResourceTypeCode.Double;
            else if (type == typeof (Decimal))
                return ResourceTypeCode.Decimal;
            else if (type == typeof(DateTime))
                return ResourceTypeCode.DateTime;
            else if (type == typeof(TimeSpan))
                return ResourceTypeCode.TimeSpan;
            else if (type == typeof(byte[]))
                return ResourceTypeCode.ByteArray;
            else if (type == typeof(StreamWrapper))
                return ResourceTypeCode.Stream;

            
            // This is a user type, or a precanned resource.  Find type 
            // table index.  If not there, add new element.
            String typeName;
            if (type == typeof(PrecannedResource)) {
                typeName = ((PrecannedResource)value).TypeName;
                if (typeName.StartsWith("ResourceTypeCode.", StringComparison.Ordinal)) {
                    typeName = typeName.Substring(17);  // Remove through '.'
                    ResourceTypeCode typeCode = (ResourceTypeCode)Enum.Parse(typeof(ResourceTypeCode), typeName);
                    return typeCode;
                }
            }
            else 
            {
                typeName = MultitargetingHelpers.GetAssemblyQualifiedName(type, typeConverter);
            }

            int typeIndex = types.IndexOf(typeName);
            if (typeIndex == -1) {
                typeIndex = types.Count;
                types.Add(typeName);
            }

            return (ResourceTypeCode)(typeIndex + ResourceTypeCode.StartOfUserTypes);
        }

#if FEATURE_SERIALIZATION
        private void WriteValue(ResourceTypeCode typeCode, Object value, BinaryWriter writer, IFormatter objFormatter)
#else 
        private void WriteValue(ResourceTypeCode typeCode, Object value, BinaryWriter writer)                                                         
#endif // FEATURE_SERIALIZATION
        {
            Contract.Requires(writer != null);

            switch(typeCode) {
            case ResourceTypeCode.Null:
                break;

            case ResourceTypeCode.String:
                writer.Write((String) value);
                break;

            case ResourceTypeCode.Boolean:
                writer.Write((bool) value);
                break;

            case ResourceTypeCode.Char:
                writer.Write((UInt16) (char) value);
                break;

            case ResourceTypeCode.Byte:
                writer.Write((byte) value);
                break;

            case ResourceTypeCode.SByte:
                writer.Write((sbyte) value);
                break;
                
            case ResourceTypeCode.Int16:
                writer.Write((Int16) value);
                break;
                
            case ResourceTypeCode.UInt16:
                writer.Write((UInt16) value);
                break;

            case ResourceTypeCode.Int32:
                writer.Write((Int32) value);
                break;
                
            case ResourceTypeCode.UInt32:
                writer.Write((UInt32) value);
                break;

            case ResourceTypeCode.Int64:
                writer.Write((Int64) value);
                break;
                
            case ResourceTypeCode.UInt64:
                writer.Write((UInt64) value);
                break;

            case ResourceTypeCode.Single:
                writer.Write((Single) value);
                break;
                
            case ResourceTypeCode.Double:
                writer.Write((Double) value);
                break;

            case ResourceTypeCode.Decimal:
                writer.Write((Decimal) value);
                break;

            case ResourceTypeCode.DateTime:
                // Use DateTime's ToBinary & FromBinary.
                Int64 data = ((DateTime) value).ToBinary();
                writer.Write(data);
                break;
                
            case ResourceTypeCode.TimeSpan:
                writer.Write(((TimeSpan) value).Ticks);
                break;

            // Special Types
            case ResourceTypeCode.ByteArray:
                {
                    byte[] bytes = (byte[]) value;
                    writer.Write(bytes.Length);
                    writer.Write(bytes, 0, bytes.Length);
                    break;
                }

            case ResourceTypeCode.Stream:
                {
                    StreamWrapper sw = (StreamWrapper)value;
                    if (sw.m_stream.GetType() == typeof(MemoryStream))
                    {
                        MemoryStream ms = (MemoryStream)sw.m_stream;
                        if (ms.Length > Int32.MaxValue)
                            throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_StreamLength"));
                        int offset, len;
                        ms.InternalGetOriginAndLength(out offset, out len);
                        byte[] bytes = ms.InternalGetBuffer();
                        writer.Write(len);
                        writer.Write(bytes, offset, len);
                    }
                    else 
                    {
                        Stream s = sw.m_stream;
                        // we've already verified that the Stream is seekable
                        if (s.Length > Int32.MaxValue)
                            throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_StreamLength"));

                        s.Position = 0;
                        writer.Write((int)s.Length);
                        byte[] buffer = new byte[_DefaultBufferSize];
                        int read = 0;
                        while ((read = s.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            writer.Write(buffer, 0, read);
                        }
                        if (sw.m_closeAfterWrite)
                        {
                            s.Close();
                        }
                    }
                    break;
                }

            default:
                Contract.Assert(typeCode >= ResourceTypeCode.StartOfUserTypes, String.Format(CultureInfo.InvariantCulture, "ResourceReader: Unsupported ResourceTypeCode in .resources file!  {0}", typeCode));
#if FEATURE_SERIALIZATION
                objFormatter.Serialize(writer.BaseStream, value);
                break;
#else
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_ResourceObjectSerialization"));
#endif // FEATURE_SERIALIZATION  
            }
        }

        private static void Write7BitEncodedInt(BinaryWriter store, int value) {
            Contract.Requires(store != null);
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint) value;   // support negative numbers
            while (v >= 0x80) {
                store.Write((byte) (v | 0x80));
                v >>= 7;
            }
            store.Write((byte)v);
        }
    }
}
