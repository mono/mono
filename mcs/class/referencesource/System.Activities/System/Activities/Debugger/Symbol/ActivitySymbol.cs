//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Debugger.Symbol
{
    using System.Globalization;
    using System.IO;
    using System.Runtime;

    // Represent the debug symbol for an Activity.
    // It defines the start/end of Activity in the Xaml file.
    public class ActivitySymbol
    {
        public int StartLine { get; internal set; }
        public int StartColumn { get; internal set; }
        public int EndLine { get; internal set; }
        public int EndColumn { get; internal set; }
        // Internal representation of QualifiedId.
        internal byte[] QualifiedId { get; set; }
        string id;

        // Publicly available Id.
        public string Id
        {
            get
            {
                if (this.id == null)
                {
                    if (this.QualifiedId != null)
                    {
                        this.id = new QualifiedId(this.QualifiedId).ToString();
                    }
                    else
                    {
                        this.id = string.Empty;
                    }
                }
                return this.id;
            }
        }

        // Binary serializer.
        internal void Write(BinaryWriter writer)
        {
            SymbolHelper.WriteEncodedInt32(writer, this.StartLine);
            SymbolHelper.WriteEncodedInt32(writer, this.StartColumn);
            SymbolHelper.WriteEncodedInt32(writer, this.EndLine);
            SymbolHelper.WriteEncodedInt32(writer, this.EndColumn);
            if (this.QualifiedId != null)
            {
                SymbolHelper.WriteEncodedInt32(writer, this.QualifiedId.Length);
                writer.Write(this.QualifiedId, 0, this.QualifiedId.Length);
            }
            else
            {
                SymbolHelper.WriteEncodedInt32(writer, 0);
            }
        }

        // Binary deserializer.
        internal ActivitySymbol(BinaryReader reader)
        {
            this.StartLine = SymbolHelper.ReadEncodedInt32(reader);
            this.StartColumn = SymbolHelper.ReadEncodedInt32(reader);
            this.EndLine = SymbolHelper.ReadEncodedInt32(reader);
            this.EndColumn = SymbolHelper.ReadEncodedInt32(reader);
            int qidLength = SymbolHelper.ReadEncodedInt32(reader);
            if (qidLength > 0)
            {
                this.QualifiedId = reader.ReadBytes(qidLength);
            }
        }

        internal ActivitySymbol()
        {
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4}", this.Id, this.StartLine, this.StartColumn, this.EndLine, this.EndColumn);
        }
    }

}
