//------------------------------------------------------------------------------
// <copyright file="ProfilePropertyMetadata.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.ApplicationServices
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ProfilePropertyMetadata : IExtensibleDataObject
    {
        private ExtensionDataObject _extensionDataObject;
        private String _propertyName;
        
        public ExtensionDataObject ExtensionData {
            get {
                return _extensionDataObject;
            }
            set {
                _extensionDataObject = value;
            }
        }

        [DataMember]
        public String PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                _propertyName = value;
            }
        }

        private String _typeName;
        [DataMember]
        public String TypeName
        {
            get
            {
                return _typeName;
            }
            set
            {
                _typeName = value;
            }
        }

        private bool _allowAnonymousAccess;
        [DataMember]
        public bool AllowAnonymousAccess
        {
            get
            {
                return _allowAnonymousAccess;
            }
            set
            {
                _allowAnonymousAccess = value;
            }
        }

        private bool _isReadOnly;
        [DataMember]
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }
            set
            {
                _isReadOnly = value;
            }
        }
        /* Uncommnet once accessmode is available
        public ProfileServiceAccess _accessMode;
        [DataMember]
        public ProfileServiceAccess AccessMode
        {
            get
            {
                return _accessMode;
            }
            set
            {
                _accessMode = value;
            }
        }
        */
        private int _serializeAs;
        [DataMember]
        public int SerializeAs
        {
            get
            {
                return _serializeAs;
            }
            set
            {
                _serializeAs = value;
            }
        }

        private string _defaultValue;
        [DataMember]
        public string DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                _defaultValue = value;
            }
        }
    }
}
