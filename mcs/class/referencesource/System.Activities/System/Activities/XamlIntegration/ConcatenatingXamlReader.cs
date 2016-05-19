//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using System.Xaml;

    class ConcatenatingXamlReader : XamlReader
    {
        // Invariant: !isEof => readers.Current != null
        bool isEof;
        IEnumerator<XamlReader> readers;
        XamlSchemaContext schemaContext;

        public ConcatenatingXamlReader(params XamlReader[] readers)
        {
            this.readers = ((IEnumerable<XamlReader>)readers).GetEnumerator();
            if (this.readers.MoveNext())
            {
                this.schemaContext = this.readers.Current.SchemaContext;
            }
            else
            {
                this.isEof = true;
            }
        }

        public override bool IsEof
        {
            get { return this.isEof ? true : this.readers.Current.IsEof; }
        }

        public override XamlMember Member
        {
            get { return this.isEof ? null : this.readers.Current.Member; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return this.isEof ? null : this.readers.Current.Namespace; }
        }

        public override XamlNodeType NodeType
        {
            get { return this.isEof ? XamlNodeType.None : this.readers.Current.NodeType; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return this.schemaContext; }
        }

        public override XamlType Type
        {
            get { return this.isEof ? null : this.readers.Current.Type; }
        }

        public override object Value
        {
            get { return this.isEof ? null : this.readers.Current.Value; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.isEof)
            {
                readers.Current.Close();
                while (readers.MoveNext())
                {
                    readers.Current.Close();
                }
                this.isEof = true;
            }
            base.Dispose(disposing);
        }

        public override bool Read()
        {
            if (!this.isEof)
            {
                do
                {
                    if (this.readers.Current.Read())
                    {
                        return true;
                    }
                    this.readers.Current.Close();
                }
                while (this.readers.MoveNext());
                this.isEof = true;
            }
            return false;
        }
    }
}


