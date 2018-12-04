// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// XmlDataValue
// A type to isolate our handling of System.Xml types so that we
// don't have to load System.Xml in the BAML case unless we really
// do have an XML Island.

using System.Xml;
using System.IO;

namespace System.Windows.Markup
{
    [ContentProperty("Text")]
    sealed public class XData
    {
        XmlReader _reader;
        string _text;

        public XData()
        {
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _reader = null;
            }
        }

        // XmlReader is typed "object" so that the calling code can read
        // and handle the value without loading System.Xml.dll.
        public object XmlReader
        {
            get
            {
                if (_reader == null)
                {
                    StringReader stringReader = new StringReader(Text);
                    _reader = System.Xml.XmlReader.Create(stringReader);
                }
                return _reader;
            }
            set
            {
                _reader = value as XmlReader;
                _text = null;
            }
        }

    }
}