//---------------------------------------------------------------------
// <copyright file="FacetDescriptionElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Diagnostics;

namespace System.Data.EntityModel.SchemaObjectModel
{
    internal abstract class FacetDescriptionElement : SchemaElement
    {
        int? _minValue;
        int? _maxValue;
        object _defaultValue;
        bool _isConstant;

        // won't be populated till you call CreateAndValidate
        FacetDescription _facetDescription;

        public FacetDescriptionElement(TypeElement type, string name)
        : base(type, name)
        {
        }

        protected override bool ProhibitAttribute(string namespaceUri, string localName)
        {
            if (base.ProhibitAttribute(namespaceUri, localName))
            {
                return true;
            }

            if (namespaceUri == null && localName == XmlConstants.Name)
            {
                return false;
            }
            return false;

        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader,  XmlConstants.MinimumAttribute))
            {
                HandleMinimumAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.MaximumAttribute))
            {
                HandleMaximumAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.DefaultValueAttribute))
            {
                HandleDefaultAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ConstantAttribute))
            {
                HandleConstantAttribute(reader);
                return true;
            }

            return false;
        }

        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        /// Handler for the Minimum attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Minimum attribute</param>
        protected void HandleMinimumAttribute(XmlReader reader)
        {
            int value = -1;
            if (HandleIntAttribute(reader, ref value))
            {
                _minValue = value;
            }
        }

        /// <summary>
        /// Handler for the Maximum attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Maximum attribute</param>
        protected void HandleMaximumAttribute(XmlReader reader)
        {
            int value = -1;
            if (HandleIntAttribute(reader, ref value))
            {
                _maxValue = value;
            }
        }

        /// <summary>
        /// Handler for the Default attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Default attribute</param>
        protected abstract void HandleDefaultAttribute(XmlReader reader);

        /// <summary>
        /// Handler for the Constant attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Constant attribute</param>
        protected void HandleConstantAttribute(XmlReader reader)
        {
            bool value = false;
            if (HandleBoolAttribute(reader, ref value))
            {
                _isConstant = value;
            }
        }

        public abstract EdmType FacetType{ get; }
        
        public int? MinValue
        {
            get { return _minValue; }
        }
        
        public int? MaxValue
        {
            get { return _maxValue; }
        }
        
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public FacetDescription FacetDescription
        {
            get
            {
                Debug.Assert(_facetDescription != null, "Did you forget to call CreateAndValidate first?");
                return _facetDescription;
            }
        }

        internal void CreateAndValidateFacetDescription(string declaringTypeName)
        {
            _facetDescription = new FacetDescription(Name, FacetType, MinValue, MaxValue, DefaultValue, _isConstant, declaringTypeName);
        }
    }
}
