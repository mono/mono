//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Debugger.Symbol
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    // Represent debug symbol of a workflow tree (similar to pdb file).
    // It contains the absolute path of the xaml file and the location of each activity in the workflow tree.
    // This is used to instrument the workflow without having access to the original xaml file.
    public class WorkflowSymbol
    {
        public string FileName { get; set; }
        public ICollection<ActivitySymbol> Symbols { get; set; }

        private byte[] checksum;
        public byte[] GetChecksum()
        {
            if (this.checksum == null)
            {
                return null;
            }
            return (byte[]) this.checksum.Clone();
        }
        
        [Flags]
        internal enum EncodingFormat : byte
        {
            String = 0x76,    // Format as well as cookie. String format is hidden from public.
            Binary = 0x77,
            Checksum = 0x80            
        }

        internal const EncodingFormat DefaultEncodingFormat = EncodingFormat.Binary;

        public WorkflowSymbol()
        {
        }

        // These constructors are private and used by Decode() method.

        // Binary deserializer.
        WorkflowSymbol(BinaryReader reader, byte[] checksum)
        {
            this.FileName = reader.ReadString();
            int numSymbols = SymbolHelper.ReadEncodedInt32(reader);
            this.Symbols = new List<ActivitySymbol>(numSymbols);
            for (int i = 0; i < numSymbols; ++i)
            {
                this.Symbols.Add(new ActivitySymbol(reader));
            }
            this.checksum = checksum;
        }

        // Decode from Base64 string.
        public static WorkflowSymbol Decode(string symbolString)
        {
            byte[] data = Convert.FromBase64String(symbolString);
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                byte[] checksum = null;
                EncodingFormat format = (EncodingFormat)reader.ReadByte();
                int payloadBytesCount = data.Length - sizeof(EncodingFormat);
                if (0 != (format & EncodingFormat.Checksum))
                {
                    int bytesCount = SymbolHelper.ReadEncodedInt32(reader);
                    checksum = reader.ReadBytes(bytesCount);
                    payloadBytesCount -= SymbolHelper.GetEncodedSize(bytesCount);
                    format &= (~EncodingFormat.Checksum);
                }
                switch (format)
                {
                    case EncodingFormat.Binary:
                        return ParseBinary(reader.ReadBytes(payloadBytesCount), checksum); // Compute the 
                    case EncodingFormat.String:
                        return ParseStringRepresentation(reader.ReadString(), checksum);
                }
            }
            throw FxTrace.Exception.AsError(new SerializationException());
        }

        // Serialization

        // Encode to Base64 string
        public string Encode()
        {
            return Encode(WorkflowSymbol.DefaultEncodingFormat); // default format
        }

        internal string Encode(EncodingFormat encodingFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    if (this.checksum != null)
                    {
                        writer.Write((byte)(encodingFormat | EncodingFormat.Checksum));
                        SymbolHelper.WriteEncodedInt32(writer, this.checksum.Length);
                        writer.Write(this.checksum);
                    }
                    else
                    {
                        writer.Write((byte)encodingFormat);
                    }
                    switch (encodingFormat)
                    {
                        case EncodingFormat.Binary:
                            this.Write(writer);
                            break;
                        case EncodingFormat.String:
                            writer.Write(this.ToString());
                            break;
                        default:
                            throw FxTrace.Exception.AsError(new SerializationException());
                    }
                    // Need to copy to a buffer to trim excess capacity.
                    byte[] buffer = new byte[ms.Length];
                    Array.Copy(ms.GetBuffer(), buffer, ms.Length);
                    return Convert.ToBase64String(buffer);
                }
            } 
        }

        // Binary deserializer
        static WorkflowSymbol ParseBinary(byte[] bytes, byte[] checksum)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                return new WorkflowSymbol(reader, checksum);
            }
        }

        // Binary serializer
        void Write(BinaryWriter writer)
        {
            writer.Write(this.FileName ?? string.Empty);
            if (this.Symbols != null)
            {
                SymbolHelper.WriteEncodedInt32(writer, this.Symbols.Count);
                foreach (ActivitySymbol actSym in this.Symbols)
                {
                    actSym.Write(writer);
                }
            }
            else
            {
                SymbolHelper.WriteEncodedInt32(writer, 0);
            }
        }

        // String encoding serialization.

        // This is used for String encoding format.
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}", this.FileName ?? string.Empty);
            if (this.Symbols != null)
            {
                foreach (ActivitySymbol symbol in Symbols)
                {
                    builder.AppendFormat(";{0}", symbol.ToString());
                }
            }
            return builder.ToString();
        }

        // Deserialization of string encoding format.
        static WorkflowSymbol ParseStringRepresentation(string symbolString, byte[] checksum)
        {
            string[] s = symbolString.Split(';');
            int numSymbols = s.Length - 1;
            ActivitySymbol[] symbols = new ActivitySymbol[numSymbols];
            for (int i = 0; i < numSymbols; ++i)
            {
                string[] symbolSegments = s[i + 1].Split(',');
                Fx.Assert(symbolSegments.Length == 5, "Invalid activity symbol");
                symbols[i] = new ActivitySymbol
                {
                    QualifiedId = QualifiedId.Parse(symbolSegments[0]).AsByteArray(),
                    StartLine = int.Parse(symbolSegments[1], CultureInfo.InvariantCulture),
                    StartColumn = int.Parse(symbolSegments[2], CultureInfo.InvariantCulture),
                    EndLine = int.Parse(symbolSegments[3], CultureInfo.InvariantCulture),
                    EndColumn = int.Parse(symbolSegments[4], CultureInfo.InvariantCulture)
                };
            }

            return new WorkflowSymbol
            {
                FileName = s[0],
                Symbols = symbols,
                checksum = checksum
            };

        }

        public bool CalculateChecksum()
        {
            this.checksum = null;
            if (!string.IsNullOrEmpty(this.FileName))
            {
                this.checksum = SymbolHelper.CalculateChecksum(this.FileName);
            }
            return (this.checksum != null);
        }

    }
}
