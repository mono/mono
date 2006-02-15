// HtmlAgilityPack V1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace HtmlAgilityPack
{
	/// <summary>
	/// Flags that describe the behavior of an Element node.
	/// </summary>
	public enum HtmlElementFlag
	{
		/// <summary>
		/// The node is a CDATA node.
		/// </summary>
		CData = 1,

		/// <summary>
		/// The node is empty. META or IMG are example of such nodes.
		/// </summary>
		Empty = 2,

		/// <summary>
		/// The node will automatically be closed during parsing.
		/// </summary>
		Closed = 4,

		/// <summary>
		/// The node can overlap.
		/// </summary>
		CanOverlap = 8
	}

	/// <summary>
	/// Represents the type of a node.
	/// </summary>
	public enum HtmlNodeType
	{
		/// <summary>
		/// The root of a document.
		/// </summary>
		Document,

		/// <summary>
		/// An HTML element.
		/// </summary>
		Element,

		/// <summary>
		/// An HTML comment.
		/// </summary>
		Comment,

		/// <summary>
		/// A text node is always the child of an element or a document node.
		/// </summary>
		Text,
	}

	/// <summary>
	/// Represents an HTML node.
	/// </summary>
	public class HtmlNode: IXPathNavigable
	{
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

		internal HtmlNodeType _nodetype;
		internal HtmlNode _nextnode;
		internal HtmlNode _prevnode;
		internal HtmlNode _parentnode;
		internal HtmlDocument _ownerdocument;
		internal HtmlNodeCollection _childnodes;
		internal HtmlAttributeCollection _attributes;
		internal int _line = 0;
		internal int _lineposition = 0;
		internal int _streamposition = 0;
		internal int _innerstartindex = 0;
		internal int _innerlength = 0;
		internal int _outerstartindex = 0;
		internal int _outerlength = 0;
		internal int _namestartindex = 0;
		internal int _namelength = 0;
		internal bool _starttag = false;
		internal string _name;
		internal HtmlNode _prevwithsamename = null;
		internal HtmlNode _endnode;

		internal bool _innerchanged = false;
		internal bool _outerchanged = false;
		internal string _innerhtml;
		internal string _outerhtml;

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
			return (((HtmlElementFlag)flag)&HtmlElementFlag.Closed) != 0;
		}

		/// <summary>
		/// Determines if an element node can be kept overlapped.
		/// </summary>
		/// <param name="name">The name of the element node to check. May not be null.</param>
		/// <returns>true if the name is the name of an element node that can be kept overlapped, false otherwise.</returns>
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
			return (((HtmlElementFlag)flag)&HtmlElementFlag.CanOverlap) != 0;
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
			return (((HtmlElementFlag)flag)&HtmlElementFlag.CData) != 0;
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
			return (((HtmlElementFlag)flag)&HtmlElementFlag.Empty) != 0;
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
		/// Creates a duplicate of the node and the subtree under it.
		/// </summary>
		/// <param name="node">The node to duplicate. May not be null.</param>
		public void CopyFrom(HtmlNode node)
		{
			CopyFrom(node, true);
		}

		/// <summary>
		/// Creates a duplicate of the node.
		/// </summary>
		/// <param name="node">The node to duplicate. May not be null.</param>
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
				foreach(HtmlAttribute att in node.Attributes)
				{
					SetAttributeValue(att.Name, att.Value);
				}
			}

			if (!deep)
			{
				RemoveAllChildren();
				if (node.HasChildNodes)
				{
					foreach(HtmlNode child in node.ChildNodes)
					{
						AppendChild(child.CloneNode(true));
					}
				}
			}
		}

		internal HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
		{
			_nodetype = type;
			_ownerdocument = ownerdocument;
			_outerstartindex = index;

			switch(type)
			{
				case HtmlNodeType.Comment:
					_name = HtmlNodeTypeNameComment;
					_endnode = this;
					break;

				case HtmlNodeType.Document:
					_name = HtmlNodeTypeNameDocument;
					_endnode = this;
					break;

				case HtmlNodeType.Text:
					_name = HtmlNodeTypeNameText;
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
			
			if ((-1 == index) && (type != HtmlNodeType.Comment) && (type != HtmlNodeType.Text))
			{
				// innerhtml and outerhtml must be calculated
				_outerchanged = true;
				_innerchanged = true;
			}
		}

		internal void CloseNode(HtmlNode endnode)
		{
			if (!_ownerdocument.OptionAutoCloseOnEnd)
			{
				// close all children
				if (_childnodes != null)
				{
					foreach(HtmlNode child in _childnodes)
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

		internal HtmlNode EndNode
		{
			get
			{
				return _endnode;
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

		/// <summary>
		/// Creates a new XPathNavigator object for navigating this HTML node.
		/// </summary>
		/// <returns>An XPathNavigator object. The XPathNavigator is positioned on the node from which the method was called. It is not positioned on the root of the document.</returns>
		public XPathNavigator CreateNavigator()
		{
			return new HtmlNodeNavigator(_ownerdocument, this);
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression.
		/// </summary>
		/// <param name="xpath">The XPath expression. May not be null.</param>
		/// <returns>The first HtmlNode that matches the XPath query or a null reference if no matching node was found.</returns>
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

			HtmlNodeNavigator node = (HtmlNodeNavigator)it.Current;
			return node.CurrentNode;
		}

		/// <summary>
		/// Selects a list of nodes matching the XPath expression.
		/// </summary>
		/// <param name="xpath">The XPath expression.</param>
		/// <returns>An HtmlNodeCollection containing a collection of nodes matching the XPath query, or null if no node matched the XPath expression.</returns>
		public HtmlNodeCollection SelectNodes(string xpath)
		{
			HtmlNodeCollection list = new HtmlNodeCollection(null);

			HtmlNodeNavigator nav = new HtmlNodeNavigator(_ownerdocument, this);
			XPathNodeIterator it = nav.Select(xpath);
			while (it.MoveNext())
			{
				HtmlNodeNavigator n = (HtmlNodeNavigator)it.Current;
				list.Add(n.CurrentNode);
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list;
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
		/// Gets the line number of this node in the document.
		/// </summary>
		public int Line
		{
			get
			{
				return _line;
			}
		}

		/// <summary>
		/// Gets the column number of this node in the document.
		/// </summary>
		public int LinePosition
		{
			get
			{
				return _lineposition;
			}
		}

		/// <summary>
		/// Gets the stream position of this node in the document, relative to the start of the document.
		/// </summary>
		public int StreamPosition
		{
			get
			{
				return _streamposition;
			}
		}

		/// <summary>
		/// Gets a value indicating if this node has been closed or not.
		/// </summary>
		public bool Closed
		{
			get
			{
				return (_endnode != null);
			}
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
					_name = _ownerdocument._text.Substring(_namestartindex, _namelength).ToLower();
				}
				return _name;
			}
			set
			{
				_name = value;
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
					return ((HtmlTextNode)this).Text;
				}

				if (_nodetype == HtmlNodeType.Comment)
				{
					return ((HtmlCommentNode)this).Comment;
				}

				// note: right now, this method is *slow*, because we recompute everything.
				// it could be optimised like innerhtml
				if (!HasChildNodes)
				{
					return string.Empty;
				}

				string s = null;
				foreach(HtmlNode node in ChildNodes)
				{
					s += node.InnerText;
				}
				return s;
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
		/// <param name="newName">The new name of the cloned node. May not be null.</param>
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
			node._name = newName;
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
			node._name = Name;

			switch(_nodetype)
			{
				case HtmlNodeType.Comment:
					((HtmlCommentNode)node).Comment = ((HtmlCommentNode)this).Comment;
					return node;
		
				case HtmlNodeType.Text:
					((HtmlTextNode)node).Text = ((HtmlTextNode)this).Text;
					return node;
			}

			// attributes
			if (HasAttributes)
			{
				foreach(HtmlAttribute att in _attributes)
				{
					HtmlAttribute newatt = att.Clone();
					node.Attributes.Append(newatt);
				}
			}

			// closing attributes
			if (HasClosingAttributes)
			{
				node._endnode = _endnode.CloneNode(false);
				foreach(HtmlAttribute att in _endnode._attributes)
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
			foreach(HtmlNode child in _childnodes)
			{
				HtmlNode newchild = child.Clone();
				node.AppendChild(newchild);
			}
			return node;
		}

		/// <summary>
		/// Gets the HTML node immediately following this element.
		/// </summary>
		public HtmlNode NextSibling
		{
			get
			{
				return _nextnode;
			}
		}

		/// <summary>
		/// Gets the node immediately preceding this node.
		/// </summary>
		public HtmlNode PreviousSibling
		{
			get
			{
				return _prevnode;
			}
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
				foreach(HtmlNode node in _childnodes)
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
		/// <param name="oldChild">The node being removed. May not be null.</param>
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

			_childnodes.Remove(index);

			_ownerdocument.SetIdForNode(null, oldChild.GetId());
			_outerchanged = true;
			_innerchanged = true;
			return oldChild;
		}

		/// <summary>
		/// Removes the specified child node.
		/// </summary>
		/// <param name="oldChild">The node being removed. May not be null.</param>
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
				foreach(HtmlNode grandchild in oldChild._childnodes)
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

			_childnodes.Replace(index, newChild);

			_ownerdocument.SetIdForNode(null, oldChild.GetId());
			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
			_outerchanged = true;
			_innerchanged = true;
			return newChild;
		}

		/// <summary>
		/// Inserts the specified node immediately before the specified reference node.
		/// </summary>
		/// <param name="newChild">The node to insert. May not be null.</param>
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

			_childnodes.Insert(index, newChild);

			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
			_outerchanged = true;
			_innerchanged = true;
			return newChild;
		}

		/// <summary>
		/// Inserts the specified node immediately after the specified reference node.
		/// </summary>
		/// <param name="newChild">The node to insert. May not be null.</param>
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

			_childnodes.Insert(index + 1, newChild);

			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
			_outerchanged = true;
			_innerchanged = true;
			return newChild;
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
		/// Gets the last child of the node.
		/// </summary>
		public HtmlNode LastChild
		{
			get
			{
				if (!HasChildNodes)
				{
					return null;
				}
				return _childnodes[_childnodes.Count-1];
			}
		}

		/// <summary>
		/// Gets the type of this node.
		/// </summary>
		public HtmlNodeType NodeType
		{
			get
			{
				return _nodetype;
			}
		}

		/// <summary>
		/// Gets the parent of this node (for nodes that can have parents).
		/// </summary>
		public HtmlNode ParentNode
		{
			get
			{
				return _parentnode;
			}
		}

		/// <summary>
		/// Gets the HtmlDocument to which this node belongs.
		/// </summary>
		public HtmlDocument OwnerDocument
		{
			get
			{
				return _ownerdocument;
			}
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
		}

		/// <summary>
		/// Adds the specified node to the beginning of the list of children of this node.
		/// </summary>
		/// <param name="newChild">The node to add. May not be null.</param>
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
		/// <param name="newChildren">The node list to add. May not be null.</param>
		public void PrependChildren(HtmlNodeCollection newChildren)
		{
			if (newChildren == null)
			{
				throw new ArgumentNullException("newChildren");
			}

			foreach(HtmlNode newChild in newChildren)
			{
				PrependChild(newChild);
			}
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

			foreach(HtmlNode newChild in newChildren)
			{
				AppendChild(newChild);
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
		/// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
		/// </summary>
		/// <param name="name">The name of the attribute to get. May not be null.</param>
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
		/// <param name="name">The name of the attribute to get. May not be null.</param>
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
		/// <param name="name">The name of the attribute to get. May not be null.</param>
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

		internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
		{
			string name;

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

				outText.Write(" " + name + "=\"" + HtmlDocument.HtmlEncode(att.XmlValue) + "\"");
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
						(att.Name[att.Name.Length-1] == '>') && (att.Name[att.Name.Length-2] == '%'))
					{
						outText.Write(" " + name);
						return;
					}
				}
				if (_ownerdocument.OptionOutputOptimizeAttributeValues)
				{
					if (att.Value.IndexOfAny(new Char[]{(char)10, (char)13, (char)9, ' '}) < 0)
					{
						outText.Write(" " + name + "=" + att.Value);
					}
					else
					{
						outText.Write(" " + name + "=\"" + att.Value + "\"");
					}
				}
				else
				{
					outText.Write(" " + name + "=\"" + att.Value + "\"");
				}
			}
		}

		internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
		{
			if (!node.HasAttributes)
			{
				return;
			}
			// we use _hashitems to make sure attributes are written only once
			foreach(HtmlAttribute att in node.Attributes._hashitems.Values)
			{
				writer.WriteAttributeString(att.XmlName, att.Value);
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
				// we use _hashitems to make sure attributes are written only once
				foreach(HtmlAttribute att in _attributes._hashitems.Values)
				{
					WriteAttribute(outText, att);
				}
				return;
			}

			if (!closing)
			{
				if (_attributes != null)
				{

					foreach(HtmlAttribute att in _attributes)
					{
						WriteAttribute(outText, att);
					}
				}
				if (_ownerdocument.OptionAddDebuggingAttributes)
				{
					WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
					WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));

					int i = 0;
					foreach(HtmlNode n in ChildNodes)
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

				foreach(HtmlAttribute att in _endnode._attributes)
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

		internal static string GetXmlComment(HtmlCommentNode comment)
		{
			string s = comment.Comment;
			return s.Substring(4, s.Length-7).Replace("--", " - -");
		}

		/// <summary>
		/// Saves the current node to the specified TextWriter.
		/// </summary>
		/// <param name="outText">The TextWriter to which you want to save.</param>
		public void WriteTo(TextWriter outText)
		{
			string html;
			switch(_nodetype)
			{
				case HtmlNodeType.Comment:
					html = ((HtmlCommentNode)this).Comment;
					if (_ownerdocument.OptionOutputAsXml)
					{
						outText.Write("<!--" + GetXmlComment((HtmlCommentNode)this) + " -->");
					}
					else
					{
						outText.Write(html);
					}
					break;

				case HtmlNodeType.Document:
					if (_ownerdocument.OptionOutputAsXml)
					{
						outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"?>");

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
					html = ((HtmlTextNode)this).Text;
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
						if (HtmlNode.IsEmptyElement(Name))
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
							if (HtmlNode.IsCDataElement(Name))
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
			string html;
			switch(_nodetype)
			{
				case HtmlNodeType.Comment:
					writer.WriteComment(GetXmlComment((HtmlCommentNode)this));
					break;

				case HtmlNodeType.Document:
					writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"");
					if (HasChildNodes)
					{
						foreach(HtmlNode subnode in ChildNodes)
						{
							subnode.WriteTo(writer);
						}
					}
					break;

				case HtmlNodeType.Text:
					html = ((HtmlTextNode)this).Text;
					writer.WriteString(html);
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
					writer.WriteStartElement(name);
					WriteAttributes(writer, this);

					if (HasChildNodes)
					{
						foreach(HtmlNode subnode in ChildNodes)
						{
							subnode.WriteTo(writer);
						}
					}
					writer.WriteEndElement();
					break;
			}
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

			foreach(HtmlNode node in _childnodes)
			{
				node.WriteTo(outText);
			}
		}

		/// <summary>
		/// Saves the current node to a string.
		/// </summary>
		/// <returns>The saved string.</returns>
		public string WriteTo()
		{
			StringWriter sw = new StringWriter();
			WriteTo(sw);
			sw.Flush();
			return sw.ToString();
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
	}

	/// <summary>
	/// Represents a combined list and collection of HTML nodes.
	/// </summary>
	public class HtmlNodeCollection: IEnumerable
	{
		private ArrayList _items = new ArrayList();
		private HtmlNode _parentnode;

		internal HtmlNodeCollection(HtmlNode parentnode)
		{
			_parentnode = parentnode; // may be null
		}

		/// <summary>
		/// Gets the number of elements actually contained in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		internal void Clear()
		{
			foreach(HtmlNode node in _items)
			{
				node._parentnode = null;
				node._nextnode = null;
				node._prevnode = null;
			}
			_items.Clear();
		}

		internal void Remove(int index)
		{
			HtmlNode next = null;
			HtmlNode prev = null;
			HtmlNode oldnode = (HtmlNode)_items[index];

			if (index > 0)
			{
				prev = (HtmlNode)_items[index-1];
			}

			if (index < (_items.Count-1))
			{
				next = (HtmlNode)_items[index+1];
			}

			_items.RemoveAt(index);
			
			if (prev != null)
			{
				if (next == prev)
				{
					throw new InvalidProgramException("Unexpected error.");
				}
				prev._nextnode = next;
			}

			if (next != null)
			{
				next._prevnode = prev;
			}

			oldnode._prevnode = null;
			oldnode._nextnode = null;
			oldnode._parentnode = null;
		}

		internal void Replace(int index, HtmlNode node)
		{
			HtmlNode next = null;
			HtmlNode prev = null;
			HtmlNode oldnode = (HtmlNode)_items[index];

			if (index>0)
			{
				prev = (HtmlNode)_items[index-1];
			}

			if (index<(_items.Count-1))
			{
				next = (HtmlNode)_items[index+1];
			}

			_items[index] = node;
			
			if (prev != null)
			{
				if (node == prev)
				{
					throw new InvalidProgramException("Unexpected error.");
				}
				prev._nextnode = node;
			}

			if (next!=null)
			{
				next._prevnode = node;
			}

			node._prevnode = prev;
			if (next == node)
			{
				throw new InvalidProgramException("Unexpected error.");
			}
			node._nextnode = next;
			node._parentnode = _parentnode;

			oldnode._prevnode = null;
			oldnode._nextnode = null;
			oldnode._parentnode = null;
		}

		internal void Insert(int index, HtmlNode node)
		{
			HtmlNode next = null;
			HtmlNode prev = null;

			if (index>0)
			{
				prev = (HtmlNode)_items[index-1];
			}

			if (index<_items.Count)
			{
				next = (HtmlNode)_items[index];
			}

			_items.Insert(index, node);

			if (prev != null)
			{
				if (node == prev)
				{
					throw new InvalidProgramException("Unexpected error.");
				}
				prev._nextnode = node;
			}

			if (next != null)
			{
				next._prevnode = node;
			}

			node._prevnode = prev;

			if (next == node)
			{
				throw new InvalidProgramException("Unexpected error.");
			}

			node._nextnode = next;
			node._parentnode = _parentnode;
		}

		internal void Append(HtmlNode node)
		{
			HtmlNode last = null;
			if (_items.Count > 0)
			{
				last = (HtmlNode)_items[_items.Count-1];
			}

			_items.Add(node);
			node._prevnode = last;
			node._nextnode = null;
			node._parentnode = _parentnode;
			if (last != null)
			{
				if (last == node)
				{
					throw new InvalidProgramException("Unexpected error.");
				}
				last._nextnode = node;
			}
		}

		internal void Prepend(HtmlNode node)
		{
			HtmlNode first = null;
			if (_items.Count > 0)
			{
				first = (HtmlNode)_items[0];
			}

			_items.Insert(0, node);

			if (node == first)
			{
				throw new InvalidProgramException("Unexpected error.");
			}
			node._nextnode = first;
			node._prevnode = null;
			node._parentnode = _parentnode;
			if (first != null)
			{
				first._prevnode = node;
			}
		}

		internal void Add(HtmlNode node)
		{
			_items.Add(node);
		}

		/// <summary>
		/// Gets the node at the specified index.
		/// </summary>
		public HtmlNode this[int index]
		{
			get
			{
				return _items[index] as HtmlNode;
			}
		}

		internal int GetNodeIndex(HtmlNode node)
		{
			// TODO: should we rewrite this? what would be the key of a node?
			for(int i=0;i<_items.Count;i++)
			{
				if (node == ((HtmlNode)_items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets a given node from the list.
		/// </summary>
		public int this[HtmlNode node]
		{
			get
			{
				int index = GetNodeIndex(node);
				if (index == -1)
				{
					throw new ArgumentOutOfRangeException("node", "Node \"" + node.CloneNode(false).OuterHtml + "\" was not found in the collection");
				}
				return index;
			}
		}

		/// <summary>
		/// Returns an enumerator that can iterate through the list.
		/// </summary>
		/// <returns>An IEnumerator for the entire list.</returns>
		public HtmlNodeEnumerator GetEnumerator() 
		{
			return new HtmlNodeEnumerator(_items);
		}

		IEnumerator IEnumerable.GetEnumerator() 
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Represents an enumerator that can iterate through the list.
		/// </summary>
		public class HtmlNodeEnumerator: IEnumerator 
		{
			int _index;
			ArrayList _items;

			internal HtmlNodeEnumerator(ArrayList items) 
			{
				_items = items;
				_index = -1;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			public void Reset() 
			{
				_index = -1;
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element, false if the enumerator has passed the end of the collection.</returns>
			public bool MoveNext() 
			{
				_index++;
				return (_index<_items.Count);
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public HtmlNode Current 
			{
				get 
				{
					return (HtmlNode)(_items[_index]);
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			object IEnumerator.Current 
			{
				get 
				{
					return (Current);
				}
			}
		}
	}

	/// <summary>
	/// Represents an HTML text node.
	/// </summary>
	public class HtmlTextNode: HtmlNode
	{
		private string _text;

		internal HtmlTextNode(HtmlDocument ownerdocument, int index):
			base(HtmlNodeType.Text, ownerdocument, index)
		{
		}

		/// <summary>
		/// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
		/// </summary>
		public override string InnerHtml
		{
			get
			{
				return OuterHtml;
			}
			set
			{
				_text = value;
			}
		}

		/// <summary>
		/// Gets or Sets the object and its content in HTML.
		/// </summary>
		public override string OuterHtml
		{
			get
			{
				if (_text == null)
				{
					return base.OuterHtml;
				}
				return _text;
			}
		}

		/// <summary>
		/// Gets or Sets the text of the node.
		/// </summary>
		public string Text
		{
			get
			{
				if (_text == null)
				{
					return base.OuterHtml;
				}
				return _text;
			}
			set
			{
				_text = value;
			}
		}
	}

	/// <summary>
	/// Represents an HTML comment.
	/// </summary>
	public class HtmlCommentNode: HtmlNode
	{
		private string _comment;

		internal HtmlCommentNode(HtmlDocument ownerdocument, int index):
			base(HtmlNodeType.Comment, ownerdocument, index)
		{
		}

		/// <summary>
		/// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
		/// </summary>
		public override string InnerHtml
		{
			get
			{
				if (_comment == null)
				{
					return base.InnerHtml;
				}
				return _comment;
			}
			set
			{
				_comment = value;
			}
		}

		/// <summary>
		/// Gets or Sets the object and its content in HTML.
		/// </summary>
		public override string OuterHtml
		{
			get
			{
				if (_comment == null)
				{
					return base.OuterHtml;
				}
				return "<!--" + _comment + "-->";
			}
		}

		/// <summary>
		/// Gets or Sets the comment text of the node.
		/// </summary>
		public string Comment
		{
			get
			{
				if (_comment == null)
				{
					return base.InnerHtml;
				}
				return _comment;
			}
			set
			{
				_comment = value;
			}
		}
	}

}
