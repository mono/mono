//------------------------------------------------------------------------------
// <copyright file="XmlDocumentValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Globalization;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Versioning;

namespace System.Xml {

    internal sealed class DocumentSchemaValidator : IXmlNamespaceResolver {
        XmlSchemaValidator validator;
        XmlSchemaSet schemas;
        
        XmlNamespaceManager nsManager;
        XmlNameTable nameTable;

        //Attributes
        ArrayList defaultAttributes;
        XmlValueGetter nodeValueGetter;
        XmlSchemaInfo attributeSchemaInfo;

        //Element PSVI 
        XmlSchemaInfo schemaInfo;

        //Event Handler
        ValidationEventHandler eventHandler;
        ValidationEventHandler internalEventHandler;

        //Store nodes
        XmlNode startNode;
        XmlNode currentNode;
        XmlDocument document;

        //List of nodes for partial validation tree walk
        XmlNode[] nodeSequenceToValidate;
        bool isPartialTreeValid;

        bool psviAugmentation;
        bool isValid;

        //To avoid SchemaNames creation
        private string NsXmlNs;
        private string NsXsi;
        private string XsiType;
        private string XsiNil;

        public DocumentSchemaValidator(XmlDocument ownerDocument, XmlSchemaSet schemas, ValidationEventHandler eventHandler) {
            this.schemas = schemas;
            this.eventHandler = eventHandler;
            document = ownerDocument;
            this.internalEventHandler = new ValidationEventHandler(InternalValidationCallBack);
            
            this.nameTable = document.NameTable;
            nsManager = new XmlNamespaceManager(nameTable);
            
            Debug.Assert(schemas != null && schemas.Count > 0);

            nodeValueGetter = new XmlValueGetter(GetNodeValue);
            psviAugmentation = true;

            //Add common strings to be compared to NameTable
            NsXmlNs = nameTable.Add(XmlReservedNs.NsXmlNs);
            NsXsi = nameTable.Add(XmlReservedNs.NsXsi);
            XsiType = nameTable.Add("type");
            XsiNil = nameTable.Add("nil");
        }

        public bool PsviAugmentation {
            get { return psviAugmentation; }
            set { psviAugmentation = value; }
        }

        public bool Validate(XmlNode nodeToValidate) {
            XmlSchemaObject partialValidationType = null;
            XmlSchemaValidationFlags validationFlags = XmlSchemaValidationFlags.AllowXmlAttributes;
            Debug.Assert(nodeToValidate.SchemaInfo != null);

            startNode = nodeToValidate;
            switch (nodeToValidate.NodeType) {
                case XmlNodeType.Document:
                    validationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
                    break;

                case XmlNodeType.DocumentFragment:
                    break;

                case XmlNodeType.Element: //Validate children of this element
                    IXmlSchemaInfo schemaInfo = nodeToValidate.SchemaInfo;
                    XmlSchemaElement schemaElement = schemaInfo.SchemaElement;
                    if (schemaElement != null) {
                        if (!schemaElement.RefName.IsEmpty) { //If it is element ref,
                            partialValidationType = schemas.GlobalElements[schemaElement.QualifiedName]; //Get Global element with correct Nillable, Default etc
                        }
                        else { //local element
                            partialValidationType = schemaElement;
                        }
                        //Verify that if there was xsi:type, the schemaElement returned has the correct type set
                        Debug.Assert(schemaElement.ElementSchemaType == schemaInfo.SchemaType);
                    }
                    else { //Can be an element that matched xs:any and had xsi:type
                        partialValidationType = schemaInfo.SchemaType;   
                     
                        if (partialValidationType == null) { //Validated against xs:any with pc= lax or skip or undeclared / not validated element
                            if (nodeToValidate.ParentNode.NodeType == XmlNodeType.Document) {
                                //If this is the documentElement and it has not been validated at all
                                nodeToValidate = nodeToValidate.ParentNode;
                            }
                            else {
                                partialValidationType = FindSchemaInfo(nodeToValidate as XmlElement);
                                if (partialValidationType == null) { 
                                    throw new XmlSchemaValidationException(Res.XmlDocument_NoNodeSchemaInfo, null, nodeToValidate);
                                }
                            }
                        }
                    }
                    break;

                case XmlNodeType.Attribute:
                    if (nodeToValidate.XPNodeType == XPathNodeType.Namespace) goto default;
                    partialValidationType = nodeToValidate.SchemaInfo.SchemaAttribute;
                    if (partialValidationType == null) { //Validated against xs:anyAttribute with pc = lax or skip / undeclared attribute
                        partialValidationType = FindSchemaInfo(nodeToValidate as XmlAttribute);
                        if (partialValidationType == null) { 
                            throw new XmlSchemaValidationException(Res.XmlDocument_NoNodeSchemaInfo, null, nodeToValidate);
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException(Res.GetString(Res.XmlDocument_ValidateInvalidNodeType, null));
            }
            isValid = true;
            CreateValidator(partialValidationType, validationFlags);
            if (psviAugmentation) {
                if (schemaInfo == null) { //Might have created it during FindSchemaInfo
                    schemaInfo = new XmlSchemaInfo();
                }
                attributeSchemaInfo = new XmlSchemaInfo();
            }
            ValidateNode(nodeToValidate);
            validator.EndValidation();    
            return isValid; 
        }

        public IDictionary<string,string> GetNamespacesInScope(XmlNamespaceScope scope) {
            IDictionary<string,string> dictionary = nsManager.GetNamespacesInScope(scope); 
            if (scope != XmlNamespaceScope.Local) {
                XmlNode node = startNode;
                while (node != null) {
                    switch (node.NodeType) {
                        case XmlNodeType.Element:
                            XmlElement elem = (XmlElement)node;
                            if (elem.HasAttributes) {
                                XmlAttributeCollection attrs = elem.Attributes;
                                for (int i = 0; i < attrs.Count; i++) {
                                    XmlAttribute attr = attrs[i];
                                    if (Ref.Equal(attr.NamespaceURI, document.strReservedXmlns)) {
                                        if (attr.Prefix.Length == 0) {
                                            // xmlns='' declaration
                                            if (!dictionary.ContainsKey(string.Empty)) {
                                                dictionary.Add(string.Empty, attr.Value);
                                            }
                                        }
                                        else {
                                            // xmlns:prefix='' declaration
                                            if (!dictionary.ContainsKey(attr.LocalName)) {
                                                dictionary.Add(attr.LocalName, attr.Value);
                                            }
                                        }
                                    }
                                }
                            }
                            node = node.ParentNode;
                            break;
                        case XmlNodeType.Attribute:
                            node = ((XmlAttribute)node).OwnerElement;
                            break;
                        default:
                            node = node.ParentNode;
                            break;
                    }
                }
            }
            return dictionary;
        }

        public string LookupNamespace(string prefix) {
            string namespaceName = nsManager.LookupNamespace(prefix);
            if (namespaceName == null) {
                namespaceName = startNode.GetNamespaceOfPrefixStrict(prefix);
            }
            return namespaceName;
        }

        public string LookupPrefix(string namespaceName) {
            string prefix = nsManager.LookupPrefix(namespaceName);
            if (prefix == null) {
                prefix = startNode.GetPrefixOfNamespaceStrict(namespaceName);
            }
            return prefix;
        }

        private IXmlNamespaceResolver NamespaceResolver {
            get {
                if ((object)startNode == (object)document) {
                    return nsManager;
                }
                return this;
            }
        }

        private void CreateValidator(XmlSchemaObject partialValidationType, XmlSchemaValidationFlags validationFlags) {
            validator = new XmlSchemaValidator(nameTable, schemas, NamespaceResolver, validationFlags);
            validator.SourceUri = XmlConvert.ToUri(document.BaseURI);
            validator.XmlResolver = null;
            validator.ValidationEventHandler += internalEventHandler;
            validator.ValidationEventSender = this;
            
            if (partialValidationType != null) {
                validator.Initialize(partialValidationType);
            }
            else {
                validator.Initialize();
            }
        }

        private void ValidateNode(XmlNode node) {
            currentNode = node;
            switch (currentNode.NodeType) {
                case XmlNodeType.Document:
                    XmlElement docElem = ((XmlDocument)node).DocumentElement;
                    if (docElem == null) {
                        throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidXmlDocument, Res.GetString(Res.Xdom_NoRootEle)));
                    }
                    ValidateNode(docElem);
                    break;

                case XmlNodeType.DocumentFragment:
                case XmlNodeType.EntityReference:
                    for (XmlNode child = node.FirstChild; child != null; child = child.NextSibling) {
                        ValidateNode(child);
                    }
                    break;

                case XmlNodeType.Element:
                    ValidateElement();
                    break;

                case XmlNodeType.Attribute: //Top-level attribute
                    XmlAttribute attr = currentNode as XmlAttribute;
                    validator.ValidateAttribute(attr.LocalName, attr.NamespaceURI, nodeValueGetter, attributeSchemaInfo);
                    if (psviAugmentation) {
                        attr.XmlName = document.AddAttrXmlName(attr.Prefix, attr.LocalName, attr.NamespaceURI, attributeSchemaInfo);
                    }
                    break;

                case XmlNodeType.Text:
                    validator.ValidateText(nodeValueGetter);
                    break;

                case XmlNodeType.CDATA:
                    validator.ValidateText(nodeValueGetter);
                    break;

                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    validator.ValidateWhitespace(nodeValueGetter);
                    break;

                case XmlNodeType.Comment:
                case XmlNodeType.ProcessingInstruction:
                    break;

                default:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_UnexpectedNodeType, new string[]{ currentNode.NodeType.ToString() } ) );
            }
        }

        // SxS: This function calls ValidateElement on XmlSchemaValidator which is annotated with ResourceExposure attribute.
        // Since the resource names passed to ValidateElement method are null and the function does not expose any resources 
        // it is fine to suppress the SxS warning. 
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void ValidateElement() {
            nsManager.PushScope();
            XmlElement elementNode = currentNode as XmlElement;
            Debug.Assert(elementNode != null);

            XmlAttributeCollection attributes = elementNode.Attributes;
            XmlAttribute attr = null;

            //Find Xsi attributes that need to be processed before validating the element
            string xsiNil = null;
            string xsiType = null; 

            for (int i = 0; i < attributes.Count; i++) {
                attr = attributes[i];
                string objectNs = attr.NamespaceURI;
                string objectName = attr.LocalName;
                Debug.Assert(nameTable.Get(attr.NamespaceURI) != null);
                Debug.Assert(nameTable.Get(attr.LocalName) != null);

                if (Ref.Equal(objectNs, NsXsi)) {
                    if (Ref.Equal(objectName, XsiType)) {
                        xsiType = attr.Value;
                    }
                    else if (Ref.Equal(objectName, XsiNil)) {
                        xsiNil = attr.Value;
                    }
                }
                else if (Ref.Equal(objectNs,NsXmlNs)) {
                    nsManager.AddNamespace(attr.Prefix.Length == 0 ? string.Empty : attr.LocalName, attr.Value);
                }
            }
            validator.ValidateElement(elementNode.LocalName, elementNode.NamespaceURI, schemaInfo, xsiType, xsiNil, null, null);
            ValidateAttributes(elementNode);
            validator.ValidateEndOfAttributes(schemaInfo);

            //If element has children, drill down
            for (XmlNode child = elementNode.FirstChild; child != null; child = child.NextSibling) {
                ValidateNode(child);
            }
            //Validate end of element
            currentNode = elementNode; //Reset current Node for validation call back
            validator.ValidateEndElement(schemaInfo);
            //Get XmlName, as memberType / validity might be set now
            if (psviAugmentation) {
                elementNode.XmlName = document.AddXmlName(elementNode.Prefix, elementNode.LocalName, elementNode.NamespaceURI, schemaInfo);
                if (schemaInfo.IsDefault) { //the element has a default value
                    XmlText textNode = document.CreateTextNode(schemaInfo.SchemaElement.ElementDecl.DefaultValueRaw);
                    elementNode.AppendChild(textNode);
                }
            }

            nsManager.PopScope(); //Pop current namespace scope
        }

        private void ValidateAttributes(XmlElement elementNode) {
            XmlAttributeCollection attributes = elementNode.Attributes;
            XmlAttribute attr = null;

            for (int i = 0; i < attributes.Count; i++) {
                attr = attributes[i];
                currentNode = attr; //For nodeValueGetter to pick up the right attribute value
                if (Ref.Equal(attr.NamespaceURI,NsXmlNs)) { //Do not validate namespace decls
                    continue;
                }
                validator.ValidateAttribute(attr.LocalName, attr.NamespaceURI, nodeValueGetter, attributeSchemaInfo);
                if (psviAugmentation) {
                    attr.XmlName = document.AddAttrXmlName(attr.Prefix, attr.LocalName, attr.NamespaceURI, attributeSchemaInfo);
                }
            }
    
            if (psviAugmentation) {
                //Add default attributes to the attributes collection
                if (defaultAttributes == null) {
                    defaultAttributes = new ArrayList();
                }
                else {
                    defaultAttributes.Clear();
                }
                validator.GetUnspecifiedDefaultAttributes(defaultAttributes);
                XmlSchemaAttribute schemaAttribute = null;
                XmlQualifiedName attrQName;
                attr = null;
                for (int i = 0; i < defaultAttributes.Count; i++) {
                    schemaAttribute = defaultAttributes[i] as XmlSchemaAttribute;
                    attrQName = schemaAttribute.QualifiedName;
                    Debug.Assert(schemaAttribute != null);
                    attr = document.CreateDefaultAttribute(GetDefaultPrefix(attrQName.Namespace), attrQName.Name, attrQName.Namespace);
                    SetDefaultAttributeSchemaInfo(schemaAttribute);
                    attr.XmlName = document.AddAttrXmlName(attr.Prefix, attr.LocalName, attr.NamespaceURI, attributeSchemaInfo);
                    attr.AppendChild(document.CreateTextNode(schemaAttribute.AttDef.DefaultValueRaw));
                    attributes.Append(attr);
                    XmlUnspecifiedAttribute defAttr = attr as XmlUnspecifiedAttribute;
                    if (defAttr != null) {
                        defAttr.SetSpecified(false);
                    }
                }
            }
        }

        private void SetDefaultAttributeSchemaInfo(XmlSchemaAttribute schemaAttribute) {
            Debug.Assert(attributeSchemaInfo != null);
            attributeSchemaInfo.Clear();
            attributeSchemaInfo.IsDefault = true;
            attributeSchemaInfo.IsNil = false;
            attributeSchemaInfo.SchemaType = schemaAttribute.AttributeSchemaType;
            attributeSchemaInfo.SchemaAttribute = schemaAttribute;
            
            //Get memberType for default attribute
            SchemaAttDef attributeDef = schemaAttribute.AttDef;                
            if (attributeDef.Datatype.Variety == XmlSchemaDatatypeVariety.Union) {
                XsdSimpleValue simpleValue = attributeDef.DefaultValueTyped as XsdSimpleValue;
                Debug.Assert(simpleValue != null);
                attributeSchemaInfo.MemberType = simpleValue.XmlType;
            }
            attributeSchemaInfo.Validity = XmlSchemaValidity.Valid;
        }

        private string GetDefaultPrefix(string attributeNS) {
            IDictionary<string,string> namespaceDecls = NamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.All);
            string defaultPrefix = null;
            string defaultNS;
            attributeNS = nameTable.Add(attributeNS); //atomize ns

            foreach (KeyValuePair<string,string> pair in namespaceDecls) {
                defaultNS = nameTable.Add(pair.Value);
                if (object.ReferenceEquals(defaultNS, attributeNS)) {
                    defaultPrefix = pair.Key;
                    if (defaultPrefix.Length != 0) { //Locate first non-empty prefix
                        return defaultPrefix;
                    }
                }
            }
            return defaultPrefix;
        }

        private object GetNodeValue() {
            return currentNode.Value;
        }

        //Code for finding type during partial validation
        private XmlSchemaObject FindSchemaInfo(XmlElement elementToValidate) {
            isPartialTreeValid = true;
            Debug.Assert(elementToValidate.ParentNode.NodeType != XmlNodeType.Document); //Handle if it is the documentElement seperately            
            
            //Create nodelist to navigate down again
            XmlNode currentNode = elementToValidate;
            IXmlSchemaInfo parentSchemaInfo = null;
            int nodeIndex = 0;
            
            //Check common case of parent node first
            XmlNode parentNode = currentNode.ParentNode;
            do {
                parentSchemaInfo = parentNode.SchemaInfo;
                if (parentSchemaInfo.SchemaElement != null || parentSchemaInfo.SchemaType != null) {
                    break; //Found ancestor with schemaInfo
                }
                CheckNodeSequenceCapacity(nodeIndex);
                nodeSequenceToValidate[nodeIndex++] = parentNode;
                parentNode = parentNode.ParentNode;
            } while (parentNode != null);

            if (parentNode == null) { //Did not find any type info all the way to the root, currentNode is Document || DocumentFragment
                nodeIndex = nodeIndex - 1; //Subtract the one for document and set the node to null
                nodeSequenceToValidate[nodeIndex] = null;
                return GetTypeFromAncestors(elementToValidate, null, nodeIndex);
            }
            else {
                //Start validating down from the parent or ancestor that has schema info and shallow validate all previous siblings
                //to correctly ascertain particle for current node
                CheckNodeSequenceCapacity(nodeIndex);
                nodeSequenceToValidate[nodeIndex++] = parentNode;
                XmlSchemaObject ancestorSchemaObject = parentSchemaInfo.SchemaElement;
                if (ancestorSchemaObject == null) {
                    ancestorSchemaObject = parentSchemaInfo.SchemaType;
                }
                return GetTypeFromAncestors(elementToValidate, ancestorSchemaObject, nodeIndex);

            }
        }

        /*private XmlSchemaElement GetTypeFromParent(XmlElement elementToValidate, XmlSchemaComplexType parentSchemaType) {
            XmlQualifiedName elementName = new XmlQualifiedName(elementToValidate.LocalName, elementToValidate.NamespaceURI);
            XmlSchemaElement elem = parentSchemaType.LocalElements[elementName] as XmlSchemaElement;
            if (elem == null) { //Element not found as direct child of the content model. It might be invalid at this position or it might be a substitution member
                SchemaInfo compiledSchemaInfo = schemas.CompiledInfo;
                XmlSchemaElement memberElem = compiledSchemaInfo.GetElement(elementName);
                if (memberElem != null) {
                }
            }
        }*/

        private void CheckNodeSequenceCapacity(int currentIndex) {
            if (nodeSequenceToValidate == null) { //Normally users would call Validate one level down, this allows for 4
                nodeSequenceToValidate = new XmlNode[4];
            }
            else if (currentIndex >= nodeSequenceToValidate.Length -1 ) { //reached capacity of array, Need to increase capacity to twice the initial
                XmlNode[] newNodeSequence = new XmlNode[nodeSequenceToValidate.Length * 2];
                Array.Copy(nodeSequenceToValidate, 0, newNodeSequence, 0, nodeSequenceToValidate.Length);
                nodeSequenceToValidate = newNodeSequence;
            }
        }

        private XmlSchemaAttribute FindSchemaInfo(XmlAttribute attributeToValidate) {
            XmlElement parentElement = attributeToValidate.OwnerElement;
            XmlSchemaObject schemaObject = FindSchemaInfo(parentElement);
            XmlSchemaComplexType elementSchemaType = GetComplexType(schemaObject);
            if (elementSchemaType == null) {
                return null;
            }
            XmlQualifiedName attName = new XmlQualifiedName(attributeToValidate.LocalName, attributeToValidate.NamespaceURI);
            XmlSchemaAttribute schemaAttribute = elementSchemaType.AttributeUses[attName] as XmlSchemaAttribute;
            if (schemaAttribute == null) {
                XmlSchemaAnyAttribute anyAttribute = elementSchemaType.AttributeWildcard;
                if (anyAttribute != null) {
                    if (anyAttribute.NamespaceList.Allows(attName)){ //Match wildcard against global attribute
                        schemaAttribute = schemas.GlobalAttributes[attName] as XmlSchemaAttribute;
                    }
                }
            }
            return schemaAttribute;
        }

        private XmlSchemaObject GetTypeFromAncestors(XmlElement elementToValidate, XmlSchemaObject ancestorType, int ancestorsCount) {
            
            //schemaInfo is currentNode's schemaInfo
            validator = CreateTypeFinderValidator(ancestorType);
            schemaInfo = new XmlSchemaInfo();

            //start at the ancestor to start validating
            int startIndex = ancestorsCount - 1;
        
            bool ancestorHasWildCard = AncestorTypeHasWildcard(ancestorType);
            for (int i = startIndex; i >= 0; i--) {
                XmlNode node = nodeSequenceToValidate[i];
                XmlElement currentElement = node as XmlElement;
                ValidateSingleElement(currentElement, false, schemaInfo);
                if (!ancestorHasWildCard) { //store type if ancestor does not have wildcard in its content model
                    currentElement.XmlName = document.AddXmlName(currentElement.Prefix, currentElement.LocalName, currentElement.NamespaceURI, schemaInfo);
                    //update wildcard flag
                    ancestorHasWildCard = AncestorTypeHasWildcard(schemaInfo.SchemaElement);
                }
                
                validator.ValidateEndOfAttributes(null);
                if (i > 0) {
                    ValidateChildrenTillNextAncestor(node, nodeSequenceToValidate[i - 1]);
                }
                else { //i == 0
                    ValidateChildrenTillNextAncestor(node, elementToValidate);
                }
            }

            Debug.Assert(nodeSequenceToValidate[0] == elementToValidate.ParentNode);
            //validate element whose type is needed,
            ValidateSingleElement(elementToValidate, false, schemaInfo);

            XmlSchemaObject schemaInfoFound = null;
            if (schemaInfo.SchemaElement != null) {
                schemaInfoFound = schemaInfo.SchemaElement;
            }
            else {
                schemaInfoFound = schemaInfo.SchemaType;
            }
            if (schemaInfoFound == null) { //Detect if the node was validated lax or skip
                if (validator.CurrentProcessContents == XmlSchemaContentProcessing.Skip) {
                    if (isPartialTreeValid) { //Then node assessed as skip; if there was error we turn processContents to skip as well. But this is not the same as validating as skip.
                        return XmlSchemaComplexType.AnyTypeSkip;
                    }
                }
                else if (validator.CurrentProcessContents == XmlSchemaContentProcessing.Lax) {
                    return XmlSchemaComplexType.AnyType;
                }
            }
            return schemaInfoFound;
        }

        private bool AncestorTypeHasWildcard(XmlSchemaObject ancestorType) {
            XmlSchemaComplexType ancestorSchemaType = GetComplexType(ancestorType);
            if (ancestorType != null) {
                return ancestorSchemaType.HasWildCard;
            }
            return false;
        }

        private XmlSchemaComplexType GetComplexType(XmlSchemaObject schemaObject) {
            if (schemaObject == null) {
                return null;
            }
            XmlSchemaElement schemaElement = schemaObject as XmlSchemaElement;
            XmlSchemaComplexType complexType = null;
            if (schemaElement != null) {
                complexType = schemaElement.ElementSchemaType as XmlSchemaComplexType;
            }
            else {
                complexType = schemaObject as XmlSchemaComplexType;
            }
            return complexType;
        }

        // SxS: This function calls ValidateElement on XmlSchemaValidator which is annotated with ResourceExposure attribute.
        // Since the resource names passed to ValidateElement method are null and the function does not expose any resources 
        // it is fine to supress the warning. 
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void ValidateSingleElement(XmlElement elementNode, bool skipToEnd, XmlSchemaInfo newSchemaInfo) {
            nsManager.PushScope();
            Debug.Assert(elementNode != null);
            
            XmlAttributeCollection attributes = elementNode.Attributes;
            XmlAttribute attr = null;

            //Find Xsi attributes that need to be processed before validating the element
            string xsiNil = null;
            string xsiType = null; 

            for (int i = 0; i < attributes.Count; i++) {
                attr = attributes[i];
                string objectNs = attr.NamespaceURI;
                string objectName = attr.LocalName;
                Debug.Assert(nameTable.Get(attr.NamespaceURI) != null);
                Debug.Assert(nameTable.Get(attr.LocalName) != null);

                if (Ref.Equal(objectNs, NsXsi)) {
                    if (Ref.Equal(objectName, XsiType)) {
                        xsiType = attr.Value;
                    }
                    else if (Ref.Equal(objectName, XsiNil)) {
                        xsiNil = attr.Value;
                    }
                }
                else if (Ref.Equal(objectNs,NsXmlNs)) {
                    nsManager.AddNamespace(attr.Prefix.Length == 0 ? string.Empty : attr.LocalName, attr.Value);
                }
            }
            validator.ValidateElement(elementNode.LocalName, elementNode.NamespaceURI, newSchemaInfo, xsiType, xsiNil, null, null);
            //Validate end of element
            if (skipToEnd) {
                validator.ValidateEndOfAttributes(newSchemaInfo);
                validator.SkipToEndElement(newSchemaInfo);
                nsManager.PopScope(); //Pop current namespace scope
            }
        }

        private void ValidateChildrenTillNextAncestor(XmlNode parentNode, XmlNode childToStopAt) {
            XmlNode child;

            for (child = parentNode.FirstChild; child != null; child = child.NextSibling) {
                if (child == childToStopAt) {
                    break;
                }
                switch (child.NodeType) {
                    case XmlNodeType.EntityReference:
                        ValidateChildrenTillNextAncestor(child, childToStopAt);
                        break;

                    case XmlNodeType.Element: //Flat validation, do not drill down into children
                        ValidateSingleElement(child as XmlElement, true, null);
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        validator.ValidateText(child.Value);
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        validator.ValidateWhitespace(child.Value);
                        break;

                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                        break;

                    default:
                        throw new InvalidOperationException( Res.GetString( Res.Xml_UnexpectedNodeType, new string[]{ currentNode.NodeType.ToString() } ) );
                }
            }
            Debug.Assert(child == childToStopAt);
        }

        private XmlSchemaValidator CreateTypeFinderValidator(XmlSchemaObject partialValidationType) {
            XmlSchemaValidator findTypeValidator = new XmlSchemaValidator(document.NameTable, document.Schemas, this.nsManager, XmlSchemaValidationFlags.None);
            findTypeValidator.ValidationEventHandler += new ValidationEventHandler(TypeFinderCallBack);
            if (partialValidationType != null) {
                findTypeValidator.Initialize(partialValidationType);
            }
            else { //If we walked up to the root and no schemaInfo was there, start validating from root 
                findTypeValidator.Initialize();
            }
            return findTypeValidator;
        }

        private void TypeFinderCallBack(object sender, ValidationEventArgs arg) {
            if (arg.Severity == XmlSeverityType.Error) {
                isPartialTreeValid = false;
            }
        }
        
        private void InternalValidationCallBack(object sender, ValidationEventArgs arg) {
            if (arg.Severity == XmlSeverityType.Error) {
                isValid = false;
            }
            XmlSchemaValidationException ex = arg.Exception as XmlSchemaValidationException;
            Debug.Assert(ex != null);
            ex.SetSourceObject(currentNode);
            if (this.eventHandler != null) { //Invoke user's event handler
                eventHandler(sender, arg);
            }
            else if (arg.Severity == XmlSeverityType.Error) {
                throw ex;
            }
        }

    }
}
