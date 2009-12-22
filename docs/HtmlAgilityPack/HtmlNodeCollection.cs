// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a combined list and collection of HTML nodes.
    /// </summary>
    public class HtmlNodeCollection : IList<HtmlNode>
    {
        #region Fields

        private readonly HtmlNode _parentnode;
        private readonly List<HtmlNode> _items = new List<HtmlNode>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the HtmlNodeCollection with the base parent node
        /// </summary>
        /// <param name="parentnode">The base node of the collection</param>
        public HtmlNodeCollection(HtmlNode parentnode)
        {
            _parentnode = parentnode; // may be null
        }

        #endregion

        #region Properties

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
                    throw new ArgumentOutOfRangeException("node",
                                                          "Node \"" + node.CloneNode(false).OuterHtml +
                                                          "\" was not found in the collection");
                }
                return index;
            }
        }

        /// <summary>
        /// Get node with tag name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public HtmlNode this[string nodeName]
        {
            get
            {
                nodeName = nodeName.ToLower();
                for (int i = 0; i < _items.Count; i++)
                    if (_items[i].Equals(nodeName))
                        return _items[i];

                return null;
            }
        }

        #endregion

        #region IList<HtmlNode> Members

        /// <summary>
        /// Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Is collection read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        public HtmlNode this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        /// <summary>
        /// Add node to the collection
        /// </summary>
        /// <param name="node"></param>
        public void Add(HtmlNode node)
        {
            _items.Add(node);
        }

        /// <summary>
        /// Clears out the collection of HtmlNodes. Removes each nodes reference to parentnode, nextnode and prevnode
        /// </summary>
        public void Clear()
        {
            foreach (HtmlNode node in _items)
            {
                node.ParentNode = null;
                node.NextSibling = null;
                node.PreviousSibling = null;
            }
            _items.Clear();
        }

        /// <summary>
        /// Gets existence of node in collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlNode item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Copy collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlNode[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Get Explicit Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlNode item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Insert node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Insert(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;

            if (index > 0)
            {
                prev = _items[index - 1];
            }

            if (index < _items.Count)
            {
                next = _items[index];
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

        /// <summary>
        /// Remove node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(HtmlNode item)
        {
            int i = _items.IndexOf(item);
            RemoveAt(i);
            return true;
        }

        /// <summary>
        /// Remove <see cref="HtmlNode"/> at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = _items[index];

            if (index > 0)
            {
                prev = _items[index - 1];
            }

            if (index < (_items.Count - 1))
            {
                next = _items[index + 1];
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Get first instance of node in supplied collection
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
        {
            foreach (HtmlNode node in items)
            {
                if (node.Name.ToLower().Contains(name))
                    return node;
                if (node.HasChildNodes)
                {
                    HtmlNode returnNode = FindFirst(node.ChildNodes, name);
                    if (returnNode != null)
                        return returnNode;
                }
            }
            return null;
        }

        /// <summary>
        /// Add node to the end of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Append(HtmlNode node)
        {
            HtmlNode last = null;
            if (_items.Count > 0)
            {
                last = _items[_items.Count - 1];
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

        /// <summary>
        /// Get first instance of node with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode FindFirst(string name)
        {
            return FindFirst(this, name);
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(HtmlNode node)
        {
            // TODO: should we rewrite this? what would be the key of a node?
            for (int i = 0; i < _items.Count; i++)
            {
                if (node == (_items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Add node to the beginning of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Prepend(HtmlNode node)
        {
            HtmlNode first = null;
            if (_items.Count > 0)
            {
                first = _items[0];
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

        /// <summary>
        /// Remove node at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Remove(int index)
        {
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Replace node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Replace(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = _items[index];

            if (index > 0)
            {
                prev = _items[index - 1];
            }

            if (index < (_items.Count - 1))
            {
                next = _items[index + 1];
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

            oldnode._prevnode = null;
            oldnode._nextnode = null;
            oldnode._parentnode = null;
        }

        #endregion

        #region LINQ Methods

        /// <summary>
        /// Get all node descended from this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantNodes()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.DescendantNodes())
                    yield return n;
        }

        /// <summary>
        /// Get all node descended from this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Descendants())
                    yield return n;
        }

        /// <summary>
        /// Get all node descended from this collection with matching name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Descendants(name))
                    yield return n;
        }

        /// <summary>
        /// Gets all first generation elements in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
        }

        /// <summary>
        /// Gets all first generation elements matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Elements(name))
                    yield return n;
        }

        /// <summary>
        /// All first generation nodes in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Nodes()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
        }

        #endregion
    }
}