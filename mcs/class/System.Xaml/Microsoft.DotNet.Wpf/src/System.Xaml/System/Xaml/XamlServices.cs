// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Xaml;

namespace System.Xaml
{
    public static class XamlServices
    {
        // The main function is Load(XamlReader)
        // all the other helper functions call this.
        // This allows the policy that the 2nd arg defaults to ObjectWriter
        // to be in one place.

        public static object Parse(string xaml)
        {
            if (xaml == null)
            {
                throw new ArgumentNullException("xaml");
            }

            StringReader stringReader = new StringReader(xaml);
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return Load(xamlReader);
            }
        }

        public static object Load(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            using (XmlReader xmlReader = XmlReader.Create(fileName))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return Load(xamlReader);
            }
        }

        public static object Load(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (XmlReader xmlReader = XmlReader.Create(stream))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return Load(xamlReader);
            }
        }

        public static object Load(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            using (XmlReader xmlReader = XmlReader.Create(textReader))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return Load(xamlReader);
            }
        }

        public static object Load(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            using (XamlXmlReader xamlReader = new XamlXmlReader(xmlReader))
            {
                return Load(xamlReader);
            }
        }

        // -----  Base case Load.

        public static object Load(XamlReader xamlReader)
        {
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }

            XamlObjectWriter objectWriter = new XamlObjectWriter(xamlReader.SchemaContext);

            Transform(xamlReader, objectWriter);

            return objectWriter.Result;
        }

        public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter)
        {
            // arguments are validated by the callee here.
            Transform(xamlReader, xamlWriter, true);
        }

        public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
        {
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }

            if (xamlWriter == null)
            {
                throw new ArgumentNullException("xamlWriter");
            }

            IXamlLineInfo xamlLineInfo = xamlReader as IXamlLineInfo;
            IXamlLineInfoConsumer xamlLineInfoConsumer = xamlWriter as IXamlLineInfoConsumer;
            bool shouldPassLineNumberInfo = false;
            if ((xamlLineInfo != null && xamlLineInfo.HasLineInfo)
                && (xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo))
            {
                shouldPassLineNumberInfo = true;
            }

            while (xamlReader.Read())
            {
                if (shouldPassLineNumberInfo)
                {
                    if (xamlLineInfo.LineNumber != 0)
                    {
                        xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
                    }
                }
                xamlWriter.WriteNode(xamlReader);
            }

            if (closeWriter)
            {
                xamlWriter.Close();
            }
        }

        public static string Save(object instance)
        {
            var sw = new StringWriter(CultureInfo.CurrentCulture);
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                Save(xw, instance);
            }

            return sw.ToString();
        }

        public static void Save(String fileName, object instance)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            //
            // At this point it can only be empty
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.Get(SRID.StringIsNullOrEmpty), "fileName");
            }
            using (var writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                Save(writer, instance);
                writer.Flush();
            }
        }

        public static void Save(Stream stream, object instance)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                Save(writer, instance);
                writer.Flush();
            }
        }

        public static void Save(TextWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                Save(xmlWriter, instance);
                xmlWriter.Flush();
            }
        }

        public static void Save(XmlWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            using (XamlXmlWriter xamlWriter = new XamlXmlWriter(writer, new XamlSchemaContext()))
            {
                Save(xamlWriter, instance);
            }
        }

        public static void Save(XamlWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            XamlObjectReader objectReader = new XamlObjectReader(instance, writer.SchemaContext);

            Transform(objectReader, writer);
        }
    }
}
