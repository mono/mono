// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an HTML node.
    /// </summary>
    [DebuggerDisplay("Name: {OriginalName}}")]
    public class HtmlNode : IXPathNavigable
    {
        #region Fields

        internal HtmlAttributeCollection _attributes;
        internal HtmlNodeCollection _childnodes;
        internal HtmlNode _endnode;

        internal bool _innerchanged;
        internal string _innerhtml;
        internal int _innerlength;
        internal int _innerstartindex;
        internal int _line;
        internal int _lineposition;
        private string _name;
        internal int _namelength;
        internal int _namestartindex;
        internal HtmlNode _nextnode;
        internal HtmlNodeType _nodetype;
        internal bool _outerchanged;
        internal string _outerhtml;
        internal int _outerlength;
        internal int _outerstartindex;
        internal HtmlDocument _ownerdocument;
        internal HtmlNode _parentnode;
        internal HtmlNode _prevnode;
        internal HtmlNode _prevwithsamename;
        internal bool _starttag;
        internal int _streamposition;

        #endregion

        #region Static Members

        /// <summary>
        /// Gets the name of a comment node. It is actually defined as '#comment'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameComment = "#comment";

        /// <summary>
        /// Gets the name of the document node. It is actually defined as '#document'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameDocument = "#document";

        /// <summary>
        /// Gets the name of a text node. It is actually defined as '#text'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameText = "#text";

        /// <summary>
        /// Gets a collection of flags that define specific behaviors for specific element nodes.
        /// The table contains a DictionaryEntry list with the lowercase tag name as the Key, and a combination of HtmlElementFlags as the Value.
        /// </summary>
        public static Hashtable ElementsFlags;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize HtmlNode. Builds a list of all tags that have special allowances
        /// </summary>
        static HtmlNode()
        {
            // tags whose content may be anything
            ElementsFlags = new Hashtable();
            ElementsFlags.Add("script", HtmlElementFlag.CData);
            ElementsFlags.Add("style", HtmlElementFlag.CData);
            ElementsFlags.Add("noxhtml", HtmlElementFlag.CData);

            // tags that can not contain other tags
            ElementsFlags.Add("base", HtmlElementFlag.Empty);
            ElementsFlags.Add("link", HtmlElementFlag.Empty);
            ElementsFlags.Add("meta", HtmlElementFlag.Empty);
            ElementsFlags.Add("isindex", HtmlElementFlag.Empty);
            ElementsFlags.Add("hr", HtmlElementFlag.Empty);
            ElementsFlags.Add("col", HtmlElementFlag.Empty);
            ElementsFlags.Add("img", HtmlElementFlag.Empty);
            ElementsFlags.Add("param", HtmlElementFlag.Empty);
            ElementsFlags.Add("embed", HtmlElementFlag.Empty);
            ElementsFlags.Add("frame", HtmlElementFlag.Empty);
            ElementsFlags.Add("wbr", HtmlElementFlag.Empty);
            ElementsFlags.Add("bgsound", HtmlElementFlag.Empty);
            ElementsFlags.Add("spacer", HtmlElementFlag.Empty);
            ElementsFlags.Add("keygen", HtmlElementFlag.Empty);
            ElementsFlags.Add("area", HtmlElementFlag.Empty);
            ElementsFlags.Add("input", HtmlElementFlag.Empty);
            ElementsFlags.Add("basefont", HtmlElementFlag.Empty);

            ElementsFlags.Add("form", HtmlElementFlag.CanOverlap | HtmlElementFlag.Empty);

            // they sometimes contain, and sometimes they don 't...
            ElementsFlags.Add("option", HtmlElementFlag.Empty);

            // tag whose closing tag is equivalent to open tag:
            // <p>bla</p>bla will be transformed into <p>bla</p>bla
            // <p>bla<p>bla will be transformed into <p>bla<p>bla and not <p>bla></p><p>bla</p> or <p>bla<p>bla</p></p>
            //<br> see above
            ElementsFlags.Add("br", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
            ElementsFlags.Add("p", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
        }

        /// <summary>
        /// Initializes HtmlNode, providing type, owner and where it exists in a collection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ownerdocument"></param>
        /// <param name="index"></param>
        public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
        {
            _nodetype = type;
            _ownerdocument = ownerdocument;
            _outerstartindex = index;

            switch (type)
            {
                case HtmlNodeType.Comment:
                    Name = HtmlNodeTypeNameComment;
                    _endnode = this;
                    break;

                case HtmlNodeType.Document:
                    Name = HtmlNodeTypeNameDocument;
                    _endnode = this;
                    break;

                case HtmlNodeType.Text:
                    Name = HtmlNodeTypeNameText;
                    _endnode = this;
                    break;
            }

            if (_ownerdocument._openednodes != null)
            {
                if (!Closed)
                {
                    // we use the index as the key

                    // -1 means the node comes from public
                    if (-1 != index)
                    {
                        _ownerdocument._openednodes.Add(index, this);
                    }
                }
            }

            if ((-1 != index) || (type == HtmlNodeType.Comment) || (type == HtmlNodeType.Text)) return;
            // innerhtml and outerhtml must be calculated
            _outerchanged = true;
            _innerchanged = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of HTML attributes for this node. May not be null.
        /// </summary>
        public HtmlAttributeCollection Attributes
        {
            get
            {
                if (!HasAttributes)
                {
                    _attributes = new HtmlAttributeCollection(this);
                }
                return _attributes;
            }
            internal set { _attributes = value; }
        }

        /// <summary>
        /// Gets all the children of the node.
        /// </summary>
        public HtmlNodeCollection ChildNodes
        {
            get
            {
                if (_childnodes == null)
                {
                    _childnodes = new HtmlNodeCollection(this);
                }
                return _childnodes;
            }
            internal set { _childnodes = value; }
        }

        /// <summary>
        /// Gets a value indicating if this node has been closed or not.
        /// </summary>
        public bool Closed
        {
            get { return (_endnode != null); }
        }

        /// <summary>
        /// Gets the collection of HTML attributes for the closing tag. May not be null.
        /// </summary>
        public HtmlAttributeCollection ClosingAttributes
        {
            get
            {
                if (!HasClosingAttributes)
                {
                    return new HtmlAttributeCollection(this);
                }
                return _endnode.Attributes;
            }
        }

        internal HtmlNode EndNode
        {
            get { return _endnode; }
        }

        /// <summary>
        /// Gets the first child of the node.
        /// </summary>
        public HtmlNode FirstChild
        {
            get
            {
                if (!HasChildNodes)
                {
                    return null;
                }
                return _childnodes[0];
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current node has any attributes.
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                if (_attributes == null)
                {
                    return false;
                }

                if (_attributes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node has any child nodes.
        /// </summary>
        public bool HasChildNodes
        {
            get
            {
                if (_childnodes == null)
                {
                    return false;
                }

                if (_childnodes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current node has any attributes on the closing tag.
        /// </summary>
        public bool HasClosingAttributes
        {
            get
            {
                if ((_endnode == null) || (_endnode == this))
                {
                    return false;
                }

                if (_endnode._attributes == null)
                {
                    return false;
                }

                if (_endnode._attributes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'id' HTML attribute. The document must have been parsed using the OptionUseIdAttribute set to true.
        /// </summary>
        public string Id
        {
            get
            {
                if (_ownerdocument._nodesid == null)
                {
                    throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
                }
                return GetId();
            }
            set
            {
                if (_ownerdocument._nodesid == null)
                {
                    throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
                }

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                SetId(value);
            }
        }

        /// <summary>
        /// Gets or Sets the HTML between the start and end tags of the object.
        /// </summary>
        public virtual string InnerHtml
        {
            get
            {
                if (_innerchanged)
                {
                    _innerhtml = WriteContentTo();
                    _innerchanged = false;
                    return _innerhtml;
                }
                if (_innerhtml != null)
                {
                    return _innerhtml;
                }

                if (_innerstartindex < 0)
                {
                    return string.Empty;
                }

                return _ownerdocument._text.Substring(_innerstartindex, _innerlength);
            }
            set
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(value);

                RemoveAllChildren();
                AppendChildren(doc.DocumentNode.ChildNodes);
            }
        }

        /// <summary>
        /// Gets or Sets the text between the start and end tags of the object.
        /// </summary>
        public virtual string InnerText
        {
            get
            {
                if (_nodetype == HtmlNodeType.Text)
                {
                    return ((HtmlTextNode) this).Text;
                }

                if (_nodetype == HtmlNodeType.Comment)
                {
                    return ((HtmlCommentNode) this).Comment;
                }

                // note: right now, this method is *slow*, because we recompute everything.
                // it could be optimised like innerhtml
                if (!HasChildNodes)
                {
                    return string.Empty;
                }

                string s = null;
                foreach (HtmlNode node in ChildNodes)
                {
                    s += node.InnerText;
                }
                return s;
            }
        }

        /// <summary>
        /// Gets the last child of the node.
        /// </summary>
        public HtmlNode LastChild
        {
            get
            {
                return !HasChildNodes ? null : _childnodes[_childnodes.Count - 1];
            }
        }

        /// <summary>
        /// Gets the line number of this node in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the column number of this node in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
            internal set { _lineposition = value; }
        }

        /// <summary>
        /// Gets or sets this node's name.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    Name = _ownerdocument._text.Substring(_namestartindex, _namelength);
                }
                return _name != null ? _name.ToLower() : string.Empty;
            }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the HTML node immediately following this element.
        /// </summary>
        public HtmlNode NextSibling
        {
            get { return _nextnode; }
            internal set { _nextnode = value; }
        }

        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        public HtmlNodeType NodeType
        {
            get { return _nodetype; }
            internal set { _nodetype = value; }
        }

        /// <summary>
        /// The original unaltered name of the tag
        /// </summary>
        public string OriginalName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets or Sets the object and its content in HTML.
        /// </summary>
        public virtual string OuterHtml
        {
            get
            {
                if (_outerchanged)
                {
                    _outerhtml = WriteTo();
                    _outerchanged = false;
                    return _outerhtml;
                }

                if (_outerhtml != null)
                {
                    return _outerhtml;
                }

                if (_outerstartindex < 0)
                {
                    return string.Empty;
                }

                return _ownerdocument._text.Substring(_outerstartindex, _outerlength);
            }
        }

        /// <summary>
        /// Gets the <see cref="HtmlDocument"/> to which this node belongs.
        /// </summary>
        public HtmlDocument OwnerDocument
        {
            get { return _ownerdocument; }
            internal set { _ownerdocument = value; }
        }

        /// <summary>
        /// Gets the parent of this node (for nodes that can have parents).
        /// </summary>
        public HtmlNode ParentNode
        {
            get { return _parentnode; }
            internal set { _parentnode = value; }
        }

        /// <summary>
        /// Gets the node immediately preceding this node.
        /// </summary>
        public HtmlNode PreviousSibling
        {
            get { return _prevnode; }
            internal set { _prevnode = value; }
        }

        /// <summary>
        /// Gets the stream position of this node in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamposition; }
        }

        /// <summary>
        /// Gets a valid XPath string that points to this node
        /// </summary>
        public string XPath
        {
            get
            {
                string basePath = (ParentNode == null || ParentNode.NodeType == HtmlNodeType.Document)
                                      ? "/"
                                      : ParentNode.XPath + "/";
                return basePath + GetRelativeXpath();
            }
        }

        #endregion

        #region IXPathNavigable Members

        /// <summary>
        /// Creates a new XPathNavigator object for navigating this HTML node.
        /// </summary>
        /// <returns>An XPathNavigator object. The XPathNavigator is positioned on the node from which the method was called. It is not positioned on the root of the document.</returns>
        public XPathNavigator CreateNavigator()
        {
            return new HtmlNodeNavigator(_ownerdocument, this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if an element node can be kept overlapped.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be <c>null</c>.</param>
        /// <returns>true if the name is the name of an element node that can be kept overlapped, <c>false</c> otherwise.</returns>
        public static bool CanOverlapElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object flag = ElementsFlags[name.ToLower()];
            if (flag == null)
            {
                return false;
            }
            return (((HtmlElementFlag) flag) & HtmlElementFlag.CanOverlap) != 0;
        }

        /// <summary>
        /// Creates an HTML node from a string representing literal HTML.
        /// </summary>
        /// <param name="html">The HTML text.</param>
        /// <returns>The newly created node instance.</returns>
        public static HtmlNode CreateNode(string html)
        {
            // REVIEW: this is *not* optimum...
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.FirstChild;
        }

        /// <summary>
        /// Determines if an element node is a CDATA element node.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of a CDATA element node, false otherwise.</returns>
        public static bool IsCDataElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object flag = ElementsFlags[name.ToLower()];
            if (flag == null)
            {
                return false;
            }
            return (((HtmlElementFlag) flag) & HtmlElementFlag.CData) != 0;
        }

        /// <summary>
        /// Determines if an element node is closed.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of a closed element node, false otherwise.</returns>
        public static bool IsClosedElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object flag = ElementsFlags[name.ToLower()];
            if (flag == null)
            {
                return false;
            }
            return (((HtmlElementFlag) flag) & HtmlElementFlag.Closed) != 0;
        }

        /// <summary>
        /// Determines if an element node is defined as empty.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of an empty element node, false otherwise.</returns>
        public static bool IsEmptyElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                return true;
            }

            // <!DOCTYPE ...
            if ('!' == name[0])
            {
                return true;
            }

            // <?xml ...
            if ('?' == name[0])
            {
                return true;
            }

            object flag = ElementsFlags[name.ToLower()];
            if (flag == null)
            {
                return false;
            }
            return (((HtmlElementFlag) flag) & HtmlElementFlag.Empty) != 0;
        }

        /// <summary>
        /// Determines if a text corresponds to the closing tag of an node that can be kept overlapped.
        /// </summary>
        /// <param name="text">The text to check. May not be null.</param>
        /// <returns>true or false.</returns>
        public static bool IsOverlappedClosingElement(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            // min is </x>: 4
            if (text.Length <= 4)
                return false;

            if ((text[0] != '<') ||
                (text[text.Length - 1] != '>') ||
                (text[1] != '/'))
                return false;

            string name = text.Substring(2, text.Length - 3);
            return CanOverlapElement(name);
        }

        /// <summary>
        /// Returns a collection of all ancestor nodes of this element.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Ancestors()
        {
            HtmlNode node = ParentNode;
            while (node.ParentNode != null)
            {
                yield return node.ParentNode;
                node = node.ParentNode;
            }
        }

        /// <summary>
        /// Get Ancestors with matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Ancestors(string name)
        {
            for (HtmlNode n = ParentNode; n != null; n = n.ParentNode)
                if (n.Name == name)
                    yield return n;
        }

        /// <summary>
        /// Returns a collection of all ancestor nodes of this element.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> AncestorsAndSelf()
        {
            for (HtmlNode n = this; n != null; n = n.ParentNode)
                yield return n;
        }

        /// <summary>
        /// Gets all anscestor nodes and the current node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
        {
            for (HtmlNode n = this; n != null; n = n.ParentNode)
                if (n.Name == name)
                    yield return n;
        }

        /// <summary>
        /// Adds the specified node to the end of the list of children of this node.
        /// </summary>
        /// <param name="newChild">The node to add. May not be null.</param>
        /// <returns>The node added.</returns>
        public HtmlNode AppendChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            ChildNodes.Append(newChild);
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return newChild;
        }

        /// <summary>
        /// Adds the specified node to the end of the list of children of this node.
        /// </summary>
        /// <param name="newChildren">The node list to add. May not be null.</param>
        public void AppendChildren(HtmlNodeCollection newChildren)
        {
            if (newChildren == null)
                throw new ArgumentNullException("newChildrend");

            foreach (HtmlNode newChild in newChildren)
            {
                AppendChild(newChild);
            }
        }

        /// <summary>
        /// Gets all Attributes with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlAttribute> ChildAttributes(string name)
        {
            return Attributes.AttributesWithName(name);
        }

        /// <summary>
        /// Creates a duplicate of the node
        /// </summary>
        /// <returns></returns>
        public HtmlNode Clone()
        {
            return CloneNode(true);
        }

        /// <summary>
        /// Creates a duplicate of the node and changes its name at the same time.
        /// </summary>
        /// <param name="newName">The new name of the cloned node. May not be <c>null</c>.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(string newName)
        {
            return CloneNode(newName, true);
        }

        /// <summary>
        /// Creates a duplicate of the node and changes its name at the same time.
        /// </summary>
        /// <param name="newName">The new name of the cloned node. May not be null.</param>
        /// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(string newName, bool deep)
        {
            if (newName == null)
            {
                throw new ArgumentNullException("newName");
            }

            HtmlNode node = CloneNode(deep);
            node.Name = newName;
            return node;
        }

        /// <summary>
        /// Creates a duplicate of the node.
        /// </summary>
        /// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(bool deep)
        {
            HtmlNode node = _ownerdocument.CreateNode(_nodetype);
            node.Name = Name;

            switch (_nodetype)
            {
                case HtmlNodeType.Comment:
                    ((HtmlCommentNode) node).Comment = ((HtmlCommentNode) this).Comment;
                    return node;

                case HtmlNodeType.Text:
                    ((HtmlTextNode) node).Text = ((HtmlTextNode) this).Text;
                    return node;
            }

            // attributes
            if (HasAttributes)
            {
                foreach (HtmlAttribute att in _attributes)
                {
                    HtmlAttribute newatt = att.Clone();
                    node.Attributes.Append(newatt);
                }
            }

            // closing attributes
            if (HasClosingAttributes)
            {
                node._endnode = _endnode.CloneNode(false);
                foreach (HtmlAttribute att in _endnode._attributes)
                {
                    HtmlAttribute newatt = att.Clone();
                    node._endnode._attributes.Append(newatt);
                }
            }
            if (!deep)
            {
                return node;
            }

            if (!HasChildNodes)
            {
                return node;
            }

            // child nodes
            foreach (HtmlNode child in _childnodes)
            {
                HtmlNode newchild = child.Clone();
                node.AppendChild(newchild);
            }
            return node;
        }

        /// <summary>
        /// Creates a duplicate of the node and the subtree under it.
        /// </summary>
        /// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
        public void CopyFrom(HtmlNode node)
        {
            CopyFrom(node, true);
        }

        /// <summary>
        /// Creates a duplicate of the node.
        /// </summary>
        /// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
        /// <param name="deep">true to recursively clone the subtree under the specified node, false to clone only the node itself.</param>
        public void CopyFrom(HtmlNode node, bool deep)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Attributes.RemoveAll();
            if (node.HasAttributes)
            {
                foreach (HtmlAttribute att in node.Attributes)
                {
                    SetAttributeValue(att.Name, att.Value);
                }
            }

            if (!deep)
            {
                RemoveAllChildren();
                if (node.HasChildNodes)
                {
                    foreach (HtmlNode child in node.ChildNodes)
                    {
                        AppendChild(child.CloneNode(true));
                    }
                }
            }
        }

        /// <summary>
        /// Creates an XPathNavigator using the root of this document.
        /// </summary>
        /// <returns></returns>
        public XPathNavigator CreateRootNavigator()
        {
            return new HtmlNodeNavigator(_ownerdocument, _ownerdocument.DocumentNode);
        }

        /// <summary>
        /// Gets all Descendant nodes for this node and each of child nodes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantNodes()
        {
            foreach (HtmlNode node in ChildNodes)
            {
                yield return node;
                foreach (HtmlNode descendant in node.DescendantNodes())
                    yield return descendant;
            }
        }

        /// <summary>
        /// Returns a collection of all descendant nodes of this element, in document order
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantNodesAndSelf()
        {
            return DescendantsAndSelf();
        }

        /// <summary>
        /// Gets all Descendant nodes in enumerated list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
        {
            foreach (HtmlNode node in DescendantNodes())
            {
                yield return node;
            }
        }

        /// <summary>
        /// Get all descendant nodes with matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            foreach (HtmlNode node in Descendants())
                if (node.Name == name)
                    yield return node;
        }

        /// <summary>
        /// Returns a collection of all descendant nodes of this element, in document order
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantsAndSelf()
        {
            yield return this;
            foreach (HtmlNode n in DescendantNodes())
            {
                HtmlNode el = n;
                if (el != null)
                    yield return el;
            }
        }

        /// <summary>
        /// Gets all descendant nodes including this node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
        {
            yield return this;
            foreach (HtmlNode node in Descendants())
                if (node.Name == name)
                    yield return node;
        }

        /// <summary>
        /// Gets first generation child node matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode Element(string name)
        {
            foreach (HtmlNode node in ChildNodes)
                if (node.Name == name)
                    return node;
            return null;
        }

        /// <summary>
        /// Gets matching first generation child nodes matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (HtmlNode node in ChildNodes)
                if (node.Name == name)
                    yield return node;
        }

        /// <summary>
        /// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public string GetAttributeValue(string name, string def)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!HasAttributes)
            {
                return def;
            }
            HtmlAttribute att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            return att.Value;
        }

        /// <summary>
        /// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public int GetAttributeValue(string name, int def)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!HasAttributes)
            {
                return def;
            }
            HtmlAttribute att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            try
            {
                return Convert.ToInt32(att.Value);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public bool GetAttributeValue(string name, bool def)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!HasAttributes)
            {
                return def;
            }
            HtmlAttribute att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            try
            {
                return Convert.ToBoolean(att.Value);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// Inserts the specified node immediately after the specified reference node.
        /// </summary>
        /// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
        /// <param name="refChild">The node that is the reference node. The newNode is placed after the refNode.</param>
        /// <returns>The node being inserted.</returns>
        public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            if (refChild == null)
            {
                return PrependChild(newChild);
            }

            if (newChild == refChild)
            {
                return newChild;
            }

            int index = -1;

            if (_childnodes != null)
            {
                index = _childnodes[refChild];
            }
            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }

            if (_childnodes != null) _childnodes.Insert(index + 1, newChild);

            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return newChild;
        }

        /// <summary>
        /// Inserts the specified node immediately before the specified reference node.
        /// </summary>
        /// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
        /// <param name="refChild">The node that is the reference node. The newChild is placed before this node.</param>
        /// <returns>The node being inserted.</returns>
        public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            if (refChild == null)
            {
                return AppendChild(newChild);
            }

            if (newChild == refChild)
            {
                return newChild;
            }

            int index = -1;

            if (_childnodes != null)
            {
                index = _childnodes[refChild];
            }

            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }

            if (_childnodes != null) _childnodes.Insert(index, newChild);

            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return newChild;
        }

        /// <summary>
        /// Adds the specified node to the beginning of the list of children of this node.
        /// </summary>
        /// <param name="newChild">The node to add. May not be <c>null</c>.</param>
        /// <returns>The node added.</returns>
        public HtmlNode PrependChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            ChildNodes.Prepend(newChild);
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return newChild;
        }

        /// <summary>
        /// Adds the specified node list to the beginning of the list of children of this node.
        /// </summary>
        /// <param name="newChildren">The node list to add. May not be <c>null</c>.</param>
        public void PrependChildren(HtmlNodeCollection newChildren)
        {
            if (newChildren == null)
            {
                throw new ArgumentNullException("newChildren");
            }

            foreach (HtmlNode newChild in newChildren)
            {
                PrependChild(newChild);
            }
        }

        /// <summary>
        /// Removes node from parent collection
        /// </summary>
        public void Remove()
        {
            if (ParentNode != null)
                ParentNode.ChildNodes.Remove(this);
        }

        /// <summary>
        /// Removes all the children and/or attributes of the current node.
        /// </summary>
        public void RemoveAll()
        {
            RemoveAllChildren();

            if (HasAttributes)
            {
                _attributes.Clear();
            }

            if ((_endnode != null) && (_endnode != this))
            {
                if (_endnode._attributes != null)
                {
                    _endnode._attributes.Clear();
                }
            }
            _outerchanged = true;
            _innerchanged = true;
        }

        /// <summary>
        /// Removes all the children of the current node.
        /// </summary>
        public void RemoveAllChildren()
        {
            if (!HasChildNodes)
            {
                return;
            }

            if (_ownerdocument.OptionUseIdAttribute)
            {
                // remove nodes from id list
                foreach (HtmlNode node in _childnodes)
                {
                    _ownerdocument.SetIdForNode(null, node.GetId());
                }
            }
            _childnodes.Clear();
            _outerchanged = true;
            _innerchanged = true;
        }

        /// <summary>
        /// Removes the specified child node.
        /// </summary>
        /// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
        /// <returns>The node removed.</returns>
        public HtmlNode RemoveChild(HtmlNode oldChild)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }

            int index = -1;

            if (_childnodes != null)
            {
                index = _childnodes[oldChild];
            }

            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }

            if (_childnodes != null)
                _childnodes.Remove(index);

            _ownerdocument.SetIdForNode(null, oldChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return oldChild;
        }

        /// <summary>
        /// Removes the specified child node.
        /// </summary>
        /// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
        /// <param name="keepGrandChildren">true to keep grand children of the node, false otherwise.</param>
        /// <returns>The node removed.</returns>
        public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }

            if ((oldChild._childnodes != null) && keepGrandChildren)
            {
                // get prev sibling
                HtmlNode prev = oldChild.PreviousSibling;

                // reroute grand children to ourselves
                foreach (HtmlNode grandchild in oldChild._childnodes)
                {
                    InsertAfter(grandchild, prev);
                }
            }
            RemoveChild(oldChild);
            _outerchanged = true;
            _innerchanged = true;
            return oldChild;
        }

        /// <summary>
        /// Replaces the child node oldChild with newChild node.
        /// </summary>
        /// <param name="newChild">The new node to put in the child list.</param>
        /// <param name="oldChild">The node being replaced in the list.</param>
        /// <returns>The node replaced.</returns>
        public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
        {
            if (newChild == null)
            {
                return RemoveChild(oldChild);
            }

            if (oldChild == null)
            {
                return AppendChild(newChild);
            }

            int index = -1;

            if (_childnodes != null)
            {
                index = _childnodes[oldChild];
            }

            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }

            if (_childnodes != null) _childnodes.Replace(index, newChild);

            _ownerdocument.SetIdForNode(null, oldChild.GetId());
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            _outerchanged = true;
            _innerchanged = true;
            return newChild;
        }

        /// <summary>
        /// Selects a list of nodes matching the <see cref="XPath"/> expression.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>An <see cref="HtmlNodeCollection"/> containing a collection of nodes matching the <see cref="XPath"/> query, or <c>null</c> if no node matched the XPath expression.</returns>
        public HtmlNodeCollection SelectNodes(string xpath)
        {
            HtmlNodeCollection list = new HtmlNodeCollection(null);

            HtmlNodeNavigator nav = new HtmlNodeNavigator(_ownerdocument, this);
            XPathNodeIterator it = nav.Select(xpath);
            while (it.MoveNext())
            {
                HtmlNodeNavigator n = (HtmlNodeNavigator) it.Current;
                list.Add(n.CurrentNode);
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list;
        }

        /// <summary>
        /// Selects the first XmlNode that matches the XPath expression.
        /// </summary>
        /// <param name="xpath">The XPath expression. May not be null.</param>
        /// <returns>The first <see cref="HtmlNode"/> that matches the XPath query or a null reference if no matching node was found.</returns>
        public HtmlNode SelectSingleNode(string xpath)
        {
            if (xpath == null)
            {
                throw new ArgumentNullException("xpath");
            }

            HtmlNodeNavigator nav = new HtmlNodeNavigator(_ownerdocument, this);
            XPathNodeIterator it = nav.Select(xpath);
            if (!it.MoveNext())
            {
                return null;
            }

            HtmlNodeNavigator node = (HtmlNodeNavigator) it.Current;
            return node.CurrentNode;
        }

        /// <summary>
        /// Helper method to set the value of an attribute of this node. If the attribute is not found, it will be created automatically.
        /// </summary>
        /// <param name="name">The name of the attribute to set. May not be null.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>The corresponding attribute instance.</returns>
        public HtmlAttribute SetAttributeValue(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlAttribute att = Attributes[name];
            if (att == null)
            {
                return Attributes.Append(_ownerdocument.CreateAttribute(name, value));
            }
            att.Value = value;
            return att;
        }

        /// <summary>
        /// Saves all the children of the node to the specified TextWriter.
        /// </summary>
        /// <param name="outText">The TextWriter to which you want to save.</param>
        public void WriteContentTo(TextWriter outText)
        {
            if (_childnodes == null)
            {
                return;
            }

            foreach (HtmlNode node in _childnodes)
            {
                node.WriteTo(outText);
            }
        }

        /// <summary>
        /// Saves all the children of the node to a string.
        /// </summary>
        /// <returns>The saved string.</returns>
        public string WriteContentTo()
        {
            StringWriter sw = new StringWriter();
            WriteContentTo(sw);
            sw.Flush();
            return sw.ToString();
        }

        /// <summary>
        /// Saves the current node to the specified TextWriter.
        /// </summary>
        /// <param name="outText">The TextWriter to which you want to save.</param>
        public void WriteTo(TextWriter outText)
        {
            string html;
            switch (_nodetype)
            {
                case HtmlNodeType.Comment:
                    html = ((HtmlCommentNode) this).Comment;
                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        outText.Write("<!--" + GetXmlComment((HtmlCommentNode) this) + " -->");
                    }
                    else
                    {
                        outText.Write(html);
                    }
                    break;

                case HtmlNodeType.Document:
                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName +
                                      "\"?>");

                        // check there is a root element
                        if (_ownerdocument.DocumentNode.HasChildNodes)
                        {
                            int rootnodes = _ownerdocument.DocumentNode._childnodes.Count;
                            if (rootnodes > 0)
                            {
                                HtmlNode xml = _ownerdocument.GetXmlDeclaration();
                                if (xml != null)
                                {
                                    rootnodes --;
                                }

                                if (rootnodes > 1)
                                {
                                    if (_ownerdocument.OptionOutputUpperCase)
                                    {
                                        outText.Write("<SPAN>");
                                        WriteContentTo(outText);
                                        outText.Write("</SPAN>");
                                    }
                                    else
                                    {
                                        outText.Write("<span>");
                                        WriteContentTo(outText);
                                        outText.Write("</span>");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    WriteContentTo(outText);
                    break;

                case HtmlNodeType.Text:
                    html = ((HtmlTextNode) this).Text;
                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        outText.Write(HtmlDocument.HtmlEncode(html));
                    }
                    else
                    {
                        outText.Write(html);
                    }
                    break;

                case HtmlNodeType.Element:
                    string name;
                    if (_ownerdocument.OptionOutputUpperCase)
                    {
                        name = Name.ToUpper();
                    }
                    else
                    {
                        name = Name;
                    }

                    if (_ownerdocument.OptionOutputOriginalCase)
                        name = OriginalName;

                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        if (name.Length > 0)
                        {
                            if (name[0] == '?')
                            {
                                // forget this one, it's been done at the document level
                                break;
                            }

                            if (name.Trim().Length == 0)
                            {
                                break;
                            }
                            name = HtmlDocument.GetXmlName(name);
                        }
                        else
                        {
                            break;
                        }
                    }

                    outText.Write("<" + name);
                    WriteAttributes(outText, false);

                    if (!HasChildNodes)
                    {
                        if (IsEmptyElement(Name))
                        {
                            if ((_ownerdocument.OptionWriteEmptyNodes) || (_ownerdocument.OptionOutputAsXml))
                            {
                                outText.Write(" />");
                            }
                            else
                            {
                                if (Name.Length > 0)
                                {
                                    if (Name[0] == '?')
                                    {
                                        outText.Write("?");
                                    }
                                }

                                outText.Write(">");
                            }
                        }
                        else
                        {
                            outText.Write("></" + name + ">");
                        }
                    }
                    else
                    {
                        outText.Write(">");
                        bool cdata = false;
                        if (_ownerdocument.OptionOutputAsXml)
                        {
                            if (IsCDataElement(Name))
                            {
                                // this code and the following tries to output things as nicely as possible for old browsers.
                                cdata = true;
                                outText.Write("\r\n//<![CDATA[\r\n");
                            }
                        }

                        if (cdata)
                        {
                            if (HasChildNodes)
                            {
                                // child must be a text
                                ChildNodes[0].WriteTo(outText);
                            }
                            outText.Write("\r\n//]]>//\r\n");
                        }
                        else
                        {
                            WriteContentTo(outText);
                        }

                        outText.Write("</" + name);
                        if (!_ownerdocument.OptionOutputAsXml)
                        {
                            WriteAttributes(outText, true);
                        }
                        outText.Write(">");
                    }
                    break;
            }
        }

        /// <summary>
        /// Saves the current node to the specified XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to which you want to save.</param>
        public void WriteTo(XmlWriter writer)
        {
            switch (_nodetype)
            {
                case HtmlNodeType.Comment:
                    writer.WriteComment(GetXmlComment((HtmlCommentNode) this));
                    break;

                case HtmlNodeType.Document:
                    writer.WriteProcessingInstruction("xml",
                                                      "version=\"1.0\" encoding=\"" +
                                                      _ownerdocument.GetOutEncoding().BodyName + "\"");
                    if (HasChildNodes)
                    {
                        foreach (HtmlNode subnode in ChildNodes)
                        {
                            subnode.WriteTo(writer);
                        }
                    }
                    break;

                case HtmlNodeType.Text:
                    string html = ((HtmlTextNode) this).Text;
                    writer.WriteString(html);
                    break;

                case HtmlNodeType.Element:
                    string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpper() : Name;

                    if (_ownerdocument.OptionOutputOriginalCase)
                        name = OriginalName;

                    writer.WriteStartElement(name);
                    WriteAttributes(writer, this);

                    if (HasChildNodes)
                    {
                        foreach (HtmlNode subnode in ChildNodes)
                        {
                            subnode.WriteTo(writer);
                        }
                    }
                    writer.WriteEndElement();
                    break;
            }
        }

        /// <summary>
        /// Saves the current node to a string.
        /// </summary>
        /// <returns>The saved string.</returns>
        public string WriteTo()
        {
            using (StringWriter sw = new StringWriter())
            {
                WriteTo(sw);
                sw.Flush();
                return sw.ToString();
            }
        }

        #endregion

        #region Internal Methods

        internal static string GetXmlComment(HtmlCommentNode comment)
        {
            string s = comment.Comment;
            return s.Substring(4, s.Length - 7).Replace("--", " - -");
        }

        internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
        {
            if (!node.HasAttributes)
            {
                return;
            }
            // we use Hashitems to make sure attributes are written only once
            foreach (HtmlAttribute att in node.Attributes.Hashitems.Values)
            {
                writer.WriteAttributeString(att.XmlName, att.Value);
            }
        }

        internal void CloseNode(HtmlNode endnode)
        {
            if (!_ownerdocument.OptionAutoCloseOnEnd)
            {
                // close all children
                if (_childnodes != null)
                {
                    foreach (HtmlNode child in _childnodes)
                    {
                        if (child.Closed)
                            continue;

                        // create a fake closer node
                        HtmlNode close = new HtmlNode(NodeType, _ownerdocument, -1);
                        close._endnode = close;
                        child.CloseNode(close);
                    }
                }
            }

            if (!Closed)
            {
                _endnode = endnode;

                if (_ownerdocument._openednodes != null)
                {
                    _ownerdocument._openednodes.Remove(_outerstartindex);
                }

                HtmlNode self = _ownerdocument._lastnodes[Name] as HtmlNode;
                if (self == this)
                {
                    _ownerdocument._lastnodes.Remove(Name);
                    _ownerdocument.UpdateLastParentNode();
                }

                if (endnode == this)
                    return;

                // create an inner section
                _innerstartindex = _outerstartindex + _outerlength;
                _innerlength = endnode._outerstartindex - _innerstartindex;

                // update full length
                _outerlength = (endnode._outerstartindex + endnode._outerlength) - _outerstartindex;
            }
        }

        internal string GetId()
        {
            HtmlAttribute att = Attributes["id"];
            if (att == null)
            {
                return null;
            }
            return att.Value;
        }

        internal void SetId(string id)
        {
            HtmlAttribute att = Attributes["id"];
            if (att == null)
            {
                att = _ownerdocument.CreateAttribute("id");
            }
            att.Value = id;
            _ownerdocument.SetIdForNode(this, att.Value);
            _outerchanged = true;
        }

        internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
        {
            string name;
            string quote = att.QuoteType == AttributeValueQuote.DoubleQuote ? "\"" : "'";
            if (_ownerdocument.OptionOutputAsXml)
            {
                if (_ownerdocument.OptionOutputUpperCase)
                {
                    name = att.XmlName.ToUpper();
                }
                else
                {
                    name = att.XmlName;
                }
                if (_ownerdocument.OptionOutputOriginalCase)
                    name = att.OriginalName;

                outText.Write(" " + name + "=" + quote + HtmlDocument.HtmlEncode(att.XmlValue) + quote);
            }
            else
            {
                if (_ownerdocument.OptionOutputUpperCase)
                {
                    name = att.Name.ToUpper();
                }
                else
                {
                    name = att.Name;
                }

                if (att.Name.Length >= 4)
                {
                    if ((att.Name[0] == '<') && (att.Name[1] == '%') &&
                        (att.Name[att.Name.Length - 1] == '>') && (att.Name[att.Name.Length - 2] == '%'))
                    {
                        outText.Write(" " + name);
                        return;
                    }
                }
                if (_ownerdocument.OptionOutputOptimizeAttributeValues)
                {
                    if (att.Value.IndexOfAny(new Char[] {(char) 10, (char) 13, (char) 9, ' '}) < 0)
                    {
                        outText.Write(" " + name + "=" + att.Value);
                    }
                    else
                    {
                        outText.Write(" " + name + "=" + quote + att.Value + quote);
                    }
                }
                else
                {
                    outText.Write(" " + name + "=" + quote + att.Value + quote);
                }
            }
        }

        internal void WriteAttributes(TextWriter outText, bool closing)
        {
            if (_ownerdocument.OptionOutputAsXml)
            {
                if (_attributes == null)
                {
                    return;
                }
                // we use Hashitems to make sure attributes are written only once
                foreach (HtmlAttribute att in _attributes.Hashitems.Values)
                {
                    WriteAttribute(outText, att);
                }
                return;
            }

            if (!closing)
            {
                if (_attributes != null)
                {
                    foreach (HtmlAttribute att in _attributes)
                    {
                        WriteAttribute(outText, att);
                    }
                }
                if (_ownerdocument.OptionAddDebuggingAttributes)
                {
                    WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
                    WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));

                    int i = 0;
                    foreach (HtmlNode n in ChildNodes)
                    {
                        WriteAttribute(outText, _ownerdocument.CreateAttribute("_child_" + i,
                                                                               n.Name));
                        i++;
                    }
                }
            }
            else
            {
                if (_endnode == null)
                {
                    return;
                }

                if (_endnode._attributes == null)
                {
                    return;
                }

                if (_endnode == this)
                {
                    return;
                }

                foreach (HtmlAttribute att in _endnode._attributes)
                {
                    WriteAttribute(outText, att);
                }
                if (_ownerdocument.OptionAddDebuggingAttributes)
                {
                    WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
                    WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
                }
            }
        }

        #endregion

        #region Private Methods

        private string GetRelativeXpath()
        {
            if (ParentNode == null)
                return Name;
            if (NodeType == HtmlNodeType.Document)
                return string.Empty;

            int i = 1;
            foreach (HtmlNode node in ParentNode.ChildNodes)
            {
                if (node.Name != Name) continue;

                if (node == this)
                    break;

                i++;
            }
            return Name + "[" + i + "]";
        }

        #endregion
    }
}