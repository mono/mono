//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    public class MessagePartSpecification
    {
        List<XmlQualifiedName> headerTypes;
        bool isBodyIncluded;
        bool isReadOnly;
        static MessagePartSpecification noParts;

        public ICollection<XmlQualifiedName> HeaderTypes
        {
            get
            {
                if (headerTypes == null)
                {
                    headerTypes = new List<XmlQualifiedName>();
                }

                if (isReadOnly)
                {
                    return new ReadOnlyCollection<XmlQualifiedName>(headerTypes);
                }
                else
                {
                    return headerTypes;
                }
            }
        }

        internal bool HasHeaders
        {
            get { return this.headerTypes != null && this.headerTypes.Count > 0; }
        }

        public bool IsBodyIncluded
        {
            get
            {
                return this.isBodyIncluded;
            }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.isBodyIncluded = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        static public MessagePartSpecification NoParts
        {
            get
            {
                if (noParts == null)
                {
                    MessagePartSpecification parts = new MessagePartSpecification();
                    parts.MakeReadOnly();
                    noParts = parts;
                }
                return noParts;
            }
        }

        public void Clear()
        {
            if (isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

            if (this.headerTypes != null)
                this.headerTypes.Clear();
            this.isBodyIncluded = false;
        }

        public void Union(MessagePartSpecification specification)
        {
            if (isReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            if (specification == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("specification");

            this.isBodyIncluded |= specification.IsBodyIncluded;

            List<XmlQualifiedName> headerTypes = specification.headerTypes;
            if (headerTypes != null && headerTypes.Count > 0)
            {
                if (this.headerTypes == null)
                {
                    this.headerTypes = new List<XmlQualifiedName>(headerTypes.Count);
                }

                for (int i = 0; i < headerTypes.Count; i++)
                {
                    XmlQualifiedName qname = headerTypes[i];
                    this.headerTypes.Add(qname);
                }
            }
        }

        public void MakeReadOnly()
        {
            if (isReadOnly)
                return;

            if (this.headerTypes != null)
            {
                List<XmlQualifiedName> noDuplicates = new List<XmlQualifiedName>(headerTypes.Count);
                for (int i = 0; i < headerTypes.Count; i++)
                {
                    XmlQualifiedName qname = headerTypes[i];
                    if (qname != null)
                    {
                        bool include = true;
                        for (int j = 0; j < noDuplicates.Count; j++)
                        {
                            XmlQualifiedName qname1 = noDuplicates[j];

                            if (qname.Name == qname1.Name && qname.Namespace == qname1.Namespace)
                            {
                                include = false;
                                break;
                            }
                        }

                        if (include)
                            noDuplicates.Add(qname);
                    }
                }

                this.headerTypes = noDuplicates;
            }

            this.isReadOnly = true;
        }

        public MessagePartSpecification() 
        {
            // empty
        }

        public MessagePartSpecification(bool isBodyIncluded) 
        {
            this.isBodyIncluded = isBodyIncluded;
        }

        public MessagePartSpecification(params XmlQualifiedName[] headerTypes) 
            : this(false, headerTypes)
        {
            // empty
        }

        public MessagePartSpecification(bool isBodyIncluded, params XmlQualifiedName[] headerTypes) 
        {
            this.isBodyIncluded = isBodyIncluded;
            if (headerTypes != null && headerTypes.Length > 0)
            {
                this.headerTypes = new List<XmlQualifiedName>(headerTypes.Length);
                for (int i = 0; i < headerTypes.Length; i++)
                {
                    this.headerTypes.Add(headerTypes[i]);
                }
            }
        }

        internal bool IsHeaderIncluded(MessageHeader header)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("header");

            return IsHeaderIncluded(header.Name, header.Namespace);
        }

        internal bool IsHeaderIncluded(string name, string ns)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");

            if (this.headerTypes != null)
            {
                for (int i = 0; i < this.headerTypes.Count; i++)
                {
                    XmlQualifiedName qname = this.headerTypes[i];
                    // Name is an optional attribute. If not present, compare with only the namespace.
                    if (String.IsNullOrEmpty(qname.Name))
                    {
                        if (qname.Namespace == ns)
                        {
                            return true;
                        }
                    }
                    else 
                    {
                        if (qname.Name == name && qname.Namespace == ns)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal bool IsEmpty()
        {
            if (this.headerTypes != null && this.headerTypes.Count > 0)
                return false;

            return !this.IsBodyIncluded;
        }
    }
}
