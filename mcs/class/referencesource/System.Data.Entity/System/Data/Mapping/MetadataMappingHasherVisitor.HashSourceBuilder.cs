//---------------------------------------------------------------------
// <copyright file="MetadataMappingHasherVisitor.HashSourceBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.Common.Utils;
using System.Security.Cryptography;
using System.Globalization;
using System.IO;


namespace System.Data.Mapping
{
    /// <summary>
    /// This class keeps recomputing the hash and adding it to the front of the 
    /// builder when the length of the string gets too long
    /// </summary>
    internal class CompressingHashBuilder : StringHashBuilder
    {

        // this max comes from the value that Md5Hasher uses for a buffer size when it is reading
        // from a stream
        private const int HashCharacterCompressionThreshold = 0x1000 / 2;  // num bytes / 2 to convert to typical unicode char size
        private const int SpacesPerIndent = 4;

        private int _indent = 0;

        // we are starting the buffer at 1.5 times the number of bytes
        // for the threshold
        internal CompressingHashBuilder(HashAlgorithm hashAlgorithm)
            : base(hashAlgorithm, (HashCharacterCompressionThreshold + (HashCharacterCompressionThreshold / 2)) * 2)
        {
        }

        internal override void Append(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.Append(content);
            CompressHash();
        }

        internal override void AppendLine(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.AppendLine(content);
            CompressHash();
        }

        /// <summary>
        /// add string like "typename Instance#1"
        /// </summary>
        /// <param name="objectIndex"></param>
        internal void AppendObjectStartDump(object o, int objectIndex)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.Append(o.GetType().ToString());
            base.Append(" Instance#");
            base.AppendLine(objectIndex.ToString(CultureInfo.InvariantCulture));
            CompressHash();

            this._indent++;
        }

        internal void AppendObjectEndDump()
        {
            Debug.Assert(this._indent > 0, "Indent and unindent should be paired");
            this._indent--;
        }


        private void CompressHash()
        {
            if(base.CharCount >= HashCharacterCompressionThreshold)
            {
                string hash = ComputeHash();
                Clear();
                base.Append(hash);
            }
        }
    }

    
    /// <summary>
    /// this class collects several strings together, and allows you to (
    /// </summary>
    internal class StringHashBuilder
    {
        private HashAlgorithm _hashAlgorithm;
        private const string NewLine = "\n";
        List<string> _strings = new List<string>();
        int _totalLength;

        byte[] _cachedBuffer;

        internal StringHashBuilder(HashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        internal StringHashBuilder(HashAlgorithm hashAlgorithm, int startingBufferSize)
        :this(hashAlgorithm)
        {
            Debug.Assert(startingBufferSize > 0, "should be a non zero positive integer");
            _cachedBuffer = new byte[startingBufferSize];
        }

        internal int CharCount { get { return _totalLength; } }

        internal virtual void Append(string s)
        {
            InternalAppend(s);
        }

        internal virtual void AppendLine(string s)
        {
            InternalAppend(s);
            InternalAppend(NewLine);
        }


        private void InternalAppend(string s)
        {
            if (s.Length == 0)
                return;

            _strings.Add(s);
            _totalLength += s.Length;
        }

        internal string ComputeHash()
        {
            int byteCount = GetByteCount();
            if(_cachedBuffer == null)
            {
                // assume it is a one time use, and 
                // it will grow later if needed
                _cachedBuffer = new byte[byteCount];
            }
            else if (_cachedBuffer.Length < byteCount)
            {
                // grow it by what is needed at a minimum, or 1.5 times bigger
                // if that is bigger than what is needed this time.  We
                // make it 1.5 times bigger in hopes to reduce the number of allocations (consider the
                // case where the next one it 1 bigger)
                int bufferSize = Math.Max(_cachedBuffer.Length + (_cachedBuffer.Length / 2), byteCount);
                _cachedBuffer = new byte[bufferSize];
            }

            int start = 0;
            foreach (string s in _strings)
            {
                start += Encoding.Unicode.GetBytes(s, 0, s.Length, _cachedBuffer, start);
            }
            Debug.Assert(start == byteCount, "Did we use a different calculation for these?");

            byte[] hash = _hashAlgorithm.ComputeHash(_cachedBuffer, 0, byteCount);
            return ConvertHashToString(hash);
        }

        internal void Clear()
        {
            _strings.Clear();
            _totalLength = 0;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            _strings.ForEach(s => builder.Append(s));
            return builder.ToString();
        }

        private int GetByteCount()
        {
            int count = 0;
            foreach (string s in _strings)
            {
                count += Encoding.Unicode.GetByteCount(s);
            }

            return count;
        }


        private static string ConvertHashToString(byte[] hash)
        {
            StringBuilder stringData = new StringBuilder(hash.Length * 2);
            // Loop through each byte of the data and format each one as a 
            // hexadecimal string
            for (int i = 0; i < hash.Length; i++)
            {
                stringData.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return stringData.ToString();
        }

        public static string ComputeHash(HashAlgorithm hashAlgorithm, string source)
        {
            StringHashBuilder builder = new StringHashBuilder(hashAlgorithm);
            builder.Append(source);
            return builder.ComputeHash();
        }
    }
}
