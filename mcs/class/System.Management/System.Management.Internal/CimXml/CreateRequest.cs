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
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Management.Internal.Batch;

namespace System.Management.Internal.CimXml
{
    internal class CreateRequest
    {        
        #region ToXml
        public static string ToXml(SingleRequest operation, CimName defaultNamespace)
        {
            return ToXml(new BatchRequest(operation, defaultNamespace));
        }

        public static string ToXml(BatchRequest batch)
        {
            CimXmlWriter cxw = new CimXmlWriter();

            if (batch.Count > 1)
                cxw.WriteMultiReqElement();


            foreach (SingleRequest curOp in batch)
            {
                CimName tmpNameSpace = string.Empty;

                // Namespace            
                if (curOp.Namespace == null)
                    tmpNameSpace = batch.DefaultNamespace;
                else
                    tmpNameSpace = curOp.Namespace;

                cxw.WriteSimpleReqElement();

                switch (curOp.ReqType)
                {
                    case SingleRequest.RequestType.GetClass:
                        GetClass(cxw, (GetClassOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.GetInstance:
                        GetInstance(cxw, (GetInstanceOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.DeleteClass:
                        DeleteClass(cxw, (DeleteClassOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.DeleteInstance:
                        DeleteInstance(cxw, (DeleteInstanceOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.CreateClass:
                        CreateClass(cxw, (CreateClassOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.ModifyClass:
                        ModifyClass(cxw, (ModifyClassOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.ModifyInstance:
                        ModifyInstance(cxw, (ModifyInstanceOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.EnumerateClasses:
                        EnumerateClasses(cxw, (EnumerateClassesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.EnumerateClassNames:
                        EnumerateClassNames(cxw, (EnumerateClassNamesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.EnumerateInstances:
                        EnumerateInstances(cxw, (EnumerateInstancesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.EnumerateInstanceNames:
                        EnumerateInstanceNames(cxw, (EnumerateInstanceNamesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.CreateInstance:
                        CreateInstance(cxw, (CreateInstanceOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.GetProperty:
                        GetProperty(cxw, (GetPropertyOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.InvokeMethod:
                        InvokeMethod(cxw, (InvokeMethodOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.SetProperty:
                        SetProperty(cxw, (SetPropertyOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.GetQualifier:
                        GetQualifier(cxw, (GetQualifierOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.SetQualifier:
                        SetQualifier(cxw, (SetQualifierOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.DeleteQualifier:
                        DeleteQualifier(cxw, (DeleteQualifierOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.EnumerateQualifiers:
                        EnumerateQualifiers(cxw, (EnumerateQualifierOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.ExecuteQuery:
                        ExecuteQuery(cxw, (ExecuteQueryOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.ExecQuery:
                        ExecQuery(cxw, (ExecQueryOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.Associators:
                        Associators(cxw, (AssociatorsOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.AssociatorNames:
                        AssociatorNames(cxw, (AssociatorNamesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.References:
                        References(cxw, (ReferencesOpSettings)curOp, tmpNameSpace);
                        break;

                    case SingleRequest.RequestType.ReferenceNames:
                        ReferenceNames(cxw, (ReferenceNamesOpSettings)curOp, tmpNameSpace);
                        break;


                    default:
                        throw (new Exception("Not Implemented Yet"));                        
                }

                //</SIMPLEREQ> 
                cxw.WriteEndElement();
            }

            if (batch.Count > 1)
                cxw.WriteEndElement();  // </MULTIREQ>

            return cxw.ToString();
        }
        #endregion

        #region 2.3.2.1. GetClass - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to return a single CIM Class from the target Namespace.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void GetClass(CimXmlWriter cxw, GetClassOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
                <MESSAGE ID="1001" PROTOCOLVERSION="1.0">
                    <SIMPLEREQ>
                        <IMETHODCALL NAME="GetClass">
                            <LOCALNAMESPACEPATH>
                                <NAMESPACE NAME="smash"/>
                            </LOCALNAMESPACEPATH>
                            <IPARAMVALUE NAME="IncludeClassOrigin">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="ClassName">
                                <CLASSNAME NAME="CIM_PhysicalAssetCapabilities"/>
                            </IPARAMVALUE><IPARAMVALUE NAME="IncludeQualifiers">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="PropertyList">
                                <VALUE.ARRAY>
                                    <VALUE>Caption</VALUE>
                                    <VALUE>InstanceID</VALUE>
                                    <VALUE>ElementName</VALUE>
                                </VALUE.ARRAY>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="LocalOnly">
                                <VALUE>FALSE</VALUE>
                            </IPARAMVALUE>
                        </IMETHODCALL>
                    </SIMPLEREQ>
                </MESSAGE>
            </CIM>
            */
            #endregion            

            // Set the Method Name
            cxw.WriteIMethodCallStartElement("GetClass");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings            
            cxw.WriteIncludeClassOrigin(settings.IncludeClassOrigin);
            cxw.WriteClassNameParam(settings.ClassName);
            cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            cxw.WriteLocalOnly(settings.LocalOnly);
            cxw.WritePropertyList(settings.PropertyList);


            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.2. GetInstance - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to return a single CIM Instance from the target Namespace.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void GetInstance(CimXmlWriter cxw, GetInstanceOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            [...]
            <IMETHODCALL NAME="GetInstance">
                <LOCALNAMESPACEPATH>
                    <NAMESPACE NAME="smash"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="IncludeClassOrigin">
                    <VALUE>TRUE</VALUE>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="IncludeQualifiers">
                    <VALUE>TRUE</VALUE>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="LocalOnly">
                    <VALUE>FALSE</VALUE>             
                </IPARAMVALUE>
                <IPARAMVALUE NAME="PropertyList">
                    <VALUE.ARRAY>
                        <VALUE>Caption</VALUE>
                        <VALUE>InstanceID</VALUE>
                        <VALUE>ElementName</VALUE>
                    </VALUE.ARRAY>
                </IPARAMVALUE>
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
                    </INSTANCENAME>
                </IPARAMVALUE>
            </IMETHODCALL>
            [...]
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("GetInstance");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings
            cxw.WriteLocalOnly(settings.LocalOnly);
            cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            cxw.WriteIncludeClassOrigin(settings.IncludeClassOrigin);
            cxw.WritePropertyList(settings.PropertyList);
            
            cxw.WriteIParameterElement();                   //  <IPARAMVALUE NAME="InstanceName">
            cxw.WriteCimNameAttributeString("InstanceName");
            cxw.WriteCimInstanceName(settings.InstanceName);   //      <INSTANCENAME CLASSNAME="OMC_InstalledOS">[...]
            cxw.WriteEndElement();                          //  </IPARAMVALUE>
            

            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.3. DeleteClass - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to delete a single CIM Class from the target Namespace.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void DeleteClass(CimXmlWriter cxw, DeleteClassOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
             <MESSAGE ID="87872" PROTOCOLVERSION="1.0">
              <SIMPLEREQ>
               <IMETHODCALL NAME="DeleteClass">
                <LOCALNAMESPACEPATH>
                 <NAMESPACE NAME="root"/>
                 <NAMESPACE NAME="cimv2"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="ClassName"><CLASSNAME NAME="CIM_VideoBIOSElement"/></IPARAMVALUE>
               </IMETHODCALL>
              </SIMPLEREQ>
             </MESSAGE>
            </CIM>
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("DeleteClass");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            //IPARAMVALUE
            cxw.WriteClassNameParam(settings.ClassName);

            //</IMETHODCALL> 
            cxw.WriteEndElement();

        }
        #endregion

        #region 2.3.2.4. DeleteInstance - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to delete a single CIM Instance from the target Namespace.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void DeleteInstance(CimXmlWriter cxw, DeleteInstanceOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
	            <MESSAGE ID="24" PROTOCOLVERSION="1.0">
		            <SIMPLEREQ>
			            <IMETHODCALL NAME="DeleteInstance">
				            <LOCALNAMESPACEPATH>
					            <NAMESPACE NAME="smash"></NAMESPACE>
				            </LOCALNAMESPACEPATH>
				            <IPARAMVALUE NAME="InstanceName">
					            <INSTANCENAME CLASSNAME="CIM_NFS">
						            <KEYBINDING NAME="CSCreationClassName">
							            <KEYVALUE VALUETYPE="string">twiest-3</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="CSName">
							            <KEYVALUE VALUETYPE="string">twiest-3</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="CreationClassName">
							            <KEYVALUE VALUETYPE="string">twiest-3</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="Name">
							            <KEYVALUE VALUETYPE="string">twiest-3</KEYVALUE>
						            </KEYBINDING>
					            </INSTANCENAME>
				            </IPARAMVALUE>
			            </IMETHODCALL>
		            </SIMPLEREQ>
	            </MESSAGE>
            </CIM>
            */
            #endregion


            // Set the Method Name
            //<IMETHODCALL NAME="DeleteInstance">
            cxw.WriteIMethodCallStartElement("DeleteInstance");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            //<IPARAMVALUE NAME="InstanceName">
			cxw.WriteIParameterElement();
            cxw.WriteCimNameAttributeString("InstanceName");

            cxw.WriteCimInstanceName(settings.InstanceName);

            //</IPARAMVALUE>
            cxw.WriteEndElement();					        

            //</IMETHODCALL> 
            cxw.WriteEndElement();

        }
        #endregion

        #region 2.3.2.5. CreateClass - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to create a single CIM Class in the target Namespace. The Class MUST NOT already exist.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void CreateClass(CimXmlWriter cxw, CreateClassOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
             <MESSAGE ID="87872" PROTOCOLVERSION="1.0">
              <SIMPLEREQ>
               <IMETHODCALL NAME="CreateClass">
                <LOCALNAMESPACEPATH>
                 <NAMESPACE NAME="root"/>
                 <NAMESPACE NAME="cimv2"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="NewClass">
                 <CLASS NAME="MySchema_VideoBIOSElement" SUPERCLASS="CIM_VideoBIOSElement">
                  ...
                 </CLASS>
                </IPARAMVALUE>
               </IMETHODCALL>
              </SIMPLEREQ>
             </MESSAGE>
            </CIM>
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("CreateClass");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);
            //<IPARAMVALUE NAME="NewInstance">

            cxw.WriteIParameterElement();
            cxw.WriteCimNameAttributeString("NewClass");

            cxw.WriteCimClass(settings.NewClass);

            //</IPARAMVALUE>
            cxw.WriteEndElement();
            
            //</IMETHODCALL> 
            cxw.WriteEndElement();

        }
        #endregion

        #region 2.3.2.6. CreateInstance - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to create a single CIM Instance in the target Namespace. The Instance MUST NOT already exist.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void CreateInstance(CimXmlWriter cxw, CreateInstanceOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
             * For CreateInstance, DeleteInstance, use the tree under CIM_NFS, OMC_NFS  (but not OMC_LinuxNFS)
             * There are no instances right now, so we instantly know which instances are ours.
             
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
	            <MESSAGE ID="40" PROTOCOLVERSION="1.0">
		            <SIMPLEREQ>
			            <IMETHODCALL NAME="CreateInstance">
				            <LOCALNAMESPACEPATH>
					            <NAMESPACE NAME="smash"></NAMESPACE>
				            </LOCALNAMESPACEPATH>
				            <IPARAMVALUE NAME="NewInstance">
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
				            </IPARAMVALUE>
			            </IMETHODCALL>
		            </SIMPLEREQ>
	            </MESSAGE>
            </CIM>
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("CreateInstance");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            //<IPARAMVALUE NAME="NewInstance">
            cxw.WriteNewInstance();

            // Set the OpSettings 
            cxw.WriteUnnamedCimInstance(settings.Instance);

            //</IPARAMVALUE>
            cxw.WriteEndElement();

            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.7. ModifyClass - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to modify an existing CIM Class in the target Namespace. The Class MUST already exist.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void ModifyClass(CimXmlWriter cxw, ModifyClassOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            */
            #endregion
            
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("ModifyClass");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteCimNameAttributeString("ModifiedClass");

            cxw.WriteCimClass(settings.ModifiedClass);

            //</IPARAMVALUE>
            cxw.WriteEndElement();
            //</IMETHODCALL> 
            cxw.WriteEndElement();

        }
        #endregion

        #region 2.3.2.8. ModifyInstance - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to modify an existing CIM Instance in the target Namespace. The Instance MUST already exist.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void ModifyInstance(CimXmlWriter cxw, ModifyInstanceOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            */
            #endregion
            
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("ModifyInstance"); 

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);
            
            
            //<IPARAMVALUE NAME="ModifiedInstance">
            cxw.WriteIParameterElement();
            cxw.WriteCimNameAttributeString("ModifiedInstance");
            cxw.WriteCimInstance(settings.ModifiedInstance);


            //</IPARAMVALUE>            
            cxw.WriteEndElement();

            cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            
            cxw.WritePropertyList(settings.PropertyList);

            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.9. EnumerateClasses - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to enumerate subclasses of a CIM Class in the target Namespace.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void EnumerateClasses(CimXmlWriter cxw, EnumerateClassesOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
                <MESSAGE ID="1001" PROTOCOLVERSION="1.0">
                    <SIMPLEREQ>
                        <IMETHODCALL NAME="EnumerateClasses">
                            <LOCALNAMESPACEPATH>
                                <NAMESPACE NAME="smash"/>
                            </LOCALNAMESPACEPATH>
                            <IPARAMVALUE NAME="ClassName">
                                <CLASSNAME NAME="CIM_PhysicalAssetCapabilities"/>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="IncludeClassOrigin">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="IncludeQualifiers">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="DeepInheritance">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="LocalOnly">
                                <VALUE>FALSE</VALUE>
                            </IPARAMVALUE>
                        </IMETHODCALL>
                    </SIMPLEREQ>
                </MESSAGE>
            </CIM>
            */
            #endregion
            

            // Set the Method Nam
            cxw.WriteIMethodCallStartElement("EnumerateClasses");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings
            if (settings.ClassName.IsSet)
            {
                cxw.WriteClassNameParam(settings.ClassName);
            }
            cxw.WriteIncludeClassOrigin(settings.IncludeClassOrigin);
            cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            cxw.WriteDeepInheritance(settings.DeepInheritance);
            cxw.WriteLocalOnly(settings.LocalOnly);


            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.10. EnumerateClassNames - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to enumerate the names of subclasses of a CIM Class in the target Namespace.
        /// </summary>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void EnumerateClassNames(CimXmlWriter cxw, EnumerateClassNamesOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
                <MESSAGE ID="1001" PROTOCOLVERSION="1.0">
                    <SIMPLEREQ>
                        <IMETHODCALL NAME="EnumerateClassNames">
                            <LOCALNAMESPACEPATH>
                                <NAMESPACE NAME="smash"/>
                            </LOCALNAMESPACEPATH>
                            <IPARAMVALUE NAME="ClassName">
                                <CLASSNAME NAME="CIM_Component"/>
                            </IPARAMVALUE>
                            <IPARAMVALUE NAME="DeepInheritance">
                                <VALUE>TRUE</VALUE>
                            </IPARAMVALUE>
                        </IMETHODCALL>
                    </SIMPLEREQ>
                </MESSAGE>
            </CIM>
            */
            #endregion

            
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("EnumerateClassNames");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings
            if (settings.ClassName.IsSet)
            {
                cxw.WriteClassNameParam(settings.ClassName);
            }
            cxw.WriteDeepInheritance(settings.DeepInheritance);


            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.11. EnumerateInstances - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to enumerate instances of a CIM Class (this includes instances in the class and any subclasses in accordance with the polymorphic nature of CIM objects) in the target Namespace.
        /// </summary>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void EnumerateInstances(CimXmlWriter cxw, EnumerateInstancesOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <IMETHODCALL NAME="EnumerateInstances">
                <LOCALNAMESPACEPATH>
                    <NAMESPACE NAME="smash"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="IncludeClassOrigin">
                    <VALUE>TRUE</VALUE>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="IncludeQualifiers">
                    <VALUE>TRUE</VALUE>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="PropertyList">
                    <VALUE.ARRAY>
                        <VALUE>Caption</VALUE>
                        <VALUE>InstanceID</VALUE>
                        <VALUE>ElementName</VALUE>
                    </VALUE.ARRAY>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="LocalOnly">
                    <VALUE>FALSE</VALUE>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="ClassName">
                    <CLASSNAME NAME="CIM_Component"/>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="DeepInheritance">
                    <VALUE>TRUE</VALUE>
                </IPARAMVALUE>
            </IMETHODCALL>
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("EnumerateInstances");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings
            cxw.WriteIncludeClassOrigin(settings.IncludeClassOrigin);
            cxw.WriteClassNameParam(settings.ClassName);
            cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            cxw.WriteDeepInheritance(settings.DeepInheritance);
            cxw.WriteLocalOnly(settings.LocalOnly);
            cxw.WritePropertyList(settings.PropertyList);


            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.12. EnumerateInstanceNames - Done
        /// <summary>
        /// <para>From DMTF Spec:</para>This operation is used to enumerate the names (model paths) of the instances of a CIM Class (this includes instances in the class and any subclasses in accordance with the polymorphic nature of CIM objects) in the target Namespace.
        /// </summary>
        /// <param name="defaultNamespace"></param>
        /// <returns>The CimXml request as a string.</returns>
        public static void EnumerateInstanceNames(CimXmlWriter cxw, EnumerateInstanceNamesOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <IMETHODCALL NAME="EnumerateInstanceNames">
                <LOCALNAMESPACEPATH>
                    <NAMESPACE NAME="smash"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="ClassName">
                    <CLASSNAME NAME="CIM_Component"/>
                </IPARAMVALUE>
            </IMETHODCALL>
            */
            #endregion


            // Set the Method Name
            cxw.WriteIMethodCallStartElement("EnumerateInstanceNames");


            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            // Set the OpSettings
            cxw.WriteClassNameParam(settings.ClassName);


            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.13 ExecQuery - Done
        public static void ExecQuery(CimXmlWriter cxw, ExecQueryOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("ExecQuery");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("QueryLanguage");
            cxw.WriteValueString(settings.QueryLanguage);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("Query");
            cxw.WriteValueString(settings.Query);
            cxw.WriteEndElement();//</IPARAMVALUE>


            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.14 Associators - Done
        public static void Associators(CimXmlWriter cxw, AssociatorsOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("Associators");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            // Write out the ObjectName
            //cxw.WriteCimObjectName(settings.ObjectName);

            cxw.WriteCimObjectName(settings);

            if (settings.AssocClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("AssocClass");
                cxw.WriteClassName(settings.AssocClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.ResultClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultClass");
                cxw.WriteClassName(settings.ResultClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.Role != null) && (settings.Role != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("Role");
                cxw.WriteValueString(settings.Role);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.ResultRole != null) && (settings.ResultRole != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultRole");
                cxw.WriteValueString(settings.ResultRole);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.IncludeQualifiers == true)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("IncludeQualifiers");
                cxw.WriteValueString(settings.IncludeQualifiers.ToString());
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.IncludeClassOrigin == true)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("IncludeClassOrigin");
                cxw.WriteValueString(settings.IncludeClassOrigin.ToString());
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.PropertyList != null) && (settings.PropertyList.Length > 0))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("PropertyList");
                cxw.WritePropertyList(settings.PropertyList);
                cxw.WriteEndElement();
            }

            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.15. AssociatorNames - Done
        
        public static void AssociatorNames(CimXmlWriter cxw, AssociatorNamesOpSettings settings, CimName defaultNamespace)
        {
            #region Acutal XML request
            /*
        <?xml version="1.0" encoding="utf-8" ?>
        <CIM CIMVERSION="2.0" DTDVERSION="2.0">
	        <MESSAGE ID="40" PROTOCOLVERSION="1.0">
		        <SIMPLEREQ>
			        <IMETHODCALL NAME="AssociatorNames">
				        <LOCALNAMESPACEPATH>
					        <NAMESPACE NAME="smash"></NAMESPACE>
				        </LOCALNAMESPACEPATH>
				        <IPARAMVALUE NAME="ObjectName">
					        <INSTANCENAME CLASSNAME="CIM_NFS">
						        <KEYBINDING NAME="CSCreationClassName">
							        <KEYVALUE VALUETYPE="string">tCSCreationClassName</KEYVALUE>
						        </KEYBINDING>
						        <KEYBINDING NAME="CSName">
							        <KEYVALUE VALUETYPE="string">tCSName</KEYVALUE>
						        </KEYBINDING>
						        <KEYBINDING NAME="CreationClassName">
							        <KEYVALUE VALUETYPE="string">tCreationClassName</KEYVALUE>
						        </KEYBINDING>
						        <KEYBINDING NAME="Name">
							        <KEYVALUE VALUETYPE="string">tName</KEYVALUE>
						        </KEYBINDING>
					        </INSTANCENAME>
				        </IPARAMVALUE>
			        </IMETHODCALL>
		        </SIMPLEREQ>
	        </MESSAGE>
        </CIM>
         */
            #endregion

            // Set the Method Name
            cxw.WriteIMethodCallStartElement("AssociatorNames");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            // Write out the ObjectName
            cxw.WriteCimObjectName(settings);
            
            if (settings.AssocClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("AssocClass");
                cxw.WriteClassName(settings.AssocClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.ResultClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultClass");
                cxw.WriteClassName(settings.ResultClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.Role != null) && (settings.Role != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("Role");
                cxw.WriteValueString(settings.Role);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.ResultRole != null) && (settings.ResultRole != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultRole");
                cxw.WriteValueString(settings.ResultRole);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.16 References
        public static void References(CimXmlWriter cxw, ReferencesOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("References");// Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            // Write out the ObjectName
            cxw.WriteCimObjectName(settings);

            if (settings.ResultClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultClass");
                cxw.WriteClassName(settings.ResultClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.Role != null) && (settings.Role != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("Role");
                cxw.WriteValueString(settings.Role);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.IncludeQualifiers == true)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("IncludeQualifiers");
                cxw.WriteValueString(settings.IncludeQualifiers.ToString());
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if (settings.IncludeClassOrigin == true)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("IncludeClassOrigin");
                cxw.WriteValueString(settings.IncludeClassOrigin.ToString());
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
            if ((settings.PropertyList != null) && (settings.PropertyList.Length > 0))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("PropertyList");
                cxw.WritePropertyList(settings.PropertyList);
                cxw.WriteEndElement();
            }
            cxw.WriteEndElement();

        }

        #endregion

        #region 2.3.2.17. ReferenceNames
        public static void ReferenceNames(CimXmlWriter cxw, ReferenceNamesOpSettings settings, CimName defaultNamespace)
        {
            #region Acutal XML request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
	            <MESSAGE ID="41" PROTOCOLVERSION="1.0">
		            <SIMPLEREQ>
			            <IMETHODCALL NAME="ReferenceNames">
				            <LOCALNAMESPACEPATH>
					            <NAMESPACE NAME="smash"></NAMESPACE>
				            </LOCALNAMESPACEPATH>
				            <IPARAMVALUE NAME="ObjectName">
					            <INSTANCENAME CLASSNAME="CIM_NFS">
						            <KEYBINDING NAME="CSCreationClassName">
							            <KEYVALUE VALUETYPE="string">tCSCreationClassName</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="CSName">
							            <KEYVALUE VALUETYPE="string">tCSName</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="CreationClassName">
							            <KEYVALUE VALUETYPE="string">tCreationClassName</KEYVALUE>
						            </KEYBINDING>
						            <KEYBINDING NAME="Name">
							            <KEYVALUE VALUETYPE="string">tName</KEYVALUE>
						            </KEYBINDING>
					            </INSTANCENAME>
				            </IPARAMVALUE>
			            </IMETHODCALL>
		            </SIMPLEREQ>
	            </MESSAGE>
            </CIM>
            */
            #endregion

            // Set the Method Name
            cxw.WriteIMethodCallStartElement("ReferenceNames");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            // Write out the ObjectName

            //throw new Exception("Not implemented yet");
            cxw.WriteCimObjectName(settings.ObjectName);

            if (settings.ResultClass != null)
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("ResultClass");
                cxw.WriteClassName(settings.ResultClass);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }

            if ((settings.Role != null) && (settings.Role != string.Empty))
            {
                cxw.WriteIParameterElement();
                cxw.WriteNameAttributeString("Role");
                cxw.WriteValueString(settings.Role);
                cxw.WriteEndElement();//</IPARAMVALUE>
            }
        }

        #endregion

        #region 2.3.2.18. GetProperty - Done
        /// <summary>
        /// Get the value of a property of an InstanceName
        /// </summary>
        /// <param name="cxw"></param>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        public static void GetProperty(CimXmlWriter cxw, GetPropertyOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
             <MESSAGE ID="87872" PROTOCOLVERSION="1.0">
              <SIMPLEREQ>
               <IMETHODCALL NAME="GetProperty">
                <LOCALNAMESPACEPATH>
                 <NAMESPACE NAME="root"/>
                 <NAMESPACE NAME="myNamespace"/>
                </LOCALNAMESPACEPATH>
                <IPARAMVALUE NAME="InstanceName">
                 <INSTANCENAME CLASSNAME="MyDisk">
                  <KEYBINDING NAME="DeviceID"><KEYVALUE>C:</KEYVALUE></KEYBINDING>
                 </INSTANCENAME>
                </IPARAMVALUE>
                <IPARAMVALUE NAME="PropertyName"><VALUE>FreeSpace</VALUE></IPARAMVALUE>
               </IMETHODCALL>
              </SIMPLEREQ>
             </MESSAGE>
            </CIM>
            */
            #endregion

            // Set the Method Name
            cxw.WriteIMethodCallStartElement("GetProperty");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("InstanceName");
            cxw.WriteCimInstanceName(settings.InstanceName);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("PropertyName");
            cxw.WriteValueString(settings.PropertyName);
            cxw.WriteEndElement();//</IPARAMVALUE>
            
            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }

        #endregion

        #region 2.3.2.19 SetProperty - Done
        /// <summary>
        /// Sets the value of a property on the Cimom
        /// </summary>
        /// <param name="cxw"></param>
        /// <param name="settings"></param>
        /// <param name="defaultNamespace"></param>
        public static void SetProperty(CimXmlWriter cxw, SetPropertyOpSettings settings, CimName defaultNamespace)
        {

            // Set the Method Name
            cxw.WriteIMethodCallStartElement("SetProperty");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("InstanceName");
            cxw.WriteCimInstanceName(settings.InstanceName);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("PropertyName");
            cxw.WriteValueString(settings.PropertyName);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("NewValue");
            cxw.WriteValueString(settings.NewValue);
            cxw.WriteEndElement();//</IPARAMVALUE>            

            //</IMETHODCALL> 
            cxw.WriteEndElement();
        }

        #endregion

        #region 2.3.2.20 GetQualifier - Done
        public static void GetQualifier(CimXmlWriter cxw, GetQualifierOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("GetQualifier");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("QualifierName");
            cxw.WriteValueString(settings.QualifierName);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.21 SetQualifier - Done
        public static void SetQualifier(CimXmlWriter cxw, SetQualifierOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("SetQualifier");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("QualifierDeclaration");
            cxw.WriteCimQualifierDeclaration(settings.QualifierDeclaration);

            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.22 DeleteQualifier - Done
        public static void DeleteQualifier(CimXmlWriter cxw, DeleteQualifierOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("DeleteQualifier");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("QualifierName");
            cxw.WriteValueString(settings.QualifierName);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteEndElement();
        }
        #endregion

        #region 2.3.2.23 EnumerateQualifiers - Done
        public static void EnumerateQualifiers(CimXmlWriter cxw, EnumerateQualifierOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("EnumerateQualifiers");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);


            cxw.WriteEndElement();
        }

        #endregion

        #region 2.3.2.24 ExecuteQuery
        public static void ExecuteQuery(CimXmlWriter cxw, ExecuteQueryOpSettings settings, CimName defaultNamespace)
        {
            // Set the Method Name
            cxw.WriteIMethodCallStartElement("ExecuteQuery");

            // Set the namespace
            cxw.WriteCimNamespace(defaultNamespace);

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("QueryLanguage");
            cxw.WriteValueString(settings.QueryLanguage);
            cxw.WriteEndElement();//</IPARAMVALUE>

            cxw.WriteIParameterElement();
            cxw.WriteNameAttributeString("Query");
            cxw.WriteValueString(settings.Query);
            cxw.WriteEndElement();//</IPARAMVALUE>


            cxw.WriteEndElement();
        }
        #endregion

        #region Extra - InvokeMethod
        public static void InvokeMethod(CimXmlWriter cxw, InvokeMethodOpSettings settings, CimName defaultNamespace)
        {
            #region Actual XML Request
            /*      
             *      --- Static Method Call ---
             * 
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
	            <MESSAGE ID="1001" PROTOCOLVERSION="1.0">
		            <SIMPLEREQ>
			            <METHODCALL NAME="KillAll">
				            <LOCALCLASSPATH>
					            <LOCALNAMESPACEPATH>
						            <NAMESPACE NAME="smash"/>
					            </LOCALNAMESPACEPATH>
					            <CLASSNAME NAME="OMC_UnixProcess"/>
				            </LOCALCLASSPATH>
				            <PARAMVALUE NAME="arg" PARAMTYPE="string">
					            <VALUE>watch</VALUE>
				            </PARAMVALUE>
			            </METHODCALL>
		            </SIMPLEREQ>
	            </MESSAGE>
            </CIM>
            */

            /*
             *      --- Method Call ---
             * 
            <?xml version="1.0" encoding="utf-8" ?>
            <CIM CIMVERSION="2.0" DTDVERSION="2.0">
                <MESSAGE ID="1001" PROTOCOLVERSION="1.0">
                    <SIMPLEREQ>
                        <METHODCALL NAME="SendSignal">
                            <LOCALINSTANCEPATH>
                                <LOCALNAMESPACEPATH>
                                    <NAMESPACE NAME="smash"/>
                                </LOCALNAMESPACEPATH>
                                <INSTANCENAME CLASSNAME="OMC_UnixProcess">
                                    <KEYBINDING NAME="Handle">
                                        <KEYVALUE VALUETYPE="string">10669</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="OSCreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_OperatingSystem</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="OSName">
                                        <KEYVALUE VALUETYPE="string">Linux</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="CSCreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="CSName">
                                        <KEYVALUE VALUETYPE="string">d1850.cim.lab.novell.com</KEYVALUE>
                                    </KEYBINDING>
                                    <KEYBINDING NAME="CreationClassName">
                                        <KEYVALUE VALUETYPE="string">OMC_UnixProcess</KEYVALUE>
                                    </KEYBINDING>
                                </INSTANCENAME>
                            </LOCALINSTANCEPATH>
                            <PARAMVALUE NAME="signal" PARAMTYPE="sint32">
                                <VALUE>9</VALUE>
                            </PARAMVALUE>
                        </METHODCALL>
                    </SIMPLEREQ>
                </MESSAGE>
            </CIM>
            */
            #endregion

            // Set the Method Name
            cxw.WriteMethodCallStartElement(settings.MethodName);


            // Write the LocalObjectPath
            if (settings.ObjectName is CimName)
            {
                cxw.WriteCimClassPath((CimName)settings.ObjectName, defaultNamespace);
            }
            else if (settings.ObjectName is CimInstanceName)
            {
                cxw.WriteCimInstancePath((CimInstanceName)settings.ObjectName, defaultNamespace);
            }
            else
            {
                throw new Exception("Not implemented yet");
            }


            // Write the parameter values
            cxw.WriteCimParameterValueList(settings.ParameterList);


            
            //// Set the OpSettings            
            //cxw.WriteIncludeClassOrigin(settings.IncludeClassOrigin);
            //cxw.WriteClassNameParam(settings.ClassName);
            //cxw.WriteIncludeQualifiers(settings.IncludeQualifiers);
            //cxw.WriteLocalOnly(settings.LocalOnly);
            //cxw.WritePropertyList(settings.PropertyList);


            //</METHODCALL> 
            cxw.WriteEndElement();
        }
        #endregion
    }
}
