namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;

    public sealed class TreeNodeCollection : ICollection, IStateManager {
        private List<TreeNode> _list;
        private TreeNode _owner;
        private bool _updateParent;
        private int _version;

        private bool _isTrackingViewState;

        private List<LogItem> _log;


        public TreeNodeCollection() : this(null, true) {
        }


        public TreeNodeCollection(TreeNode owner) : this(owner, true) {
        }

        internal TreeNodeCollection(TreeNode owner, bool updateParent) {
            _owner = owner;
            _list = new List<TreeNode>();
            _updateParent = updateParent;
        }


        public int Count {
            get {
                return _list.Count;
            }
        }


        public bool IsSynchronized {
            get {
                return ((ICollection)_list).IsSynchronized;
            }
        }

        private List<LogItem> Log {
            get {
                if (_log == null) {
                    _log = new List<LogItem>();
                }
                return _log;
            }
        }


        public object SyncRoot {
            get {
                return ((ICollection)_list).SyncRoot;
            }
        }


        public TreeNode this[int index] {
            get {
                return _list[index];
            }
        }


        public void Add(TreeNode child) {
            AddAt(Count, child);
        }


        public void AddAt(int index, TreeNode child) {
            if (child == null) {
                throw new ArgumentNullException("child");
            }

            if (_updateParent) {
                if (child.Owner != null && child.Parent == null) {
                    child.Owner.Nodes.Remove(child);
                }
                if (child.Parent != null) {
                    child.Parent.ChildNodes.Remove(child);
                }
                if (_owner != null) {
                    child.SetParent(_owner);
                    child.SetOwner(_owner.Owner);
                }
            }

            _list.Insert(index, child);
            _version++;

            if (_isTrackingViewState) {
                ((IStateManager)child).TrackViewState();
                child.SetDirty();
            }
            Log.Add(new LogItem(LogItemType.Insert, index, _isTrackingViewState));
        }


        public void Clear() {
            if (this.Count == 0) return;
            if (_owner != null) {
                TreeView owner = _owner.Owner;
                if (owner != null) {
                    // Clear checked nodes if necessary
                    if (owner.CheckedNodes.Count != 0) {
                        owner.CheckedNodes.Clear();
                    }
                    TreeNode current = owner.SelectedNode;
                    // Check if the selected item is under this collection
                    while (current != null) {
                        if (this.Contains(current)) {
                            owner.SetSelectedNode(null);
                            break;
                        }
                        current = current.Parent;
                    }
                }
            }
            foreach (TreeNode node in _list) {
                node.SetParent(null);
            }

            _list.Clear();
            _version++;
            if (_isTrackingViewState) {
                // Clearing invalidates all previous log entries, so we can just clear them out and save some space
                Log.Clear();
            }
            Log.Add(new LogItem(LogItemType.Clear, 0, _isTrackingViewState));
        }


        public void CopyTo(TreeNode[] nodeArray, int index) {
            ((ICollection)this).CopyTo(nodeArray, index);
        }


        public bool Contains(TreeNode c) {
            return _list.Contains(c);
        }

        internal TreeNode FindNode(string[] path, int pos) {
            if (pos == path.Length) {
                return _owner;
            }

            string pathPart = TreeView.UnEscape(path[pos]);
            for (int i = 0; i < Count; i++) {
                TreeNode node = this[i];
                if (node.Value == pathPart) {
                    return node.ChildNodes.FindNode(path, pos + 1);
                }
            }

            return null;
        }


        public IEnumerator GetEnumerator() {
            return new TreeNodeCollectionEnumerator(this);
        }


        public int IndexOf(TreeNode value) {
            return _list.IndexOf(value);
        }


        public void Remove(TreeNode value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            int index = _list.IndexOf(value);
            if (index != -1) {
                RemoveAt(index);
            }
        }


        public void RemoveAt(int index) {
            TreeNode node = _list[index];
            if (_updateParent) {
                TreeView owner = node.Owner;
                if (owner != null) {
                    if (owner.CheckedNodes.Count != 0) {
                        // We have to scan the whole tree of subnodes to remove any checked nodes
                        // (and unselect the selected node if it is a descendant).
                        // That could badly hurt performance, except that removing a node is a pretty
                        // exceptional event.
                        UnCheckUnSelectRecursive(node);
                    }
                    else {
                        // otherwise, we can just climb the tree up from the selected node
                        // to see if it is a descendant of the removed node.
                        TreeNode current = owner.SelectedNode;
                        // Check if the selected item is under this collection
                        while (current != null) {
                            if (current == node) {
                                owner.SetSelectedNode(null);
                                break;
                            }
                            current = current.Parent;
                        }
                    }
                }
                node.SetParent(null);
            }

            _list.RemoveAt(index);
            _version++;
            Log.Add(new LogItem(LogItemType.Remove, index, _isTrackingViewState));
        }

        internal void SetDirty() {
            foreach (LogItem item in Log) {
                item.Tracked = true;
            }
            for (int i = 0; i < Count; i++) {
                this[i].SetDirty();
            }
        }

        private static void UnCheckUnSelectRecursive(TreeNode node) {
            TreeNodeCollection checkedNodes = node.Owner.CheckedNodes;
            if (node.Checked) {
                checkedNodes.Remove(node);
            }
            TreeNode selectedNode = node.Owner.SelectedNode;
            if (node == selectedNode) {
                node.Owner.SetSelectedNode(null);
                selectedNode = null;
            }
            // Only recurse if there could be some more work to do
            if (selectedNode != null || checkedNodes.Count != 0) {
                foreach (TreeNode child in node.ChildNodes) {
                    UnCheckUnSelectRecursive(child);
                }
            }
        }

        #region ICollection implementation

        void ICollection.CopyTo(Array array, int index) {
            if (!(array is TreeNode[])) {
                throw new ArgumentException(SR.GetString(SR.TreeNodeCollection_InvalidArrayType), "array");
            }
            _list.CopyTo((TreeNode[])array, index);
        }
        #endregion

        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            object[] nodeState = (object[])state;
            if (nodeState != null) {
                if (nodeState[0] != null) {
                    string logString = (string)nodeState[0];
                    // Process each log entry
                    string[] items = logString.Split(',');
                    for (int i = 0; i < items.Length; i++) {
                        string[] parts = items[i].Split(':');
                        LogItemType type = (LogItemType)Int32.Parse(parts[0], CultureInfo.InvariantCulture);
                        int index = Int32.Parse(parts[1], CultureInfo.InvariantCulture);

                        if (type == LogItemType.Insert) {
                            if (_owner != null && _owner.Owner != null) {
                                AddAt(index, _owner.Owner.CreateNode());
                            }
                            else {
                                AddAt(index, new TreeNode());
                            }
                        }
                        else if (type == LogItemType.Remove) {
                            RemoveAt(index);
                        }
                        else if (type == LogItemType.Clear) {
                            Clear();
                        }
                    }
                }

                for (int i = 0; i < nodeState.Length - 1; i++) {
                    if ((nodeState[i + 1] != null) && (this[i] != null)) {
                        ((IStateManager)this[i]).LoadViewState(nodeState[i + 1]);
                    }
                }
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            object[] nodes = new object[Count + 1];

            bool hasViewState = false;

            if ((_log != null) && (_log.Count > 0)) {
                // Construct a string representation of the log, delimiting entries with commas
                // and seperator command and index with a colon
                StringBuilder builder = new StringBuilder();
                int realLogCount = 0;
                for (int i = 0; i < _log.Count; i++) {
                    LogItem item = _log[i];
                    if (item.Tracked) {
                        builder.Append((int)item.Type);
                        builder.Append(":");
                        builder.Append(item.Index);
                        if (i < (_log.Count - 1)) {
                            builder.Append(",");
                        }

                        realLogCount++;
                    }
                }

                if (realLogCount > 0) {
                    nodes[0] = builder.ToString();
                    hasViewState = true;
                }
            }

            for (int i = 0; i < Count; i++) {
                nodes[i + 1] = ((IStateManager)this[i]).SaveViewState();
                if (nodes[i + 1] != null) {
                    hasViewState = true;
                }
            }

            return (hasViewState ? nodes : null);
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTrackingViewState = true;
            for (int i = 0; i < Count; i++) {
                ((IStateManager)this[i]).TrackViewState();
            }
        }
        #endregion

        /// <devdoc>
        ///     Convenience class for storing and using log entries.
        /// </devdoc>
        private class LogItem {
            private LogItemType _type;
            private int _index;
            private bool _tracked;

            public LogItem(LogItemType type, int index, bool tracked) {
                _type = type;
                _index = index;
                _tracked = tracked;
            }

            public int Index {
                get {
                    return _index;
                }
            }

            public bool Tracked {
                get {
                    return _tracked;
                }
                set {
                    _tracked = value;
                }
            }

            public LogItemType Type {
                get {
                    return _type;
                }
            }

        }

        /// <devdoc>
        ///     Convenience enumeration for identifying log commands
        /// </devdoc>
        private enum LogItemType {
            Insert = 0,
            Remove = 1,
            Clear = 2
        }

        // This is a copy of the ArrayListEnumeratorSimple in ArrayList.cs
        private class TreeNodeCollectionEnumerator : IEnumerator {
            private TreeNodeCollection list;
            private int index;
            private int version;
            private TreeNode currentElement;

            internal TreeNodeCollectionEnumerator(TreeNodeCollection list) {
                this.list = list;
                this.index = -1;
                version = list._version;
            }

            public bool MoveNext() {
                if (version != list._version)
                    throw new InvalidOperationException(SR.GetString(SR.ListEnumVersionMismatch));

                if (index < (list.Count - 1)) {
                    index++;
                    currentElement = list[index];
                    return true;
                }
                else
                    index = list.Count;
                return false;
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public TreeNode Current {
                get {
                    if (index == -1)
                        throw new InvalidOperationException(SR.GetString(SR.ListEnumCurrentOutOfRange));
                    if (index >= list.Count)
                        throw new InvalidOperationException(SR.GetString(SR.ListEnumCurrentOutOfRange));
                    return currentElement;
                }
            }

            public void Reset() {
                if (version != list._version)
                    throw new InvalidOperationException(SR.GetString(SR.ListEnumVersionMismatch));
                currentElement = null;
                index = -1;
            }
        }
    }
}
