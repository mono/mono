//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Claims;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    using ISecurityElement = System.IdentityModel.ISecurityElement;

    class SendSecurityHeaderElement
    {
        string id;
        ISecurityElement item;
        bool markedForEncryption;

        public SendSecurityHeaderElement(string id, ISecurityElement item)
        {
            this.id = id;
            this.item = item;
            markedForEncryption = false;
        }

        public string Id
        {
            get { return this.id; }
        }

        public ISecurityElement Item
        {
            get { return this.item; }
        }

        public bool MarkedForEncryption
        {
            get { return this.markedForEncryption; }
            set { this.markedForEncryption = value; }
        }

        public bool IsSameItem(ISecurityElement item)
        {
            return this.item == item || this.item.Equals(item);
        }

        public void Replace(string id, ISecurityElement item)
        {
            this.item = item;
            this.id = id;
        }
    }
}
