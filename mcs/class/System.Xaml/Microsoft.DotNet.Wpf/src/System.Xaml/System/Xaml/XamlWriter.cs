// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Xaml
{
    public abstract class XamlWriter : IDisposable
    {
        public abstract void WriteGetObject();
        public abstract void WriteStartObject(XamlType type);
        public abstract void WriteEndObject();
        public abstract void WriteStartMember(XamlMember xamlMember);
        public abstract void WriteEndMember();
        public abstract void WriteValue(object value);

        public abstract void WriteNamespace(NamespaceDeclaration namespaceDeclaration);

        public abstract XamlSchemaContext SchemaContext { get; }

        public void WriteNode(XamlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            switch (reader.NodeType)
            {
            case XamlNodeType.NamespaceDeclaration:
                WriteNamespace(reader.Namespace);
                break;

            case XamlNodeType.StartObject:
                WriteStartObject(reader.Type);
                break;

            case XamlNodeType.GetObject:
                WriteGetObject();
                break;

            case XamlNodeType.EndObject:
                WriteEndObject();
                break;

            case XamlNodeType.StartMember:
                WriteStartMember(reader.Member);
                break;

            case XamlNodeType.EndMember:
                WriteEndMember();
                break;

            case XamlNodeType.Value:
                WriteValue(reader.Value);
                break;

            case XamlNodeType.None:
                break;

            default:
                throw new NotImplementedException(SR.Get(SRID.MissingCaseXamlNodes));
            }
        }

        #region IDisposable

        // See Framework Design Guidelines, pp. 254-256.

        protected bool IsDisposed { get; private set; }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true; // must call the base class to get IsDisposed == true;
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        #endregion
    }
}
