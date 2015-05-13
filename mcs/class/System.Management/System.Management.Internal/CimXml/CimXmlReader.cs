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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using System.Management.Internal;
using System.Management.Internal.Batch;

namespace System.Management.Internal.CimXml
{
    internal class CimXmlReader
    {
        #region CimXmlElementType enum
        public enum CimXmlElementType
        {
            TextValue,
            XmlDeclaration,

            #region 3.2.1 - Top Level Elements
            CimStart,
            CimEnd,
            #endregion

            #region 3.2.2 - Declaration Elements
            DeclarationStart,
            DeclarationEnd,
            DeclarationGroupStart,
            DeclarationGroupEnd,
            DeclarationGroupWithNameStart,
            DeclarationGroupWithNameEnd,
            DeclarationGroupWithPathStart,
            DeclarationGroupWithPathEnd,
            QualifierDeclarationStart,
            QualifierDeclarationEnd,
            ScopeStart,
            ScopeEnd,
            #endregion

            #region 3.2.3 - Value Elements
            ValueStart,
            ValueEnd,
            ValueArrayStart,
            ValueArrayEnd,
            ValueReferenceStart,
            ValueReferenceEnd,
            ValueReferenceArrayStart,
            ValueReferenceArrayEnd,
            ValueObjectStart,
            ValueObjectEnd,
            ValueNamedInstanceStart,
            ValueNamedInstanceEnd,
            ValueNamedObjectStart,
            ValueNamedObjectEnd,
            ValueObjectWithPathStart,
            ValueObjectWithPathEnd,
            ValueObjectWithLocalPathStart,
            ValueObjectWithLocalPathEnd,
            ValueNullStart,
            ValueNullEnd,
            #endregion

            #region 3.2.4 - Naming and Location Elements
            NamespacePathStart,
            NamespacePathEnd,
            LocalNamespacePathStart,
            LocalNamespacePathEnd,
            HostStart,
            HostEnd,
            NamespaceStart,
            NamespaceEnd,
            ClassPathStart,
            ClassPathEnd,
            LocalClassPathStart,
            LocalClassPathEnd,
            ClassNameStart,
            ClassNameEnd,
            InstancePathStart,
            InstancePathEnd,
            LocalInstancePathStart,
            LocalInstancePathEnd,
            InstanceNameStart,
            InstanceNameEnd,
            ObjectPathStart,
            ObjectPathEnd,
            KeyBindingStart,
            KeyBindingEnd,
            KeyValueStart,
            KeyValueEnd,
            #endregion

            #region 3.2.5 - Object Definition Elements
            ClassStart,
            ClassEnd,
            InstanceStart,
            InstanceEnd,
            QualifierStart,
            QualifierEnd,
            PropertyStart,
            PropertyEnd,
            PropertyArrayStart,
            PropertyArrayEnd,
            PropertyReferenceStart,
            PropertyReferenceEnd,
            MethodStart,
            MethodEnd,
            ParameterStart,
            ParameterEnd,
            ParameterReferenceStart,
            ParameterReferenceEnd,
            ParameterArrayStart,
            ParameterArrayEnd,
            ParameterReferenceArrayStart,
            ParameterReferenceArrayEnd,
            TableCellDeclarationStart,
            TableCellDeclarationEnd,
            TableCellReferenceStart,
            TableCellReferenceEnd,
            TableRowDeclarationStart,
            TableRowDeclarationEnd,
            TableStart,
            TableEnd,
            TableRowStart,
            TableRowEnd,
            #endregion

            #region 3.2.6 - Message Elements
            MessageStart,
            MessageEnd,
            MultipleRequestStart,
            MultipleRequestEnd,
            SimpleRequestStart,
            SimpleRequestEnd,
            MethodCallStart,
            MethodCallEnd,
            ParameterValueStart,
            ParameterValueEnd,
            IMethodCallStart,
            IMethodCallEnd,
            IParameterValueStart,
            IParameterValueEnd,
            MultipleResponseStart,
            MultipleResponseEnd,
            SimpleResponseStart,
            SimpleResponseEnd,
            MethodResponseStart,
            MethodResponseEnd,
            IMethodResponseStart,
            IMethodResponseEnd,
            ErrorStart,
            ErrorEnd,
            ReturnValueStart,
            ReturnValueEnd,
            IReturnValueStart,
            IReturnValueEnd,
            MultipleExportRequestStart,
            MultipleExportRequestEnd,
            SimpleExportRequestStart,
            SimpleExportRequestEnd,
            ExportMethodCallStart,
            ExportMethodCallEnd,
            MultipleExportResponseStart,
            MultipleExportResponseEnd,
            SimpleExportResponseStart,
            SimpleExportResponseEnd,
            ExportMethodResponseStart,
            ExportMethodResponseEnd,
            ExportParameterValueStart,
            ExportParameterValueEnd,
            ResponseDestinationStart,
            ResponseDestinationEnd,
            SimpleRequestAcknowledgementStart,
            SimpleRequestAcknowledgementEnd,
            #endregion
        }
        #endregion

        #region Delegates
        public delegate void CimXmlReaderHandler(int percent);
        public static event CimXmlReaderHandler OnPercentChanged = null;
        #endregion

        XmlTextReader _mainXmlTextReader;
        string _xmlResponse;
        int estimatedLength;
        List<int> lineLens;
        int currentPos;
        int lastPercentFired;
        CimDataTypeHandler dataCallBack;
        static int ctr = 0;
        

        #region Constructors
        public CimXmlReader(string xml)
        {
            // Temporary hack to work around bug number 207586
            //
            int indexOfBellChar = xml.IndexOf((char)0x07);
            while (indexOfBellChar >= 0)
            {
                xml = xml.Remove(indexOfBellChar, 1);
                indexOfBellChar = xml.IndexOf((char)0x07);
            }
            // End of hack

            _xmlResponse = xml;
            estimatedLength = xml.Length;
            lineLens = new List<int>();

            currentPos = 0;
            lastPercentFired = 0;
            _mainXmlTextReader = new XmlTextReader(new StringReader(xml));

            this.ReadXmlElement();   // Xml Header
        }
        #endregion


        #region Properties and Indexers
        public string Name
        {
            get { return _mainXmlTextReader.Name; }
        }

        public string Value
        {
            get { return _mainXmlTextReader.Value; }
        }

        public CimXmlElementType ElementType
        {
            get
            {
                // Chomp the whitespace
                TrimStart();

                switch (_mainXmlTextReader.NodeType)
                {
                    #region Declaration Elements
                    case XmlNodeType.XmlDeclaration:
                        switch (_mainXmlTextReader.Name.ToLower())
                        {
                            case "xml":
                                return CimXmlElementType.XmlDeclaration;

                            default:
                                throw (new Exception("Not implemented yet"));
                        }
                    #endregion

                    #region Start Elements
                    case XmlNodeType.Element:
                        switch (_mainXmlTextReader.Name.ToLower())
                        {
                            #region 3.2.1 - Top Level Elements
                            case "cim":
                                return CimXmlElementType.CimStart;
                            #endregion

                            #region 3.2.2 - Declaration Elements
                            case "declaration":
                                return CimXmlElementType.DeclarationStart;

                            case "declgroup":
                                return CimXmlElementType.DeclarationGroupStart;

                            case "declgroup.withname":
                                return CimXmlElementType.DeclarationGroupWithNameStart;

                            case "declgroup.withpath":
                                return CimXmlElementType.DeclarationGroupWithPathStart;

                            case "qualifier.declaration":
                                return CimXmlElementType.QualifierDeclarationStart;

                            case "scope":
                                return CimXmlElementType.ScopeStart;
                            #endregion

                            #region 3.2.3 - Value Elements
                            case "value":
                                return CimXmlElementType.ValueStart;

                            case "value.array":
                                return CimXmlElementType.ValueArrayStart;

                            case "value.reference":
                                return CimXmlElementType.ValueReferenceStart;

                            case "value.refarray":
                                return CimXmlElementType.ValueReferenceArrayStart;

                            case "value.object":
                                return CimXmlElementType.ValueObjectStart;

                            case "value.namedinstance":
                                return CimXmlElementType.ValueNamedInstanceStart;

                            case "value.namedobject":
                                return CimXmlElementType.ValueNamedObjectStart;
                            
                            case "value.objectwithpath":
                                return CimXmlElementType.ValueObjectWithPathStart;

                            case "value.objectwithlocalpath":
                                return CimXmlElementType.ValueObjectWithLocalPathStart;

                            case "value.null":
                                return CimXmlElementType.ValueNullStart;
                            #endregion

                            #region 3.2.4 - Naming and Location Elements
                            case "namespacepath":
                                return CimXmlElementType.NamespacePathStart;

                            case "localnamespacepath":
                                return CimXmlElementType.LocalNamespacePathStart;

                            case "host":
                                return CimXmlElementType.HostStart;

                            case "namespace":
                                return CimXmlElementType.NamespaceStart;

                            case "classpath":
                                return CimXmlElementType.ClassPathStart;

                            case "localclasspath":
                                return CimXmlElementType.LocalClassPathStart;

                            case "classname":
                                return CimXmlElementType.ClassNameStart;

                            case "instancepath":
                                return CimXmlElementType.InstancePathStart;

                            case "localinstancepath":
                                return CimXmlElementType.LocalInstancePathStart;

                            case "instancename":
                                return CimXmlElementType.InstanceNameStart;

                            case "objectpath":
                                return CimXmlElementType.ObjectPathStart;

                            case "keybinding":
                                return CimXmlElementType.KeyBindingStart;

                            case "keyvalue":
                                return CimXmlElementType.KeyValueStart;
                            #endregion

                            #region 3.2.5 - Object Definition Elements
                            case "class":
                                return CimXmlElementType.ClassStart;

                            case "instance":
                                return CimXmlElementType.InstanceStart;

                            case "qualifier":
                                return CimXmlElementType.QualifierStart;

                            case "property":
                                return CimXmlElementType.PropertyStart;

                            case "property.array":
                                return CimXmlElementType.PropertyArrayStart;

                            case "property.reference":
                                return CimXmlElementType.PropertyReferenceStart;

                            case "method":
                                return CimXmlElementType.MethodStart;

                            case "parameter":
                                return CimXmlElementType.ParameterStart;

                            case "parameter.reference":
                                return CimXmlElementType.ParameterReferenceStart;

                            case "parameter.array":
                                return CimXmlElementType.ParameterArrayStart;

                            case "parameter.refarray":
                                return CimXmlElementType.ParameterReferenceArrayStart;

                            case "tablecell.declaration":
                                return CimXmlElementType.TableCellDeclarationStart;

                            case "tablecell.reference":
                                return CimXmlElementType.TableCellReferenceStart;

                            case "tablerow.declaration":
                                return CimXmlElementType.TableRowDeclarationStart;

                            case "table":
                                return CimXmlElementType.TableStart;

                            case "tablerow":
                                return CimXmlElementType.TableRowStart;
                            #endregion

                            #region 3.2.6 - Message Elements
                            case "message":
                                return CimXmlElementType.MessageStart;

                            case "multireq":
                                return CimXmlElementType.MultipleRequestStart;

                            case "simplereq":
                                return CimXmlElementType.SimpleRequestStart;

                            case "methodcall":
                                return CimXmlElementType.MethodCallStart;

                            case "paramvalue":
                                return CimXmlElementType.ParameterValueStart;

                            case "imethodcall":
                                return CimXmlElementType.IMethodCallStart;

                            case "iparamvalue":
                                return CimXmlElementType.IParameterValueStart;

                            case "multirsp":
                                return CimXmlElementType.MultipleResponseStart;

                            case "simplersp":
                                return CimXmlElementType.SimpleResponseStart;

                            case "methodresponse":
                                return CimXmlElementType.MethodResponseStart;

                            case "imethodresponse":
                                return CimXmlElementType.IMethodResponseStart;

                            case "error":
                                return CimXmlElementType.ErrorStart;

                            case "returnvalue":
                                return CimXmlElementType.ReturnValueStart;

                            case "ireturnvalue":
                                return CimXmlElementType.IReturnValueStart;

                            case "multiexpreq":
                                return CimXmlElementType.MultipleExportRequestStart;
                                                                
                            case "simpleexpreq":
                                return CimXmlElementType.SimpleExportRequestStart;
                                                
                            case "expmethodcall":
                                return CimXmlElementType.ExportMethodCallStart;

                            case "multiexprsp":
                                return CimXmlElementType.MultipleExportResponseStart;

                            case "simpleexprsp":
                                return CimXmlElementType.SimpleExportResponseStart;

                            case "expmethodresponse":
                                return CimXmlElementType.ExportMethodResponseStart;

                            case "expparamvalue":
                                return CimXmlElementType.ExportParameterValueStart;

                            case "responsedestinaton":
                                return CimXmlElementType.ResponseDestinationStart;

                            case "simplereqack":
                                return CimXmlElementType.SimpleRequestAcknowledgementStart;
                            #endregion

                            default:
                                throw (new Exception("Not implemented yet"));
                        }
                    #endregion

                    #region End Elements
                    case XmlNodeType.EndElement:
                        switch (_mainXmlTextReader.Name.ToLower())
                        {
                            #region 3.2.1 - Top Level Elements
                            case "cim":
                                return CimXmlElementType.CimEnd;
                            #endregion

                            #region 3.2.2 - Declaration Elements
                            case "declaration":
                                return CimXmlElementType.DeclarationEnd;

                            case "declgroup":
                                return CimXmlElementType.DeclarationGroupEnd;

                            case "declgroup.withname":
                                return CimXmlElementType.DeclarationGroupWithNameEnd;

                            case "declgroup.withpath":
                                return CimXmlElementType.DeclarationGroupWithPathEnd;

                            case "qualifier.declaration":
                                return CimXmlElementType.QualifierDeclarationEnd;

                            case "scope":
                                return CimXmlElementType.ScopeEnd;
                            #endregion

                            #region 3.2.3 - Value Elements
                            case "value":
                                return CimXmlElementType.ValueEnd;

                            case "value.array":
                                return CimXmlElementType.ValueArrayEnd;

                            case "value.reference":
                                return CimXmlElementType.ValueReferenceEnd;

                            case "value.refarray":
                                return CimXmlElementType.ValueReferenceArrayEnd;

                            case "value.object":
                                return CimXmlElementType.ValueObjectEnd;

                            case "value.namedinstance":
                                return CimXmlElementType.ValueNamedInstanceEnd;

                            case "value.namedobject":
                                return CimXmlElementType.ValueNamedObjectEnd;

                            case "value.objectwithpath":
                                return CimXmlElementType.ValueObjectWithPathEnd;

                            case "value.objectwithlocalpath":
                                return CimXmlElementType.ValueObjectWithLocalPathEnd;

                            case "value.null":
                                return CimXmlElementType.ValueNullEnd;
                            #endregion

                            #region 3.2.4 - Naming and Location Elements
                            case "namespacepath":
                                return CimXmlElementType.NamespacePathEnd;

                            case "localnamespacepath":
                                return CimXmlElementType.LocalNamespacePathEnd;

                            case "host":
                                return CimXmlElementType.HostEnd;

                            case "namespace":
                                return CimXmlElementType.NamespaceEnd;

                            case "classpath":
                                return CimXmlElementType.ClassPathEnd;

                            case "localclasspath":
                                return CimXmlElementType.LocalClassPathEnd;

                            case "classname":
                                return CimXmlElementType.ClassNameEnd;

                            case "instancepath":
                                return CimXmlElementType.InstancePathEnd;

                            case "localinstancepath":
                                return CimXmlElementType.LocalInstancePathEnd;

                            case "instancename":
                                return CimXmlElementType.InstanceNameEnd;

                            case "objectpath":
                                return CimXmlElementType.ObjectPathEnd;

                            case "keybinding":
                                return CimXmlElementType.KeyBindingEnd;

                            case "keyvalue":
                                return CimXmlElementType.KeyValueEnd;
                            #endregion

                            #region 3.2.5 - Object Definition Elements
                            case "class":
                                return CimXmlElementType.ClassEnd;

                            case "instance":
                                return CimXmlElementType.InstanceEnd;

                            case "qualifier":
                                return CimXmlElementType.QualifierEnd;

                            case "property":
                                return CimXmlElementType.PropertyEnd;

                            case "property.array":
                                return CimXmlElementType.PropertyArrayEnd;

                            case "property.reference":
                                return CimXmlElementType.PropertyReferenceEnd;

                            case "method":
                                return CimXmlElementType.MethodEnd;

                            case "parameter":
                                return CimXmlElementType.ParameterEnd;

                            case "parameter.reference":
                                return CimXmlElementType.ParameterReferenceEnd;

                            case "parameter.array":
                                return CimXmlElementType.ParameterArrayEnd;

                            case "parameter.refarray":
                                return CimXmlElementType.ParameterReferenceArrayEnd;

                            case "tablecell.declaration":
                                return CimXmlElementType.TableCellDeclarationEnd;

                            case "tablecell.reference":
                                return CimXmlElementType.TableCellReferenceEnd;

                            case "tablerow.declaration":
                                return CimXmlElementType.TableRowDeclarationEnd;

                            case "table":
                                return CimXmlElementType.TableEnd;

                            case "tablerow":
                                return CimXmlElementType.TableRowEnd;
                            #endregion

                            #region 3.2.6 - Message Elements
                            case "message":
                                return CimXmlElementType.MessageEnd;

                            case "multireq":
                                return CimXmlElementType.MultipleRequestEnd;

                            case "simplereq":
                                return CimXmlElementType.SimpleRequestEnd;

                            case "methodcall":
                                return CimXmlElementType.MethodCallEnd;

                            case "paramvalue":
                                return CimXmlElementType.ParameterValueEnd;

                            case "imethodcall":
                                return CimXmlElementType.IMethodCallEnd;

                            case "iparamvalue":
                                return CimXmlElementType.IParameterValueEnd;

                            case "multirsp":
                                return CimXmlElementType.MultipleResponseEnd;

                            case "simplersp":
                                return CimXmlElementType.SimpleResponseEnd;

                            case "methodresponse":
                                return CimXmlElementType.MethodResponseEnd;

                            case "imethodresponse":
                                return CimXmlElementType.IMethodResponseEnd;

                            case "error":
                                return CimXmlElementType.ErrorEnd;

                            case "returnvalue":
                                return CimXmlElementType.ReturnValueEnd;

                            case "ireturnvalue":
                                return CimXmlElementType.IReturnValueEnd;

                            case "multiexpreq":
                                return CimXmlElementType.MultipleExportRequestEnd;

                            case "simpleexpreq":
                                return CimXmlElementType.SimpleExportRequestEnd;

                            case "expmethodcall":
                                return CimXmlElementType.ExportMethodCallEnd;

                            case "multiexprsp":
                                return CimXmlElementType.MultipleExportResponseEnd;

                            case "simpleexprsp":
                                return CimXmlElementType.SimpleExportResponseEnd;

                            case "expmethodresponse":
                                return CimXmlElementType.ExportMethodResponseEnd;

                            case "expparamvalue":
                                return CimXmlElementType.ExportParameterValueEnd;

                            case "responsedestinaton":
                                return CimXmlElementType.ResponseDestinationEnd;

                            case "simplereqack":
                                return CimXmlElementType.SimpleRequestAcknowledgementEnd;
                            #endregion

                            default:
                                throw (new Exception("Not implemented yet"));
                        }
                    #endregion

                    case XmlNodeType.Text:
                        return CimXmlElementType.TextValue;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }
        }     
        #endregion

        #region Methods

        #region General Purpose Methods
        public void TrimStart()
        {
            // Eliminate all empty elements
            // This seems to happen because of \n's in the XML stream.
            while ((_mainXmlTextReader.NodeType == XmlNodeType.Whitespace) ||
                   (_mainXmlTextReader.NodeType == XmlNodeType.None))
            {
                // Chomp the whitespace
                _mainXmlTextReader.Read();
            }
        }

        public bool NextElement()
        {
            // Chomp the whitespace
            TrimStart();


            if (OnPercentChanged != null)
            {
                while (lineLens.Count <= _mainXmlTextReader.LineNumber)
                {
                    lineLens.Add(0);
                }

                lineLens[_mainXmlTextReader.LineNumber] = _mainXmlTextReader.LinePosition;

                currentPos = 0;
                foreach (int curInt in lineLens)
                {
                    // Add up the lens of all of the lines
                    currentPos += curInt;
                }
                
                int percentCompleted = (int)(((float)currentPos / (float)estimatedLength) * 100);

                if ( lastPercentFired <= (percentCompleted - 1) )
                {
                    lastPercentFired = percentCompleted;

                    if (percentCompleted > 100)
                        percentCompleted = 100;
                    

                    //if ((OnPercentChanged.Target is System.Windows.Forms.Control) &&
                    //     ((System.Windows.Forms.Control)OnPercentChanged.Target).InvokeRequired)
                    //{
                    //    OnPercentChanged.Invoke(percentCompleted);
                    //}
                    //else
                    //{
                        OnPercentChanged(percentCompleted);
                    //}
                }
            }           

            CimXmlReader.ctr++;

            // DELME: This was used to debug bug # 207586
            //  Also delete the ctr var!
            //
            //Console.WriteLine(CimXmlReader.ctr);

            //int distance = 4848442 - 170432;
            //int baseNum = 4848387 - distance;
            
            //if (ctr == baseNum)
            //{
            //    CimXmlReader.ctr = 0;
            //    Console.WriteLine(CimXmlReader.ctr);
            //    char a = _xmlResponse[baseNum + distance - 1];
            //    string aaaa = _xmlResponse.Substring(baseNum + distance - 100);
            //}

            return _mainXmlTextReader.Read();
        }

        /// <summary>
        /// Matches the element and moves this.ElementType to the next one
        /// </summary>
        /// <param name="type">ElementType to read</param>
        private void ReadElement(CimXmlElementType expectedType)
        {
            MatchElement(expectedType);
            NextElement();
        }

        /// <summary>
        /// Compares the type to this.ElementType
        /// </summary>
        /// <param name="type">expected type</param>
        private void MatchElement(CimXmlElementType expectedType)
        {
            if (this.ElementType != expectedType)
                throw (new Exception("Not a " + expectedType.ToString() + " element"));
        }
        #endregion

        #region Read Methods

        #region Xml
        public void ReadXmlElement()
        {
            ReadElement(CimXmlElementType.XmlDeclaration);
        }
        #endregion

        #region Cim
        public void ReadCim(CimDataTypeHandler dataHandler)
        {
            #region DTD
            /* 
             <!ELEMENT CIM (MESSAGE|DECLARATION)> 
             <!ATTLIST CIM 
                    CIMVERSION CDATA #REQUIRED 
                    DTDVERSION CDATA #REQUIRED>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            dataCallBack = dataHandler;

            CimXmlHeader header = new CimXmlHeader();            

            MatchElement(CimXmlElementType.CimStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "cimversion":
                        header.CimVersion = _mainXmlTextReader.Value;
                        break;

                    case "dtdversion":
                        header.DtdVersion = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            NextElement();  // Move off of the attributes

            switch (this.ElementType)
            {
                case CimXmlElementType.MessageStart:
                    ReadMessage(header);
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.CimEnd);
        }
        #endregion

        #region Message
        public void ReadMessage(CimXmlHeader header)
        {
            #region DTD
            /* 
             <!ELEMENT MESSAGE (SIMPLEREQ|MULTIREQ|SIMPLERSP|MULTIRSP| SIMPLEEXPREQ|MULTIEXPREQ|SIMPLEEXPRSP|MULTIEXPRSP)> 
             <!ATTLIST MESSAGE 
                    ID CDATA #REQUIRED 
                    PROTOCOLVERSION CDATA #REQUIRED>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.MessageStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "id":
                        header.MessageId = _mainXmlTextReader.Value;
                        break;

                    case "protocolversion":
                        header.ProtocolVersion = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            NextElement();  // Move off of the attributes

            switch (this.ElementType)
            {
                case CimXmlElementType.SimpleResponseStart:
                    header.ResponseNumber = 0;
                    header.IsMultipleResponse = false;
                    ReadSimpleResponse(header);
                    break;

                case CimXmlElementType.MultipleResponseStart:
                    ReadMultipleResponse(header);
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.MessageEnd);
        }
        #endregion

        #region SimpleResponse
        public void ReadSimpleResponse(CimXmlHeader header)
        {
            #region DTD
            /* 
             <!ELEMENT SIMPLERSP (METHODRESPONSE|IMETHODRESPONSE|SIMPLEREQACK)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.SimpleResponseStart);

            switch (this.ElementType)
            {
                case CimXmlElementType.IMethodResponseStart:
                    ReadIMethodResponse(header);                    
                    break;

                case CimXmlElementType.MethodResponseStart:
                    ReadMethodResponse(header);
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.SimpleResponseEnd);
        }
        #endregion

        #region MultipleResponse
        public void ReadMultipleResponse(CimXmlHeader header)
        {
            #region DTD
            /* 
             <!ELEMENT MULTIRSP (SIMPLERSP,SIMPLERSP+)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.MultipleResponseStart);

            header.IsMultipleResponse = true;
            header.ResponseNumber = 0;

            do
            {
                ReadSimpleResponse(header);
                header.ResponseNumber++;
            }
            while (this.ElementType != CimXmlElementType.MultipleResponseEnd);

            ReadElement(CimXmlElementType.MultipleResponseEnd);

            //return retVal;
        }
        #endregion

        #region ReturnValue
        public CimReturnValue ReadReturnValue()
        {
            #region DTD
            /* 
            <!ELEMENT RETURNVALUE (VALUE|VALUE.REFERENCE)>
            <!ATTLIST RETURNVALUE
                %ParamType;     #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ReturnValueStart);

            CimReturnValue retVal = new CimReturnValue();

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "paramtype":
                        retVal.Type = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if (! retVal.Type.IsSet)
                throw (new Exception("Element doesn't have the 'ParamType' Attribute"));

            NextElement();  // Move off of the attributes 


            switch (this.ElementType)
            {
                case CimXmlElementType.ValueStart:
                    retVal.Value = this.ReadValue();                    
                    break;

                case CimXmlElementType.ValueReferenceStart:
                    retVal.ValueReference = this.ReadValueReference();
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.ReturnValueEnd);

            return retVal;
        }
        #endregion

        #region IReturnValue
        public void ReadIReturnValue(CimXmlHeader header)
        {
            #region DTD
            /* 
             <!ELEMENT IRETURNVALUE (CLASSNAME*|INSTANCENAME*|VALUE*|VALUE.OBJECTWITHPATH*|
                                     VALUE.OBJECTWITHLOCALPATH* VALUE.OBJECT*|
                                     OBJECTPATH*|QUALIFIER.DECLARATION*|VALUE.ARRAY?|
                                     VALUE.REFERENCE?| CLASS*|INSTANCE*|VALUE.NAMEDINSTANCE*)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.IReturnValueStart);            

            switch (this.ElementType)
            {
                case CimXmlElementType.ClassNameStart:
                    //retVal = new SingleResponse(SingleResponse.ResponseType.CimNameList, new CimNameList());                    
                    while (this.ElementType != CimXmlReader.CimXmlElementType.IReturnValueEnd)
                    {
                        dataCallBack(header, this.ReadClassName());
                        //((CimNameList)retVal.Value).Add(this.ReadClassName());
                    }
                    break;

                case CimXmlElementType.ClassStart:
                    //retVal = new SingleResponse(SingleResponse.ResponseType.CimClassList, new CimClassList());
                    while (this.ElementType != CimXmlReader.CimXmlElementType.IReturnValueEnd)
                    {
                        dataCallBack(header, this.ReadClass());
                        //((CimClassList)retVal.Value).Add(this.ReadClass());
                    }
                    break;

                case CimXmlElementType.ValueNamedInstanceStart:
                    //retVal = new SingleResponse(SingleResponse.ResponseType.CimNamedInstanceList, new CimNamedInstanceList());
                    while (this.ElementType != CimXmlReader.CimXmlElementType.IReturnValueEnd)
                    {
                        dataCallBack(header, this.ReadValueNamedInstance());
                        //((CimNamedInstanceList)retVal.Value).Add(this.ReadValueNamedInstance());
                    }
                    break;

                case CimXmlElementType.IReturnValueEnd:
                    // There was no tag inside, so just break out
                    dataCallBack(header, null);
                    //retVal = new SingleResponse(SingleResponse.ResponseType.Empty, null);
                    break;

                case CimXmlElementType.InstanceStart:
                    //retVal = new SingleResponse(SingleResponse.ResponseType.CimInstanceList, new CimInstanceList());
                    while (this.ElementType != CimXmlReader.CimXmlElementType.IReturnValueEnd)
                    {
                        dataCallBack(header, this.ReadInstance());
                        //((CimInstanceList)retVal.Value).Add(this.ReadInstance());
                    }
                    break;

                case CimXmlElementType.InstanceNameStart:
                    //retVal = new SingleResponse(SingleResponse.ResponseType.CimInstanceNameList, new CimInstanceNameList());
                    while (this.ElementType != CimXmlReader.CimXmlElementType.IReturnValueEnd)
                    {
                        dataCallBack(header, this.ReadInstanceName());
                        //((CimInstanceNameList)retVal.Value).Add(this.ReadInstanceName());
                    }
                    break;

                case CimXmlElementType.ValueStart:
                    //while (this.ElementType != CimXmlReader.CimXmlElementType.ValueEnd)
                    {
                        dataCallBack(header, this.ReadValue());
                    }
                    break;

                case CimXmlElementType.QualifierDeclarationStart:
                    while (this.ElementType == CimXmlReader.CimXmlElementType.QualifierDeclarationStart)
                    {
                        dataCallBack(header, this.ReadQualifierDeclataion());
                    }
                    break;

                case CimXmlElementType.ValueObjectWithPathStart:
                    while (this.ElementType == CimXmlReader.CimXmlElementType.ValueObjectWithPathStart)
                    {
                        dataCallBack(header, this.ReadValueObjectWithPath());
                    }
                    break;

                case CimXmlElementType.ObjectPathStart:
                    while (this.ElementType == CimXmlReader.CimXmlElementType.ObjectPathStart)
                    {
                        dataCallBack(header, this.ReadObjectPath());
                    }
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.IReturnValueEnd);
        }
        #endregion

        #region ErrorElement
        public CimomError ReadError()
        {
            #region DTD
            /*
             <!ELEMENT ERROR (INSTANCE*) 
             <!ATTLIST ERROR 
                    CODE CDATA #REQUIRED 
                    DESCRIPTION CDATA #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            CimomError retVal = new CimomError(string.Empty, string.Empty);

            MatchElement(CimXmlElementType.ErrorStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "code":
                        retVal.ErrorCode = _mainXmlTextReader.Value;
                        break;

                    case "description":
                        retVal.Description = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            NextElement();  // Move off of the attributes

            while (this.ElementType != CimXmlElementType.ErrorEnd)
            {
                retVal.Instances.Add(ReadInstance());
            }

            ReadElement(CimXmlElementType.ErrorEnd);

            retVal.XmlResponse = _xmlResponse;

            return retVal;
        }
        #endregion

        #region MethodResponse
        public void ReadMethodResponse(CimXmlHeader header)
        {
            #region DTD
            /* 
            <!ELEMENT METHODRESPONSE (ERROR|(RETURNVALUE?,PARAMVALUE*))>
            <!ATTLIST METHODRESPONSE
                %CIMName;>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <METHODRESPONSE NAME="KillAll">
			    <RETURNVALUE PARAMTYPE="string">
				    <VALUE>Fail</VALUE> 
		        </RETURNVALUE>
	        </METHODRESPONSE>
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.MethodResponseStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        header.MethodName = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if (header.MethodName == string.Empty)
                throw (new Exception("Element doesn't have the 'Name' Attribute"));

            NextElement();  // Move off of the attributes                         

            if (this.ElementType == CimXmlElementType.ErrorStart)
            {
                dataCallBack(header, ReadError());                
            }
            else
            {
                CimMethodResponse tmpMR = new CimMethodResponse();

                while (this.ElementType != CimXmlElementType.MethodResponseEnd)
                {
                    switch (this.ElementType)
                    {
                        case CimXmlElementType.ReturnValueStart:
                            tmpMR.RetVal = ReadReturnValue();
                            break;

                        case CimXmlElementType.ParameterValueStart:
                            tmpMR.ParamVals.AddRange(ReadParameterValueList());
                            break;

                        default:
                            throw (new Exception("Not implemented yet"));
                    }
                }

                dataCallBack(header, tmpMR);
            }

            ReadElement(CimXmlElementType.MethodResponseEnd);
        }
        #endregion

        #region IMethodResponse
        public void ReadIMethodResponse(CimXmlHeader header)
        {
            #region DTD
            /* 
             <!ELEMENT IMETHODRESPONSE (ERROR|IRETURNVALUE?)> 
             <!ATTLIST IMETHODRESPONSE 
                    %CIMName;>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.IMethodResponseStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        header.MethodName = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if (header.MethodName == string.Empty)
                throw (new Exception("Element doesn't have the 'Name' Attribute"));

            NextElement();  // Move off of the attributes                         

            switch (this.ElementType)
            {
                case CimXmlElementType.IReturnValueStart:
                    ReadIReturnValue(header);
                    break;

                case CimXmlElementType.ErrorStart:
                    dataCallBack(header, ReadError());
                    break;

                case CimXmlElementType.IMethodResponseEnd:
                    // There was no tag inside, so just break out
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            ReadElement(CimXmlElementType.IMethodResponseEnd);
        }
        #endregion

        #region ClassName
        public CimName ReadClassName()
        {
            #region DTD
            /* 
             <!ELEMENT CLASSNAME EMPTY> 
             <!ATTLIST CLASSNAME 
                    %CIMName;>
            */
            #endregion

            #region Actual Xml Response
            /*            
                  [...]
                  <CLASSNAME NAME="CIM_DirectoryContainsFile" />
                  [...]
             */
            #endregion

            string retVal = string.Empty;

            MatchElement(CimXmlElementType.ClassNameStart);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        retVal = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if (retVal == string.Empty)
                throw (new Exception("Element doesn't have the 'Name' Attribute"));

            NextElement();  // Move off of the attributes 

            if (this.ElementType == CimXmlElementType.ClassNameEnd)
                NextElement(); // Consume the closing element, if there.

            return new CimName(retVal);;
        }
        #endregion

        #region Class
        public CimClass ReadClass()
        {
            #region DTD
            /* 
             <!ELEMENT CLASS (QUALIFIER*,(PROPERTY|PROPERTY.ARRAY|PROPERTY.REFERENCE)*,METHOD*)> 
             <!ATTLIST CLASS 
                    %CIMName; 
                    %SuperClass;>
            */
            #endregion

            #region Actual Xml Response
            /*
                    [...]
                     <IRETURNVALUE>
                         <CLASS NAME="CIM_PhysicalAssetCapabilities"SUPERCLASS="CIM_Capabilities">
                             <QUALIFIER NAME="Description"TYPE="string"TRANSLATABLE="true">
                                <VALUE>Capability for a physical asset. We include this class here until it gets added to the offical CIM schema. Need to do this in order to make some attempt in implementing the Physical Assets Profile.</VALUE>
                             </QUALIFIER>
                             [...]   
                             <PROPERTY NAME="InstanceID"TYPE="string"CLASSORIGIN="CIM_Capabilities"PROPAGATED="true">
                                 <QUALIFIER NAME="Key"TYPE="boolean"OVERRIDABLE="false">
                                    <VALUE>true</VALUE>
                                 </QUALIFIER>
                                 [...]
                             </PROPERTY>
                             [...]
                             <METHOD NAME="CreateGoalSettings"TYPE="uint16"CLASSORIGIN="CIM_Capabilities"PROPAGATED="true">
                                <QUALIFIER NAME="Experimental"TYPE="boolean"TOSUBCLASS="false">
                                    <VALUE>true</VALUE>
                                </QUALIFIER>
                                [...]                             
                                <QUALIFIER NAME="ValueMap"TYPE="string">
                                    <VALUE.ARRAY>
                                        <VALUE>0</VALUE>
                                        <VALUE>1</VALUE>
                                        <VALUE>2</VALUE>
                                        <VALUE>3</VALUE>
                                        <VALUE>4</VALUE>
                                        <VALUE>5</VALUE>
                                        <VALUE>6</VALUE>
                                        <VALUE>..</VALUE>
                                        <VALUE>32768..65535</VALUE>
                                    </VALUE.ARRAY>
                                </QUALIFIER>                             
                                <PARAMETER.ARRAY TYPE="string"NAME="TemplateGoalSettings">
                                    <QUALIFIER NAME="IN"TYPE="boolean"OVERRIDABLE="false">
                                        <VALUE>true</VALUE>
                                    </QUALIFIER>
                                    [...] 
                                </PARAMETER.ARRAY>                             
                             </METHOD>
                         </CLASS>
                     </IRETURNVALUE>
                    [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ClassStart);

            CimClass newCimClass = new CimClass();

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        newCimClass.ClassName = new CimName(_mainXmlTextReader.Value);
                        break;

                    case "superclass":
                        newCimClass.SuperClass = new CimName(_mainXmlTextReader.Value);
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the Attributes

            while (this.ElementType != CimXmlElementType.ClassEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.QualifierStart:
                        newCimClass.Qualifiers.Add(ReadQualifier());                        
                        break;

                    case CimXmlElementType.PropertyStart:
                        newCimClass.Properties.Add(ReadProperty());                        
                        break;

                    case CimXmlElementType.MethodStart:
                        newCimClass.Methods.Add(ReadMethod());                        
                        break;

                    case CimXmlElementType.PropertyArrayStart:
                        newCimClass.Properties.Add(ReadPropertyArray());                        
                        break;

                    case CimXmlElementType.PropertyReferenceStart:
                        newCimClass.Properties.Add(ReadPropertyReference());                        
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.ClassEnd);

            return newCimClass;
        }
        #endregion

        #region Qualifier
        public CimQualifier ReadQualifier()
        {
            #region DTD
            /*
             <!ELEMENT QUALIFIER ((VALUE|VALUE.ARRAY)?)>
             <!ATTLIST QUALIFIER 
                    %CIMName; 
                    %CIMType; #REQUIRED 
                    %Propagated; 
                    %QualifierFlavor; 
                    xml:lang NMTOKEN #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
                  [...]
                    <QUALIFIER NAME="Experimental"TYPE="boolean"TOSUBCLASS="false">
                        <VALUE>true</VALUE>
                    </QUALIFIER>
                    <QUALIFIER NAME="ValueMap"TYPE="string">
                        <VALUE.ARRAY>
                            <VALUE>0</VALUE>
                            <VALUE>1</VALUE>
                            <VALUE>2</VALUE>
                            <VALUE>3</VALUE>
                            <VALUE>4</VALUE>
                            <VALUE>5</VALUE>
                            <VALUE>6</VALUE>
                            <VALUE>..</VALUE>
                            <VALUE>32768..65535</VALUE>
                        </VALUE.ARRAY>
                    </QUALIFIER>
                  [...]
             */
            #endregion

            MatchElement(CimXmlElementType.QualifierStart);

            CimQualifier newCimQualifier = new CimQualifier(CimType.INVALID, string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        newCimQualifier.Name = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimQualifier.Type = _mainXmlTextReader.Value;
                        break;

                    case "propagated":
                        newCimQualifier.IsPropagated = _mainXmlTextReader.Value;
                        break;

                    // Flavors
                    case "tosubclass":
                        newCimQualifier.Flavor.ToSubClass = _mainXmlTextReader.Value;
                        break;

                    case "overridable":
                        newCimQualifier.Flavor.Overridable = _mainXmlTextReader.Value;
                        break;

                    case "translatable":
                        newCimQualifier.Flavor.Translatable = _mainXmlTextReader.Value;
                        break;
                    // End of Flavors

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move to either Value or Value.Array

            switch (this.ElementType)
            {
                case CimXmlElementType.ValueStart:                    
                    newCimQualifier.Values.Add(ReadValue());                    
                    break;

                case CimXmlElementType.ValueArrayStart:
                    newCimQualifier.Values.AddRange(ReadValueArray());
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.QualifierEnd);

            return newCimQualifier;
        }

        /// <summary>
        /// This methods begins with the first start element tag of a sequence of qualifiers
        /// </summary>
        /// <returns>Returns the CimQualifierList</returns>
        private CimQualifierList ReadQualifierList()
        {
            CimQualifierList list = new CimQualifierList();
            while (this.ElementType == CimXmlElementType.QualifierStart)
            {
                list.Add(ReadQualifier());                
            }

            return list;
        }
        #endregion

        #region Property
        public CimProperty ReadProperty()
        {
            #region DTD
            /*
             <!ELEMENT PROPERTY (QUALIFIER*,VALUE?)> 
             <!ATTLIST PROPERTY 
                    %CIMName; 
                    %CIMType; #REQUIRED 
                    %ClassOrigin; 
                    %Propagated; 
                    xml:lang NMTOKEN #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                     <PROPERTY NAME="InstanceID"TYPE="string"CLASSORIGIN="CIM_Capabilities"PROPAGATED="true">
                         <QUALIFIER NAME="Key"TYPE="boolean"OVERRIDABLE="false">
                            <VALUE>true</VALUE>
                         </QUALIFIER>
                         [...]
                     </PROPERTY>
                     [...]
             */
            #endregion

            MatchElement(CimXmlElementType.PropertyStart);

            CimProperty newCimProperty = new CimProperty(string.Empty, CimType.INVALID);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":                        
                        newCimProperty.Name = _mainXmlTextReader.Value;
                        break;

                    case "classorigin":
                        newCimProperty.ClassOrigin = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimProperty.Type = _mainXmlTextReader.Value;
                        break;

                    case "propagated":
                        newCimProperty.IsPropagated = _mainXmlTextReader.Value;
                        break;
					case   "embeddedobject":
						//TODO: IMPORTANT Console.WriteLine ("Found Embedded Object: " + _mainXmlTextReader.Value);
						break;
                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if ((newCimProperty.Name == string.Empty) ||
                (newCimProperty.Type == CimType.INVALID))
            {
                throw new Exception("Required Attributes Not Set");
            }

            this.NextElement();

            newCimProperty.Qualifiers.AddRange(ReadQualifierList());

            //read in the optional value
            if (this.ElementType == CimXmlElementType.ValueStart)
            {
                newCimProperty.Value = ReadValue();                
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.PropertyEnd);

            return newCimProperty;
        }
        #endregion

        #region PropertyArray
        public CimPropertyArray ReadPropertyArray()
        {
            #region DTD
            /*
             <!ELEMENT PROPERTY.ARRAY (QUALIFIER*,VALUE.ARRAY?)> 
             <!ATTLIST PROPERTY.ARRAY 
                    %CIMName; 
                    %CIMType; #REQUIRED 
                    %ArraySize; 
                    %ClassOrigin; 
                    %Propagated; 
                    xml:lang NMTOKEN #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   

                     [...]
             */
            #endregion

            MatchElement(CimXmlElementType.PropertyArrayStart);

            CimPropertyArray newCimPropertyArray = new CimPropertyArray(string.Empty, CimType.INVALID);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":                        
                        newCimPropertyArray.Name = _mainXmlTextReader.Value;
                        break;

                    case "classorigin":
                        newCimPropertyArray.ClassOrigin = _mainXmlTextReader.Value;
                        break;

                    case "arraysize":
                        newCimPropertyArray.ArraySize = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimPropertyArray.Type = _mainXmlTextReader.Value;
                        break;

                    case "propagated":
                        newCimPropertyArray.IsPropagated = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if ((newCimPropertyArray.Name == string.Empty) ||
                 (newCimPropertyArray.Type == CimType.INVALID))
            {
                throw new Exception("Required Attributes Not Set");
            }

            this.NextElement();

            newCimPropertyArray.Qualifiers.AddRange(ReadQualifierList());
            if (this.ElementType == CimXmlElementType.ValueArrayStart)
            {
                newCimPropertyArray.ValueArray.AddRange(ReadValueArray());
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.PropertyArrayEnd);

            return newCimPropertyArray;
        }
        #endregion

        #region PropertyReference
        public CimPropertyReference ReadPropertyReference()
        {
            #region DTD
            /*
            <!ELEMENT PROPERTY.REFERENCE (QUALIFIER*,VALUE.REFERENCE?)> 
            <!ATTLIST PROPERTY.REFERENCE 
                    %CIMName; 
                    %ReferenceClass; 
                    %ClassOrigin; 
                    %Propagated;>
             */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.PropertyReferenceStart);

            CimPropertyReference propRef = new CimPropertyReference(string.Empty);            

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":                        
                        propRef.Name = _mainXmlTextReader.Value;
                        break;

                    case "classorigin":
                        propRef.ClassOrigin = _mainXmlTextReader.Value;
                        break;

                    case "propagated":
                        propRef.IsPropagated = _mainXmlTextReader.Value;
                        break;

                    case "referenceclass":
                        propRef.ReferenceClass = _mainXmlTextReader.Value;
                        break;                    

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }
            this.NextElement();

            while (this.ElementType == CimXmlElementType.QualifierStart)
            {
                propRef.Qualifiers.AddRange(ReadQualifierList());
            }

            if (this.ElementType == CimXmlElementType.ValueReferenceStart)
            {
                propRef.ValueReference = ReadValueReference();
            }


            // Consume the closing element
            ReadElement(CimXmlElementType.PropertyReferenceEnd);

            return propRef;
        }

        #endregion

        #region Method
        public CimMethod ReadMethod()
        {
            #region DTD
            /*
             <!ELEMENT METHOD (QUALIFIER*,(PARAMETER|PARAMETER.REFERENCE|PARAMETER.ARRAY|PARAMETER.REFARRAY)*)> 
             <!ATTLIST METHOD 
                    %CIMName; 
                    %CIMType; #IMPLIED 
                    %ClassOrigin; 
                    %Propagated;> 
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                     <METHOD NAME="CreateGoalSettings"TYPE="uint16"CLASSORIGIN="CIM_Capabilities"PROPAGATED="true">
                        <QUALIFIER NAME="Experimental"TYPE="boolean"TOSUBCLASS="false">
                            <VALUE>true</VALUE>
                        </QUALIFIER>
                        [...]                             
                        <QUALIFIER NAME="ValueMap"TYPE="string">
                            <VALUE.ARRAY>
                                <VALUE>0</VALUE>
                                <VALUE>1</VALUE>
                                <VALUE>2</VALUE>
                                <VALUE>3</VALUE>
                                <VALUE>4</VALUE>
                                <VALUE>5</VALUE>
                                <VALUE>6</VALUE>
                                <VALUE>..</VALUE>
                                <VALUE>32768..65535</VALUE>
                            </VALUE.ARRAY>
                        </QUALIFIER>                             
                        <PARAMETER.ARRAY TYPE="string"NAME="TemplateGoalSettings">
                            <QUALIFIER NAME="IN"TYPE="boolean"OVERRIDABLE="false">
                                <VALUE>true</VALUE>
                            </QUALIFIER>
                            [...] 
                        </PARAMETER.ARRAY>                             
                     </METHOD>
                     [...]
             */
            #endregion

            MatchElement(CimXmlElementType.MethodStart);

            CimMethod newCimMethod = new CimMethod(string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        newCimMethod.Name = _mainXmlTextReader.Value;
                        break;

                    case "classorigin":                        
                        newCimMethod.ClassOrigin = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimMethod.Type = _mainXmlTextReader.Value;
                        break;

                    case "propagated":
                        newCimMethod.IsPropagated = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the attributes

            while (this.ElementType != CimXmlElementType.MethodEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.QualifierStart:
                        newCimMethod.Qualifiers.Add(ReadQualifier());                        
                        break;

                    case CimXmlElementType.ParameterStart:
                        newCimMethod.Parameters.Add(ReadParameter());                        
                        break;

                    case CimXmlElementType.ParameterArrayStart:
                        newCimMethod.Parameters.Add(ReadParameterArray());                        
                        break;

                    case CimXmlElementType.ParameterReferenceStart:
                        newCimMethod.Parameters.Add(ReadParameterReference());                        
                        break;

                    case CimXmlElementType.ParameterReferenceArrayStart:
                        newCimMethod.Parameters.Add(ReadParameterReferenceArray());                        
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }

            }


            // Consume the closing element
            ReadElement(CimXmlElementType.MethodEnd);

            return newCimMethod;
        }
        #endregion

        #region Parameter
        public CimParameter ReadParameter()
        {
            #region DTD
            /*
             <!ELEMENT PARAMETER (QUALIFIER*)> 
             <!ATTLIST PARAMETER 
                    %CIMName; 
                    %CIMType; #REQUIRED> 
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                     <PARAMETER.ARRAY TYPE="string"NAME="TemplateGoalSettings">
                         <QUALIFIER NAME="IN"TYPE="boolean"OVERRIDABLE="false">
                             <VALUE>true</VALUE>
                         </QUALIFIER>
                         [...] 
                     </PARAMETER.ARRAY> 
                     [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ParameterStart);

            CimParameter newCimParameter = new CimParameter(CimType.INVALID, string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":                        
                        newCimParameter.Name = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimParameter.Type = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement();

            while (this.ElementType != CimXmlElementType.ParameterEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.QualifierStart:
                        newCimParameter.Qualifiers.Add(ReadQualifier());                        
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }

            }

            // Consume the closing element
            ReadElement(CimXmlElementType.ParameterEnd);

            return newCimParameter;
        }
        #endregion

        #region ParameterArray
        public CimParameterArray ReadParameterArray()
        {
            #region DTD
            /*
             <!ELEMENT PARAMETER.ARRAY (QUALIFIER*)> 
             <!ATTLIST PARAMETER.ARRAY 
                    %CIMName; 
                    %CIMType; #REQUIRED 
                    %ArraySize;> 
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                     <PARAMETER.ARRAY TYPE="string"NAME="TemplateGoalSettings">
                         <QUALIFIER NAME="IN"TYPE="boolean"OVERRIDABLE="false">
                             <VALUE>true</VALUE>
                         </QUALIFIER>
                         [...] 
                     </PARAMETER.ARRAY> 
                     [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ParameterArrayStart);

            CimParameterArray newCimParameterArray = new CimParameterArray(CimType.INVALID, string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        newCimParameterArray.Name = _mainXmlTextReader.Value;
                        break;

                    case "type":
                        newCimParameterArray.Type = _mainXmlTextReader.Value;
                        break;

                    case "arraysize":
                        newCimParameterArray.ArraySize = _mainXmlTextReader.Value;
                        break;                        

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement();

            while (this.ElementType != CimXmlElementType.ParameterArrayEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.QualifierStart:
                        newCimParameterArray.Qualifiers.Add(ReadQualifier());                        
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }

            }

            // Consume the closing element
            ReadElement(CimXmlElementType.ParameterArrayEnd);

            return newCimParameterArray;
        }
        #endregion

        #region ParameterReference
        public CimParameterReference ReadParameterReference()
        {
            #region DTD
            /*
             <!ELEMENT PARAMETER.REFERENCE (QUALIFIER*)> 
             <!ATTLIST PARAMETER.REFERENCE 
                    %CIMName; 
                    %ReferenceClass;> 
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ParameterReferenceStart);

            CimParameterReference paramRef = new CimParameterReference(string.Empty);
            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        paramRef.Name = _mainXmlTextReader.Value;
                        break;

                    case "referenceclass":
                        paramRef.ReferenceClass = _mainXmlTextReader.Value;
                        break;
                    default:
                        throw new Exception("Not implemented yet");
                }
            }
            this.NextElement();
            
            paramRef.Qualifiers.AddRange(ReadQualifierList());

            // Consume the closing element
            ReadElement(CimXmlElementType.ParameterReferenceEnd);

            return paramRef;
        }
       

        #endregion

        #region ParameterReferenceArray
        public CimParameterRefArray ReadParameterReferenceArray()
        {
            #region DTD
            /*
             <!ELEMENT PARAMETER.REFARRAY (QUALIFIER*)> 
             <!ATTLIST PARAMETER.REFARRAY 
                    %CIMName; 
                    %ReferenceClass; 
                    %ArraySize;> 
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ParameterReferenceArrayStart);
            CimParameterRefArray refArray = new CimParameterRefArray(string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        refArray.Name = _mainXmlTextReader.Value;
                        break;

                    case "referenceclass":
                        refArray.ReferenceClass = _mainXmlTextReader.Value;
                        break;
                    case "arraysize":
                        refArray.ArraySize = _mainXmlTextReader.Value;
                        break;
                }
            }
            this.NextElement();

            refArray.Qualifiers.AddRange(ReadQualifierList());

            // Consume the closing element
            ReadElement(CimXmlElementType.ParameterReferenceArrayEnd);

            return refArray;
        }
        #endregion

        #region ReadParameterValue
        public CimParameterValue ReadParameterValue()
        {
            #region DTD
            /*
                <!ELEMENT PARAMVALUE (VALUE|VALUE.REFERENCE|VALUE.ARRAY|VALUE.REFARRAY)?>
                <!ATTLIST PARAMVALUE
                    %CIMName;
                    %ParamType;    #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
                    [...]   
                    <PARAMVALUE NAME="b" PARAMTYPE="boolean">
                        <VALUE>true</VALUE> 
                    </PARAMVALUE>
                    <PARAMVALUE NAME="r64" 	PARAMTYPE="real64">
                        <VALUE>9.8765432109876504e+32</VALUE> 
                    </PARAMVALUE>
                    [...]
             */
            #endregion

            MatchElement(CimXmlElementType.ParameterValueStart);

            CimParameterValue retVal = new CimParameterValue(null, null);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        retVal.Name = _mainXmlTextReader.Value;
                        break;

                    case "paramtype":
                        retVal.Type = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            NextElement();  // Move off of the attributes  

            //if (! retVal.Type.IsSet)
            //    throw (new Exception("Element doesn't have the 'ParamType' Attribute"));


            switch (this.ElementType)
            {
                case CimXmlElementType.ValueStart:
                    retVal.ValueArray.Add(ReadValue());
                    break;

                case CimXmlElementType.ValueReferenceStart:
                    throw new Exception("Not implemented yet");
                    // CimValueReference doesn't mix well with CimValueList
                    //retVal.ValueArray.Add(ReadValueReference());
                    break;

                case CimXmlElementType.ValueArrayStart:
                    retVal.ValueArray.AddRange(ReadValueArray());
                    break;

                case CimXmlElementType.ValueReferenceArrayStart:                    
                     throw new Exception("Not implemented yet");
                     // ReadValueReferenceArray not done yet
                    //retVal.ValueArray.AddRange(ReadValueReferenceArray());
                    break;

                case CimXmlElementType.ParameterValueEnd:
                    // No value elements, so just break out
                    break;

                default:
                    throw new Exception("Not implemented yet");
            }            

            // Consume the closing element
            ReadElement(CimXmlElementType.ParameterValueEnd);

            return retVal;
        }
        #endregion ReadParameterValueList
        public CimParameterValueList ReadParameterValueList()
        {
            CimParameterValueList retVal = new CimParameterValueList();

            do
            {
                retVal.Add(ReadParameterValue());
            }
            while (this.ElementType == CimXmlElementType.ParameterValueStart);

            return retVal;
        }
        #endregion

        #region

        #region Value
        public string ReadValue()
        {
            #region DTD
            /*
             <!ELEMENT VALUE (#PCDATA)> 
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                         <VALUE>0</VALUE>
                     [...]
             */
            #endregion

            ReadElement(CimXmlElementType.ValueStart);

            string value = null;
            if (ElementType == CimXmlElementType.TextValue)
            {
                value = this.Value;
                this.NextElement();
            }
            else
                value = string.Empty;

            // Consume the closing element
            ReadElement(CimXmlElementType.ValueEnd);

            return value;
        }
        #endregion

        #region ValueArray
        public CimValueList ReadValueArray()
        {
            #region DTD
            /*
             <!ELEMENT VALUE.ARRAY (VALUE*)> 
            */
            #endregion

            #region Actual Xml Response
            /*
                     [...]   
                     <VALUE.ARRAY>
                         <VALUE>0</VALUE>
                         <VALUE>1</VALUE>
                         <VALUE>2</VALUE>
                         <VALUE>3</VALUE>
                         <VALUE>4</VALUE>
                         <VALUE>5</VALUE>
                         <VALUE>6</VALUE>
                         <VALUE>..</VALUE>
                         <VALUE>32768..65535</VALUE>
                     </VALUE.ARRAY>
                     [...]
             */
            #endregion

            ReadElement(CimXmlElementType.ValueArrayStart);

            //this.NextElement();

            CimValueList newCimValueList = new CimValueList();

            while (this.ElementType != CimXmlElementType.ValueArrayEnd)
            {
                newCimValueList.Add(ReadValue());                
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.ValueArrayEnd);

            return newCimValueList;
        }
        #endregion

        #region ValueNamedInstance
        public CimInstance ReadValueNamedInstance()
        {
            #region DTD
            /*
             <!ELEMENT VALUE.NAMEDINSTANCE (INSTANCENAME,INSTANCE)> 
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <IRETURNVALUE>
                <VALUE.NAMEDINSTANCE>
                    <INSTANCENAME CLASSNAME="OMC_InstalledOS">
                        [...]
                    </INSTANCENAME>
                    <INSTANCE CLASSNAME="OMC_InstalledOS">
                        [...]
                    </INSTANCE>
                </VALUE.NAMEDINSTANCE>
            </IRETURNVALUE>
            [...]
            */
            #endregion

            ReadElement(CimXmlElementType.ValueNamedInstanceStart);


            CimInstance ci = null;
            CimInstanceName cin = null;

            while (this.ElementType != CimXmlElementType.ValueNamedInstanceEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.InstanceNameStart:
                        cin = ReadInstanceName();
                        break;

                    case CimXmlElementType.InstanceStart:
                        ci = ReadInstance();
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            if (ci.ClassName != cin.ClassName)
                throw new Exception("InstanceName.ClassName != Instance.ClassName");

            ci.InstanceName = cin;

            // Consume the closing element
            ReadElement(CimXmlElementType.ValueNamedInstanceEnd);

            return ci;
        }
        #endregion

        #region ReadInstanceName
        public CimInstanceName ReadInstanceName()
        {
            #region DTD
            /*
             <!ELEMENT INSTANCENAME (KEYBINDING*|KEYVALUE?|VALUE.REFERENCE?)> 
             <!ATTLIST INSTANCENAME 
                %ClassName;> 
            */
            #endregion

            #region Actual Xml Response
            /*
                    [...]
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
                        [...]
                    </INSTANCENAME>
                    [...]
            */
            #endregion

            MatchElement(CimXmlElementType.InstanceNameStart);

            CimInstanceName newCimInstanceName = new CimInstanceName(string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "classname":
                        newCimInstanceName.ClassName = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the Attributes

            while (this.ElementType != CimXmlElementType.InstanceNameEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.KeyBindingStart:
                        if (newCimInstanceName.KeyBindings == null)
                        {
                            newCimInstanceName.KeyBindings = new CimKeyBindingList();
                        }

                        ((CimKeyBindingList)newCimInstanceName.KeyBindings).Add(ReadKeyBinding()); 
                        break;

                    case CimXmlElementType.KeyValueStart:
                        if (newCimInstanceName.KeyBindings == null)
                        {
                            newCimInstanceName.KeyBindings = new CimKeyBindingList();
                        }

                        ((CimKeyBindingList)newCimInstanceName.KeyBindings).Add(new CimKeyBinding(new CimName("KeyValue"), ReadKeyValue()));
                        break;

                    case CimXmlElementType.ValueReferenceStart:
                        if (newCimInstanceName.KeyBindings == null)
                        {
                            newCimInstanceName.KeyBindings = new CimKeyBindingList();
                        }
                                                
                        ((CimKeyBindingList)newCimInstanceName.KeyBindings).Add(new CimKeyBinding(new CimName("ValueReference"), ReadValueReference()));
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.InstanceNameEnd);

            return newCimInstanceName;
        }
        #endregion

        #region ReadInstance
        public CimInstance ReadInstance()
        {
            #region DTD
            /* 
             <!ELEMENT INSTANCE (QUALIFIER*,(PROPERTY|PROPERTY.ARRAY|PROPERTY.REFERENCE)*)>
             <!ATTLIST INSTANCE 
                    %ClassName;
                    xml:lang   NMTOKEN  #IMPLIED>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
                    <INSTANCE CLASSNAME="OMC_InstalledOS">
                        <PROPERTY.REFERENCE NAME="GroupComponent"REFERENCECLASS="CIM_ComputerSystem"CLASSORIGIN="OMC_InstalledOS">
                        [...]
                        </PROPERTY.REFERENCE>
                        <PROPERTY.REFERENCE NAME="PartComponent"REFERENCECLASS="OMC_OperatingSystem"CLASSORIGIN="OMC_InstalledOS">
                        [...]
                        </PROPERTY.REFERENCE>
                        <PROPERTY NAME="PrimaryOS"TYPE="boolean"CLASSORIGIN="CIM_InstalledOS"PROPAGATED="true">
                        [...]
                        </PROPERTY>
                    </INSTANCE>
            [...]
            */
            #endregion
            MatchElement(CimXmlElementType.InstanceStart);

            CimInstance newCimInstance = new CimInstance(string.Empty);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "classname":
                        newCimInstance.ClassName = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the Attributes

            while (this.ElementType != CimXmlElementType.InstanceEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.PropertyStart:
                        newCimInstance.Properties.Add(ReadProperty());
                        break;

                    case CimXmlElementType.QualifierStart:
                        newCimInstance.Qualifiers.Add(ReadQualifier());
                        break;

                    case CimXmlElementType.PropertyArrayStart:
                        newCimInstance.Properties.Add(ReadPropertyArray());
                        break;

                    case CimXmlElementType.PropertyReferenceStart:
                        newCimInstance.Properties.Add(ReadPropertyReference());
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.InstanceEnd);

            return newCimInstance;
        }
        #endregion

        #region ReadKeyBinding
        public CimKeyBinding ReadKeyBinding()
        {
            #region DTD
            /*
             <!ELEMENT KEYBINDING (KEYVALUE|VALUE.REFERENCE)>
             <!ATTLIST KEYBINDING
                    %CIMName;>
             */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <KEYBINDING NAME="CreationClassName">
                <KEYVALUE VALUETYPE="string">OMC_OperatingSystem</KEYVALUE>
            </KEYBINDING>
            [...]
            */
            #endregion

            MatchElement(CimXmlElementType.KeyBindingStart);

            CimKeyBinding newCimKeyBinding = new CimKeyBinding(new CimName(null));

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        newCimKeyBinding.Name = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the Attributes

            switch (this.ElementType)
            {
                case CimXmlElementType.KeyValueStart:
                    newCimKeyBinding.Value = ReadKeyValue();
                    break;

                case CimXmlElementType.ValueReferenceStart:
                    newCimKeyBinding.Value = ReadValueReference();
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.KeyBindingEnd);

            return newCimKeyBinding;
        }
        #endregion

        #region ReadKeyValue
        public CimKeyValue ReadKeyValue()
        {
            #region DTD
            /* 
             <!ELEMENT KEYVALUE (#PCDATA)>
             <!ATTLIST KEYVALUE
                    VALUETYPE    (string|boolean|numeric)  "string"
                    %CIMType;    #IMPLIED>
             */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <KEYVALUE VALUETYPE="string">OMC_OperatingSystem</KEYVALUE>
            [...]
            */
            #endregion

            MatchElement(CimXmlElementType.KeyValueStart);

            CimKeyValue newCimKeyValue = new CimKeyValue();

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "valuetype":
                        newCimKeyValue.ValueType = _mainXmlTextReader.Value;

                        if (newCimKeyValue.ValueType != "string")
                        {
                            throw new Exception("twiest: This is a test to see if anything but string ever comes back.");
                        }
                        break;

                    case "type":
                        newCimKeyValue.Type = _mainXmlTextReader.Value;
                        throw new Exception("twiest: This is a test to see if type is ever set.");
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement(); // Move off of the Attributes

            if (this.ElementType != CimXmlElementType.KeyValueEnd)
            {
                newCimKeyValue.Value = _mainXmlTextReader.Value;
                ReadElement(CimXmlElementType.TextValue);
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.KeyValueEnd);

            return newCimKeyValue;
        }
        #endregion

        #region ReadValueReference
        public CimValueReference ReadValueReference()
        {
            #region DTD
            /* 
             <!ELEMENT VALUE.REFERENCE 
                    (CLASSPATH|LOCALCLASSPATH|CLASSNAME|INSTANCEPATH|LOCALINSTANCEPATH|INSTANCENAME)>
             */
            #endregion

            #region Actual Xml Response
            /*
            [...]
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
            [...]
            */
            #endregion

            ReadElement(CimXmlElementType.ValueReferenceStart);

            CimValueReference newCimValueReference;


            switch (this.ElementType)
            {
                case CimXmlElementType.ClassPathStart:
                    newCimValueReference = new CimValueReference(ReadClassPath());
                    break;

                case CimXmlElementType.LocalClassPathStart:
                    newCimValueReference = new CimValueReference(ReadLocalClassPath());
                    break;

                case CimXmlElementType.ClassNameStart:
                    newCimValueReference = new CimValueReference(ReadClassName());
                    break;

                case CimXmlElementType.InstancePathStart:
                    newCimValueReference = new CimValueReference(ReadInstancePath());
                    break;

                case CimXmlElementType.LocalInstancePathStart:
                    newCimValueReference = new CimValueReference(ReadLocalInstancePath());
                    break;

                case CimXmlElementType.InstanceNameStart:
                    newCimValueReference = new CimValueReference(ReadInstanceName());
                    break;

                default:
                    throw (new Exception("Not implemented yet"));
            }

            // Consume the closing element
            ReadElement(CimXmlElementType.ValueReferenceEnd);

            return newCimValueReference;
        }
        #endregion

        #region ReadClassPath
        public CimClassNamePath ReadClassPath()
        {
            #region DTD
            /* 
             <!ELEMENT CLASSPATH (NAMESPACEPATH,CLASSNAME)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.ClassPathStart);

            CimClassNamePath newClassNamePath = new CimClassNamePath();

            while (this.ElementType != CimXmlElementType.ClassPathEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.NamespacePathStart:
                        newClassNamePath.NamespacePath = ReadNamespacePath();
                        break;

                    case CimXmlElementType.ClassNameStart:
                        newClassNamePath.ClassName = ReadClassName();
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            ReadElement(CimXmlElementType.ClassPathEnd);

            return newClassNamePath;
        }
        #endregion

        #region ReadLocalClassPath
        public CimClassPath ReadLocalClassPath()
        {
            #region DTD
            /*
             <!ELEMENT LOCALCLASSPATH (LOCALNAMESPACEPATH, CLASSNAME)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.LocalClassPathStart);

            CimClassPath newCimClassPath = new CimClassPath();

            while (this.ElementType != CimXmlElementType.LocalClassPathEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.LocalNamespacePathStart:
                        newCimClassPath.Namespace = ReadLocalNamespacePath();
                        break;

                    case CimXmlElementType.ClassNameStart:
                        newCimClassPath.Class.ClassName = ReadClassName();
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            ReadElement(CimXmlElementType.LocalClassPathEnd);

            return newCimClassPath;
        }
        #endregion

        #region ReadInstancePath
        public CimInstanceNamePath ReadInstancePath()
        {
            #region DTD
            /* 
             <!ELEMENT INSTANCEPATH (NAMESPACEPATH,INSTANCENAME)>
            */
            #endregion

            #region Actual Xml Response
            /*
            <INSTANCEPATH>
                <NAMESPACEPATH>
	                <HOST>127.0.0.1</HOST>
	                <LOCALNAMESPACEPATH>
		                <NAMESPACE NAME="root"/>
		                <NAMESPACE NAME="cimv2"/>
	                </LOCALNAMESPACEPATH>
                </NAMESPACEPATH>
                <INSTANCENAME CLASSNAME="OMC_SyslogNGRecordLog">
	                <KEYBINDING NAME="InstanceID">
		                <KEYVALUE VALUETYPE="string">OMCSyslogNGRecordLog:/var/log/news/news.crit</KEYVALUE>
	                </KEYBINDING>
                </INSTANCENAME>
            </INSTANCEPATH>
             * 
             */
            #endregion

            ReadElement(CimXmlElementType.InstancePathStart);

            CimInstanceNamePath newInstanceNamePath = new CimInstanceNamePath();

            while (this.ElementType != CimXmlElementType.InstancePathEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.LocalNamespacePathStart:
                    case CimXmlElementType.NamespacePathStart:
                        newInstanceNamePath.NamespacePath = ReadNamespacePath();
                        break;

                    case CimXmlElementType.ClassNameStart:
                    case CimXmlElementType.InstanceNameStart:
                        newInstanceNamePath.InstanceName = ReadInstanceName();                       
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            ReadElement(CimXmlElementType.InstancePathEnd);

            return newInstanceNamePath;
        }
        #endregion

        #region ReadLocalInstancePath
        public CimInstanceNamePath ReadLocalInstancePath()
        {
            #region DTD
            /* 
             <!ELEMENT LOCALINSTANCEPATH (LOCALNAMESPACEPATH,INSTANCENAME)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.LocalInstancePathStart);
                        
            CimInstanceNamePath newCimInstanceNamePath = new CimInstanceNamePath();

            while (this.ElementType != CimXmlElementType.LocalInstancePathEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.LocalNamespacePathStart:
                        newCimInstanceNamePath.Namespace = ReadLocalNamespacePath();
                        break;

                    case CimXmlElementType.InstanceNameStart:
                        //newCimInstancePath.Instance = ReadInstance();
                        newCimInstanceNamePath.InstanceName = ReadInstanceName();                        
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            ReadElement(CimXmlElementType.LocalInstancePathEnd);

            return newCimInstanceNamePath;
        }
        #endregion

        #region ReadNamespacePath
        public CimNamespacePath ReadNamespacePath()
        {
            #region DTD
            /* 
             <!ELEMENT NAMESPACEPATH (HOST,LOCALNAMESPACEPATH)> 
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
             */
            #endregion

            ReadElement(CimXmlElementType.NamespacePathStart);

            CimNamespacePath newCimLocalInstancePath = new CimNamespacePath();

            while (this.ElementType != CimXmlElementType.NamespacePathEnd)
            {
                switch (this.ElementType)
                {
                    case CimXmlElementType.HostStart:
                        newCimLocalInstancePath.Host = ReadHost();
                        break;

                    case CimXmlElementType.LocalNamespacePathStart:
                        newCimLocalInstancePath.Namespace = ReadLocalNamespacePath();
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            ReadElement(CimXmlElementType.NamespacePathEnd);

            return newCimLocalInstancePath;
        }
        #endregion

        #region ReadHost
        public string ReadHost()
        {
            #region DTD
            /* 
             <!ELEMENT HOST (#PCDATA)>
            */
            #endregion

            #region Actual Xml Response
            /*
            <HOST>127.0.0.1</HOST>
             */
            #endregion
            ReadElement(CimXmlElementType.HostStart);
            string host;

            if (ElementType == CimXmlElementType.TextValue)
            {
                host = this.Value;
                this.NextElement();
            }
            else
                host = string.Empty;

            ReadElement(CimXmlElementType.HostEnd);
            return host;
        }
        #endregion

        #region ReadLocalNamespacePath
        public string ReadLocalNamespacePath()
        {
            #region DTD
            /* 
             <!ELEMENT LOCALNAMESPACEPATH (NAMESPACE+)>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <LOCALNAMESPACEPATH>
                <NAMESPACE NAME="root"/>
                <NAMESPACE NAME="cimv2"/>
            </LOCALNAMESPACEPATH>
            [...]
             */
            #endregion

            string retVal = string.Empty;

            ReadElement(CimXmlElementType.LocalNamespacePathStart);

            do
            {
                retVal += ReadNamespace() + "/";
            }
            while (this.ElementType != CimXmlElementType.LocalNamespacePathEnd);

            ReadElement(CimXmlElementType.LocalNamespacePathEnd);

            return retVal.TrimEnd('/');
        }
        #endregion

        #region ReadNamespace
        public string ReadNamespace()
        {
            #region DTD
            /* 
             <!ELEMENT NAMESPACE EMPTY> 
             <!ATTLIST NAMESPACE 
                    %CIMName;>
            */
            #endregion

            #region Actual Xml Response
            /*
            [...]
            <NAMESPACE NAME="root"/>            
            [...]
             */
            #endregion

            MatchElement(CimXmlElementType.NamespaceStart);

            string retVal = string.Empty;

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        retVal = _mainXmlTextReader.Value;
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }

            this.NextElement();     // Move off of the attributes

            if (this.ElementType == CimXmlElementType.NamespaceEnd)
                this.ReadElement(CimXmlElementType.NamespaceEnd);

            return retVal;
        }
        #endregion

        #region ReadQualifierDeclaration
        public CimQualifierDeclaration ReadQualifierDeclataion()
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
            MatchElement(CimXmlElementType.QualifierDeclarationStart);
            CimQualifierDeclaration qualDec = new CimQualifierDeclaration(CimType.BOOLEAN, null);

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "name":
                        qualDec.Name = new CimName(_mainXmlTextReader.Value);
                        break;

                    case "type":
                        qualDec.Type = CimTypeUtils.StrToCimType(_mainXmlTextReader.Value);
                        break;

                    case "isarray":
                        qualDec.IsArray = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "tosubclass":
                        qualDec.QualifierFlavor.ToSubClass = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "translatable":
                        qualDec.QualifierFlavor.Translatable = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "overridable":
                        qualDec.QualifierFlavor.Overridable = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    default:
                        throw (new Exception("Not implemented yet"));
                }
            }
            this.NextElement();

            if (this.ElementType == CimXmlReader.CimXmlElementType.ScopeStart)
            {
                qualDec.Scope = ReadScope();
            }

            if (this.ElementType != CimXmlReader.CimXmlElementType.QualifierDeclarationEnd)
            {
                if (this.ElementType == CimXmlReader.CimXmlElementType.ValueStart)
                {
                    qualDec.Values.Add(ReadValue());
                }
                else if (this.ElementType == CimXmlReader.CimXmlElementType.ValueArrayStart)
                {
                    qualDec.Values.AddRange(ReadValueArray());
                }
            }

            ReadElement(CimXmlElementType.QualifierDeclarationEnd);

            return qualDec;
        }
        #endregion

        #region ReadScope
        public CimScope ReadScope()
        {
            #region Actual Xml Response
            /* 
		    <SCOPE CLASS="true" ASSOCIATION="true" INDICATION="true"></SCOPE>
             * */
            #endregion
            MatchElement(CimXmlElementType.ScopeStart);
            CimScope scope = new CimScope();

            for (int i = 0; i < _mainXmlTextReader.AttributeCount; ++i)
            {
                _mainXmlTextReader.MoveToAttribute(i);
                switch (_mainXmlTextReader.Name.ToLower())
                {
                    case "class":
                        scope.IsClass = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "association":
                        scope.IsAssociation = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "reference":
                        scope.IsReference = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "property":
                        scope.IsProperty = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "method":
                        scope.IsMethod = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "parameter":
                        scope.IsParameter = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    case "indication":
                        scope.IsIndication = new NullableBool(_mainXmlTextReader.Value);
                        break;

                    default:
                        throw (new Exception("Invalid attribute for 'CimScope'"));
                }
            }
            this.NextElement();

            this.ReadElement(CimXmlElementType.ScopeEnd);
            return scope;
        }
        #endregion

        #region ReadValueObjectWithPath
        /// <summary>
        /// Associators and References
        /// </summary>
        /// <returns></returns>
        public CimObjectPath ReadValueObjectWithPath()
        {
            //throw new Exception("Not implemented yet");
            ReadElement(CimXmlElementType.ValueObjectWithPathStart);
            CimObjectPath retVal;

            if (this.ElementType == CimXmlElementType.ClassPathStart)
            {
                CimClassNamePath cNamePath = ReadClassPath();
                CimClass cimClass = ReadClass();

                retVal = new CimClassPath(cimClass, cNamePath.NamespacePath);

            }
            else if (this.ElementType == CimXmlElementType.InstancePathStart)
            {
                CimInstanceNamePath iNamePath = ReadInstancePath();
                CimInstance instance = ReadInstance();
                retVal = new CimInstancePath(instance, iNamePath.NamespacePath);
            }
            else
            {
                throw new Exception("Not implemented yet");
            }

            ReadElement(CimXmlElementType.ValueObjectWithPathEnd);
            return retVal;
        }
        #endregion

        /// <summary>
        /// AssociatorNames and ReferenceNames
        /// </summary>
        /// <returns></returns>
        public CimObjectPath ReadObjectPath()
        {
            /*
            <OBJECTPATH>
                <CLASSPATH>
                    <NAMESPACEPATH>
                        <HOST>d1850.cim.lab.novell.com</HOST>
                        <LOCALNAMESPACEPATH>
                            <NAMESPACE NAME="smash"/>
                        </LOCALNAMESPACEPATH>
                    </NAMESPACEPATH>
                    <CLASSNAME NAME="CIM_Directory"/>
                </CLASSPATH>
            </OBJECTPATH>
            */
            ReadElement(CimXmlElementType.ObjectPathStart);

            CimObjectPath objectPath;
            if (this.ElementType == CimXmlElementType.ClassPathStart)
            {
                objectPath = ReadClassPath();
            }
            else if (this.ElementType == CimXmlElementType.InstancePathStart)
            {
                objectPath = ReadInstancePath();
            }
            else
            {
                throw new Exception("Not implemented yet");
            }
            ReadElement(CimXmlElementType.ObjectPathEnd);
            return objectPath;
        }

        #endregion

        #endregion
    }
}
