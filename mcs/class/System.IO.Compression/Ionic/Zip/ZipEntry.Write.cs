//#define Trace

// ZipEntry.Write.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// Last Saved: <2011-July-30 14:55:47>
//
// ------------------------------------------------------------------
//
// This module defines logic for writing (saving) the ZipEntry into a
// zip file.
//
// ------------------------------------------------------------------


using System;
using System.IO;
using RE = System.Text.RegularExpressions;

namespace Ionic.Zip
{
    internal partial class ZipEntry
    {
        internal void WriteCentralDirectoryEntry(Stream s)
        {
            byte[] bytes = new byte[4096];
            int i = 0;
            // signature
            bytes[i++] = (byte)(ZipConstants.ZipDirEntrySignature & 0x000000FF);
            bytes[i++] = (byte)((ZipConstants.ZipDirEntrySignature & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((ZipConstants.ZipDirEntrySignature & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((ZipConstants.ZipDirEntrySignature & 0xFF000000) >> 24);

            // Version Made By
            // workitem 7071
            // We must not overwrite the VersionMadeBy field when writing out a zip
            // archive.  The VersionMadeBy tells the zip reader the meaning of the
            // File attributes.  Overwriting the VersionMadeBy will result in
            // inconsistent metadata.  Consider the scenario where the application
            // opens and reads a zip file that had been created on Linux. Then the
            // app adds one file to the Zip archive, and saves it.  The file
            // attributes for all the entries added on Linux will be significant for
            // Linux.  Therefore the VersionMadeBy for those entries must not be
            // changed.  Only the entries that are actually created on Windows NTFS
            // should get the VersionMadeBy indicating Windows/NTFS.
            bytes[i++] = (byte)(_VersionMadeBy & 0x00FF);
            bytes[i++] = (byte)((_VersionMadeBy & 0xFF00) >> 8);

            // Apparently we want to duplicate the extra field here; we cannot
            // simply zero it out and assume tools and apps will use the right one.

            ////Int16 extraFieldLengthSave = (short)(_EntryHeader[28] + _EntryHeader[29] * 256);
            ////_EntryHeader[28] = 0;
            ////_EntryHeader[29] = 0;

            // Version Needed, Bitfield, compression method, lastmod,
            // crc, compressed and uncompressed sizes, filename length and extra field length.
            // These are all present in the local file header, but they may be zero values there.
            // So we cannot just copy them.

            // workitem 11969: Version Needed To Extract in central directory must be
            // the same as the local entry or MS .NET System.IO.Zip fails read.
            Int16 vNeeded = (Int16)(VersionNeeded != 0 ? VersionNeeded : 20);
            // workitem 12964
            if (_OutputUsesZip64==null)
            {
                // a zipentry in a zipoutputstream, with zero bytes written
                _OutputUsesZip64 = new Nullable<bool>(_container.Zip64 == Zip64Option.Always);
            }

            Int16 versionNeededToExtract = (Int16)(_OutputUsesZip64.Value ? 45 : vNeeded);
#if BZIP
            if (this.CompressionMethod == Ionic.Zip.CompressionMethod.BZip2)
                versionNeededToExtract = 46;
#endif

            bytes[i++] = (byte)(versionNeededToExtract & 0x00FF);
            bytes[i++] = (byte)((versionNeededToExtract & 0xFF00) >> 8);

            bytes[i++] = (byte)(_BitField & 0x00FF);
            bytes[i++] = (byte)((_BitField & 0xFF00) >> 8);

            bytes[i++] = (byte)(_CompressionMethod & 0x00FF);
            bytes[i++] = (byte)((_CompressionMethod & 0xFF00) >> 8);

#if AESCRYPTO
            if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
            Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                i -= 2;
                bytes[i++] = 0x63;
                bytes[i++] = 0;
            }
#endif

            bytes[i++] = (byte)(_TimeBlob & 0x000000FF);
            bytes[i++] = (byte)((_TimeBlob & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((_TimeBlob & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((_TimeBlob & 0xFF000000) >> 24);
            bytes[i++] = (byte)(_Crc32 & 0x000000FF);
            bytes[i++] = (byte)((_Crc32 & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((_Crc32 & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((_Crc32 & 0xFF000000) >> 24);

            int j = 0;
            if (_OutputUsesZip64.Value)
            {
                // CompressedSize (Int32) and UncompressedSize - all 0xFF
                for (j = 0; j < 8; j++)
                    bytes[i++] = 0xFF;
            }
            else
            {
                bytes[i++] = (byte)(_CompressedSize & 0x000000FF);
                bytes[i++] = (byte)((_CompressedSize & 0x0000FF00) >> 8);
                bytes[i++] = (byte)((_CompressedSize & 0x00FF0000) >> 16);
                bytes[i++] = (byte)((_CompressedSize & 0xFF000000) >> 24);

                bytes[i++] = (byte)(_UncompressedSize & 0x000000FF);
                bytes[i++] = (byte)((_UncompressedSize & 0x0000FF00) >> 8);
                bytes[i++] = (byte)((_UncompressedSize & 0x00FF0000) >> 16);
                bytes[i++] = (byte)((_UncompressedSize & 0xFF000000) >> 24);
            }

            byte[] fileNameBytes = GetEncodedFileNameBytes();
            Int16 filenameLength = (Int16)fileNameBytes.Length;
            bytes[i++] = (byte)(filenameLength & 0x00FF);
            bytes[i++] = (byte)((filenameLength & 0xFF00) >> 8);

            // do this again because now we have real data
            _presumeZip64 = _OutputUsesZip64.Value;

            // workitem 11131
            //
            // cannot generate the extra field again, here's why: In the case of a
            // zero-byte entry, which uses encryption, DotNetZip will "remove" the
            // encryption from the entry.  It does this in PostProcessOutput; it
            // modifies the entry header, and rewrites it, resetting the Bitfield
            // (one bit indicates encryption), and potentially resetting the
            // compression method - for AES the Compression method is 0x63, and it
            // would get reset to zero (no compression).  It then calls SetLength()
            // to truncate the stream to remove the encryption header (12 bytes for
            // AES256).  But, it leaves the previously-generated "Extra Field"
            // metadata (11 bytes) for AES in the entry header. This extra field
            // data is now "orphaned" - it refers to AES encryption when in fact no
            // AES encryption is used. But no problem, the PKWARE spec says that
            // unrecognized extra fields can just be ignored. ok.  After "removal"
            // of AES encryption, the length of the Extra Field can remains the
            // same; it's just that there will be 11 bytes in there that previously
            // pertained to AES which are now unused. Even the field code is still
            // there, but it will be unused by readers, as the encryption bit is not
            // set.
            //
            // Re-calculating the Extra field now would produce a block that is 11
            // bytes shorter, and that mismatch - between the extra field in the
            // local header and the extra field in the Central Directory - would
            // cause problems. (where? why? what problems?)  So we can't do
            // that. It's all good though, because though the content may have
            // changed, the length definitely has not. Also, the _EntryHeader
            // contains the "updated" extra field (after PostProcessOutput) at
            // offset (30 + filenameLength).

            _Extra = ConstructExtraField(true);

            Int16 extraFieldLength = (Int16)((_Extra == null) ? 0 : _Extra.Length);
            bytes[i++] = (byte)(extraFieldLength & 0x00FF);
            bytes[i++] = (byte)((extraFieldLength & 0xFF00) >> 8);

            // File (entry) Comment Length
            // the _CommentBytes private field was set during WriteHeader()
            int commentLength = (_CommentBytes == null) ? 0 : _CommentBytes.Length;

            // the size of our buffer defines the max length of the comment we can write
            if (commentLength + i > bytes.Length) commentLength = bytes.Length - i;
            bytes[i++] = (byte)(commentLength & 0x00FF);
            bytes[i++] = (byte)((commentLength & 0xFF00) >> 8);

            // Disk number start
            bool segmented = (this._container.ZipFile != null) &&
                (this._container.ZipFile.MaxOutputSegmentSize != 0);
            if (segmented) // workitem 13915
            {
                // Emit nonzero disknumber only if saving segmented archive.
                bytes[i++] = (byte)(_diskNumber & 0x00FF);
                bytes[i++] = (byte)((_diskNumber & 0xFF00) >> 8);
            }
            else
            {
                // If reading a segmneted archive and saving to a regular archive,
                // ZipEntry._diskNumber will be non-zero but it should be saved as
                // zero.
                bytes[i++] = 0;
                bytes[i++] = 0;
            }

            // internal file attrs
            // workitem 7801
            bytes[i++] = (byte)((_IsText) ? 1 : 0); // lo bit: filetype hint.  0=bin, 1=txt.
            bytes[i++] = 0;

            // external file attrs
            // workitem 7071
            bytes[i++] = (byte)(_ExternalFileAttrs & 0x000000FF);
            bytes[i++] = (byte)((_ExternalFileAttrs & 0x0000FF00) >> 8);
            bytes[i++] = (byte)((_ExternalFileAttrs & 0x00FF0000) >> 16);
            bytes[i++] = (byte)((_ExternalFileAttrs & 0xFF000000) >> 24);

            // workitem 11131
            // relative offset of local header.
            //
            // If necessary to go to 64-bit value, then emit 0xFFFFFFFF,
            // else write out the value.
            //
            // Even if zip64 is required for other reasons - number of the entry
            // > 65534, or uncompressed size of the entry > MAX_INT32, the ROLH
            // need not be stored in a 64-bit field .
            if (_RelativeOffsetOfLocalHeader > 0xFFFFFFFFL) // _OutputUsesZip64.Value
            {
                bytes[i++] = 0xFF;
                bytes[i++] = 0xFF;
                bytes[i++] = 0xFF;
                bytes[i++] = 0xFF;
            }
            else
            {
                bytes[i++] = (byte)(_RelativeOffsetOfLocalHeader & 0x000000FF);
                bytes[i++] = (byte)((_RelativeOffsetOfLocalHeader & 0x0000FF00) >> 8);
                bytes[i++] = (byte)((_RelativeOffsetOfLocalHeader & 0x00FF0000) >> 16);
                bytes[i++] = (byte)((_RelativeOffsetOfLocalHeader & 0xFF000000) >> 24);
            }

            // actual filename
            Buffer.BlockCopy(fileNameBytes, 0, bytes, i, filenameLength);
            i += filenameLength;

            // "Extra field"
            if (_Extra != null)
            {
                // workitem 11131
                //
                // copy from EntryHeader if available - it may have been updated.
                // if not, copy from Extra. This would be unnecessary if I just
                // updated the Extra field when updating EntryHeader, in
                // PostProcessOutput.

                //?? I don't understand why I wouldn't want to just use
                // the recalculated Extra field. ??

                // byte[] h = _EntryHeader ?? _Extra;
                // int offx = (h == _EntryHeader) ? 30 + filenameLength : 0;
                // Buffer.BlockCopy(h, offx, bytes, i, extraFieldLength);
                // i += extraFieldLength;

                byte[] h = _Extra;
                int offx = 0;
                Buffer.BlockCopy(h, offx, bytes, i, extraFieldLength);
                i += extraFieldLength;
            }

            // file (entry) comment
            if (commentLength != 0)
            {
                // now actually write the comment itself into the byte buffer
                Buffer.BlockCopy(_CommentBytes, 0, bytes, i, commentLength);
                // for (j = 0; (j < commentLength) && (i + j < bytes.Length); j++)
                //     bytes[i + j] = _CommentBytes[j];
                i += commentLength;
            }

            s.Write(bytes, 0, i);
        }


#if INFOZIP_UTF8
        static private bool FileNameIsUtf8(char[] FileNameChars)
        {
            bool isUTF8 = false;
            bool isUnicode = false;
            for (int j = 0; j < FileNameChars.Length; j++)
            {
                byte[] b = System.BitConverter.GetBytes(FileNameChars[j]);
                isUnicode |= (b.Length != 2);
                isUnicode |= (b[1] != 0);
                isUTF8 |= ((b[0] & 0x80) != 0);
            }

            return isUTF8;
        }
#endif


        private byte[] ConstructExtraField(bool forCentralDirectory)
        {
            var listOfBlocks = new System.Collections.Generic.List<byte[]>();
            byte[] block;

            // Conditionally emit an extra field with Zip64 information.  If the
            // Zip64 option is Always, we emit the field, before knowing that it's
            // necessary.  Later, if it turns out this entry does not need zip64,
            // we'll set the header ID to rubbish and the data will be ignored.
            // This results in additional overhead metadata in the zip file, but
            // it will be small in comparison to the entry data.
            //
            // On the other hand if the Zip64 option is AsNecessary and it's NOT
            // for the central directory, then we do the same thing.  Or, if the
            // Zip64 option is AsNecessary and it IS for the central directory,
            // and the entry requires zip64, then emit the header.
            if (_container.Zip64 == Zip64Option.Always ||
                (_container.Zip64 == Zip64Option.AsNecessary &&
                 (!forCentralDirectory || _entryRequiresZip64.Value)))
            {
                // add extra field for zip64 here
                // workitem 7924
                int sz = 4 + (forCentralDirectory ? 28 : 16);
                block = new byte[sz];
                int i = 0;

                if (_presumeZip64 || forCentralDirectory)
                {
                    // HeaderId = always use zip64 extensions.
                    block[i++] = 0x01;
                    block[i++] = 0x00;
                }
                else
                {
                    // HeaderId = dummy data now, maybe set to 0x0001 (ZIP64) later.
                    block[i++] = 0x99;
                    block[i++] = 0x99;
                }

                // DataSize
                block[i++] = (byte)(sz - 4);  // decimal 28 or 16  (workitem 7924)
                block[i++] = 0x00;

                // The actual metadata - we may or may not have real values yet...

                // uncompressed size
                Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, block, i, 8);
                i += 8;
                // compressed size
                Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, block, i, 8);
                i += 8;

                // workitem 7924 - only include this if the "extra" field is for
                // use in the central directory.  It is unnecessary and not useful
                // for local header; makes WinZip choke.
                if (forCentralDirectory)
                {
                    // relative offset
                    Array.Copy(BitConverter.GetBytes(_RelativeOffsetOfLocalHeader), 0, block, i, 8);
                    i += 8;

                    // starting disk number
                    Array.Copy(BitConverter.GetBytes(0), 0, block, i, 4);
                }
                listOfBlocks.Add(block);
            }


#if AESCRYPTO
            if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                block = new byte[4 + 7];
                int i = 0;
                // extra field for WinZip AES
                // header id
                block[i++] = 0x01;
                block[i++] = 0x99;

                // data size
                block[i++] = 0x07;
                block[i++] = 0x00;

                // vendor number
                block[i++] = 0x01;  // AE-1 - means "Verify CRC"
                block[i++] = 0x00;

                // vendor id "AE"
                block[i++] = 0x41;
                block[i++] = 0x45;

                // key strength
                int keystrength = GetKeyStrengthInBits(Encryption);
                if (keystrength == 128)
                    block[i] = 1;
                else if (keystrength == 256)
                    block[i] = 3;
                else
                    block[i] = 0xFF;
                i++;

                // actual compression method
                block[i++] = (byte)(_CompressionMethod & 0x00FF);
                block[i++] = (byte)(_CompressionMethod & 0xFF00);

                listOfBlocks.Add(block);
            }
#endif

            if (_ntfsTimesAreSet && _emitNtfsTimes)
            {
                block = new byte[32 + 4];
                // HeaderId   2 bytes    0x000a == NTFS times
                // Datasize   2 bytes    32
                // reserved   4 bytes    ?? don't care
                // timetag    2 bytes    0x0001 == NTFS time
                // size       2 bytes    24 == 8 bytes each for ctime, mtime, atime
                // mtime      8 bytes    win32 ticks since win32epoch
                // atime      8 bytes    win32 ticks since win32epoch
                // ctime      8 bytes    win32 ticks since win32epoch
                int i = 0;
                // extra field for NTFS times
                // header id
                block[i++] = 0x0a;
                block[i++] = 0x00;

                // data size
                block[i++] = 32;
                block[i++] = 0;

                i += 4; // reserved

                // time tag
                block[i++] = 0x01;
                block[i++] = 0x00;

                // data size (again)
                block[i++] = 24;
                block[i++] = 0;

                Int64 z = _Mtime.ToFileTime();
                Array.Copy(BitConverter.GetBytes(z), 0, block, i, 8);
                i += 8;
                z = _Atime.ToFileTime();
                Array.Copy(BitConverter.GetBytes(z), 0, block, i, 8);
                i += 8;
                z = _Ctime.ToFileTime();
                Array.Copy(BitConverter.GetBytes(z), 0, block, i, 8);
                i += 8;

                listOfBlocks.Add(block);
            }

            if (_ntfsTimesAreSet && _emitUnixTimes)
            {
                int len = 5 + 4;
                if (!forCentralDirectory) len += 8;

                block = new byte[len];
                // local form:
                // --------------
                // HeaderId   2 bytes    0x5455 == unix timestamp
                // Datasize   2 bytes    13
                // flags      1 byte     7 (low three bits all set)
                // mtime      4 bytes    seconds since unix epoch
                // atime      4 bytes    seconds since unix epoch
                // ctime      4 bytes    seconds since unix epoch
                //
                // central directory form:
                //---------------------------------
                // HeaderId   2 bytes    0x5455 == unix timestamp
                // Datasize   2 bytes    5
                // flags      1 byte     7 (low three bits all set)
                // mtime      4 bytes    seconds since unix epoch
                //
                int i = 0;
                // extra field for "unix" times
                // header id
                block[i++] = 0x55;
                block[i++] = 0x54;

                // data size
                block[i++] = unchecked((byte)(len - 4));
                block[i++] = 0;

                // flags
                block[i++] = 0x07;

                Int32 z = unchecked((int)((_Mtime - _unixEpoch).TotalSeconds));
                Array.Copy(BitConverter.GetBytes(z), 0, block, i, 4);
                i += 4;
                if (!forCentralDirectory)
                {
                    z = unchecked((int)((_Atime - _unixEpoch).TotalSeconds));
                    Array.Copy(BitConverter.GetBytes(z), 0, block, i, 4);
                    i += 4;
                    z = unchecked((int)((_Ctime - _unixEpoch).TotalSeconds));
                    Array.Copy(BitConverter.GetBytes(z), 0, block, i, 4);
                    i += 4;
                }
                listOfBlocks.Add(block);
            }


            // inject other blocks here...


            // concatenate any blocks we've got:
            byte[] aggregateBlock = null;
            if (listOfBlocks.Count > 0)
            {
                int totalLength = 0;
                int i, current = 0;
                for (i = 0; i < listOfBlocks.Count; i++)
                    totalLength += listOfBlocks[i].Length;
                aggregateBlock = new byte[totalLength];
                for (i = 0; i < listOfBlocks.Count; i++)
                {
                    System.Array.Copy(listOfBlocks[i], 0, aggregateBlock, current, listOfBlocks[i].Length);
                    current += listOfBlocks[i].Length;
                }
            }

            return aggregateBlock;
        }



        // private System.Text.Encoding GenerateCommentBytes()
        // {
        //     var getEncoding = new Func<System.Text.Encoding>({
        //     switch (AlternateEncodingUsage)
        //     {
        //         case ZipOption.Always:
        //             return AlternateEncoding;
        //         case ZipOption.Never:
        //             return ibm437;
        //     }
        //     var cb = ibm437.GetBytes(_Comment);
        //     // need to use this form of GetString() for .NET CF
        //     string s1 = ibm437.GetString(cb, 0, cb.Length);
        //     if (s1 == _Comment)
        //         return ibm437;
        //     return AlternateEncoding;
        //     });
        //
        //     var encoding = getEncoding();
        //     _CommentBytes = encoding.GetBytes(_Comment);
        //     return encoding;
        // }


        private string NormalizeFileName()
        {
            // here, we need to flip the backslashes to forward-slashes,
            // also, we need to trim the \\server\share syntax from any UNC path.
            // and finally, we need to remove any leading .\

            string SlashFixed = FileName.Replace("\\", "/");
            string s1 = null;
            if ((_TrimVolumeFromFullyQualifiedPaths) && (FileName.Length >= 3)
                && (FileName[1] == ':') && (SlashFixed[2] == '/'))
            {
                // trim off volume letter, colon, and slash
                s1 = SlashFixed.Substring(3);
            }
            else if ((FileName.Length >= 4)
                     && ((SlashFixed[0] == '/') && (SlashFixed[1] == '/')))
            {
                int n = SlashFixed.IndexOf('/', 2);
                if (n == -1)
                    throw new ArgumentException("The path for that entry appears to be badly formatted");
                s1 = SlashFixed.Substring(n + 1);
            }
            else if ((FileName.Length >= 3)
                     && ((SlashFixed[0] == '.') && (SlashFixed[1] == '/')))
            {
                // trim off dot and slash
                s1 = SlashFixed.Substring(2);
            }
            else
            {
                s1 = SlashFixed;
            }
            return s1;
        }


        /// <summary>
        ///   generate and return a byte array that encodes the filename
        ///   for the entry.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     side effects: generate and store into _CommentBytes the
        ///     byte array for any comment attached to the entry. Also
        ///     sets _actualEncoding to indicate the actual encoding
        ///     used. The same encoding is used for both filename and
        ///     comment.
        ///   </para>
        /// </remarks>
        private byte[] GetEncodedFileNameBytes()
        {
            // workitem 6513
            var s1 = NormalizeFileName();

            switch(AlternateEncodingUsage)
            {
                case ZipOption.Always:
                    if (!(_Comment == null || _Comment.Length == 0))
                        _CommentBytes = AlternateEncoding.GetBytes(_Comment);
                    _actualEncoding = AlternateEncoding;
                    return AlternateEncoding.GetBytes(s1);

                case ZipOption.Never:
                    if (!(_Comment == null || _Comment.Length == 0))
                        _CommentBytes = ibm437.GetBytes(_Comment);
                    _actualEncoding = ibm437;
                    return ibm437.GetBytes(s1);
            }

            // arriving here means AlternateEncodingUsage is "AsNecessary"

            // case ZipOption.AsNecessary:
            // workitem 6513: when writing, use the alternative encoding
            // only when _actualEncoding is not yet set (it can be set
            // during Read), and when ibm437 will not do.

            byte[] result = ibm437.GetBytes(s1);
            // need to use this form of GetString() for .NET CF
            string s2 = ibm437.GetString(result, 0, result.Length);
            _CommentBytes = null;
            if (s2 != s1)
            {
                // Encoding the filename with ibm437 does not allow round-trips.
                // Therefore, use the alternate encoding.  Assume it will work,
                // no checking of round trips here.
                result = AlternateEncoding.GetBytes(s1);
                if (_Comment != null && _Comment.Length != 0)
                    _CommentBytes = AlternateEncoding.GetBytes(_Comment);
                _actualEncoding = AlternateEncoding;
                return result;
            }

            _actualEncoding = ibm437;

            // Using ibm437, FileName can be encoded without information
            // loss; now try the Comment.

            // if there is no comment, use ibm437.
            if (_Comment == null || _Comment.Length == 0)
                return result;

            // there is a comment. Get the encoded form.
            byte[] cbytes = ibm437.GetBytes(_Comment);
            string c2 = ibm437.GetString(cbytes,0,cbytes.Length);

            // Check for round-trip.
            if (c2 != Comment)
            {
                // Comment cannot correctly be encoded with ibm437.  Use
                // the alternate encoding.

                result = AlternateEncoding.GetBytes(s1);
                _CommentBytes = AlternateEncoding.GetBytes(_Comment);
                _actualEncoding = AlternateEncoding;
                return result;
            }

            // use IBM437
            _CommentBytes = cbytes;
            return result;
        }



        private bool WantReadAgain()
        {
            if (_UncompressedSize < 0x10) return false;
            if (_CompressionMethod == 0x00) return false;
            if (CompressionLevel == Ionic.Zlib.CompressionLevel.None) return false;
            if (_CompressedSize < _UncompressedSize) return false;

            if (this._Source == ZipEntrySource.Stream && !this._sourceStream.CanSeek) return false;

#if AESCRYPTO
            if (_aesCrypto_forWrite != null && (CompressedSize - _aesCrypto_forWrite.SizeOfEncryptionMetadata) <= UncompressedSize + 0x10) return false;
#endif

            if (_zipCrypto_forWrite != null && (CompressedSize - 12) <= UncompressedSize) return false;

            return true;
        }



        private void MaybeUnsetCompressionMethodForWriting(int cycle)
        {
            // if we've already tried with compression... turn it off this time
            if (cycle > 1)
            {
                _CompressionMethod = 0x0;
                return;
            }
            // compression for directories = 0x00 (No Compression)
            if (IsDirectory)
            {
                _CompressionMethod = 0x0;
                return;
            }

            if (this._Source == ZipEntrySource.ZipFile)
            {
                return; // do nothing
            }

            // If __FileDataPosition is zero, then that means we will get the data
            // from a file or stream.

            // It is never possible to compress a zero-length file, so we check for
            // this condition.

            if (this._Source == ZipEntrySource.Stream)
            {
                // workitem 7742
                if (_sourceStream != null && _sourceStream.CanSeek)
                {
                    // Length prop will throw if CanSeek is false
                    long fileLength = _sourceStream.Length;
                    if (fileLength == 0)
                    {
                        _CompressionMethod = 0x00;
                        return;
                    }
                }
            }
            else if ((this._Source == ZipEntrySource.FileSystem) && (SharedUtilities.GetFileLength(LocalFileName) == 0L))
            {
                _CompressionMethod = 0x00;
                return;
            }

            // Ok, we're getting the data to be compressed from a
            // non-zero-length file or stream, or a file or stream of
            // unknown length, and we presume that it is non-zero.  In
            // that case we check the callback to see if the app wants
            // to tell us whether to compress or not.
            if (SetCompression != null)
                CompressionLevel = SetCompression(LocalFileName, _FileNameInArchive);

            // finally, set CompressionMethod to None if CompressionLevel is None
            if (CompressionLevel == (short)Ionic.Zlib.CompressionLevel.None &&
                CompressionMethod == Ionic.Zip.CompressionMethod.Deflate)
                _CompressionMethod = 0x00;

            return;
        }



        // write the header info for an entry
        internal void WriteHeader(Stream s, int cycle)
        {
            // Must remember the offset, within the output stream, of this particular
            // entry header.
            //
            // This is for 2 reasons:
            //
            //  1. so we can determine the RelativeOffsetOfLocalHeader (ROLH) for
            //     use in the central directory.
            //  2. so we can seek backward in case there is an error opening or reading
            //     the file, and the application decides to skip the file. In this case,
            //     we need to seek backward in the output stream to allow the next entry
            //     to be added to the zipfile output stream.
            //
            // Normally you would just store the offset before writing to the output
            // stream and be done with it.  But the possibility to use split archives
            // makes this approach ineffective.  In split archives, each file or segment
            // is bound to a max size limit, and each local file header must not span a
            // segment boundary; it must be written contiguously.  If it will fit in the
            // current segment, then the ROLH is just the current Position in the output
            // stream.  If it won't fit, then we need a new file (segment) and the ROLH
            // is zero.
            //
            // But we only can know if it is possible to write a header contiguously
            // after we know the size of the local header, a size that varies with
            // things like filename length, comments, and extra fields.  We have to
            // compute the header fully before knowing whether it will fit.
            //
            // That takes care of item #1 above.  Now, regarding #2.  If an error occurs
            // while computing the local header, we want to just seek backward. The
            // exception handling logic (in the caller of WriteHeader) uses ROLH to
            // scroll back.
            //
            // All this means we have to preserve the starting offset before computing
            // the header, and also we have to compute the offset later, to handle the
            // case of split archives.

            var counter = s as CountingStream;

            // workitem 8098: ok (output)
            // This may change later, for split archives

            // Don't set _RelativeOffsetOfLocalHeader. Instead, set a temp variable.
            // This allows for re-streaming, where a zip entry might be read from a
            // zip archive (and maybe decrypted, and maybe decompressed) and then
            // written to another zip archive, with different settings for
            // compression method, compression level, or encryption algorithm.
            _future_ROLH = (counter != null)
                ? counter.ComputedPosition
                : s.Position;

            int j = 0, i = 0;

            byte[] block = new byte[30];

            // signature
            block[i++] = (byte)(ZipConstants.ZipEntrySignature & 0x000000FF);
            block[i++] = (byte)((ZipConstants.ZipEntrySignature & 0x0000FF00) >> 8);
            block[i++] = (byte)((ZipConstants.ZipEntrySignature & 0x00FF0000) >> 16);
            block[i++] = (byte)((ZipConstants.ZipEntrySignature & 0xFF000000) >> 24);

            // Design notes for ZIP64:
            //
            // The specification says that the header must include the Compressed
            // and Uncompressed sizes, as well as the CRC32 value.  When creating
            // a zip via streamed processing, these quantities are not known until
            // after the compression is done.  Thus, a typical way to do it is to
            // insert zeroes for these quantities, then do the compression, then
            // seek back to insert the appropriate values, then seek forward to
            // the end of the file data.
            //
            // There is also the option of using bit 3 in the GP bitfield - to
            // specify that there is a data descriptor after the file data
            // containing these three quantities.
            //
            // This works when the size of the quantities is known, either 32-bits
            // or 64 bits as with the ZIP64 extensions.
            //
            // With Zip64, the 4-byte fields are set to 0xffffffff, and there is a
            // corresponding data block in the "extra field" that contains the
            // actual Compressed, uncompressed sizes.  (As well as an additional
            // field, the "Relative Offset of Local Header")
            //
            // The problem is when the app desires to use ZIP64 extensions
            // optionally, only when necessary.  Suppose the library assumes no
            // zip64 extensions when writing the header, then after compression
            // finds that the size of the data requires zip64.  At this point, the
            // header, already written to the file, won't have the necessary data
            // block in the "extra field".  The size of the entry header is fixed,
            // so it is not possible to just "add on" the zip64 data block after
            // compressing the file.  On the other hand, always using zip64 will
            // break interoperability with many other systems and apps.
            //
            // The approach we take is to insert a 32-byte dummy data block in the
            // extra field, whenever zip64 is to be used "as necessary". This data
            // block will get the actual zip64 HeaderId and zip64 metadata if
            // necessary.  If not necessary, the data block will get a meaningless
            // HeaderId (0x1111), and will be filled with zeroes.
            //
            // When zip64 is actually in use, we also need to set the
            // VersionNeededToExtract field to 45.
            //
            // There is one additional wrinkle: using zip64 as necessary conflicts
            // with output to non-seekable devices.  The header is emitted and
            // must indicate whether zip64 is in use, before we know if zip64 is
            // necessary.  Because there is no seeking, the header can never be
            // changed.  Therefore, on non-seekable devices,
            // Zip64Option.AsNecessary is the same as Zip64Option.Always.
            //


            // version needed- see AppNote.txt.
            //
            // need v5.1 for PKZIP strong encryption, or v2.0 for no encryption or
            // for PK encryption, 4.5 for zip64.  We may reset this later, as
            // necessary or zip64.

            _presumeZip64 = (_container.Zip64 == Zip64Option.Always ||
                             (_container.Zip64 == Zip64Option.AsNecessary && !s.CanSeek));
            Int16 VersionNeededToExtract = (Int16)(_presumeZip64 ? 45 : 20);
#if BZIP
            if (this.CompressionMethod == Ionic.Zip.CompressionMethod.BZip2)
                VersionNeededToExtract = 46;
#endif

            // (i==4)
            block[i++] = (byte)(VersionNeededToExtract & 0x00FF);
            block[i++] = (byte)((VersionNeededToExtract & 0xFF00) >> 8);

            // Get byte array. Side effect: sets ActualEncoding.
            // Must determine encoding before setting the bitfield.
            // workitem 6513
            byte[] fileNameBytes = GetEncodedFileNameBytes();
            Int16 filenameLength = (Int16)fileNameBytes.Length;

            // general purpose bitfield
            // In the current implementation, this library uses only these bits
            // in the GP bitfield:
            //  bit 0 = if set, indicates the entry is encrypted
            //  bit 3 = if set, indicates the CRC, C and UC sizes follow the file data.
            //  bit 6 = strong encryption - for pkware's meaning of strong encryption
            //  bit 11 = UTF-8 encoding is used in the comment and filename


            // Here we set or unset the encryption bit.
            // _BitField may already be set, as with a ZipEntry added into ZipOutputStream, which
            // has bit 3 always set. We only want to set one bit
            if (_Encryption == EncryptionAlgorithm.None)
                _BitField &= ~1;  // encryption bit OFF
            else
                _BitField |= 1;   // encryption bit ON


            // workitem 7941: WinZip does not the "strong encryption" bit  when using AES.
            // This "Strong Encryption" is a PKWare Strong encryption thing.
            //                 _BitField |= 0x0020;

            // set the UTF8 bit if necessary
#if SILVERLIGHT
            if (_actualEncoding.WebName == "utf-8")
#else
            if (_actualEncoding.CodePage == System.Text.Encoding.UTF8.CodePage)
#endif
                _BitField |= 0x0800;

            // The PKZIP spec says that if bit 3 is set (0x0008) in the General
            // Purpose BitField, then the CRC, Compressed size, and uncompressed
            // size are written directly after the file data.
            //
            // These 3 quantities are normally present in the regular zip entry
            // header. But, they are not knowable until after the compression is
            // done. So, in the normal case, we
            //
            //  - write the header, using zeros for these quantities
            //  - compress the data, and incidentally compute these quantities.
            //  - seek back and write the correct values them into the header.
            //
            // This is nice because, while it is more complicated to write the zip
            // file, it is simpler and less error prone to read the zip file, and
            // as a result more applications can read zip files produced this way,
            // with those 3 quantities in the header.
            //
            // But if seeking in the output stream is not possible, then we need
            // to set the appropriate bitfield and emit these quantities after the
            // compressed file data in the output.
            //
            // workitem 7216 - having trouble formatting a zip64 file that is
            // readable by WinZip.  not sure why!  What I found is that setting
            // bit 3 and following all the implications, the zip64 file is
            // readable by WinZip 12. and Perl's IO::Compress::Zip .  Perl takes
            // an interesting approach - it always sets bit 3 if ZIP64 in use.
            // DotNetZip now does the same; this gives better compatibility with
            // WinZip 12.

            if (IsDirectory || cycle == 99)
            {
                // (cycle == 99) indicates a zero-length entry written by ZipOutputStream

                _BitField &= ~0x0008;  // unset bit 3 - no "data descriptor" - ever
                _BitField &= ~0x0001;  // unset bit 1 - no encryption - ever
                Encryption = EncryptionAlgorithm.None;
                Password = null;
            }
            else if (!s.CanSeek)
                _BitField |= 0x0008;

#if DONT_GO_THERE
            else if (this.Encryption == EncryptionAlgorithm.PkzipWeak  &&
                     this._Source != ZipEntrySource.ZipFile)
            {
                // Set bit 3 to avoid the double-read perf issue.
                //
                // When PKZIP encryption is used, byte 11 of the encryption header is
                // used as a consistency check. It is normally set to the MSByte of the
                // CRC.  But this means the cRC must be known ebfore compression and
                // encryption, which means the entire stream has to be read twice.  To
                // avoid that, the high-byte of the time blob (when in DOS format) can
                // be used for the consistency check (byte 11 in the encryption header).
                // But this means the entry must have bit 3 set.
                //
                // Previously I used a more complex arrangement - using the methods like
                // FigureCrc32(), PrepOutputStream() and others, in order to manage the
                // seek-back in the source stream.  Why?  Because bit 3 is not always
                // friendly with third-party zip tools, like those on the Mac.
                //
                // This is why this code is still ifdef'd  out.
                //
                // Might consider making this yet another programmable option -
                // AlwaysUseBit3ForPkzip.  But that's for another day.
                //
                _BitField |= 0x0008;
            }
#endif

            // (i==6)
            block[i++] = (byte)(_BitField & 0x00FF);
            block[i++] = (byte)((_BitField & 0xFF00) >> 8);

            // Here, we want to set values for Compressed Size, Uncompressed Size,
            // and CRC.  If we have __FileDataPosition as not -1 (zero is a valid
            // FDP), then that means we are reading this zip entry from a zip
            // file, and we have good values for those quantities.
            //
            // If _FileDataPosition is -1, then we are constructing this Entry
            // from nothing.  We zero those quantities now, and we will compute
            // actual values for the three quantities later, when we do the
            // compression, and then seek back to write them into the appropriate
            // place in the header.
            if (this.__FileDataPosition == -1)
            {
                //_UncompressedSize = 0; // do not unset - may need this value for restream
                // _Crc32 = 0;           // ditto
                _CompressedSize = 0;
                _crcCalculated = false;
            }

            // set compression method here
            MaybeUnsetCompressionMethodForWriting(cycle);

            // (i==8) compression method
            block[i++] = (byte)(_CompressionMethod & 0x00FF);
            block[i++] = (byte)((_CompressionMethod & 0xFF00) >> 8);

            if (cycle == 99)
            {
                // (cycle == 99) indicates a zero-length entry written by ZipOutputStream
                SetZip64Flags();
            }

#if AESCRYPTO
            else if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                i -= 2;
                block[i++] = 0x63;
                block[i++] = 0;
            }
#endif

            // LastMod
            _TimeBlob = Ionic.Zip.SharedUtilities.DateTimeToPacked(LastModified);

            // (i==10) time blob
            block[i++] = (byte)(_TimeBlob & 0x000000FF);
            block[i++] = (byte)((_TimeBlob & 0x0000FF00) >> 8);
            block[i++] = (byte)((_TimeBlob & 0x00FF0000) >> 16);
            block[i++] = (byte)((_TimeBlob & 0xFF000000) >> 24);

            // (i==14) CRC - if source==filesystem, this is zero now, actual value
            // will be calculated later.  if source==archive, this is a bonafide
            // value.
            block[i++] = (byte)(_Crc32 & 0x000000FF);
            block[i++] = (byte)((_Crc32 & 0x0000FF00) >> 8);
            block[i++] = (byte)((_Crc32 & 0x00FF0000) >> 16);
            block[i++] = (byte)((_Crc32 & 0xFF000000) >> 24);

            if (_presumeZip64)
            {
                // (i==18) CompressedSize (Int32) and UncompressedSize - all 0xFF for now
                for (j = 0; j < 8; j++)
                    block[i++] = 0xFF;
            }
            else
            {
                // (i==18) CompressedSize (Int32) - this value may or may not be
                // bonafide.  if source == filesystem, then it is zero, and we'll
                // learn it after we compress.  if source == archive, then it is
                // bonafide data.
                block[i++] = (byte)(_CompressedSize & 0x000000FF);
                block[i++] = (byte)((_CompressedSize & 0x0000FF00) >> 8);
                block[i++] = (byte)((_CompressedSize & 0x00FF0000) >> 16);
                block[i++] = (byte)((_CompressedSize & 0xFF000000) >> 24);

                // (i==22) UncompressedSize (Int32) - this value may or may not be
                // bonafide.
                block[i++] = (byte)(_UncompressedSize & 0x000000FF);
                block[i++] = (byte)((_UncompressedSize & 0x0000FF00) >> 8);
                block[i++] = (byte)((_UncompressedSize & 0x00FF0000) >> 16);
                block[i++] = (byte)((_UncompressedSize & 0xFF000000) >> 24);
            }

            // (i==26) filename length (Int16)
            block[i++] = (byte)(filenameLength & 0x00FF);
            block[i++] = (byte)((filenameLength & 0xFF00) >> 8);

            _Extra = ConstructExtraField(false);

            // (i==28) extra field length (short)
            Int16 extraFieldLength = (Int16)((_Extra == null) ? 0 : _Extra.Length);
            block[i++] = (byte)(extraFieldLength & 0x00FF);
            block[i++] = (byte)((extraFieldLength & 0xFF00) >> 8);

            // workitem 13542
            byte[] bytes = new byte[i + filenameLength + extraFieldLength];

            // get the fixed portion
            Buffer.BlockCopy(block, 0, bytes, 0, i);
            //for (j = 0; j < i; j++) bytes[j] = block[j];

            // The filename written to the archive.
            Buffer.BlockCopy(fileNameBytes, 0, bytes, i, fileNameBytes.Length);
            // for (j = 0; j < fileNameBytes.Length; j++)
            //     bytes[i + j] = fileNameBytes[j];

            i += fileNameBytes.Length;

            // "Extra field"
            if (_Extra != null)
            {
                Buffer.BlockCopy(_Extra, 0, bytes, i, _Extra.Length);
                // for (j = 0; j < _Extra.Length; j++)
                //     bytes[i + j] = _Extra[j];
                i += _Extra.Length;
            }

            _LengthOfHeader = i;

            // handle split archives
            var zss = s as ZipSegmentedStream;
            if (zss != null)
            {
                zss.ContiguousWrite = true;
                UInt32 requiredSegment = zss.ComputeSegment(i);
                if (requiredSegment != zss.CurrentSegment)
                    _future_ROLH = 0; // rollover!
                else
                    _future_ROLH = zss.Position;

                _diskNumber = requiredSegment;
            }

            // validate the ZIP64 usage
            if (_container.Zip64 == Zip64Option.Never && (uint)_RelativeOffsetOfLocalHeader >= 0xFFFFFFFF)
                throw new ZipException("Offset within the zip archive exceeds 0xFFFFFFFF. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");


            // finally, write the header to the stream
            s.Write(bytes, 0, i);

            // now that the header is written, we can turn off the contiguous write restriction.
            if (zss != null)
                zss.ContiguousWrite = false;

            // Preserve this header data, we'll use it again later.
            // ..when seeking backward, to write again, after we have the Crc, compressed
            //   and uncompressed sizes.
            // ..and when writing the central directory structure.
            _EntryHeader = bytes;
        }




        private Int32 FigureCrc32()
        {
            if (_crcCalculated == false)
            {
                Stream input = null;
                // get the original stream:
                if (this._Source == ZipEntrySource.WriteDelegate)
                {
                    var output = new Ionic.Crc.CrcCalculatorStream(Stream.Null);
                    // allow the application to write the data
                    this._WriteDelegate(this.FileName, output);
                    _Crc32 = output.Crc;
                }
                else if (this._Source == ZipEntrySource.ZipFile)
                {
                    // nothing to do - the CRC is already set
                }
                else
                {
                    if (this._Source == ZipEntrySource.Stream)
                    {
                        PrepSourceStream();
                        input = this._sourceStream;
                    }
                    else if (this._Source == ZipEntrySource.JitStream)
                    {
                        // allow the application to open the stream
                        if (this._sourceStream == null)
                            _sourceStream = this._OpenDelegate(this.FileName);
                        PrepSourceStream();
                        input = this._sourceStream;
                    }
                    else if (this._Source == ZipEntrySource.ZipOutputStream)
                    {
                    }
                    else
                    {
                        //input = File.OpenRead(LocalFileName);
                        input = File.Open(LocalFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }

                    var crc32 = new Ionic.Crc.CRC32();
                    _Crc32 = crc32.GetCrc32(input);

                    if (_sourceStream == null)
                    {
#if NETCF
                        input.Close();
#else
                        input.Dispose();
#endif
                    }
                }
                _crcCalculated = true;
            }
            return _Crc32;
        }


        /// <summary>
        ///   Stores the position of the entry source stream, or, if the position is
        ///   already stored, seeks to that position.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This method is called in prep for reading the source stream.  If PKZIP
        ///   encryption is used, then we need to calc the CRC32 before doing the
        ///   encryption, because the CRC is used in the 12th byte of the PKZIP
        ///   encryption header.  So, we need to be able to seek backward in the source
        ///   when saving the ZipEntry. This method is called from the place that
        ///   calculates the CRC, and also from the method that does the encryption of
        ///   the file data.
        /// </para>
        ///
        /// <para>
        ///   The first time through, this method sets the _sourceStreamOriginalPosition
        ///   field. Subsequent calls to this method seek to that position.
        /// </para>
        /// </remarks>
        private void PrepSourceStream()
        {
            if (_sourceStream == null)
                throw new ZipException(String.Format("The input stream is null for entry '{0}'.", FileName));

            if (this._sourceStreamOriginalPosition != null)
            {
                // this will happen the 2nd cycle through, if the stream is seekable
                this._sourceStream.Position = this._sourceStreamOriginalPosition.Value;
            }
            else if (this._sourceStream.CanSeek)
            {
                // this will happen the first cycle through, if seekable
                this._sourceStreamOriginalPosition = new Nullable<Int64>(this._sourceStream.Position);
            }
            else if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
            {
                // In general, using PKZIP encryption on a a zip entry whose input
                // comes from a non-seekable stream, is tricky.  Here's why:
                //
                // Byte 11 of the PKZIP encryption header is used for password
                // validation and consistency checknig.
                //
                // Normally, the highest byte of the CRC is used as the 11th (last) byte
                // in the PKZIP encryption header. This means the CRC must be known
                // before encryption is performed. Normally that means we read the full
                // data stream, compute the CRC, then seek back and read it again for
                // the compression+encryption phase. Obviously this is bad for
                // performance with a large input file.
                //
                // There's a twist in the ZIP spec (actually documented only in infozip
                // code, not in the spec itself) that allows the high-order byte of the
                // last modified time for the entry, when the lastmod time is in packed
                // (DOS) format, to be used for Byte 11 in the encryption header. In
                // this case, the bit 3 "data descriptor" must be used.
                //
                // An intelligent implementation would therefore force the use of the
                // bit 3 data descriptor when PKZIP encryption is in use, regardless.
                // This avoids the double-read of the stream to be encrypted.  So far,
                // DotNetZip doesn't do that; it just punts when the input stream is
                // non-seekable, and the output does not use Bit 3.
                //
                // The other option is to use the CRC when it is already available, eg,
                // when the source for the data is a ZipEntry (when the zip file is
                // being updated). In this case we already know the CRC and can just use
                // what we know.

                if (this._Source != ZipEntrySource.ZipFile && ((this._BitField & 0x0008) != 0x0008))
                    throw new ZipException("It is not possible to use PKZIP encryption on a non-seekable input stream");
            }
        }


        /// <summary>
        /// Copy metadata that may have been changed by the app.  We do this when
        /// resetting the zipFile instance.  If the app calls Save() on a ZipFile, then
        /// tries to party on that file some more, we may need to Reset() it , which
        /// means re-reading the entries and then copying the metadata.  I think.
        /// </summary>
        internal void CopyMetaData(ZipEntry source)
        {
            this.__FileDataPosition = source.__FileDataPosition;
            this.CompressionMethod = source.CompressionMethod;
            this._CompressionMethod_FromZipFile = source._CompressionMethod_FromZipFile;
            this._CompressedFileDataSize = source._CompressedFileDataSize;
            this._UncompressedSize = source._UncompressedSize;
            this._BitField = source._BitField;
            this._Source = source._Source;
            this._LastModified = source._LastModified;
            this._Mtime = source._Mtime;
            this._Atime = source._Atime;
            this._Ctime = source._Ctime;
            this._ntfsTimesAreSet = source._ntfsTimesAreSet;
            this._emitUnixTimes = source._emitUnixTimes;
            this._emitNtfsTimes = source._emitNtfsTimes;
        }


        private void OnWriteBlock(Int64 bytesXferred, Int64 totalBytesToXfer)
        {
            if (_container.ZipFile != null)
                _ioOperationCanceled = _container.ZipFile.OnSaveBlock(this, bytesXferred, totalBytesToXfer);
        }



        private void _WriteEntryData(Stream s)
        {
            // Read in the data from the input stream (often a file in the filesystem),
            // and write it to the output stream, calculating a CRC on it as we go.
            // We will also compress and encrypt as necessary.

            Stream input = null;
            long fdp = -1L;
            try
            {
                // Want to record the position in the zip file of the zip entry
                // data (as opposed to the metadata).  s.Position may fail on some
                // write-only streams, eg stdout or System.Web.HttpResponseStream.
                // We swallow that exception, because we don't care, in that case.
                // But, don't set __FileDataPosition directly.  It may be needed
                // to READ the zip entry from the zip file, if this is a
                // "re-stream" situation. In other words if the zip entry has
                // changed compression level, or compression method, or (maybe?)
                // encryption algorithm.  In that case if the original entry is
                // encrypted, we need __FileDataPosition to be the value for the
                // input zip file.  This s.Position is for the output zipfile.  So
                // we copy fdp to __FileDataPosition after this entry has been
                // (maybe) restreamed.
                fdp = s.Position;
            }
            catch (Exception) { }

            try
            {
                // Use fileLength for progress updates, and to decide whether we can
                // skip encryption and compression altogether (in case of length==zero)
                long fileLength = SetInputAndFigureFileLength(ref input);

                // Wrap a counting stream around the raw output stream:
                // This is the last thing that happens before the bits go to the
                // application-provided stream.
                //
                // Sometimes s is a CountingStream. Doesn't matter. Wrap it with a
                // counter anyway. We need to count at both levels.

                CountingStream entryCounter = new CountingStream(s);

                Stream encryptor;
                Stream compressor;

                if (fileLength != 0L)
                {
                    // Maybe wrap an encrypting stream around the counter: This will
                    // happen BEFORE output counting, and AFTER compression, if encryption
                    // is used.
                    encryptor = MaybeApplyEncryption(entryCounter);

                    // Maybe wrap a compressing Stream around that.
                    // This will happen BEFORE encryption (if any) as we write data out.
                    compressor = MaybeApplyCompression(encryptor, fileLength);
                }
                else
                {
                    encryptor = compressor = entryCounter;
                }

                // Wrap a CrcCalculatorStream around that.
                // This will happen BEFORE compression (if any) as we write data out.
                var output = new Ionic.Crc.CrcCalculatorStream(compressor, true);

                // output.Write() causes this flow:
                // calc-crc -> compress -> encrypt -> count -> actually write

                if (this._Source == ZipEntrySource.WriteDelegate)
                {
                    // allow the application to write the data
                    this._WriteDelegate(this.FileName, output);
                }
                else
                {
                    // synchronously copy the input stream to the output stream-chain
                    byte[] buffer = new byte[BufferSize];
                    int n;
                    while ((n = SharedUtilities.ReadWithRetry(input, buffer, 0, buffer.Length, FileName)) != 0)
                    {
                        output.Write(buffer, 0, n);
                        OnWriteBlock(output.TotalBytesSlurped, fileLength);
                        if (_ioOperationCanceled)
                            break;
                    }
                }

                FinishOutputStream(s, entryCounter, encryptor, compressor, output);
            }
            finally
            {
                if (this._Source == ZipEntrySource.JitStream)
                {
                    // allow the application to close the stream
                    if (this._CloseDelegate != null)
                        this._CloseDelegate(this.FileName, input);
                }
                else if ((input as FileStream) != null)
                {
#if NETCF
                    input.Close();
#else
                    input.Dispose();
#endif
                }
            }

            if (_ioOperationCanceled)
                return;

            // set FDP now, to allow for re-streaming
            this.__FileDataPosition = fdp;
            PostProcessOutput(s);
        }


        /// <summary>
        ///   Set the input stream and get its length, if possible.  The length is
        ///   used for progress updates, AND, to allow an optimization in case of
        ///   a stream/file of zero length. In that case we skip the Encrypt and
        ///   compression Stream. (like DeflateStream or BZip2OutputStream)
        /// </summary>
        private long SetInputAndFigureFileLength(ref Stream input)
        {
            long fileLength = -1L;
            // get the original stream:
            if (this._Source == ZipEntrySource.Stream)
            {
                PrepSourceStream();
                input = this._sourceStream;

                // Try to get the length, no big deal if not available.
                try { fileLength = this._sourceStream.Length; }
                catch (NotSupportedException) { }
            }
            else if (this._Source == ZipEntrySource.ZipFile)
            {
                // we are "re-streaming" the zip entry.
                string pwd = (_Encryption_FromZipFile == EncryptionAlgorithm.None) ? null : (this._Password ?? this._container.Password);
                this._sourceStream = InternalOpenReader(pwd);
                PrepSourceStream();
                input = this._sourceStream;
                fileLength = this._sourceStream.Length;
            }
            else if (this._Source == ZipEntrySource.JitStream)
            {
                // allow the application to open the stream
                if (this._sourceStream == null) _sourceStream = this._OpenDelegate(this.FileName);
                PrepSourceStream();
                input = this._sourceStream;
                try { fileLength = this._sourceStream.Length; }
                catch (NotSupportedException) { }
            }
            else if (this._Source == ZipEntrySource.FileSystem)
            {
                // workitem 7145
                FileShare fs = FileShare.ReadWrite;
#if !NETCF
                // FileShare.Delete is not defined for the Compact Framework
                fs |= FileShare.Delete;
#endif
                // workitem 8423
                input = File.Open(LocalFileName, FileMode.Open, FileAccess.Read, fs);
                fileLength = input.Length;
            }

            return fileLength;
        }



        internal void FinishOutputStream(Stream s,
                                         CountingStream entryCounter,
                                         Stream encryptor,
                                         Stream compressor,
                                         Ionic.Crc.CrcCalculatorStream output)
        {
            if (output == null) return;

            output.Close();

            // by calling Close() on the deflate stream, we write the footer bytes, as necessary.
            if ((compressor as Ionic.Zlib.DeflateStream) != null)
                compressor.Close();
#if BZIP
            else if ((compressor as Ionic.BZip2.BZip2OutputStream) != null)
                compressor.Close();
#if !NETCF
            else if ((compressor as Ionic.BZip2.ParallelBZip2OutputStream) != null)
                compressor.Close();
#endif
#endif

#if !NETCF
            else if ((compressor as Ionic.Zlib.ParallelDeflateOutputStream) != null)
                compressor.Close();
#endif

            encryptor.Flush();
            encryptor.Close();

            _LengthOfTrailer = 0;

            _UncompressedSize = output.TotalBytesSlurped;

#if AESCRYPTO
            WinZipAesCipherStream wzacs = encryptor as WinZipAesCipherStream;
            if (wzacs != null && _UncompressedSize > 0)
            {
                s.Write(wzacs.FinalAuthentication, 0, 10);
                _LengthOfTrailer += 10;
            }
#endif
            _CompressedFileDataSize = entryCounter.BytesWritten;
            _CompressedSize = _CompressedFileDataSize;   // may be adjusted
            _Crc32 = output.Crc;

            // Set _RelativeOffsetOfLocalHeader now, to allow for re-streaming
            StoreRelativeOffset();
        }




        internal void PostProcessOutput(Stream s)
        {
            var s1 = s as CountingStream;

            // workitem 8931 - for WriteDelegate.
            // The WriteDelegate changes things because there can be a zero-byte stream
            // written. In all other cases DotNetZip knows the length of the stream
            // before compressing and encrypting. In this case we have to circle back,
            // and omit all the crypto stuff - the GP bitfield, and the crypto header.
            if (_UncompressedSize == 0 && _CompressedSize == 0)
            {
                if (this._Source == ZipEntrySource.ZipOutputStream) return;  // nothing to do...

                if (_Password != null)
                {
                    int headerBytesToRetract = 0;
                    if (Encryption == EncryptionAlgorithm.PkzipWeak)
                        headerBytesToRetract = 12;
#if AESCRYPTO
                    else if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                             Encryption == EncryptionAlgorithm.WinZipAes256)
                    {
                        headerBytesToRetract = _aesCrypto_forWrite._Salt.Length + _aesCrypto_forWrite.GeneratedPV.Length;
                    }
#endif
                    if (this._Source == ZipEntrySource.ZipOutputStream && !s.CanSeek)
                        throw new ZipException("Zero bytes written, encryption in use, and non-seekable output.");

                    if (Encryption != EncryptionAlgorithm.None)
                    {
                        // seek back in the stream to un-output the security metadata
                        s.Seek(-1 * headerBytesToRetract, SeekOrigin.Current);
                        s.SetLength(s.Position);
                        // workitem 10178
                        Ionic.Zip.SharedUtilities.Workaround_Ladybug318918(s);

                        // workitem 11131
                        // adjust the count on the CountingStream as necessary
                        if (s1 != null) s1.Adjust(headerBytesToRetract);

                        // subtract the size of the security header from the _LengthOfHeader
                        _LengthOfHeader -= headerBytesToRetract;
                        __FileDataPosition -= headerBytesToRetract;
                    }
                    _Password = null;

                    // turn off the encryption bit
                    _BitField &= ~(0x0001);

                    // copy the updated bitfield value into the header
                    int j = 6;
                    _EntryHeader[j++] = (byte)(_BitField & 0x00FF);
                    _EntryHeader[j++] = (byte)((_BitField & 0xFF00) >> 8);

#if AESCRYPTO
                    if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                        Encryption == EncryptionAlgorithm.WinZipAes256)
                    {
                        // Fix the extra field - overwrite the 0x9901 headerId
                        // with dummy data. (arbitrarily, 0x9999)
                        Int16 fnLength = (short)(_EntryHeader[26] + _EntryHeader[27] * 256);
                        int offx = 30 + fnLength;
                        int aesIndex = FindExtraFieldSegment(_EntryHeader, offx, 0x9901);
                        if (aesIndex >= 0)
                        {
                            _EntryHeader[aesIndex++] = 0x99;
                            _EntryHeader[aesIndex++] = 0x99;
                        }
                    }
#endif
                }

                CompressionMethod = 0;
                Encryption = EncryptionAlgorithm.None;
            }
            else if (_zipCrypto_forWrite != null
#if AESCRYPTO
                     || _aesCrypto_forWrite != null
#endif
                     )

            {
                if (Encryption == EncryptionAlgorithm.PkzipWeak)
                {
                    _CompressedSize += 12; // 12 extra bytes for the encryption header
                }
#if AESCRYPTO
                else if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                         Encryption == EncryptionAlgorithm.WinZipAes256)
                {
                    // adjust the compressed size to include the variable (salt+pv)
                    // security header and 10-byte trailer. According to the winzip AES
                    // spec, that metadata is included in the "Compressed Size" figure
                    // when encoding the zip archive.
                    _CompressedSize += _aesCrypto_forWrite.SizeOfEncryptionMetadata;
                }
#endif
            }

            int i = 8;
            _EntryHeader[i++] = (byte)(_CompressionMethod & 0x00FF);
            _EntryHeader[i++] = (byte)((_CompressionMethod & 0xFF00) >> 8);

            i = 14;
            // CRC - the correct value now
            _EntryHeader[i++] = (byte)(_Crc32 & 0x000000FF);
            _EntryHeader[i++] = (byte)((_Crc32 & 0x0000FF00) >> 8);
            _EntryHeader[i++] = (byte)((_Crc32 & 0x00FF0000) >> 16);
            _EntryHeader[i++] = (byte)((_Crc32 & 0xFF000000) >> 24);

            SetZip64Flags();

            // (i==26) filename length (Int16)
            Int16 filenameLength = (short)(_EntryHeader[26] + _EntryHeader[27] * 256);
            Int16 extraFieldLength = (short)(_EntryHeader[28] + _EntryHeader[29] * 256);

            if (_OutputUsesZip64.Value)
            {
                // VersionNeededToExtract - set to 45 to indicate zip64
                _EntryHeader[4] = (byte)(45 & 0x00FF);
                _EntryHeader[5] = 0x00;

                // workitem 7924 - don't need bit 3
                // // workitem 7917
                // // set bit 3 for ZIP64 compatibility with WinZip12
                // _BitField |= 0x0008;
                // _EntryHeader[6] = (byte)(_BitField & 0x00FF);

                // CompressedSize and UncompressedSize - 0xFF
                for (int j = 0; j < 8; j++)
                    _EntryHeader[i++] = 0xff;

                // At this point we need to find the "Extra field" that follows the
                // filename.  We had already emitted it, but the data (uncomp, comp,
                // ROLH) was not available at the time we did so.  Here, we emit it
                // again, with final values.

                i = 30 + filenameLength;
                _EntryHeader[i++] = 0x01;  // zip64
                _EntryHeader[i++] = 0x00;

                i += 2; // skip over data size, which is 16+4

                Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, _EntryHeader, i, 8);
                i += 8;
                Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, _EntryHeader, i, 8);
            }
            else
            {
                // VersionNeededToExtract - reset to 20 since no zip64
                _EntryHeader[4] = (byte)(20 & 0x00FF);
                _EntryHeader[5] = 0x00;

                // CompressedSize - the correct value now
                i = 18;
                _EntryHeader[i++] = (byte)(_CompressedSize & 0x000000FF);
                _EntryHeader[i++] = (byte)((_CompressedSize & 0x0000FF00) >> 8);
                _EntryHeader[i++] = (byte)((_CompressedSize & 0x00FF0000) >> 16);
                _EntryHeader[i++] = (byte)((_CompressedSize & 0xFF000000) >> 24);

                // UncompressedSize - the correct value now
                _EntryHeader[i++] = (byte)(_UncompressedSize & 0x000000FF);
                _EntryHeader[i++] = (byte)((_UncompressedSize & 0x0000FF00) >> 8);
                _EntryHeader[i++] = (byte)((_UncompressedSize & 0x00FF0000) >> 16);
                _EntryHeader[i++] = (byte)((_UncompressedSize & 0xFF000000) >> 24);

                // The HeaderId in the extra field header, is already dummied out.
                if (extraFieldLength != 0)
                {
                    i = 30 + filenameLength;
                    // For zip archives written by this library, if the zip64
                    // header exists, it is the first header. Because of the logic
                    // used when first writing the _EntryHeader bytes, the
                    // HeaderId is not guaranteed to be any particular value.  So
                    // we determine if the first header is a putative zip64 header
                    // by examining the datasize.  UInt16 HeaderId =
                    // (UInt16)(_EntryHeader[i] + _EntryHeader[i + 1] * 256);
                    Int16 DataSize = (short)(_EntryHeader[i + 2] + _EntryHeader[i + 3] * 256);
                    if (DataSize == 16)
                    {
                        // reset to Header Id to dummy value, effectively dummy-ing out the zip64 metadata
                        _EntryHeader[i++] = 0x99;
                        _EntryHeader[i++] = 0x99;
                    }
                }
            }


#if AESCRYPTO

            if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                // Must set compressionmethod to 0x0063 (decimal 99)
                //
                // and then set the compression method bytes inside the extra
                // field to the actual compression method value.

                i = 8;
                _EntryHeader[i++] = 0x63;
                _EntryHeader[i++] = 0;

                i = 30 + filenameLength;
                do
                {
                    UInt16 HeaderId = (UInt16)(_EntryHeader[i] + _EntryHeader[i + 1] * 256);
                    Int16 DataSize = (short)(_EntryHeader[i + 2] + _EntryHeader[i + 3] * 256);
                    if (HeaderId != 0x9901)
                    {
                        // skip this header
                        i += DataSize + 4;
                    }
                    else
                    {
                        i += 9;
                        // actual compression method
                        _EntryHeader[i++] = (byte)(_CompressionMethod & 0x00FF);
                        _EntryHeader[i++] = (byte)(_CompressionMethod & 0xFF00);
                    }
                } while (i < (extraFieldLength - 30 - filenameLength));
            }
#endif

            // finally, write the data.

            // workitem 7216 - sometimes we don't seek even if we CAN.  ASP.NET
            // Response.OutputStream, or stdout are non-seekable.  But we may also want
            // to NOT seek in other cases, eg zip64.  For all cases, we just check bit 3
            // to see if we want to seek.  There's one exception - if using a
            // ZipOutputStream, and PKZip encryption is in use, then we set bit 3 even
            // if the out is seekable. This is so the check on the last byte of the
            // PKZip Encryption Header can be done on the current time, as opposed to
            // the CRC, to prevent streaming the file twice.  So, test for
            // ZipOutputStream and seekable, and if so, seek back, even if bit 3 is set.

            if ((_BitField & 0x0008) != 0x0008 ||
                 (this._Source == ZipEntrySource.ZipOutputStream && s.CanSeek))
            {
                // seek back and rewrite the entry header
                var zss = s as ZipSegmentedStream;
                if (zss != null && _diskNumber != zss.CurrentSegment)
                {
                    // In this case the entry header is in a different file,
                    // which has already been closed. Need to re-open it.
                    using (Stream hseg = ZipSegmentedStream.ForUpdate(this._container.ZipFile.Name, _diskNumber))
                    {
                        hseg.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
                        hseg.Write(_EntryHeader, 0, _EntryHeader.Length);
                    }
                }
                else
                {
                    // seek in the raw output stream, to the beginning of the header for
                    // this entry.
                    // workitem 8098: ok (output)
                    s.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);

                    // write the updated header to the output stream
                    s.Write(_EntryHeader, 0, _EntryHeader.Length);

                    // adjust the count on the CountingStream as necessary
                    if (s1 != null) s1.Adjust(_EntryHeader.Length);

                    // seek in the raw output stream, to the end of the file data
                    // for this entry
                    s.Seek(_CompressedSize, SeekOrigin.Current);
                }
            }

            // emit the descriptor - only if not a directory.
            if (((_BitField & 0x0008) == 0x0008) && !IsDirectory)
            {
                byte[] Descriptor = new byte[16 + (_OutputUsesZip64.Value ? 8 : 0)];
                i = 0;

                // signature
                Array.Copy(BitConverter.GetBytes(ZipConstants.ZipEntryDataDescriptorSignature), 0, Descriptor, i, 4);
                i += 4;

                // CRC - the correct value now
                Array.Copy(BitConverter.GetBytes(_Crc32), 0, Descriptor, i, 4);
                i += 4;

                // workitem 7917
                if (_OutputUsesZip64.Value)
                {
                    // CompressedSize - the correct value now
                    Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, Descriptor, i, 8);
                    i += 8;

                    // UncompressedSize - the correct value now
                    Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, Descriptor, i, 8);
                    i += 8;
                }
                else
                {
                    // CompressedSize - (lower 32 bits) the correct value now
                    Descriptor[i++] = (byte)(_CompressedSize & 0x000000FF);
                    Descriptor[i++] = (byte)((_CompressedSize & 0x0000FF00) >> 8);
                    Descriptor[i++] = (byte)((_CompressedSize & 0x00FF0000) >> 16);
                    Descriptor[i++] = (byte)((_CompressedSize & 0xFF000000) >> 24);

                    // UncompressedSize - (lower 32 bits) the correct value now
                    Descriptor[i++] = (byte)(_UncompressedSize & 0x000000FF);
                    Descriptor[i++] = (byte)((_UncompressedSize & 0x0000FF00) >> 8);
                    Descriptor[i++] = (byte)((_UncompressedSize & 0x00FF0000) >> 16);
                    Descriptor[i++] = (byte)((_UncompressedSize & 0xFF000000) >> 24);
                }

                // finally, write the trailing descriptor to the output stream
                s.Write(Descriptor, 0, Descriptor.Length);

                _LengthOfTrailer += Descriptor.Length;
            }
        }



        private void SetZip64Flags()
        {
            // zip64 housekeeping
            _entryRequiresZip64 = new Nullable<bool>
                (_CompressedSize >= 0xFFFFFFFF || _UncompressedSize >= 0xFFFFFFFF || _RelativeOffsetOfLocalHeader >= 0xFFFFFFFF);

            // validate the ZIP64 usage
            if (_container.Zip64 == Zip64Option.Never && _entryRequiresZip64.Value)
                throw new ZipException("Compressed or Uncompressed size, or offset exceeds the maximum value. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");

            _OutputUsesZip64 = new Nullable<bool>(_container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value);
        }



        /// <summary>
        ///   Prepare the given stream for output - wrap it in a CountingStream, and
        ///   then in a CRC stream, and an encryptor and deflator as appropriate.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Previously this was used in ZipEntry.Write(), but in an effort to
        ///     introduce some efficiencies in that method I've refactored to put the
        ///     code inline.  This method still gets called by ZipOutputStream.
        ///   </para>
        /// </remarks>
        internal void PrepOutputStream(Stream s,
                                       long streamLength,
                                       out CountingStream outputCounter,
                                       out Stream encryptor,
                                       out Stream compressor,
                                       out Ionic.Crc.CrcCalculatorStream output)
        {
            TraceWriteLine("PrepOutputStream: e({0}) comp({1}) crypto({2}) zf({3})",
                           FileName,
                           CompressionLevel,
                           Encryption,
                           _container.Name);

            // Wrap a counting stream around the raw output stream:
            // This is the last thing that happens before the bits go to the
            // application-provided stream.
            outputCounter = new CountingStream(s);

            // Sometimes the incoming "raw" output stream is already a CountingStream.
            // Doesn't matter. Wrap it with a counter anyway. We need to count at both
            // levels.

            if (streamLength != 0L)
            {
                // Maybe wrap an encrypting stream around that:
                // This will happen BEFORE output counting, and AFTER deflation, if encryption
                // is used.
                encryptor = MaybeApplyEncryption(outputCounter);

                // Maybe wrap a compressing Stream around that.
                // This will happen BEFORE encryption (if any) as we write data out.
                compressor = MaybeApplyCompression(encryptor, streamLength);
            }
            else
            {
                encryptor = compressor = outputCounter;
            }
            // Wrap a CrcCalculatorStream around that.
            // This will happen BEFORE compression (if any) as we write data out.
            output = new Ionic.Crc.CrcCalculatorStream(compressor, true);
        }



        private Stream MaybeApplyCompression(Stream s, long streamLength)
        {
            if (_CompressionMethod == 0x08 && CompressionLevel != Ionic.Zlib.CompressionLevel.None)
            {
#if !NETCF
                // ParallelDeflateThreshold == 0    means ALWAYS use parallel deflate
                // ParallelDeflateThreshold == -1L  means NEVER use parallel deflate
                // Other values specify the actual threshold.
                if (_container.ParallelDeflateThreshold == 0L ||
                    (streamLength > _container.ParallelDeflateThreshold &&
                     _container.ParallelDeflateThreshold > 0L))
                {
                    // This is sort of hacky.
                    //
                    // It's expensive to create a ParallelDeflateOutputStream, because
                    // of the large memory buffers.  But the class is unlike most Stream
                    // classes in that it can be re-used, so the caller can compress
                    // multiple files with it, one file at a time.  The key is to call
                    // Reset() on it, in between uses.
                    //
                    // The ParallelDeflateOutputStream is attached to the container
                    // itself - there is just one for the entire ZipFile or
                    // ZipOutputStream. So it gets created once, per save, and then
                    // re-used many times.
                    //
                    // This approach will break when we go to a "parallel save"
                    // approach, where multiple entries within the zip file are being
                    // compressed and saved at the same time.  But for now it's ok.
                    //

                    // instantiate the ParallelDeflateOutputStream
                    if (_container.ParallelDeflater == null)
                    {
                        _container.ParallelDeflater =
                            new Ionic.Zlib.ParallelDeflateOutputStream(s,
                                                                       CompressionLevel,
                                                                       _container.Strategy,
                                                                       true);
                        // can set the codec buffer size only before the first call to Write().
                        if (_container.CodecBufferSize > 0)
                            _container.ParallelDeflater.BufferSize = _container.CodecBufferSize;
                        if (_container.ParallelDeflateMaxBufferPairs > 0)
                            _container.ParallelDeflater.MaxBufferPairs =
                                _container.ParallelDeflateMaxBufferPairs;
                    }
                    // reset it with the new stream
                    Ionic.Zlib.ParallelDeflateOutputStream o1 = _container.ParallelDeflater;
                    o1.Reset(s);
                    return o1;
                }
#endif
                var o = new Ionic.Zlib.DeflateStream(s, Ionic.Zlib.CompressionMode.Compress,
                                                     CompressionLevel,
                                                     true);
                if (_container.CodecBufferSize > 0)
                    o.BufferSize = _container.CodecBufferSize;
                o.Strategy = _container.Strategy;
                return o;
            }


#if BZIP
            if (_CompressionMethod == 0x0c)
            {
#if !NETCF
                if (_container.ParallelDeflateThreshold == 0L ||
                    (streamLength > _container.ParallelDeflateThreshold &&
                     _container.ParallelDeflateThreshold > 0L))
                {

                    var o1 = new Ionic.BZip2.ParallelBZip2OutputStream(s, true);
                    return o1;
                }
#endif
                var o = new Ionic.BZip2.BZip2OutputStream(s, true);
                return o;
            }
#endif

            return s;
        }



        private Stream MaybeApplyEncryption(Stream s)
        {
            if (Encryption == EncryptionAlgorithm.PkzipWeak)
            {
                TraceWriteLine("MaybeApplyEncryption: e({0}) PKZIP", FileName);

                return new ZipCipherStream(s, _zipCrypto_forWrite, CryptoMode.Encrypt);
            }
#if AESCRYPTO
            if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                     Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                TraceWriteLine("MaybeApplyEncryption: e({0}) AES", FileName);

                return new WinZipAesCipherStream(s, _aesCrypto_forWrite, CryptoMode.Encrypt);
            }
#endif
            TraceWriteLine("MaybeApplyEncryption: e({0}) None", FileName);

            return s;
        }



        private void OnZipErrorWhileSaving(Exception e)
        {
            if (_container.ZipFile != null)
                _ioOperationCanceled = _container.ZipFile.OnZipErrorSaving(this, e);
        }



        internal void Write(Stream s)
        {
            var cs1 = s as CountingStream;
            var zss1 = s as ZipSegmentedStream;

            bool done = false;
            do
            {
                try
                {
                    // When the app is updating a zip file, it may be possible to
                    // just copy data for a ZipEntry from the source zipfile to the
                    // destination, as a block, without decompressing and
                    // recompressing, etc.  But, in some cases the app modifies the
                    // properties on a ZipEntry prior to calling Save(). A change to
                    // any of the metadata - the FileName, CompressioLeve and so on,
                    // means DotNetZip cannot simply copy through the existing
                    // ZipEntry data unchanged.
                    //
                    // There are two cases:
                    //
                    //  1. Changes to only metadata, which means the header and
                    //     central directory must be changed.
                    //
                    //  2. Changes to the properties that affect the compressed
                    //     stream, such as CompressionMethod, CompressionLevel, or
                    //     EncryptionAlgorithm. In this case, DotNetZip must
                    //     "re-stream" the data: the old entry data must be maybe
                    //     decrypted, maybe decompressed, then maybe re-compressed
                    //     and maybe re-encrypted.
                    //
                    // This test checks if the source for the entry data is a zip file, and
                    // if a restream is necessary.  If NOT, then it just copies through
                    // one entry, potentially changing the metadata.

                    if (_Source == ZipEntrySource.ZipFile && !_restreamRequiredOnSave)
                    {
                        CopyThroughOneEntry(s);
                        return;
                    }

                    // Is the entry a directory?  If so, the write is relatively simple.
                    if (IsDirectory)
                    {
                        WriteHeader(s, 1);
                        StoreRelativeOffset();
                        _entryRequiresZip64 = new Nullable<bool>(_RelativeOffsetOfLocalHeader >= 0xFFFFFFFF);
                        _OutputUsesZip64 = new Nullable<bool>(_container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value);
                        // handle case for split archives
                        if (zss1 != null)
                            _diskNumber = zss1.CurrentSegment;

                        return;
                    }

                    // At this point, the source for this entry is not a directory, and
                    // not a previously created zip file, or the source for the entry IS
                    // a previously created zip but the settings whave changed in
                    // important ways and therefore we will need to process the
                    // bytestream (compute crc, maybe compress, maybe encrypt) in order
                    // to write the content into the new zip.
                    //
                    // We do this in potentially 2 passes: The first time we do it as
                    // requested, maybe with compression and maybe encryption.  If that
                    // causes the bytestream to inflate in size, and if compression was
                    // on, then we turn off compression and do it again.


                    bool readAgain = true;
                    int nCycles = 0;
                    do
                    {
                        nCycles++;

                        WriteHeader(s, nCycles);

                        // write the encrypted header
                        WriteSecurityMetadata(s);

                        // write the (potentially compressed, potentially encrypted) file data
                        _WriteEntryData(s);

                        // track total entry size (including the trailing descriptor and MAC)
                        _TotalEntrySize = _LengthOfHeader + _CompressedFileDataSize + _LengthOfTrailer;

                        // The file data has now been written to the stream, and
                        // the file pointer is positioned directly after file data.

                        if (nCycles > 1) readAgain = false;
                        else if (!s.CanSeek) readAgain = false;
                        else readAgain = WantReadAgain();

                        if (readAgain)
                        {
                            // Seek back in the raw output stream, to the beginning of the file
                            // data for this entry.

                            // handle case for split archives
                            if (zss1 != null)
                            {
                                // Console.WriteLine("***_diskNumber/first: {0}", _diskNumber);
                                // Console.WriteLine("***_diskNumber/current: {0}", zss.CurrentSegment);
                                zss1.TruncateBackward(_diskNumber, _RelativeOffsetOfLocalHeader);
                            }
                            else
                                // workitem 8098: ok (output).
                                s.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);

                            // If the last entry expands, we read again; but here, we must
                            // truncate the stream to prevent garbage data after the
                            // end-of-central-directory.

                            // workitem 8098: ok (output).
                            s.SetLength(s.Position);

                            // Adjust the count on the CountingStream as necessary.
                            if (cs1 != null) cs1.Adjust(_TotalEntrySize);
                        }
                    }
                    while (readAgain);
                    _skippedDuringSave = false;
                    done = true;
                }
                catch (System.Exception exc1)
                {
                    ZipErrorAction orig = this.ZipErrorAction;
                    int loop = 0;
                    do
                    {
                        if (ZipErrorAction == ZipErrorAction.Throw)
                            throw;

                        if (ZipErrorAction == ZipErrorAction.Skip ||
                            ZipErrorAction == ZipErrorAction.Retry)
                        {
                            // must reset file pointer here.
                            // workitem 13903 - seek back only when necessary
                            long p1 = (cs1 != null)
                                ? cs1.ComputedPosition
                                : s.Position;
                            long delta = p1 - _future_ROLH;
                            if (delta > 0)
                            {
                                s.Seek(delta, SeekOrigin.Current); // may throw
                                long p2 = s.Position;
                                s.SetLength(s.Position);  // to prevent garbage if this is the last entry
                                if (cs1 != null) cs1.Adjust(p1 - p2);
                            }
                            if (ZipErrorAction == ZipErrorAction.Skip)
                            {
                                WriteStatus("Skipping file {0} (exception: {1})", LocalFileName, exc1.ToString());

                                _skippedDuringSave = true;
                                done = true;
                            }
                            else
                                this.ZipErrorAction = orig;
                            break;
                        }

                        if (loop > 0) throw;

                        if (ZipErrorAction == ZipErrorAction.InvokeErrorEvent)
                        {
                            OnZipErrorWhileSaving(exc1);
                            if (_ioOperationCanceled)
                            {
                                done = true;
                                break;
                            }
                        }
                        loop++;
                    }
                    while (true);
                }
            }
            while (!done);
        }


        internal void StoreRelativeOffset()
        {
            _RelativeOffsetOfLocalHeader = _future_ROLH;
        }



        internal void NotifySaveComplete()
        {
            // When updating a zip file, there are two contexts for properties
            // like Encryption or CompressionMethod - the values read from the
            // original zip file, and the values used in the updated zip file.
            // The _FromZipFile versions are the originals.  At the end of a save,
            // these values are the same.  So we need to update them.  This takes
            // care of the boundary case where a single zipfile instance can be
            // saved multiple times, with distinct changes to the properties on
            // the entries, in between each Save().
            _Encryption_FromZipFile = _Encryption;
            _CompressionMethod_FromZipFile = _CompressionMethod;
            _restreamRequiredOnSave = false;
            _metadataChanged = false;
            //_Source = ZipEntrySource.None;
            _Source = ZipEntrySource.ZipFile; // workitem 10694
        }


        internal void WriteSecurityMetadata(Stream outstream)
        {
            if (Encryption == EncryptionAlgorithm.None)
                return;

            string pwd = this._Password;

            // special handling for source == ZipFile.
            // Want to support the case where we re-stream an encrypted entry. This will involve,
            // at runtime, reading, decrypting, and decompressing from the original zip file, then
            // compressing, encrypting, and writing to the output zip file.

            // If that's what we're doing, and the password hasn't been set on the entry,
            // we use the container (ZipFile/ZipOutputStream) password to decrypt.
            // This test here says to use the container password to re-encrypt, as well,
            // with that password, if the entry password is null.

            if (this._Source == ZipEntrySource.ZipFile && pwd == null)
                pwd = this._container.Password;

            if (pwd == null)
            {
                _zipCrypto_forWrite = null;
#if AESCRYPTO
                _aesCrypto_forWrite = null;
#endif
                return;
            }

            TraceWriteLine("WriteSecurityMetadata: e({0}) crypto({1}) pw({2})",
                           FileName, Encryption.ToString(), pwd);

            if (Encryption == EncryptionAlgorithm.PkzipWeak)
            {
                // If PKZip (weak) encryption is in use, then the encrypted entry data
                // is preceded by 12-byte "encryption header" for the entry.

                _zipCrypto_forWrite = ZipCrypto.ForWrite(pwd);

                // generate the random 12-byte header:
                var rnd = new System.Random();
                byte[] encryptionHeader = new byte[12];
                rnd.NextBytes(encryptionHeader);

                // workitem 8271
                if ((this._BitField & 0x0008) == 0x0008)
                {
                    // In the case that bit 3 of the general purpose bit flag is set to
                    // indicate the presence of a 'data descriptor' (signature
                    // 0x08074b50), the last byte of the decrypted header is sometimes
                    // compared with the high-order byte of the lastmodified time,
                    // rather than the high-order byte of the CRC, to verify the
                    // password.
                    //
                    // This is not documented in the PKWare Appnote.txt.
                    // This was discovered this by analysis of the Crypt.c source file in the
                    // InfoZip library
                    // http://www.info-zip.org/pub/infozip/

                    // Also, winzip insists on this!
                    _TimeBlob = Ionic.Zip.SharedUtilities.DateTimeToPacked(LastModified);
                    encryptionHeader[11] = (byte)((this._TimeBlob >> 8) & 0xff);
                }
                else
                {
                    // When bit 3 is not set, the CRC value is required before
                    // encryption of the file data begins. In this case there is no way
                    // around it: must read the stream in its entirety to compute the
                    // actual CRC before proceeding.
                    FigureCrc32();
                    encryptionHeader[11] = (byte)((this._Crc32 >> 24) & 0xff);
                }

                // Encrypt the random header, INCLUDING the final byte which is either
                // the high-order byte of the CRC32, or the high-order byte of the
                // _TimeBlob.  Must do this BEFORE encrypting the file data.  This
                // step changes the state of the cipher, or in the words of the PKZIP
                // spec, it "further initializes" the cipher keys.

                byte[] cipherText = _zipCrypto_forWrite.EncryptMessage(encryptionHeader, encryptionHeader.Length);

                // Write the ciphered bonafide encryption header.
                outstream.Write(cipherText, 0, cipherText.Length);
                _LengthOfHeader += cipherText.Length;  // 12 bytes
            }

#if AESCRYPTO
            else if (Encryption == EncryptionAlgorithm.WinZipAes128 ||
                Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                // If WinZip AES encryption is in use, then the encrypted entry data is
                // preceded by a variable-sized Salt and a 2-byte "password
                // verification" value for the entry.

                int keystrength = GetKeyStrengthInBits(Encryption);
                _aesCrypto_forWrite = WinZipAesCrypto.Generate(pwd, keystrength);
                outstream.Write(_aesCrypto_forWrite.Salt, 0, _aesCrypto_forWrite._Salt.Length);
                outstream.Write(_aesCrypto_forWrite.GeneratedPV, 0, _aesCrypto_forWrite.GeneratedPV.Length);
                _LengthOfHeader += _aesCrypto_forWrite._Salt.Length + _aesCrypto_forWrite.GeneratedPV.Length;

                TraceWriteLine("WriteSecurityMetadata: AES e({0}) keybits({1}) _LOH({2})",
                               FileName, keystrength, _LengthOfHeader);

            }
#endif

        }



        private void CopyThroughOneEntry(Stream outStream)
        {
            // Just read the entry from the existing input zipfile and write to the output.
            // But, if metadata has changed (like file times or attributes), or if the ZIP64
            // option has changed, we can re-stream the entry data but must recompute the
            // metadata.
            if (this.LengthOfHeader == 0)
                throw new BadStateException("Bad header length.");

            // is it necessary to re-constitute new metadata for this entry?
            bool needRecompute = _metadataChanged ||
                (this.ArchiveStream is ZipSegmentedStream) ||
                (outStream is ZipSegmentedStream) ||
                (_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Never) ||
                (!_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Always);

            if (needRecompute)
                CopyThroughWithRecompute(outStream);
            else
                CopyThroughWithNoChange(outStream);

            // zip64 housekeeping
            _entryRequiresZip64 = new Nullable<bool>
                (_CompressedSize >= 0xFFFFFFFF || _UncompressedSize >= 0xFFFFFFFF ||
                _RelativeOffsetOfLocalHeader >= 0xFFFFFFFF
                );

            _OutputUsesZip64 = new Nullable<bool>(_container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value);
        }



        private void CopyThroughWithRecompute(Stream outstream)
        {
            int n;
            byte[] bytes = new byte[BufferSize];
            var input = new CountingStream(this.ArchiveStream);

            long origRelativeOffsetOfHeader = _RelativeOffsetOfLocalHeader;

            // The header length may change due to rename of file, add a comment, etc.
            // We need to retain the original.
            int origLengthOfHeader = LengthOfHeader; // including crypto bytes!

            // WriteHeader() has the side effect of changing _RelativeOffsetOfLocalHeader
            // and setting _LengthOfHeader.  While ReadHeader() reads the crypto header if
            // present, WriteHeader() does not write the crypto header.
            WriteHeader(outstream, 0);
            StoreRelativeOffset();

            if (!this.FileName.EndsWith("/"))
            {
                // Not a directory; there is file data.
                // Seek to the beginning of the entry data in the input stream.

                long pos = origRelativeOffsetOfHeader + origLengthOfHeader;
                int len = GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);
                pos -= len; // want to keep the crypto header
                _LengthOfHeader += len;

                input.Seek(pos, SeekOrigin.Begin);

                // copy through everything after the header to the output stream
                long remaining = this._CompressedSize;

                while (remaining > 0)
                {
                    len = (remaining > bytes.Length) ? bytes.Length : (int)remaining;

                    // read
                    n = input.Read(bytes, 0, len);
                    //_CheckRead(n);

                    // write
                    outstream.Write(bytes, 0, n);
                    remaining -= n;
                    OnWriteBlock(input.BytesRead, this._CompressedSize);
                    if (_ioOperationCanceled)
                        break;
                }

                // bit 3 descriptor
                if ((this._BitField & 0x0008) == 0x0008)
                {
                    int size = 16;
                    if (_InputUsesZip64) size += 8;
                    byte[] Descriptor = new byte[size];
                    input.Read(Descriptor, 0, size);

                    if (_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Never)
                    {
                        // original descriptor was 24 bytes, now we need 16.
                        // Must check for underflow here.
                        // signature + CRC.
                        outstream.Write(Descriptor, 0, 8);

                        // Compressed
                        if (_CompressedSize > 0xFFFFFFFF)
                            throw new InvalidOperationException("ZIP64 is required");
                        outstream.Write(Descriptor, 8, 4);

                        // UnCompressed
                        if (_UncompressedSize > 0xFFFFFFFF)
                            throw new InvalidOperationException("ZIP64 is required");
                        outstream.Write(Descriptor, 16, 4);
                        _LengthOfTrailer -= 8;
                    }
                    else if (!_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Always)
                    {
                        // original descriptor was 16 bytes, now we need 24
                        // signature + CRC
                        byte[] pad = new byte[4];
                        outstream.Write(Descriptor, 0, 8);
                        // Compressed
                        outstream.Write(Descriptor, 8, 4);
                        outstream.Write(pad, 0, 4);
                        // UnCompressed
                        outstream.Write(Descriptor, 12, 4);
                        outstream.Write(pad, 0, 4);
                        _LengthOfTrailer += 8;
                    }
                    else
                    {
                        // same descriptor on input and output. Copy it through.
                        outstream.Write(Descriptor, 0, size);
                        //_LengthOfTrailer += size;
                    }
                }
            }

            _TotalEntrySize = _LengthOfHeader + _CompressedFileDataSize + _LengthOfTrailer;
        }


        private void CopyThroughWithNoChange(Stream outstream)
        {
            int n;
            byte[] bytes = new byte[BufferSize];
            var input = new CountingStream(this.ArchiveStream);

            // seek to the beginning of the entry data in the input stream
            input.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);

            if (this._TotalEntrySize == 0)
            {
                // We've never set the length of the entry.
                // Set it here.
                this._TotalEntrySize = this._LengthOfHeader + this._CompressedFileDataSize + _LengthOfTrailer;

                // The CompressedSize includes all the leading metadata associated
                // to encryption, if any, as well as the compressed data, or
                // compressed-then-encrypted data, and the trailer in case of AES.

                // The CompressedFileData size is the same, less the encryption
                // framing data (12 bytes header for PKZip; 10/18 bytes header and
                // 10 byte trailer for AES).

                // The _LengthOfHeader includes all the zip entry header plus the
                // crypto header, if any.  The _LengthOfTrailer includes the
                // 10-byte MAC for AES, where appropriate, and the bit-3
                // Descriptor, where applicable.
            }


            // workitem 5616
            // remember the offset, within the output stream, of this particular entry header.
            // This may have changed if any of the other entries changed (eg, if a different
            // entry was removed or added.)
            var counter = outstream as CountingStream;
            _RelativeOffsetOfLocalHeader = (counter != null)
                ? counter.ComputedPosition
                : outstream.Position;  // BytesWritten

            // copy through the header, filedata, trailer, everything...
            long remaining = this._TotalEntrySize;
            while (remaining > 0)
            {
                int len = (remaining > bytes.Length) ? bytes.Length : (int)remaining;

                // read
                n = input.Read(bytes, 0, len);
                //_CheckRead(n);

                // write
                outstream.Write(bytes, 0, n);
                remaining -= n;
                OnWriteBlock(input.BytesRead, this._TotalEntrySize);
                if (_ioOperationCanceled)
                    break;
            }
        }




        [System.Diagnostics.ConditionalAttribute("Trace")]
        private void TraceWriteLine(string format, params object[] varParams)
        {
            lock (_outputLock)
            {
                int tid = System.Threading.Thread.CurrentThread.GetHashCode();
#if ! (NETCF || SILVERLIGHT)
                Console.ForegroundColor = (ConsoleColor)(tid % 8 + 8);
#endif
                Console.Write("{0:000} ZipEntry.Write ", tid);
                Console.WriteLine(format, varParams);
#if ! (NETCF || SILVERLIGHT)
                Console.ResetColor();
#endif
            }
        }

        private object _outputLock = new Object();
    }
}
