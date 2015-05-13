/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.


using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text;

namespace System.Management.Internal.CimXml
{
    internal class CimXmlWriter
    {
        #region nested enums / classes
        enum TagType : int
        {
            MultiReq,
            SimpleReq,
            MethodCall,
            IMethodCall,
            LocalInstancePath,
            LocalNamespacePath,
            Namespace,
            IParameterElement,
            ClassNameElement,
            InstanceElement,
            InstanceNameElement,
            NamedInstanceElement,
            KeyBindingElement,
            KeyValueElement,
            ValueArray,
            ValueReference,
            ValueString,
            Class,
            LocalClassPath,
            Method,
            Parameter,
            ParameterValue,
            Property,
            PropertyArray,
            PropertyReference,
            Qualifier,
            QualifierDeclaration,
            Propagated,
            NameAttributeString,
            ValueTypeAttributeString,
            ParameterTypeAttributeString,
            ClassNameAttributeString,
            ClassOriginAttributeString,
            SuperClassAttributeString,
            Overridable,            
            Translatable,
            ToSubClass,
            ToInstance,
            IsArray,
            Type,
            ElementValue,
            EndElement
        }

        class CimXmlOperation
        {
            TagType _tag;
            string _val;

            public CimXmlOperation(TagType tag)
                : this(tag, string.Empty)
            {
            }

            public CimXmlOperation(TagType tag, string val)
            {
                Tag = tag;
                Val = val;
            }

            public TagType Tag
            {
                get { return _tag; }
                set { _tag = value; }
            }

            public string Val
            {
                get { return _val; }
                set { _val = value; }
            }
        }
        #endregion

        int _messageID;
        ArrayList _body;
        XmlTextWriter _verifyXTW;
        private static int _nextMessageID = 1;

        #region Constructors
        public CimXmlWriter()
        {
            _messageID = _nextMessageID++;
            _body = new ArrayList();


            // This XmlTextWriter is to make sure that as XML elements are added, 
            // that they are still generating valid XML. It is NOT used for the 
            // actual creation of the XML. This is kind of hokie, but it's the only
            // way that I can continue to leave the "body" open after the ToString method 
            // is called (as you can't combine two XmlTextWriter objects.
            _verifyXTW = new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
            _verifyXTW.Formatting = Formatting.Indented;            
        }
        #endregion

        #region Properties and Indexers
        public int MessageID
        {
            get { return _messageID; }
            set { _messageID = value; }
        }
        #endregion

        #region Methods

        #region Xml Element / Attribute Writers
        public void WriteMultiReqElement()
        {
            _body.Add(new CimXmlOperation(TagType.MultiReq));
            _verifyXTW.WriteStartElement("MULTIREQ");
        }

        public void WriteSimpleReqElement()
        {
            _body.Add(new CimXmlOperation(TagType.SimpleReq));
            _verifyXTW.WriteStartElement("SIMPLEREQ");
        }

        public void WriteMethodCallElement()
        {
            _body.Add(new CimXmlOperation(TagType.MethodCall));
            _verifyXTW.WriteStartElement("METHODCALL");
        }


        public void WriteIMethodCallElement()
        {
            _body.Add(new CimXmlOperation(TagType.IMethodCall));
            _verifyXTW.WriteStartElement("IMETHODCALL");
        }

        public void WriteLocalInstancePathElement()
        {
            _body.Add(new CimXmlOperation(TagType.LocalInstancePath));
            _verifyXTW.WriteStartElement("LOCALINSTANCEPATH");
        }       

        public void WriteLocalNamespacePathElement()
        {
            _body.Add(new CimXmlOperation(TagType.LocalNamespacePath));
            _verifyXTW.WriteStartElement("LOCALNAMESPACEPATH");
        }

        public void WriteNamespaceElement()
        {
            _body.Add(new CimXmlOperation(TagType.Namespace));
            _verifyXTW.WriteStartElement("NAMESPACE");
        }

        public void WriteIParameterElement()
        {
            _body.Add(new CimXmlOperation(TagType.IParameterElement));      
            _verifyXTW.WriteStartElement("IPARAMVALUE");
        }

        public void WriteClassNameElement()
        {
            _body.Add(new CimXmlOperation(TagType.ClassNameElement));   
            _verifyXTW.WriteStartElement("CLASSNAME");
        }

        public void WriteInstanceElement()
        {            
            _body.Add(new CimXmlOperation(TagType.InstanceElement));
            _verifyXTW.WriteStartElement("INSTANCE");
        }
        public void WriteClassElement()
        {
            _body.Add(new CimXmlOperation(TagType.Class));
            _verifyXTW.WriteStartElement("CLASS");
        }

        public void WriteMethodElement()
        {
            _body.Add(new CimXmlOperation(TagType.Method));
            _verifyXTW.WriteStartElement("METHOD");
        }

        public void WriteParameterElement()
        {
            _body.Add(new CimXmlOperation(TagType.Parameter));
            _verifyXTW.WriteStartElement("PARAMETER");
        }

        public void WriteParameterValueElement()
        {
            _body.Add(new CimXmlOperation(TagType.ParameterValue));
            _verifyXTW.WriteStartElement("PARAMVALUE");
        }


        public void WriteInstanceNameElement()
        {
            _body.Add(new CimXmlOperation(TagType.InstanceNameElement));
            _verifyXTW.WriteStartElement("INSTANCENAME");
        }

        public void WriteNamedInstanceElement()
        {
            _body.Add(new CimXmlOperation(TagType.NamedInstanceElement));
            _verifyXTW.WriteStartElement("VALUE.NAMEDINSTANCE");
        }

        public void WriteKeyBindingElement()
        {
            _body.Add(new CimXmlOperation(TagType.KeyBindingElement));
            _verifyXTW.WriteStartElement("KEYBINDING");
        }

        public void WriteKeyValueElement()
        {
            _body.Add(new CimXmlOperation(TagType.KeyValueElement));
            _verifyXTW.WriteStartElement("KEYVALUE");            
        }

        public void WriteValueArrayElement()
        {
            _body.Add(new CimXmlOperation(TagType.ValueArray));
            _verifyXTW.WriteStartElement("VALUE.ARRAY");
        }

        public void WriteValueReferenceElement()
        {
            _body.Add(new CimXmlOperation(TagType.ValueReference));
            _verifyXTW.WriteStartElement("VALUE.REFERENCE");
        }        

        public void WriteValueString(string value)
        {
            _body.Add(new CimXmlOperation(TagType.ValueString,value));         
            _verifyXTW.WriteElementString("VALUE", value);
        }

        public void WriteCimNameAttributeString(CimName name)
        {
            //_body.Add(new CimXmlOperation(TagType.NameAttributeString, name.ToString()));
            //_verifyXTW.WriteAttributeString("NAME", name.ToString());
            WriteNameAttributeString(name.ToString());
        }

        public void WriteCimClassPath(CimName className, CimName ns)
        {
            _body.Add(new CimXmlOperation(TagType.LocalClassPath));
            _verifyXTW.WriteStartElement("LOCALCLASSPATH");

            WriteCimNamespace(ns);
            WriteClassName(className);

            _body.Add(new CimXmlOperation(TagType.EndElement));
            _verifyXTW.WriteEndElement();
        }

        public void WriteCimInstancePath(CimInstanceName instanceName, CimName ns)
        {
            _body.Add(new CimXmlOperation(TagType.LocalInstancePath));
            _verifyXTW.WriteStartElement("LOCALINSTANCEPATH");

            WriteCimNamespace(ns);
            WriteCimInstanceName(instanceName);

            _body.Add(new CimXmlOperation(TagType.EndElement));
            _verifyXTW.WriteEndElement();
        }
        

        public void WriteNameAttributeString(CimName name)
        {
            _body.Add(new CimXmlOperation(TagType.NameAttributeString, name.ToString()));
            _verifyXTW.WriteAttributeString("NAME", name.ToString());
        }

        public void WriteParameterTypeAttributeString(string paramType)
        {
            _body.Add(new CimXmlOperation(TagType.ParameterTypeAttributeString, paramType.ToLower()));
            _verifyXTW.WriteAttributeString("PARAMTYPE", paramType.ToLower());
        }

        public void WriteValueTypeAttributeString(string valueType)
        {
            _body.Add(new CimXmlOperation(TagType.ValueTypeAttributeString, valueType));
            _verifyXTW.WriteAttributeString("VALUETYPE", valueType);
        }

        public void WriteClassNameAttributeString(CimName name)
        {
            _body.Add(new CimXmlOperation(TagType.ClassNameAttributeString, name.ToString()));
            _verifyXTW.WriteAttributeString("CLASSNAME", name.ToString());
        }

        public void WriteClassOriginAttributeString(CimName name)
        {
            _body.Add(new CimXmlOperation(TagType.ClassOriginAttributeString, name.ToString()));
            _verifyXTW.WriteAttributeString("CLASSORIGIN", name.ToString());
        }
        public void WriteSuperClassAttributeString(CimName name)
        {
            _body.Add(new CimXmlOperation(TagType.SuperClassAttributeString, name.ToString()));
            _verifyXTW.WriteAttributeString("SUPERCLASS", name.ToString());
        }
        public void WriteIsArrayAttributeString(bool isArray)
        {
            _body.Add(new CimXmlOperation(TagType.IsArray, isArray.ToString()));
            _verifyXTW.WriteAttributeString("ISARRAY", isArray.ToString());
        }

        public void WritePropertyElement()
        {
            _body.Add(new CimXmlOperation(TagType.Property));
            _verifyXTW.WriteStartElement("PROPERTY");
        }

        public void WritePropertyArrayElement()
        {
            _body.Add(new CimXmlOperation(TagType.Property));
            _verifyXTW.WriteStartElement("PROPERTY.ARRAY");
        }

        public void WritePropertyReferenceElement()
        {
            _body.Add(new CimXmlOperation(TagType.Property));
            _verifyXTW.WriteStartElement("PROPERTY.REFERENCE");
        }

        public void WriteQualifierElement()
        {
            _body.Add(new CimXmlOperation(TagType.Qualifier));
            _verifyXTW.WriteStartElement("QUALIFIER");
        }
        public void WriteQualifierDeclarationElement()
        {
            _body.Add(new CimXmlOperation(TagType.QualifierDeclaration));
            _verifyXTW.WriteStartElement("QUALIFIER.DECLARATION");
        }

        public void WriteTypeAttribute(CimType type)
        {
            _body.Add(new CimXmlOperation(TagType.Type, type.ToString()));
            _verifyXTW.WriteAttributeString("TYPE", type.ToString());
        }

        public void WritePropagatedAttribute(bool isPropagated)
        {
            _body.Add(new CimXmlOperation(TagType.Propagated, isPropagated.ToString()));
            _verifyXTW.WriteAttributeString("PROPAGATED", isPropagated.ToString());
        }

        public void WriteOverridableAttribute(bool overridable)
        {
            _body.Add(new CimXmlOperation(TagType.Overridable, overridable.ToString()));
            _verifyXTW.WriteAttributeString("OVERRIDABLE", overridable.ToString());
        }

        public void WriteToSubClassAttribute(bool toSubClass)
        {
            _body.Add(new CimXmlOperation(TagType.ToSubClass, toSubClass.ToString()));
            _verifyXTW.WriteAttributeString("TOSUBCLASS", toSubClass.ToString());
        }

        public void WriteTranslatableAttribute(bool translatable)
        {
            _body.Add(new CimXmlOperation(TagType.Translatable, translatable.ToString()));
            _verifyXTW.WriteAttributeString("TRANSLATABLE", translatable.ToString());
        }

        public void WriteToInstanceAttribute(bool toInstance)
        {
            _body.Add(new CimXmlOperation(TagType.ToInstance, toInstance.ToString()));
            _verifyXTW.WriteAttributeString("TOINSTANCE", toInstance.ToString());
        }

        public void WriteElementValue(string value)
        {
            _body.Add(new CimXmlOperation(TagType.ElementValue, value));
            _verifyXTW.WriteValue(value);
        }

        public void WriteEndElement()
        {
            _body.Add(new CimXmlOperation(TagType.EndElement));            
            _verifyXTW.WriteEndElement();
        }
        #endregion

        #region Intelligent Writers

        #region Bool Writers
        private void WriteBool(CimName name, bool value)
        {
            this.WriteIParameterElement();
            this.WriteCimNameAttributeString(name);
            if (value)
                this.WriteValueString("TRUE");
            else
                this.WriteValueString("FALSE");
            this.WriteEndElement();
        }

        public void WriteIncludeClassOrigin(bool includeClassOrigin)
        {            
            this.WriteBool("IncludeClassOrigin", includeClassOrigin);
        }

        public void WriteIncludeQualifiers(bool includeQualifiers)
        {
            this.WriteBool("IncludeQualifiers", includeQualifiers);
        }

        public void WriteLocalOnly(bool localOnly)
        {
            this.WriteBool("LocalOnly", localOnly);
        }

        public void WriteDeepInheritance(bool deepInheritance)
        {
            this.WriteBool("DeepInheritance", deepInheritance);
        }        
        #endregion

        #region String Writers
        public void WriteCimNamespace(CimName LocalNamespacePath)
        {
            //<LOCALNAMESPACEPATH> 
            this.WriteLocalNamespacePathElement();

            foreach (string curPart in LocalNamespacePath.ToString().Split('/'))
            {
                //	<NAMESPACE NAME="$curPart"/>
                this.WriteNamespaceElement();
                this.WriteCimNameAttributeString(curPart);
                this.WriteEndElement();
            }

            //</LOCALNAMESPACEPATH>             
            this.WriteEndElement();
        }

        public void WritePropertyList(string[] propertyList)
        {
            if (propertyList != null)
            {
                this.WriteIParameterElement();
                this.WriteCimNameAttributeString("PropertyList");
                this.WriteValueArrayElement();

                foreach (string curStr in propertyList)
                {
                    this.WriteValueString(curStr);
                }

                this.WriteEndElement();
                this.WriteEndElement();
            }
        }

        public void WriteMethodCallStartElement(CimName methodName)
        {
            this.WriteMethodCallElement();
            this.WriteCimNameAttributeString(methodName);
        }

        public void WriteCimObjectName(ICimObjectName objectName)
        {
            this.WriteIParameterElement();
            this.WriteNameAttributeString("ObjectName");
            if (objectName is CimName)
            {
                this.WriteClassName((CimName)objectName);
            }
            else if (objectName is CimInstanceName)
            {
                this.WriteCimInstanceName((CimInstanceName)objectName);
            }
            else
            {
                throw new Exception("Not implemented yet");
            }
            this.WriteEndElement();//</IPARAMVALUE>
        }
        public void WriteCimObjectName(ICimObjectNameSettings settings)
        {
           // this.WriteIParameterElement();
            //this.WriteNameAttributeString("ObjectName");

            this.WriteCimObjectName(settings.ObjectName);
            
            //this.WriteEndElement();//</IPARAMVALUE>
        }
        //public void WriteCimObjectName(ICimObject settings)
        //{
        //    this.WriteIParameterElement();
        //    this.WriteNameAttributeString("ObjectName");

        //    this.WriteCimInstanceName(settings.InstanceName);

        //    this.WriteEndElement();//</IPARAMVALUE>
        //}
        //public void WriteCimObjectName(AssociatorNamesOpSettings settings)
        //{
        //    this.WriteIParameterElement();
        //    this.WriteNameAttributeString("ObjectName");
        //    if (settings is AssociatorNamesWithClassNameOpSettings)
        //    {
        //        this.WriteClassName(((AssociatorNamesWithClassNameOpSettings)settings).ClassName);
        //    }
        //    else if (settings is AssociatorNamesWithInstanceNameOpSettings)
        //    {
        //        this.WriteCimInstanceName(((AssociatorNamesWithInstanceNameOpSettings)settings).InstanceName);
        //    }
        //    else
        //    {
        //        throw new Exception("Not implemented yet");
        //    }
        //    //if (objectName is CimName)
        //    //{
        //    //    this.WriteClassName((CimName)objectName);
        //    //}
        //    //else if (objectName is CimInstanceName)
        //    //{
        //    //    this.WriteCimInstanceName((CimInstanceName)objectName);
        //    //}
        //    //else
        //    //{
        //    //    throw new Exception("Not implemented yet");
        //    //}
        //    this.WriteEndElement();//</IPARAMVALUE>
        //}
        //public void WriteCimObjectName(AssociatorsOpSettings settings)
        //{
        //    this.WriteIParameterElement();
        //    this.WriteNameAttributeString("ObjectName");
        //    if (settings is AssociatorsWithClassNameOpSettings)
        //    {
        //        this.WriteClassName(((AssociatorsWithClassNameOpSettings)settings).ClassName);
        //    }
        //    else if (settings is AssociatorsWithInstanceNameOpSettings)
        //    {
        //        this.WriteCimInstanceName(((AssociatorsWithInstanceNameOpSettings)settings).InstanceName);
        //    }
        //    else
        //    {
        //        throw new Exception("Not implemented yet");
        //    }
        //}

        public void WriteIMethodCallStartElement(CimName methodName)
        {
            this.WriteIMethodCallElement();
            this.WriteCimNameAttributeString(methodName);
        }

        public void WriteClassNameParam(CimName className)
        {
            this.WriteIParameterElement();
            this.WriteCimNameAttributeString("ClassName");
            //this.WriteClassNameElement();
            //this.WriteCimNameAttributeString(className);
            //this.WriteEndElement();
            this.WriteClassName(className);
            this.WriteEndElement();
        }
        public void WriteClassName(CimName className)
        {
            this.WriteClassNameElement();
            this.WriteCimNameAttributeString(className.ToString());
            this.WriteEndElement();
        }
        #endregion

        #region Other Writers
        public void WriteCimInstanceNamePath(CimInstanceNamePath instanceNamePath)
        {
            #region DTD
            /*
             <!ELEMENT LOCALINSTANCEPATH (LOCALNAMESPACEPATH,INSTANCENAME)>
            */
            #endregion

            #region Actual XML Request
            /*
            [...]
            <LOCALINSTANCEPATH>
                <LOCALNAMESPACEPATH>
                    <NAMESPACE NAME="smash"/>
                </LOCALNAMESPACEPATH>
                <INSTANCENAME CLASSNAME="OMC_UnitaryComputerSystem">
                    <KEYBINDING NAME="CreationClassName">
                        <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
                    </KEYBINDING>
                    <KEYBINDING NAME="Name">
                        <KEYVALUE VALUETYPE="string">d1850.cim.lab.novell.com</KEYVALUE>
                    </KEYBINDING>
                </INSTANCENAME>
            </LOCALINSTANCEPATH>
            */
            #endregion

            // <LOCALINSTANCEPATH>
            this.WriteLocalInstancePathElement();

            /* 
            <LOCALNAMESPACEPATH>
                <NAMESPACE NAME="smash"/>
            </LOCALNAMESPACEPATH>
            */
            this.WriteCimNamespace(instanceNamePath.Namespace);

            // <INSTANCENAME CLASSNAME="OMC_UnitaryComputerSystem">
            // [...]
            // </INSTANCENAME>

            this.WriteCimInstanceName(instanceNamePath.InstanceName);

            // </LOCALINSTANCEPATH>
            this.WriteEndElement();
        }

        public void WriteCimValueReference(CimValueReference valRef)
        {
            // <VALUE.REFERENCE>
            this.WriteValueReferenceElement();            
            switch (valRef.Type)
            {
                case CimValueReference.RefType.InstanceNamePath:
                    // <LOCALINSTANCEPATH>                            
                    this.WriteCimInstanceNamePath((CimInstanceNamePath)valRef.CimObject);
                    break;

                default:
                    throw (new Exception("Not Implemented Yet"));
            }

            // </VALUE.REFERENCE>
            this.WriteEndElement();
        }

        public void WriteCimKeyValue(CimKeyValue keyValue)
        {
            // <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>            
            this.WriteKeyValueElement();            
            this.WriteValueTypeAttributeString(keyValue.ValueType);
            this.WriteElementValue(keyValue.Value);
            this.WriteEndElement();
        }

        public void WriteCimKeyBinding(CimKeyBinding keyBinding)
        {
            #region Actual XML Request
            /*
            [...]
            <KEYBINDING NAME="GroupComponent">
                <VALUE.REFERENCE>
                    <LOCALINSTANCEPATH>
                    [...]
                    </LOCALINSTANCEPATH>
                </VALUE.REFERENCE>
            </KEYBINDING>
            [...]

            ---OR---

            <KEYBINDING NAME="CreationClassName">
                <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
            </KEYBINDING>
            */
            #endregion

            // <KEYBINDING NAME="GroupComponent">
            this.WriteKeyBindingElement();
            this.WriteCimNameAttributeString(keyBinding.Name);


            switch (keyBinding.Type)
            {
                case CimKeyBinding.RefType.ValueReference:
                    // <VALUE.REFERENCE>
                    this.WriteCimValueReference((CimValueReference)keyBinding.Value);
                    break;

                case CimKeyBinding.RefType.KeyValue:                    
                    // <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
                    WriteCimKeyValue((CimKeyValue)keyBinding.Value);
                    break;

                default:
                    throw(new Exception("Not Implemented Yet"));
            }

            // </KEYBINDING>
            this.WriteEndElement();
        }

        public void WriteCimValueList(CimValueList valueList)
        {
            #region Actual XML Request
            /*
            [...]            
            <VALUE.ARRAY>
	            <VALUE>CIM_FileShare</VALUE>
            </VALUE.ARRAY>
            [...]
            */
            #endregion

            switch (valueList.Count)
            {
                case 0:
                    // <VALUE></VALUE>
                    this.WriteValueString(string.Empty);
                    break;

                case 1:
                    this.WriteValueString(valueList[0]);
                    break;

                default:
                    this.WriteValueArrayElement();          // <VALUE.ARRAY>

                    for(int i = 0; i < valueList.Count; ++i)
                    {
                    	this.WriteValueString(valueList[i]);
                    }
                                        
                    this.WriteEndElement();                 // </VALUE.ARRAY>                    
                    break;
            }
        }

        public void WriteCimQualifier(CimQualifier qualifier)
        {
            #region Actual XML Request
            /*
            [...]
            <QUALIFIER NAME="Deprecated" TYPE="string" TOSUBCLASS="false" >
	            <VALUE.ARRAY>
		            <VALUE>CIM_FileShare</VALUE>
	            </VALUE.ARRAY>
            </QUALIFIER>
            [...]
            */

        /* <!ELEMENT QUALIFIER ((VALUE|VALUE.ARRAY)?)>
         * <!ATTLIST QUALIFIER 
         *      %CIMName; 
         *      %CIMType; #REQUIRED 
         *      %Propagated; 
         *      %QualifierFlavor; 
         *      xml:lang NMTOKEN #IMPLIED>
         * */
            #endregion
            
            WriteQualifierElement();
            if (qualifier.Name.IsSet)
                WriteCimNameAttributeString(qualifier.Name);
            if (qualifier.Type.IsSet)
                WriteTypeAttribute(qualifier.Type.ToCimType());

            if (qualifier.IsPropagated.IsSet)
                WritePropagatedAttribute(qualifier.IsPropagated.ToBool());

            WriteCimQualifierFlavorAttributes(qualifier.Flavor);

            // This could be a problem: 
            // How do we know if the qual has a single value, or a Value.Array with a single value?
            // Or does this matter?
            // Solution: Have member bool HasValueArray that we can check for this.
            if (qualifier.Values.Count != 0)
            {
                if (qualifier.HasValueArray)
                    WriteCimValueList(qualifier.Values);
                else
                    WriteValueString(qualifier.Values[0]);//only the first value in the list
            }

            this.WriteEndElement();

        }

        public void WriteCimQualifierList(CimQualifierList qualifierList)
        {
            #region Actual XML Request
            /*
            [...]
            <QUALIFIER NAME="Deprecated" TYPE="string" TOSUBCLASS="false" >
	            <VALUE.ARRAY>
		            <VALUE>CIM_FileShare</VALUE>
	            </VALUE.ARRAY>
            </QUALIFIER>
            <QUALIFIER NAME="Version" TYPE="string" TOSUBCLASS="false" TRANSLATABLE="true" >
	            <VALUE>2.6.0</VALUE>
            </QUALIFIER>
            [...]
            */
            #endregion
            if (qualifierList == null)
                return;

            //Changing to for loop for MONO
            for (int i = 0; i < qualifierList.Count; ++i)
            {
            	WriteCimQualifier(qualifierList[i]);
            }

        }

        public void WriteCimQualifierFlavorAttributes(CimQualifierFlavor flavor)
        {
            if (!flavor.IsSet)
                return;

            if (flavor.Overridable.IsSet)
                WriteOverridableAttribute(flavor.Overridable.ToBool());
            if (flavor.ToInstance.IsSet)
                WriteOverridableAttribute(flavor.ToInstance.ToBool());
            if (flavor.ToSubClass.IsSet)
                WriteOverridableAttribute(flavor.ToSubClass.ToBool());
            if (flavor.Translatable.IsSet)
                WriteOverridableAttribute(flavor.Translatable.ToBool());

        }
        public void WriteCimQualifierDeclaration(CimQualifierDeclaration qualifierDeclaration)
        {
            #region Actual Xml Response
            /* 
			<QUALIFIER.DECLARATION NAME="Version" 
                TYPE="string" 
                ISARRAY="false" 
                TOSUBCLASS="false" 
                TRANSLATABLE="true" >
				    <SCOPE CLASS="true" ASSOCIATION="true" INDICATION="true"></SCOPE>
			</QUALIFIER.DECLARATION>
             * */
            #endregion

            this.WriteQualifierDeclarationElement();
            this.WriteNameAttributeString(qualifierDeclaration.Name);
            this.WriteTypeAttribute(qualifierDeclaration.Type.ToCimType());
            if (qualifierDeclaration.IsArray.IsSet)
                WriteIsArrayAttributeString(qualifierDeclaration.IsArray.ToBool());

            WriteCimQualifierFlavorAttributes(qualifierDeclaration.QualifierFlavor);
            if (qualifierDeclaration.Scope != null)
            {
                WriteCimScope(qualifierDeclaration.Scope);
            }

            WriteEndElement();
        }
        public void WriteCimScope(CimScope scope)
        {
            throw new Exception("Not implemented yet");
        }
      
        public void WriteCimProperty(CimProperty property)
        {
            #region Actual XML Request
            /*            
            <PROPERTY NAME="CSCreationClassName" TYPE="string" CLASSORIGIN="CIM_FileSystem" PROPAGATED="true" >
	            <QUALIFIER NAME="Key" TYPE="boolean" OVERRIDABLE="false" >
		            <VALUE>true</VALUE>
	            </QUALIFIER>
                [...]
	            <VALUE>tCSCreationClassName</VALUE>
            </PROPERTY>            
             
                        -or-
             
            <PROPERTY.ARRAY NAME="OperationalStatus" TYPE="uint16" CLASSORIGIN="CIM_ManagedSystemElement" PROPAGATED="true" >
	            <QUALIFIER NAME="Description" TYPE="string" TRANSLATABLE="true" >
		            <VALUE>Indicates the current statuses of the element. Various operational statuses are defined. Many of the enumeration&apos;s values are self-explanatory. However, a few are not and are described here in more detail. &quot;Stressed&quot; indicates that the element is functioning, but needs attention. Examples of &quot;Stressed&quot; states are overload, overheated, and so on. &quot;Predictive Failure&quot; indicates that an element is functioning nominally but predicting a failure in the near future. &quot;In Service&quot; describes an element being configured, maintained, cleaned, or otherwise administered. &quot;No Contact&quot; indicates that the monitoring system has knowledge of this element, but has never been able to establish communications with it. &quot;Lost Communication&quot; indicates that the ManagedSystem Element is known to exist and has been contacted successfully in the past, but is currently unreachable. &quot;Stopped&quot; and &quot;Aborted&quot; are similar, although the former implies a clean and orderly stop, while the latter implies an abrupt stop where the state and configuration of the element might need to be updated. &quot;Dormant&quot; indicates that the element is inactive or quiesced. &quot;Supporting Entity in Error&quot; indicates that this element might be &quot;OK&quot; but that another element, on which it is dependent, is in error. An example is a network service or endpoint that cannot function due to lower-layer networking problems. &quot;Completed&quot; indicates that the element has completed its operation. This value should be combined with either OK, Error, or Degraded so that a client can tell if the complete operation Completed with OK (passed), Completed with Error (failed), or Completed with Degraded (the operation finished, but it did not complete OK or did not report an error). &quot;Power Mode&quot; indicates that the element has additional power model information contained in the Associated PowerManagementService association. OperationalStatus replaces the Status property on ManagedSystemElement to provide a consistent approach to enumerations, to address implementation needs for an array property, and to provide a migration path from today&apos;s environment to the future. This change was not made earlier because it required the deprecated qualifier. Due to the widespread use of the existing Status property in management applications, it is strongly recommended that providers or instrumentation provide both the Status and OperationalStatus properties. Further, the first value of OperationalStatus should contain the primary status for the element. When instrumented, Status (because it is single-valued) should also provide the primary status of the element.</VALUE>
	            </QUALIFIER>
                [...]
	            <VALUE.ARRAY>
		            <VALUE></VALUE>
	            </VALUE.ARRAY>
            </PROPERTY.ARRAY>
            */
            #endregion
            WritePropertyElement();

            if (property.Name.IsSet)
                WriteCimNameAttributeString(property.Name);

            if (property.Type.IsSet)
                WriteTypeAttribute(property.Type.ToCimType());

            if (property.ClassOrigin.IsSet)
                WriteClassOriginAttributeString(property.ClassOrigin);

            if (property.IsPropagated.IsSet)
                WritePropagatedAttribute(property.IsPropagated.ToBool());


            WriteCimQualifierList(property.Qualifiers);

            if (property.Value != string.Empty)
                WriteValueString(property.Value);

            this.WriteEndElement();
            
        }

        public void WriteCimPropertyList(CimPropertyList propertyList)
        {
            #region Actual XML Request
            /*
            [...]
            <PROPERTY NAME="CSCreationClassName" TYPE="string" CLASSORIGIN="CIM_FileSystem" PROPAGATED="true" >
	            <QUALIFIER NAME="Key" TYPE="boolean" OVERRIDABLE="false" >
		            <VALUE>true</VALUE>
	            </QUALIFIER>
                [...]
	            <VALUE>tCSCreationClassName</VALUE>
            </PROPERTY>
            [...]
            <PROPERTY NAME="Root" TYPE="string" CLASSORIGIN="CIM_FileSystem" PROPAGATED="true" >
	            <QUALIFIER NAME="Description" TYPE="string" TRANSLATABLE="true" >
		            <VALUE>Path name or other information defining the root of the FileSystem.</VALUE>
	            </QUALIFIER>
                [...]
            </PROPERTY>
            [...]
            <PROPERTY.ARRAY NAME="OperationalStatus" TYPE="uint16" CLASSORIGIN="CIM_ManagedSystemElement" PROPAGATED="true" >
	            <QUALIFIER NAME="Description" TYPE="string" TRANSLATABLE="true" >
		            <VALUE>Indicates the current statuses of the element. Various operational statuses are defined. Many of the enumeration&apos;s values are self-explanatory. However, a few are not and are described here in more detail. &quot;Stressed&quot; indicates that the element is functioning, but needs attention. Examples of &quot;Stressed&quot; states are overload, overheated, and so on. &quot;Predictive Failure&quot; indicates that an element is functioning nominally but predicting a failure in the near future. &quot;In Service&quot; describes an element being configured, maintained, cleaned, or otherwise administered. &quot;No Contact&quot; indicates that the monitoring system has knowledge of this element, but has never been able to establish communications with it. &quot;Lost Communication&quot; indicates that the ManagedSystem Element is known to exist and has been contacted successfully in the past, but is currently unreachable. &quot;Stopped&quot; and &quot;Aborted&quot; are similar, although the former implies a clean and orderly stop, while the latter implies an abrupt stop where the state and configuration of the element might need to be updated. &quot;Dormant&quot; indicates that the element is inactive or quiesced. &quot;Supporting Entity in Error&quot; indicates that this element might be &quot;OK&quot; but that another element, on which it is dependent, is in error. An example is a network service or endpoint that cannot function due to lower-layer networking problems. &quot;Completed&quot; indicates that the element has completed its operation. This value should be combined with either OK, Error, or Degraded so that a client can tell if the complete operation Completed with OK (passed), Completed with Error (failed), or Completed with Degraded (the operation finished, but it did not complete OK or did not report an error). &quot;Power Mode&quot; indicates that the element has additional power model information contained in the Associated PowerManagementService association. OperationalStatus replaces the Status property on ManagedSystemElement to provide a consistent approach to enumerations, to address implementation needs for an array property, and to provide a migration path from today&apos;s environment to the future. This change was not made earlier because it required the deprecated qualifier. Due to the widespread use of the existing Status property in management applications, it is strongly recommended that providers or instrumentation provide both the Status and OperationalStatus properties. Further, the first value of OperationalStatus should contain the primary status for the element. When instrumented, Status (because it is single-valued) should also provide the primary status of the element.</VALUE>
	            </QUALIFIER>
                [...]
	            <VALUE.ARRAY>
		            <VALUE></VALUE>
	            </VALUE.ARRAY>
            </PROPERTY.ARRAY>
            [...]
            */
            #endregion

            if (propertyList == null)
                return;

            //Changing to for loop for MONO
            for(int i = 0; i < propertyList.Count; ++i)
            {
            	WriteCimProperty(propertyList[i]);
            }
        }

        public void WriteCimInstanceName(CimInstanceName instanceName)
        {
            #region Actual XML Request
            /*
            [...]
            <IPARAMVALUE NAME="InstanceName">
                <INSTANCENAME CLASSNAME="OMC_InstalledOS">
                    <KEYBINDING NAME="GroupComponent">
                        <VALUE.REFERENCE>
                            <LOCALINSTANCEPATH>
                                <LOCALNAMESPACEPATH>
                                    <NAMESPACE NAME="smash"/>
                                </LOCALNAMESPACEPATH>
                                <INSTANCENAME CLASSNAME="OMC_UnitaryComputerSystem">
                                    <KEYBINDING NAME="CreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="Name">
                                        <KEYVALUE VALUETYPE="string">d1850.cim.lab.novell.com</KEYVALUE>
                                    </KEYBINDING>
                                </INSTANCENAME>
                            </LOCALINSTANCEPATH>
                        </VALUE.REFERENCE>
                    </KEYBINDING>
            */
            #region 2nd KeyBinding
            /*
                    <KEYBINDING NAME="PartComponent">
                        <VALUE.REFERENCE>
                            <LOCALINSTANCEPATH>
                                <LOCALNAMESPACEPATH>
                                    <NAMESPACE NAME="smash"/>
                                </LOCALNAMESPACEPATH>
                                <INSTANCENAME CLASSNAME="OMC_OperatingSystem">
                                    <KEYBINDING NAME="CSName">
                                        <KEYVALUE VALUETYPE="string">d1850.cim.lab.novell.com</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="CreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_OperatingSystem</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="Name">
                                        <KEYVALUE VALUETYPE="string">Linux</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="CSCreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
                                    </KEYBINDING>
                                </INSTANCENAME>
                            </LOCALINSTANCEPATH>
                        </VALUE.REFERENCE>
                    </KEYBINDING>
            */
                    #endregion
            /*
                </INSTANCENAME>
            </IPARAMVALUE>
            [...]
            */
            #endregion

            //// <IPARAMVALUE NAME="InstanceName">
            //this.WriteIParameterElement();
            //this.WriteNameAttributeString("InstanceName");

            // <INSTANCENAME CLASSNAME="OMC_InstalledOS">
            this.WriteInstanceNameElement();
            this.WriteClassNameAttributeString(instanceName.ClassName);

            if ( (instanceName.KeyBindings.Count == 1) && 
                 (instanceName.KeyBindings[0].Type == CimKeyBinding.RefType.KeyValue) &&
                 (instanceName.KeyBindings[0].Name == "KeyValue"))
            {
                WriteCimKeyValue((CimKeyValue)instanceName.KeyBindings[0].Value);
            }
            else if ((instanceName.KeyBindings.Count == 1) &&
                     (instanceName.KeyBindings[0].Type == CimKeyBinding.RefType.ValueReference) &&
                     (instanceName.KeyBindings[0].Name == "ValueReference"))
            {
                WriteCimValueReference((CimValueReference)instanceName.KeyBindings[0].Value);
            }
            else
            {
            	//Chaning for MONO
            	for(int i = 0; i < instanceName.KeyBindings.Count;++i)
                {
            		WriteCimKeyBinding(instanceName.KeyBindings[i]);
                }             
            }

            //     </INSTANCENAME>
            this.WriteEndElement();

            ////  </IPARAMVALUE>
            //this.WriteEndElement();
        }

        public void WriteCimInstance(CimInstance namedInstance)
        {
            //<VALUE.NAMEDINSTANCE>
            this.WriteNamedInstanceElement();
            this.WriteCimInstanceName(namedInstance.InstanceName);
            this.WriteUnnamedCimInstance(namedInstance);

            //</VALUE.NAMEDINSTANCE>
            this.WriteEndElement();
        }

        public void WriteUnnamedCimInstance(CimInstance instance)
        {
            #region Actual XML Request
            /*
            [...]
            <INSTANCE CLASSNAME="CIM_NFS">
	            <QUALIFIER NAME="Deprecated" TYPE="string" TOSUBCLASS="false" >
		            <VALUE.ARRAY>
			            <VALUE>CIM_FileShare</VALUE>
		            </VALUE.ARRAY>
	            </QUALIFIER>
	            <QUALIFIER NAME="Version" TYPE="string" TOSUBCLASS="false" TRANSLATABLE="true" >
		            <VALUE>2.6.0</VALUE>
	            </QUALIFIER>
                [...]
	            <PROPERTY NAME="CSCreationClassName" TYPE="string" CLASSORIGIN="CIM_FileSystem" PROPAGATED="true" >
		            <QUALIFIER NAME="Key" TYPE="boolean" OVERRIDABLE="false" >
			            <VALUE>true</VALUE>
		            </QUALIFIER>
                    [...]
		            <VALUE>tCSCreationClassName</VALUE>
	            </PROPERTY>
                [...]
	            <PROPERTY NAME="Root" TYPE="string" CLASSORIGIN="CIM_FileSystem" PROPAGATED="true" >
		            <QUALIFIER NAME="Description" TYPE="string" TRANSLATABLE="true" >
			            <VALUE>Path name or other information defining the root of the FileSystem.</VALUE>
		            </QUALIFIER>
                    [...]
	            </PROPERTY>
                [...]
	            <PROPERTY.ARRAY NAME="OperationalStatus" TYPE="uint16" CLASSORIGIN="CIM_ManagedSystemElement" PROPAGATED="true" >
		            <QUALIFIER NAME="Description" TYPE="string" TRANSLATABLE="true" >
			            <VALUE>Indicates the current statuses of the element. Various operational statuses are defined. Many of the enumeration&apos;s values are self-explanatory. However, a few are not and are described here in more detail. &quot;Stressed&quot; indicates that the element is functioning, but needs attention. Examples of &quot;Stressed&quot; states are overload, overheated, and so on. &quot;Predictive Failure&quot; indicates that an element is functioning nominally but predicting a failure in the near future. &quot;In Service&quot; describes an element being configured, maintained, cleaned, or otherwise administered. &quot;No Contact&quot; indicates that the monitoring system has knowledge of this element, but has never been able to establish communications with it. &quot;Lost Communication&quot; indicates that the ManagedSystem Element is known to exist and has been contacted successfully in the past, but is currently unreachable. &quot;Stopped&quot; and &quot;Aborted&quot; are similar, although the former implies a clean and orderly stop, while the latter implies an abrupt stop where the state and configuration of the element might need to be updated. &quot;Dormant&quot; indicates that the element is inactive or quiesced. &quot;Supporting Entity in Error&quot; indicates that this element might be &quot;OK&quot; but that another element, on which it is dependent, is in error. An example is a network service or endpoint that cannot function due to lower-layer networking problems. &quot;Completed&quot; indicates that the element has completed its operation. This value should be combined with either OK, Error, or Degraded so that a client can tell if the complete operation Completed with OK (passed), Completed with Error (failed), or Completed with Degraded (the operation finished, but it did not complete OK or did not report an error). &quot;Power Mode&quot; indicates that the element has additional power model information contained in the Associated PowerManagementService association. OperationalStatus replaces the Status property on ManagedSystemElement to provide a consistent approach to enumerations, to address implementation needs for an array property, and to provide a migration path from today&apos;s environment to the future. This change was not made earlier because it required the deprecated qualifier. Due to the widespread use of the existing Status property in management applications, it is strongly recommended that providers or instrumentation provide both the Status and OperationalStatus properties. Further, the first value of OperationalStatus should contain the primary status for the element. When instrumented, Status (because it is single-valued) should also provide the primary status of the element.</VALUE>
		            </QUALIFIER>
                    [...]
		            <VALUE.ARRAY>
			            <VALUE></VALUE>
		            </VALUE.ARRAY>
	            </PROPERTY.ARRAY>
                [...]
            </INSTANCE>
            */
            #endregion

            this.WriteInstanceElement();
            this.WriteClassNameAttributeString(instance.ClassName);

            this.WriteCimQualifierList(instance.Qualifiers);
            this.WriteCimPropertyList(instance.Properties);


            this.WriteEndElement();
        }

        public void WriteNewInstance()
        {
            //<IPARAMVALUE NAME="NewInstance">
            this.WriteIParameterElement();
            this.WriteCimNameAttributeString("NewInstance");

        }

        public void WriteCimClass(CimClass _class)
        {
            //<CLASS NAME="CIM_NFS"SUPERCLASS="CIM_RemoteFileSystem">
            
            this.WriteClassElement();
            this.WriteCimNameAttributeString(_class.ClassName);
            this.WriteSuperClassAttributeString(_class.SuperClass);
            
            this.WriteCimQualifierList(_class.Qualifiers);
            this.WriteCimPropertyList(_class.Properties);
            this.WriteCimMethodList(_class.Methods);

            WriteEndElement();       		
        }
        public void WriteCimMethod(CimMethod method)
        {
            //<METHOD NAME="RequestStateChange" TYPE="uint32" CLASSORIGIN="CIM_EnabledLogicalElement" PROPAGATED="true">

            this.WriteMethodElement();
            this.WriteCimNameAttributeString(method.Name);
            WriteTypeAttribute(method.Type.ToCimType());
            WriteClassOriginAttributeString(method.ClassOrigin);
            WritePropagatedAttribute(method.IsPropagated.ToBool());
            

            WriteCimQualifierList(method.Qualifiers);
            WriteCimParameterList(method.Parameters);

            this.WriteEndElement();
            
        }
        public void WriteCimMethodList(CimMethodList list)
        {
        	for(int i = 0; i < list.Count; ++i)
            {
        		WriteCimMethod(list[i]);
            }
        }

        public void WriteCimParameter(CimParameter parameter)
        {
            //<PARAMETER TYPE="datetime" NAME="TimeoutPeriod">
            WriteParameterElement();
            WriteTypeAttribute(parameter.Type.ToCimType());
            WriteCimNameAttributeString(parameter.Name);

            WriteCimQualifierList(parameter.Qualifiers);

            //</PARAMETER>
            WriteEndElement();
        }

        public void WriteCimParameterValue(CimParameterValue parameter)
        {
            //<PARAMVALUE NAME="arg" PARAMTYPE="string">
            WriteParameterValueElement();
            WriteNameAttributeString(parameter.Name);
            WriteParameterTypeAttributeString(parameter.Type.ToString());
            WriteCimValueList(parameter.ValueArray);
            
            //</PARAMVALUE>
            WriteEndElement();
        }

        public void WriteCimParameterList(CimParameterList list)
        {           
            for(int i = 0; i < list.Count; ++i)
            {
        		WriteCimParameter(list[i]);
            }
        }

        public void WriteCimParameterValueList(CimParameterValueList list)
        {           
            for(int i = 0; i < list.Count; ++i)
            {
        		WriteCimParameterValue(list[i]);
            }
        }
        #endregion
        #endregion

        public override string ToString()
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            MemoryStream memoryStream = new MemoryStream();

            XmlTextWriter xwriter = new XmlTextWriter(memoryStream, Encoding.UTF8);            
            xwriter.Formatting = Formatting.Indented;
            memoryStream.Position = 0;

            // Write the CimXml Header
            // -----------------------
            xwriter.WriteStartDocument(false);
            //<CIM CIMVERSION="2.0" DTDVERSION="2.0">
            xwriter.WriteStartElement("CIM");
            xwriter.WriteAttributeString("DTDVERSION", "2.0");
            xwriter.WriteAttributeString("CIMVERSION", "2.0");

            //<MESSAGE ID="msgid" PROTOCOLVERSION="1.0"> 
            xwriter.WriteStartElement("MESSAGE");
            xwriter.WriteAttributeString("ID", _messageID.ToString());
            xwriter.WriteAttributeString("PROTOCOLVERSION", "1.0");
            // --------------------
            // End of CimXml Header

            
            for (int ix=0; ix < _body.Count; ++ix)                
            {
                CimXmlOperation curOp = (CimXmlOperation)_body[ix];

                switch (curOp.Tag)
                {
                    case TagType.MultiReq:
                        xwriter.WriteStartElement("MULTIREQ");
                        break;

                    case TagType.SimpleReq:
                        xwriter.WriteStartElement("SIMPLEREQ");
                        break;

                    case TagType.MethodCall:
                        xwriter.WriteStartElement("METHODCALL");
                        break;

                    case TagType.IMethodCall:
                        xwriter.WriteStartElement("IMETHODCALL");
                        break;

                    case TagType.LocalInstancePath:
                        xwriter.WriteStartElement("LOCALINSTANCEPATH");
                        break;

                    case TagType.LocalNamespacePath:
                        xwriter.WriteStartElement("LOCALNAMESPACEPATH");
                        break;

                    case TagType.Namespace:
                        xwriter.WriteStartElement("NAMESPACE");
                        break;

                    case TagType.IParameterElement:
                        xwriter.WriteStartElement("IPARAMVALUE");
                        break;

                    case TagType.ClassNameElement:
                        xwriter.WriteStartElement("CLASSNAME");
                        break;

                    case TagType.InstanceElement:
                        xwriter.WriteStartElement("INSTANCE");
                        break;

                    case TagType.InstanceNameElement:
                        xwriter.WriteStartElement("INSTANCENAME");
                        break;

                    case TagType.NamedInstanceElement:
                        xwriter.WriteStartElement("VALUE.NAMEDINSTANCE");
                        break;

                    case TagType.KeyBindingElement:
                        xwriter.WriteStartElement("KEYBINDING");
                        break;

                    case TagType.KeyValueElement:
                        xwriter.WriteStartElement("KEYVALUE");
                        break;

                    case TagType.Property:
                        xwriter.WriteStartElement("PROPERTY");
                        break;

                    case TagType.Method:
                        xwriter.WriteStartElement("METHOD");
                        break;

                    case TagType.Parameter:
                        xwriter.WriteStartElement("PARAMETER");
                        break;

                    case TagType.ParameterValue:
                        xwriter.WriteStartElement("PARAMVALUE");
                        break;

                    case TagType.Class:
                        xwriter.WriteStartElement("CLASS");
                        break;

                    case TagType.LocalClassPath:
                        xwriter.WriteStartElement("LOCALCLASSPATH");
                        break;

                    case TagType.Qualifier:
                        xwriter.WriteStartElement("QUALIFIER");
                        break;

                    case TagType.ValueArray:
                        xwriter.WriteStartElement("VALUE.ARRAY");
                        break;

                    case TagType.ValueReference:
                        xwriter.WriteStartElement("VALUE.REFERENCE");
                        break;

                    case TagType.ValueString:
                        xwriter.WriteElementString("VALUE", curOp.Val);
                        break;

                    case TagType.NameAttributeString:
                        xwriter.WriteAttributeString("NAME", curOp.Val);
                        break;

                    case TagType.ValueTypeAttributeString:
                        xwriter.WriteAttributeString("VALUETYPE", curOp.Val);
                        break;

                    case TagType.ParameterTypeAttributeString:
                        xwriter.WriteAttributeString("PARAMTYPE", curOp.Val);
                        break;

                    case TagType.ToSubClass:
                        xwriter.WriteAttributeString("TOSUBCLASS", curOp.Val);
                        break;

                    case TagType.ToInstance:
                        xwriter.WriteAttributeString("TOINSTANCE", curOp.Val);
                        break;

                    case TagType.Translatable:
                        xwriter.WriteAttributeString("TRANSLATABLE", curOp.Val);
                        break;

                    case TagType.Propagated:
                        xwriter.WriteAttributeString("PROPAGATED", curOp.Val);
                        break;

                    case TagType.Overridable:
                        xwriter.WriteAttributeString("OVERRIDABLE", curOp.Val);
                        break;

                    case TagType.ClassNameAttributeString:
                        xwriter.WriteAttributeString("CLASSNAME", curOp.Val);
                        break;

                    case TagType.ClassOriginAttributeString:
                        xwriter.WriteAttributeString("CLASSORIGIN", curOp.Val);
                        break;

                    case TagType.SuperClassAttributeString:
                        xwriter.WriteAttributeString("SUPERCLASS", curOp.Val);
                        break;

                    case TagType.Type:
                        xwriter.WriteAttributeString("TYPE", curOp.Val);
                        break;

                    case TagType.QualifierDeclaration:
                        xwriter.WriteStartElement("QUALIFIER.DECLARATION", curOp.Val);
                        break;

                    case TagType.ElementValue:
                        xwriter.WriteValue(curOp.Val);
                        break;

                    case TagType.EndElement:
                        xwriter.WriteEndElement();
                        break;

                    default:
                        throw (new Exception("CIMXML Operation Not Implemented: " + curOp.Tag.ToString())); // This should never happen
                }
            }

            //</MESSAGE> 
            xwriter.WriteEndElement();
            //</CIM> 
            xwriter.WriteEndElement();
            xwriter.WriteEndDocument();

            xwriter.Close();

            return utf8.GetString(memoryStream.ToArray()).Substring(1);            
        }
        #endregion
    }
}
