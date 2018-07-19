//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    
    class DigestTraceRecord : TraceRecord
    {
        MemoryStream _logStream;
        HashAlgorithm _hash;
        string _traceName;

        const string Empty = "Empty";

        const string CanonicalElementString = "CanonicalElementString";
        const string CanonicalElementStringLength = "CanonicalElementStringLength";
        
        const string CanonicalOctets = "CanonicalOctets";
        const string CanonicalOctetsLength = "CanonicalOctetsLength";

        const string CanonicalOctetsHash = "CanonicalOctetsHash";
        const string CanonicalOctetsHashLength = "CanonicalOctetsHashLength";

        const string Key = "Key";
        const string Length = "Length";
        const string FirstByte = "FirstByte";
        const string LastByte = "LastByte";

        internal DigestTraceRecord(string traceName, MemoryStream logStream, HashAlgorithm hash)
        {   
            if (string.IsNullOrEmpty(traceName))
                _traceName = Empty;
            else
                _traceName = traceName;

            _logStream = logStream;
            _hash = hash;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + _traceName + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            
            base.WriteTo(writer);

            //
            // canonical element string
            //
            byte[] contentBuffer = _logStream.GetBuffer();
            string contentAsString = System.Text.Encoding.UTF8.GetString(contentBuffer, 0, (int)_logStream.Length);


            writer.WriteElementString(CanonicalElementStringLength, contentAsString.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteComment(CanonicalElementString + ":" + contentAsString);

            //
            // canonical element base64 format
            //
            writer.WriteElementString(CanonicalOctetsLength, contentBuffer.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(CanonicalOctets, Convert.ToBase64String(contentBuffer));

            //
            // Hash
            //
            writer.WriteElementString(CanonicalOctetsHashLength, _hash.Hash.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(CanonicalOctetsHash, Convert.ToBase64String(_hash.Hash));

            //
            // Key: this will only be printed out for the symmetric key case
            //
            if (_hash is KeyedHashAlgorithm)
            {
                KeyedHashAlgorithm keyedHash = _hash as KeyedHashAlgorithm;
                byte[] key = keyedHash.Key;

                writer.WriteStartElement(Key); // start the key element

                writer.WriteElementString(Length, key.Length.ToString(CultureInfo.InvariantCulture)); // key length
                writer.WriteElementString(FirstByte, key[0].ToString(CultureInfo.InvariantCulture)); // first byte of the key
                writer.WriteElementString(LastByte, key[key.Length - 1].ToString(CultureInfo.InvariantCulture)); // last byte of the key

                writer.WriteEndElement();  // close the key element
            }
        }
    }

    internal static class DigestTraceRecordHelper
    {
        const string DigestTrace = "DigestTrace";
        static bool _shouldTraceDigest = false;
        static bool _initialized = false;

        internal static bool ShouldTraceDigest
        {
            get
            {
                if (!_initialized)
                    InitializeShouldTraceDigest();

                return _shouldTraceDigest;
            }
        }

        static void InitializeShouldTraceDigest()
        {
            //
            // Log the digest only if
            // 1.Users ask for verbose AND
            // 2.Users are fine with logging Pii ( private identity information )
            //
            if (DiagnosticUtility.DiagnosticTrace != null &&
                DiagnosticUtility.DiagnosticTrace.TraceSource != null &&
                DiagnosticUtility.DiagnosticTrace.ShouldLogPii &&
                DiagnosticUtility.ShouldTraceVerbose)
            {
                _shouldTraceDigest = true;
            }

            _initialized = true;
        }

        internal static void TraceDigest(MemoryStream logStream, HashAlgorithm hash)
        {
            if (DigestTraceRecordHelper.ShouldTraceDigest)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.IdentityModel, SR.GetString(SR.TraceCodeIdentityModel), new DigestTraceRecord(DigestTrace, logStream, hash), null, null);
            }
        }
    }
}
