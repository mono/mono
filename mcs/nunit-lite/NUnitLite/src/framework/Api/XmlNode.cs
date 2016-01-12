// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// XmlNode represents a single node in the XML representation
    /// of a Test or TestResult. It replaces System.Xml.XmlNode and
    /// provides a minimal set of methods for operating on the XML 
    /// in a platform-independent manner.
    /// </summary>
    public class XmlNode
    {
        #region Private Fields

        private string name;

        private AttributeDictionary attributes;
        
        private NodeList childNodes;

        private string textContent;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of XmlNode
        /// </summary>
        /// <param name="name">The name of the node</param>
        public XmlNode(string name)
        {
            this.name = name;
            this.attributes = new AttributeDictionary();
            this.childNodes = new NodeList();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a new top level element node.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <returns></returns>
        public static XmlNode CreateTopLevelElement(string name)
        {
            return new XmlNode(name);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the node
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the text content of the node
        /// </summary>
        public string TextContent
        {
            get { return textContent; }
            set { textContent = value; }
        }

        /// <summary>
        /// Gets the text content of the node escaped as needed.
        /// This is for use in writing out the XML representation.
        /// </summary>
        public string EscapedTextContent
        {
            get { return Escape(textContent); }
        }

        /// <summary>
        /// Gets the dictionary of attributes
        /// </summary>
        public AttributeDictionary Attributes
        {
            get { return attributes; }
        }

        /// <summary>
        /// Gets a list of child nodes
        /// </summary>
        public NodeList ChildNodes
        {
            get { return childNodes; }
        }

        /// <summary>
        /// Gets the first child of this node, or null
        /// </summary>
        public XmlNode FirstChild
        {
            get
            {
                return ChildNodes.Count > 0
                    ? ChildNodes[0] as XmlNode
                    : null;
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Adds a new element as a child of the current node and returns it.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <returns>The newly created child element</returns>
        public XmlNode AddElement(string name)
        {
            XmlNode childResult = new XmlNode(name);
            ChildNodes.Add(childResult);
            return childResult;
        }

        /// <summary>
        /// Adds an attribute with a specified name and value to the XmlNode.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public void AddAttribute(string name, string value)
        {
            this.Attributes.Add(name, value);
        }

        /// <summary>
        /// Finds a single descendant of this node matching an xpath
        /// specification. The format of the specification is
        /// limited to what is needed by NUnit and its tests.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XmlNode FindDescendant(string xpath)
        {
            NodeList nodes = FindDescendants(xpath);

            return nodes.Count > 0
                ? nodes[0] as XmlNode
                : null;
        }

        /// <summary>
        /// Finds all descendants of this node matching an xpath
        /// specification. The format of the specification is
        /// limited to what is needed by NUnit and its tests.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public NodeList FindDescendants(string xpath)
        {
            NodeList nodeList = new NodeList();
            nodeList.Add(this);

            return ApplySelection(nodeList, xpath);
        }
        
        /// <summary>
        /// Writes the XML representation of the node to an XmlWriter
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(this.Name);

            foreach (string name in this.Attributes.Keys)
                writer.WriteAttributeString(name, Attributes[name]);

            if (this.TextContent != null)
                writer.WriteChars(this.TextContent.ToCharArray(), 0, this.TextContent.Length);

            foreach (XmlNode node in this.ChildNodes)
                node.WriteTo(writer);

            writer.WriteEndElement();
        }

        #endregion

        #region Helper Methods

        private static NodeList ApplySelection(NodeList nodeList, string xpath)
        {
            Guard.ArgumentNotNullOrEmpty(xpath, "xpath");
            if (xpath[0] == '/')
                throw new ArgumentException("XPath expressions starting with '/' are not supported", "xpath");
            if (xpath.IndexOf("//") >= 0)
                throw new ArgumentException("XPath expressions with '//' are not supported", "xpath");

            string head = xpath;
            string tail = null;

            int slash = xpath.IndexOf('/');
            if (slash >= 0)
            {
                head = xpath.Substring(0, slash);
                tail = xpath.Substring(slash + 1);
            }

            NodeList resultNodes = new NodeList();
            NodeFilter filter = new NodeFilter(head);

            foreach(XmlNode node in nodeList)
                foreach (XmlNode childNode in node.ChildNodes)
                    if (filter.Pass(childNode))
                        resultNodes.Add(childNode);

            return tail != null
                ? ApplySelection(resultNodes, tail)
                : resultNodes;
        }

        private static string Escape(string original)
        {
            return original
                .Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        #endregion

        #region Nested NodeFilter class

        class NodeFilter
        {
            private string nodeName;
            private string propName;
            private string propValue;

            public NodeFilter(string xpath)
            {
                this.nodeName = xpath;
                
                int lbrack = xpath.IndexOf('[');
                if (lbrack >= 0)
                {
                    if (!xpath.EndsWith("]"))
                        throw new ArgumentException("Invalid property expression", "xpath");

                    nodeName = xpath.Substring(0, lbrack);
                    string filter = xpath.Substring(lbrack+1, xpath.Length - lbrack - 2);

                    int equals = filter.IndexOf('=');
                    if (equals < 0 || filter[0] != '@')
                        throw new ArgumentException("Invalid property expression", "xpath");

                    this.propName = filter.Substring(1, equals - 1).Trim();
                    this.propValue = filter.Substring(equals + 1).Trim(new char[] { ' ', '"', '\'' });
                }
            }

            public bool Pass(XmlNode node)
            {
                if (node.Name != nodeName)
                    return false;
                
                if (propName == null)
                    return true;
                
                return (string)node.Attributes[propName] == propValue;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class used to represent a list of XmlResults
    /// </summary>
#if CLR_2_0 || CLR_4_0
    public class NodeList : System.Collections.Generic.List<XmlNode>
    {
    }
#else
    public class NodeList : System.Collections.ArrayList
    {
    }
#endif

    /// <summary>
    /// Class used to represent the attributes of a node
    /// </summary>
#if CLR_2_0 || CLR_4_0
    public class AttributeDictionary : System.Collections.Generic.Dictionary<string, string>
    {
    }
#else
    public class AttributeDictionary : System.Collections.Specialized.StringDictionary
    {
        private System.Collections.ArrayList orderedKeys = new System.Collections.ArrayList();

        /// <summary>
        /// Adds a key and value to the dictionary. Overridden to
        /// save the order in which keys are added.
        /// </summary>
        /// <param name="key">The attribute key</param>
        /// <param name="value">The attribute value</param>
        public override void Add(string key, string value)
        {
            base.Add(key, value);
            orderedKeys.Add(key);
        }

        /// <summary>
        /// Gets the keys in the same order they were added.
        /// </summary>
        public override System.Collections.ICollection Keys
        {
            get
            {
                return orderedKeys;
            }
        }
    }
#endif
}
