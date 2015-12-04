//---------------------------------------------------------------------
// <copyright file="TypeElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Responsible for parsing Type ProviderManifest 
    /// xml elements
    /// </summary>
    internal class TypeElement : SchemaType
    {

        PrimitiveType _primitiveType = new PrimitiveType();
        List<FacetDescriptionElement> _facetDescriptions = new List<FacetDescriptionElement>();

        public TypeElement(Schema parent)
            : base(parent)
        {
            _primitiveType.NamespaceName = Schema.Namespace;
        }


        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.FacetDescriptionsElement))
            {
                SkipThroughElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.PrecisionElement))
            {
                HandlePrecisionElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ScaleElement))
            {
                HandleScaleElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.MaxLengthElement))
            {
                HandleMaxLengthElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.UnicodeElement))
            {
                HandleUnicodeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.FixedLengthElement))
            {
                HandleFixedLengthElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.SridElement))
            {
                HandleSridElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.IsStrictElement))
            {
                HandleIsStrictElement(reader);
                return true;
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.PrimitiveTypeKindAttribute))
            {
                HandlePrimitiveTypeKindAttribute(reader);
                return true;
            }

            return false;
        }

        /////////////////////////////////////////////////////////////////////
        // Element Handlers

        /// <summary>
        /// Handler for the Precision element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Precision element</param>
        private void HandlePrecisionElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            ByteFacetDescriptionElement facetDescription = new ByteFacetDescriptionElement(this, DbProviderManifest.PrecisionFacetName);
            facetDescription.Parse(reader);

            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the Scale element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Scale element</param>
        private void HandleScaleElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            ByteFacetDescriptionElement facetDescription = new ByteFacetDescriptionElement(this, DbProviderManifest.ScaleFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the MaxLength element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at MaxLength element</param>
        private void HandleMaxLengthElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            IntegerFacetDescriptionElement facetDescription = new IntegerFacetDescriptionElement(this, DbProviderManifest.MaxLengthFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the Unicode element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Unicode element</param>
        private void HandleUnicodeElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            BooleanFacetDescriptionElement facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.UnicodeFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the FixedLength element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at FixedLength element</param>
        private void HandleFixedLengthElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            BooleanFacetDescriptionElement facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.FixedLengthFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the SRID element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at SRID element</param>
        private void HandleSridElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            SridFacetDescriptionElement facetDescription = new SridFacetDescriptionElement(this, DbProviderManifest.SridFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        /// Handler for the IsStrict element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at SRID element</param>
        private void HandleIsStrictElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            BooleanFacetDescriptionElement facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.IsStrictFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }
        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        /// Handler for the PrimitiveTypeKind attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Version attribute</param>
        private void HandlePrimitiveTypeKindAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            string value = reader.Value;
            try
            {
                _primitiveType.PrimitiveTypeKind = (PrimitiveTypeKind)Enum.Parse(typeof(PrimitiveTypeKind), value);
                _primitiveType.BaseType = MetadataItem.EdmProviderManifest.GetPrimitiveType(_primitiveType.PrimitiveTypeKind);
            }
            catch (ArgumentException)
            {
                AddError(ErrorCode.InvalidPrimitiveTypeKind, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidPrimitiveTypeKind(value));
            }
        }

        public override string Name
        {
            get
            {
                return _primitiveType.Name;
            }
            set
            {
                _primitiveType.Name = value;
            }
        }

        public PrimitiveType PrimitiveType
        {
            get
            {
                return _primitiveType; 
            }
        }

        public IEnumerable<FacetDescription> FacetDescriptions
        {
            get 
            {
                foreach (FacetDescriptionElement element in _facetDescriptions)
                {
                    yield return element.FacetDescription;
                }
            }
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            // Call validate on the facet descriptions
            foreach (FacetDescriptionElement facetDescription in _facetDescriptions)
            {
                try
                {
                    facetDescription.CreateAndValidateFacetDescription(this.Name);
                }
                catch (ArgumentException e)
                {
                    AddError(ErrorCode.InvalidFacetInProviderManifest,
                             EdmSchemaErrorSeverity.Error,
                             e.Message);
                }
            }
            // facet descriptions don't have any names to resolve
        }

        internal override void Validate()
        {
            base.Validate();

            if (!ValidateSufficientFacets())
            {
                // the next checks will fail, so get out
                // if we had errors
                return;
            }

            if (!ValidateInterFacetConsistency())
            {
                return;
            }
        }

        private bool ValidateInterFacetConsistency()
        {
            if (PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Decimal)
            {
                FacetDescription precisionFacetDescription = Helper.GetFacet(FacetDescriptions, EdmProviderManifest.PrecisionFacetName);
                FacetDescription scaleFacetDescription = Helper.GetFacet(FacetDescriptions, EdmProviderManifest.ScaleFacetName);

                if(precisionFacetDescription.MaxValue.Value < scaleFacetDescription.MaxValue.Value)
                {
                    AddError(ErrorCode.BadPrecisionAndScale,
                             EdmSchemaErrorSeverity.Error,
                             System.Data.Entity.Strings.BadPrecisionAndScale(
                                           precisionFacetDescription.MaxValue.Value,
                                           scaleFacetDescription.MaxValue.Value));
                    return false;
                }
            }

            return true;
        }

        private bool ValidateSufficientFacets()
        {
            PrimitiveType baseType = _primitiveType.BaseType as PrimitiveType;
            // the base type will be an edm type
            // the edm type is the athority for which facets are required
            if (baseType == null)
            {
                // an error will already have been added for this
                return false;
            }

            bool addedErrors = false;
            foreach (FacetDescription systemFacetDescription in baseType.FacetDescriptions)
            {
                FacetDescription providerFacetDescription = Helper.GetFacet(FacetDescriptions, systemFacetDescription.FacetName);
                if (providerFacetDescription == null)
                {
                    AddError(ErrorCode.RequiredFacetMissing,
                             EdmSchemaErrorSeverity.Error,
                             System.Data.Entity.Strings.MissingFacetDescription(
                                           PrimitiveType.Name,
                                           PrimitiveType.PrimitiveTypeKind,
                                           systemFacetDescription.FacetName));
                    addedErrors = true;
                }
            }
            
            return !addedErrors;
        }
    }
}
