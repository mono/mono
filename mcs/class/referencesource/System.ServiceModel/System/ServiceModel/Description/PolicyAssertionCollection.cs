//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml;

    public class PolicyAssertionCollection : Collection<XmlElement>
    {
        public PolicyAssertionCollection()
        {
        }

        public PolicyAssertionCollection(IEnumerable<XmlElement> elements)
        {
            if (elements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");

            AddRange(elements);
        }

        internal void AddRange(IEnumerable<XmlElement> elements)
        {
            foreach (XmlElement element in elements)
            {
                base.Add(element);
            }
        }

        public bool Contains(string localName, string namespaceUri)
        {
            if (localName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            if (namespaceUri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");

            for (int i = 0; i < this.Count; i++)
            {
                XmlElement item = this[i];
                if (item.LocalName == localName && item.NamespaceURI == namespaceUri)
                    return true;
            }
            return false;
        }

        public XmlElement Find(string localName, string namespaceUri)
        {
            return Find(localName, namespaceUri, false);
        }

        public XmlElement Remove(string localName, string namespaceUri)
        {
            return Find(localName, namespaceUri, true);
        }

        XmlElement Find(string localName, string namespaceUri, bool remove) 
        {
            if (localName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            if (namespaceUri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");

            for (int index = 0; index < this.Count; index++)
            {
                XmlElement item = this[index];
                if (item.LocalName == localName && item.NamespaceURI == namespaceUri)
                {
                    if (remove)
                    {
                        RemoveAt(index);
                    }
                    return item;
                }
            }
            return null;
        }

        public Collection<XmlElement> FindAll(string localName, string namespaceUri)
        {
            return FindAll(localName, namespaceUri, false);
        }

        public Collection<XmlElement> RemoveAll(string localName, string namespaceUri)
        {
            return FindAll(localName, namespaceUri, true);
        }

        Collection<XmlElement> FindAll(string localName, string namespaceUri, bool remove)
        {
            if (localName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            if (namespaceUri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");

            Collection<XmlElement> collection = new Collection<XmlElement>();

            for (int index = 0; index < this.Count; index++)
            {
                XmlElement item = this[index];
                if (item.LocalName == localName && item.NamespaceURI == namespaceUri)
                {
                    if (remove)
                    {
                        RemoveAt(index);
                        // back up the index so we inspect the new item at this location
                        index--;
                    }
                    collection.Add(item);
                }
            }

            return collection;
        }

        protected override void InsertItem(int index, XmlElement item)
        {
            if (item == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, XmlElement item)
        {
            if (item == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");

            base.SetItem(index, item);
        }
    }

}
