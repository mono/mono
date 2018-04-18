//---------------------------------------------------------------------
// <copyright file="XmlExpressionDumper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.Common.CommandTrees.Internal
{
    /// <summary>
    /// An implementation of ExpressionDumper that produces an XML string.
    /// </summary>
    internal class XmlExpressionDumper : ExpressionDumper
    {
        internal static Encoding DefaultEncoding { get { return Encoding.UTF8; } }

        private XmlWriter _writer;

        internal XmlExpressionDumper(Stream stream)
            : this(stream, XmlExpressionDumper.DefaultEncoding, true) {}
        
        internal XmlExpressionDumper(Stream stream, Encoding encoding, bool indent) : base()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.Indent = true;
            settings.Encoding = encoding;
            _writer = XmlWriter.Create(stream, settings);
            _writer.WriteStartDocument(true);
        }

        internal void Close()
        {
            _writer.WriteEndDocument();
            _writer.Flush();
            _writer.Close();
        }

        internal override void Begin(string name, Dictionary<string, object> attrs)
        {
            _writer.WriteStartElement(name);
            if (attrs != null)
            {
                foreach (KeyValuePair<string, object> attr in attrs)
                {

                    _writer.WriteAttributeString(attr.Key, (null == attr.Value ? "" : attr.Value.ToString()));
                }
            }  
        }

        internal override void End(string name)
        {
            _writer.WriteEndElement();
        }
    }
}
