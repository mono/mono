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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Management.Internal.CimXml;
using System.Management.Internal.Net;
using System.Management.Internal.Batch;
 
namespace System.Management.Internal
{
    #region Delegates
	internal delegate void CimDataTypeHandler(CimXmlHeader header, object CimObject);    
    #endregion

    /// <summary>
    /// 
    /// </summary>
    internal class WbemClient
    {
        protected delegate string FakeCimomHandler(string request);


        string _hostname;
        int _port;
        CimName _defaultNamespace;
        NetworkCredential _credentials;
        bool _isSecure;
        bool _isAuthenticated;
        ICertificatePolicy _certificatePolicy;
        FakeCimomHandler _fakeCimom;

        


        /// <summary>
        /// 
        /// </summary>
        public int TestType;

        #region Constructors
        public WbemClient(string hostname, string username, string password, string defaultNamespace)
            : this(hostname, -1, new NetworkCredential(username, password), defaultNamespace)
        {
        }

        public WbemClient(string hostname, int port, string username, string password, string defaultNamespace)
            : this(hostname, port, new NetworkCredential(username, password), defaultNamespace)
        {
        }

        public WbemClient(string hostname, NetworkCredential credentials, string defaultNamespace)
            : this(hostname, -1, credentials, defaultNamespace)
        {
        }

        public WbemClient(string hostname, int port, NetworkCredential credentials, string defaultNamespace)
        {
            Hostname = hostname;
            Port = port;
            Credentials = credentials;
            DefaultNamespace = defaultNamespace;

            IsSecure = true;
            IsAuthenticated = false;
            FakeCimom = null;
        }
        #endregion

        #region Properties and Indexers
        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }

        public int Port
        {
            get 
            {
                if (_port == -1)
                {
                    if (IsSecure)
                        return 5989;    // Default secure port
                    else
                        return 5988;    // Default non-secure port
                }
                else
                    return _port; 
            }

            set { _port = value; }
        }

        public CimName DefaultNamespace
        {
            get { return _defaultNamespace; }
            set { _defaultNamespace = value; }
        }

        public NetworkCredential Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        public string Username
        {
            get { return Credentials.UserName; }
            set { Credentials.UserName = value; }
        }

        public string Password
        {
            get { return Credentials.Password; }
            set { Credentials.Password = value; }
        }

        public bool IsSecure
        {
            get { return _isSecure; }
            set { _isSecure = value; }
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
            private set { _isAuthenticated = value; }
        }

        protected FakeCimomHandler FakeCimom
        {
            get { return _fakeCimom; }
            set { _fakeCimom = value; }
        }

        public string Url
        {
            get
            {
                string prefix;
                if (this.IsSecure)
                    prefix = "https://";
                else
                    prefix = "http://";

                return (prefix + this.Hostname + ":" + this.Port + "/cimom");
            }
        }

        public ICertificatePolicy CertificatePolicy
        {
            get { return _certificatePolicy; }
            set { _certificatePolicy = value; }
        }
        #endregion

        #region Methods

        #region From Spec
        // http://www.dmtf.org/standards/published_documents/DSP0200.html

        #region 2.3.2.1. GetClass
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public CimClass GetClass(CimName className)
        {
            return GetClass(new GetClassOpSettings(className));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public CimClass GetClass(GetClassOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("GetClass", settings);

            if (response.Value == null)
            {
                return null;
            }

            CheckSingleResponse(response, typeof(CimClassList));

            return ((CimClassList)response.Value)[0];
        }
        #endregion

        #region 2.3.2.2. GetInstance
        public CimInstance GetInstance(CimInstanceName instanceName)
        {
            return GetInstance(new GetInstanceOpSettings(instanceName));
        }

        public CimInstance GetInstance(GetInstanceOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("GetInstance", settings);

            if (response.Value == null)
            {
                return null;
            }

            CheckSingleResponse(response, typeof(CimInstanceList));

            return CombineInstanceAndInstanceName(((CimInstanceList)response.Value)[0], settings.InstanceName);
        }
        #endregion

        #region 2.3.2.3 DeleteClass
        /// <summary>
        /// Deletes a class definition from the cimom
        /// </summary>
        /// <param name="className"></param>
        public void DeleteClass(CimName className)
        {
            DeleteClass(new DeleteClassOpSettings(className));
        }

        /// <summary>
        /// Deletes a class definition from the cimom
        /// </summary>
        /// <param name="settings"></param>
        public void DeleteClass(DeleteClassOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("DeleteClass", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.4. DeleteInstance
        /// <summary>
        /// Deletes the instance from the Cimom
        /// </summary>
        /// <param name="instanceName">Name of the instance to delete</param>
        public void DeleteInstance(CimInstanceName instanceName)
        {
            DeleteInstanceOpSettings settings = new DeleteInstanceOpSettings(instanceName);
            DeleteInstance(settings);
        }

        public void DeleteInstance(DeleteInstanceOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("DeleteInstance", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.5 CreateClass

        /// <summary>
        /// Creates a class on the Cimom
        /// </summary>
        /// <param name="newClass">CimClass to create</param>
        public void CreateClass(CimClass newClass)
        {
            CreateClass(new CreateClassOpSettings(newClass));
        }

        public void CreateClass(CreateClassOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("CreateClass", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }

        #endregion

        #region 2.3.2.6 CreateInstance
        public CimInstance CreateInstance(CimInstance instance)
        {
            return CreateInstance(new CreateInstanceOpSettings(instance));
        }

        public CimInstance CreateInstance(CreateInstanceOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("CreateInstance", settings);

            if (response.Value == null)
            {
                return null;
            }

            CheckSingleResponse(response, typeof(CimInstanceNameList));
            
            return CombineInstanceAndInstanceName(settings.Instance, ((CimInstanceNameList)response.Value)[0]);
        }
        #endregion

        #region 2.3.2.7. ModifyClass
        /// <summary>
        /// Modify a class on the Cimom
        /// </summary>
        /// <param name="modifiedClass">Class to modify</param>
        public void ModifyClass(CimClass modifiedClass)
        {
            ModifyClass(new ModifyClassOpSettings(modifiedClass));
        }

        public void ModifyClass(ModifyClassOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("ModifyClass", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.8. ModifyInstance
        public void ModifyInstance(CimInstance instance)
        {
            ModifyInstanceOpSettings settings = new ModifyInstanceOpSettings(instance);
            ModifyInstance(settings);
        }

        public void ModifyInstance(ModifyInstanceOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("ModifyInstance", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.9. EnumerateClasses

        #region Call Back Versions
        public void EnumerateClasses(CimDataTypeHandler callBack)
        {
            EnumerateClasses(new EnumerateClassesOpSettings(), callBack);
        }

        public void EnumerateClasses(EnumerateClassesOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();
            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("EnumerateClasses", opXml);            
            
            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimClassList EnumerateClasses()
        {
            return EnumerateClasses(new EnumerateClassesOpSettings());
        }

        public CimClassList EnumerateClasses(EnumerateClassesOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("EnumerateClasses", settings);

            if (response.Value == null)
            {
                return new CimClassList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimClassList));

            return (CimClassList)response.Value;
        }
        #endregion

        #region 2.3.2.10. EnumerateClassNames

        #region Call Back Versions
        public void EnumerateClassNames(CimDataTypeHandler callBack)
        {
            EnumerateClassNames(new EnumerateClassNamesOpSettings(), callBack);
        }

        public void EnumerateClassNames(CimName className, CimDataTypeHandler callBack)
        {
            EnumerateClassNames(new EnumerateClassNamesOpSettings(className), callBack);
        }

        public void EnumerateClassNames(EnumerateClassNamesOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();
            string reqXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("EnumerateClassNames", reqXml);
            
            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimNameList EnumerateClassNames()
        {
            return EnumerateClassNames(new EnumerateClassNamesOpSettings());
        }

        public CimNameList EnumerateClassNames(CimName className)
        {
            return EnumerateClassNames(new EnumerateClassNamesOpSettings(className));
        }

        public CimNameList EnumerateClassNames(EnumerateClassNamesOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("EnumerateClassNames", settings);

            if (response.Value == null)
            {
                return new CimNameList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimNameList));


            return (CimNameList)response.Value;
        }
        #endregion 

        #region 2.3.2.11. EnumerateInstances

        #region Call Back Versions
        public void EnumerateInstances(CimName className, CimDataTypeHandler callBack)
        {
            EnumerateInstances(new EnumerateInstancesOpSettings(className), callBack);
        }

        public void EnumerateInstances(EnumerateInstancesOpSettings settings, CimDataTypeHandler callBack)
        {
            string reqXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("EnumerateInstances", reqXml);

            ParseResponse pr = new ParseResponse();

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimInstanceList EnumerateInstances(CimName className)
        {
            return EnumerateInstances(new EnumerateInstancesOpSettings(className));
        }

        public CimInstanceList EnumerateInstances(EnumerateInstancesOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("EnumerateInstances", settings);
                        
            if (response.Value == null)
            {
                return new CimInstanceList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstanceList));

            return (CimInstanceList)response.Value;            
        }
        #endregion

        #region 2.3.2.12. EnumerateInstanceNames

        #region Call Back Versions
        public void EnumerateInstanceNames(CimName className, CimDataTypeHandler callBack)
        {
            EnumerateInstanceNames(new EnumerateInstanceNamesOpSettings(className), callBack);
        }

        public void EnumerateInstanceNames(EnumerateInstanceNamesOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();
            string reqXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("EnumerateInstances", reqXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion
        
        public CimInstanceNameList EnumerateInstanceNames(CimName className)
        {
            return EnumerateInstanceNames(new EnumerateInstanceNamesOpSettings(className));
        }

        public CimInstanceNameList EnumerateInstanceNames(EnumerateInstanceNamesOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("EnumerateInstanceNames", settings);

            if (response.Value == null)
            {
                return new CimInstanceNameList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstanceNameList));

            return (CimInstanceNameList)response.Value;
        }
        #endregion

        #region 2.3.2.13 ExecQuery
        public object ExecQuery(string queryLanguage, string query)
        {
            ExecQueryOpSettings settings = new ExecQueryOpSettings(queryLanguage, query);
            return ExecQuery(settings);
        }

        public object ExecQuery(ExecQueryOpSettings settings)
        {
            // OpenWbem doesn't support this call yet :(

            ////I think this returns a list of ValueObjectWithPath objects
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("ExecQuery", opXml);

            BatchResponse responses = pr.ParseXml(respXml);

            if (responses.Count != 1)
                throw (new Exception("Not a single response to a single request"));
				
			return responses [0].Value;
        }
        #endregion

        #region 2.3.2.14 Associators

        #region 2.3.2.14a Associators with class name

        #region Call Back Versions
        public void Associators(CimName className, CimDataTypeHandler callBack)
        {
            Associators(new AssociatorsWithClassNameOpSettings(className), callBack);
        }

        public void Associators(AssociatorsWithClassNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("Associators", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion


        public CimClassPathList Associators(CimName className)
        {
            return Associators(new AssociatorsWithClassNameOpSettings(className));
        }

        public CimClassPathList Associators(AssociatorsWithClassNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("Associators", settings);

            if (response.Value == null)
            {
                return new CimClassPathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimClassPathList));
            
            return (CimClassPathList)response.Value;
        }
        #endregion

        #region 2.3.2.14b Associators with instance name

        #region Call Back Versions

        public void Associators(CimInstanceName instanceName, CimDataTypeHandler callBack)
        {
            Associators(new AssociatorsWithInstanceNameOpSettings(instanceName), callBack);
        }

        public void Associators(AssociatorsWithInstanceNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("Associators", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimInstancePathList Associators(CimInstanceName instanceName)
        {
            return Associators(new AssociatorsWithInstanceNameOpSettings(instanceName));
        }

        public CimInstancePathList Associators(AssociatorsWithInstanceNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("Associators", settings);

            if (response.Value == null)
            {
                return new CimInstancePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstancePathList));

            return (CimInstancePathList)response.Value;
        }
        #endregion

        #endregion

        #region 2.3.2.15 AssociatorNames

        #region 2.3.2.15a AssociatorNames with class names

        #region Call Back Versions
        public void AssociatorNames(CimName className, CimDataTypeHandler callBack)
        {
            AssociatorNames(new AssociatorNamesWithClassNameOpSettings(className), callBack);
        }

        public void AssociatorNames(AssociatorNamesWithClassNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("AssociatorNames", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimClassNamePathList AssociatorNames(CimName className)
        {
            return AssociatorNames(new AssociatorNamesWithClassNameOpSettings(className));
        }

        public CimClassNamePathList AssociatorNames(AssociatorNamesWithClassNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("AssociatorNames", settings);

            if (response.Value == null)
            {
                return new CimClassNamePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimClassNamePathList));

            return (CimClassNamePathList)response.Value;
        }
        #endregion

        #region 2.3.2.15b AssociatorNames with instance names

        #region Call Back Versions
        public void AssociatorNames(CimInstanceName instanceName, CimDataTypeHandler callBack)
        {
            AssociatorNames(new AssociatorNamesWithInstanceNameOpSettings(instanceName), callBack);
        }

        public void AssociatorNames(AssociatorNamesWithInstanceNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("AssociatorNames", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimInstanceNamePathList AssociatorNames(CimInstanceName instanceName)
        {
            return AssociatorNames(new AssociatorNamesWithInstanceNameOpSettings(instanceName));
        }

        public CimInstanceNamePathList AssociatorNames(AssociatorNamesWithInstanceNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("AssociatorNames", settings);

            if (response.Value == null)
            {
                return new CimInstanceNamePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstanceNamePathList));

            return (CimInstanceNamePathList)response.Value;
        }
        #endregion

        #endregion

        #region 2.3.2.16 References

        #region 2.3.2.16 References with class names

        #region Call Back Versions
        public void References(CimName className, CimDataTypeHandler callBack)
        {
            References(new ReferencesWithClassNameOpSettings(className), callBack);
        }

        public void References(ReferencesWithClassNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("References", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimClassPathList References(CimName className)
        {
            return References(new ReferencesWithClassNameOpSettings(className));
        }

        public CimClassPathList References(ReferencesWithClassNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("References", settings);

            if (response.Value == null)
            {
                return new CimClassPathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimClassPathList));

            return (CimClassPathList)response.Value;
        }
        #endregion

        #region 2.3.2.16 References with instance names

        #region Call Back Versions
        public void References(CimInstanceName instanceName, CimDataTypeHandler callBack)
        {
            References(new ReferencesWithInstanceNameOpSettings(instanceName), callBack);
        }

        public void References(ReferencesWithInstanceNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("References", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimInstancePathList References(CimInstanceName instanceName)
        {
            return References(new ReferencesWithInstanceNameOpSettings(instanceName));
        }

        public CimInstancePathList References(ReferencesWithInstanceNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("References", settings);

            if (response.Value == null)
            {
                return new CimInstancePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstancePathList));

            return (CimInstancePathList)response.Value;
        }
        #endregion

        #endregion

        #region 2.3.2.17 ReferenceNames

        #region 2.3.2.17a ReferenceNames with class names

        #region Call Back Versions
        public void ReferenceNames(CimName className, CimDataTypeHandler callBack)
        {
            ReferenceNames(new ReferenceNamesWithClassNameOpSettings(className), callBack);
        }

        public void ReferenceNames(ReferenceNamesWithClassNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("References", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimClassNamePathList ReferenceNames(CimName className)
        {
            return ReferenceNames(new ReferenceNamesWithClassNameOpSettings(className));
        }

        public CimClassNamePathList ReferenceNames(ReferenceNamesWithClassNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("References", settings);

            if (response.Value == null)
            {
                return new CimClassNamePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimClassNamePathList));

            return (CimClassNamePathList)response.Value;
        }
        #endregion

        #region 2.3.2.17b ReferenceNames with instance names

        #region Call Back Versions

        public void ReferenceNames(CimInstanceName instanceName, CimDataTypeHandler callBack)
        {
            ReferenceNames(new ReferenceNamesWithInstanceNameOpSettings(instanceName), callBack);
        }

        public void ReferenceNames(ReferenceNamesWithInstanceNameOpSettings settings, CimDataTypeHandler callBack)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(settings, this.DefaultNamespace);
            string respXml = ExecuteRequest("References", opXml);

            pr.ParseXml(respXml, callBack);
        }
        #endregion

        public CimInstanceNamePathList ReferenceNames(CimInstanceName instanceName)
        {
            return ReferenceNames(new ReferenceNamesWithInstanceNameOpSettings(instanceName));
        }

        public CimInstanceNamePathList ReferenceNames(ReferenceNamesWithInstanceNameOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("ReferenceNames", settings);

            if (response.Value == null)
            {
                return new CimInstanceNamePathList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimInstanceNamePathList));

            return (CimInstanceNamePathList)response.Value;
        }
        #endregion

        #endregion

        #region 2.3.2.18. GetProperty
        public string GetProperty(CimInstanceName instanceName, string propertyName)
        {
            GetPropertyOpSettings settings = new GetPropertyOpSettings(instanceName, propertyName);
            return GetProperty(settings);
        }

        public string GetProperty(GetPropertyOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("GetProperty", settings);

            if (response.Value == null)
            {
                return string.Empty;
            }

            CheckSingleResponse(response, typeof(string));

            return ((string)response.Value);
        }
        #endregion

        #region 2.3.2.19 SetProperty
        public void SetProperty(CimInstanceName instanceName, string propertyName)
        {
            SetPropertyOpSettings settings = new SetPropertyOpSettings(instanceName, propertyName);
            SetProperty(settings);
        }

        public void SetProperty(CimInstanceName instanceName, string propertyName, string value)
        {
            SetPropertyOpSettings settings = new SetPropertyOpSettings(instanceName, propertyName, value);
            SetProperty(settings);
        }

        public void SetProperty(SetPropertyOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("SetProperty", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.20 GetQualifier
        public CimQualifierDeclaration GetQualifier(string qualifierName)
        {
            return GetQualifier(new GetQualifierOpSettings(qualifierName));
        }

        public CimQualifierDeclaration GetQualifier(GetQualifierOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("GetQualifier", settings);

            if (response.Value == null)
            {
                return null;
            }

            CheckSingleResponse(response, typeof(CimQualifierDeclarationList));

            return ((CimQualifierDeclarationList)response.Value)[0];

        }
        #endregion

        #region 2.3.2.21 SetQualifier
        public void SetQualifier(CimQualifierDeclaration qualifierDeclaration)
        {
            SetQualifier(new SetQualifierOpSettings(qualifierDeclaration));
        }

        public void SetQualifier(SetQualifierOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("SetQualifier", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.22 DeleteQualifier
        public void DeleteQualifier(string qualifierName)
        {
            DeleteQualifier(new DeleteQualifierOpSettings(qualifierName));
        }

        public void DeleteQualifier(DeleteQualifierOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("DeleteQualifier", settings);

            if (response != null)
            {
                // A Cimom error occurred
                CheckSingleResponse(response, typeof(CimomError));
            }
        }
        #endregion

        #region 2.3.2.23 EnumerateQualifiers
        public CimQualifierDeclarationList EnumerateQualifiers()
        {
            return EnumerateQualifiers(new EnumerateQualifierOpSettings());
        }

        public CimQualifierDeclarationList EnumerateQualifiers(EnumerateQualifierOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("DeleteQualifier", settings);

            if (response.Value == null)
            {
                return new CimQualifierDeclarationList();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimQualifierDeclarationList));

            return (CimQualifierDeclarationList)response.Value;
        }
        #endregion

        #region 2.3.2.24 ExecuteQuery
        public CimTable ExecuteQuery(string queryLanguage, string query)
        {
            ExecuteQueryOpSettings settings = new ExecuteQueryOpSettings(queryLanguage, query);
            return ExecuteQuery(settings);
        }

        public CimTable ExecuteQuery(ExecuteQueryOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("ExecuteQuery", settings);

            if (response.Value == null)
            {
                return null;
            }

            CheckSingleResponse(response, typeof(CimQualifierDeclarationList));

            return null;
        }
        #endregion

        #endregion

        #region Extra

        #region EnumerateLeafClasses
        public CimClassList EnumerateLeafClasses()
        {
            return EnumerateLeafClasses(new EnumerateLeafClassesOpSettings());
        }

        public CimClassList EnumerateLeafClasses(EnumerateLeafClassesOpSettings settings)
        {
            CimClassList allClasses = EnumerateClasses(new EnumerateClassesOpSettings(settings));
            CimClassList leafClasses = new CimClassList();
            Dictionary<CimName, bool> superClasses = new Dictionary<CimName, bool>();

            //changed for MONO
            //foreach (CimClass curClass in allClasses)
            for (int i = 0; i < allClasses.Count; ++i)
            {
                CimClass curClass = allClasses[i];
                if (curClass.SuperClass.IsSet)
                {
                    if (!superClasses.ContainsKey(curClass.SuperClass))
                    {
                        superClasses.Add(curClass.SuperClass, true);
                    }
                }
            }

            //foreach (CimClass curClass in allClasses)
            for (int i = 0; i < allClasses.Count; ++i)
            {
                CimClass curClass = allClasses[i];
                if (!superClasses.ContainsKey(curClass.ClassName))
                {
                    leafClasses.Add(curClass);
                }
            }

            return leafClasses;
        }
        #endregion

        #region EnumerateLeafClassNames
        public CimNameList EnumerateLeafClassNames()
        {
            CimClassList leafClasses = EnumerateLeafClasses();
            CimNameList leafClassNames = new CimNameList();

            //changed for MONO
            //foreach (CimClass curLeafClass in leafClasses)
            for (int i = 0; i < leafClasses.Count; ++i)
            {
                leafClassNames.Add(leafClasses[i].ClassName);
            }

            return leafClassNames;
        }
        #endregion

        #region EnumerateClassHierarchy
        public CimTreeNode EnumerateClassHierarchy()
        {
            return EnumerateClassHierarchy(null);
        }

        public CimTreeNode EnumerateClassHierarchy(CimName className)
        {
            EnumerateClassesOpSettings ec = new EnumerateClassesOpSettings(className);
            ec.DeepInheritance = true;
            ec.IncludeClassOrigin = false;
            ec.IncludeQualifiers = false;
            ec.LocalOnly = true;


            CimClassList classList = EnumerateClasses(ec);            
            Dictionary<CimName, CimTreeNode> hash = new Dictionary<CimName, CimTreeNode>();
            
            if (className == null)
            {
                className = this.DefaultNamespace;
            }
            
            hash.Add(className, new CimTreeNode(className));

            for (int i = 0; i < classList.Count; i++)
            {                
                CimClass curClass = classList[i];

                CimTreeNode curNode = new CimTreeNode(curClass.ClassName);

				if (!hash.ContainsKey(curClass.ClassName))
                	hash.Add(curClass.ClassName, curNode);

                if (curClass.SuperClass != string.Empty)
                {
                    if (!hash.ContainsKey(curClass.SuperClass))
                    {
                        hash.Add(curClass.SuperClass, new CimTreeNode(curClass.SuperClass));
                    }
                    hash[curClass.SuperClass].Children.Add(curNode);
                }
                else
                {
                    hash[className].Children.Add(curNode);
                }
            }

            return hash[className];
        }
        #endregion

        #region EnumerateNamespaces
        public string[] EnumerateNamespaces()
        {
            List<string> nsStrings = new List<string>();
            EnumerateInstanceNamesOpSettings eino = new EnumerateInstanceNamesOpSettings("CIM_NamespaceInManager");
            eino.Namespace = "Interop";

            CimInstanceNameList cinl = this.EnumerateInstanceNames(eino);

            //Changed for MONO
            for (int i = 0; i < cinl.Count; ++i)
            {
                CimInstanceName curIN = cinl[i];

                CimKeyBinding tmpKB = curIN.KeyBindings["Dependent"];
                CimInstanceNamePath tmpCLIP = (CimInstanceNamePath)((CimValueReference)tmpKB.Value).CimObject;
                CimKeyValue tmpKV = (CimKeyValue)tmpCLIP.InstanceName.KeyBindings["name"].Value;
                nsStrings.Add(tmpKV.Value);
            }

            return nsStrings.ToArray();
        }
        #endregion

        #region  GetBaseKeyClassName
        /// <summary>
        /// Gets the base class of the class
        /// </summary>
        /// <param name="className"></param>
        /// <returns>Returns the name of the class</returns>
        public CimName GetBaseKeyClassName(string className)
        {
            return GetBaseKeyClassName(new CimName(className));
        }

        /// <summary>
        /// Gets the base class of the class
        /// </summary>
        /// <param name="className"></param>
        /// <returns>Returns the name of the class</returns>
        public CimName GetBaseKeyClassName(CimName className)
        {
            GetClassOpSettings gcop = new GetClassOpSettings(className);
            gcop.LocalOnly = false;

            CimClass curClass = this.GetClass(gcop);

            CimClass startClass = curClass;
            CimName lastClassName = null;   // default value if className doesn't have a BKC

            while (curClass.HasKeyProperty == true)
            {
                lastClassName = curClass.ClassName;

                if (curClass.SuperClass != string.Empty)
                {
                    gcop.ClassName = curClass.SuperClass;

                    curClass = this.GetClass(gcop);
                }
                else
                    break;
            }
            
            return lastClassName;
            
        }
        #endregion

        #region CreateTemplateInstance
        public CimInstance CreateTemplateInstance(CimName className)
        {
            GetClassOpSettings gcos = new GetClassOpSettings(className);
            gcos.IncludeQualifiers = false;
            gcos.LocalOnly = false;
            gcos.IncludeClassOrigin = false;

            CimClass tmpClass = this.GetClass(gcos);

            CimInstance retVal = new CimInstance(className);
            retVal.Properties = tmpClass.Properties;

            return retVal;
        }
        #endregion

        #region ExecuteBatchRequest
        public BatchResponse ExecuteBatchRequest(BatchRequest request)
        {
            ParseResponse pr = new ParseResponse();

            string opXml = CimXml.CreateRequest.ToXml(request);
            string respXml = ExecuteRequest("BatchRequest", opXml);

            return pr.ParseXml(respXml);
        }
        #endregion

        #region InvokeMethod
        public CimMethodResponse InvokeMethod(CimName className, CimName methodName, CimParameterValueList parameterList)
        {
            return InvokeMethod(new InvokeMethodOpSettings(className, methodName, parameterList));
        }

        public CimMethodResponse InvokeMethod(CimInstanceName instanceName, CimName methodName, CimParameterValueList parameterList)
        {
            return InvokeMethod(new InvokeMethodOpSettings(instanceName, methodName, parameterList));
        }

        public CimMethodResponse InvokeMethod(InvokeMethodOpSettings settings)
        {
            SingleResponse response = MakeSingleRequest("InvokeMethod", settings);

            if (response.Value == null)
            {
                return new CimMethodResponse();  // return an empty list                
            }

            CheckSingleResponse(response, typeof(CimMethodResponse));

            return (CimMethodResponse)response.Value;
        }
        #endregion

        #endregion

        #region Helpers

        private string ExecuteRequest(string cimOperation, string cimMessage)
        {
            string response = string.Empty;

            if (FakeCimom != null)
            {
                // Use the fake cimom for unit testing
                response = FakeCimom(cimMessage);
            }
            else
            {
                CimomResponse cimResp = null;

                CimomRequest cimReq;

                cimReq = new CimomRequest(this.Url, this.CertificatePolicy);

                cimReq.Credentials = this.Credentials;

                // Optional
                cimReq.Headers.Add(cimReq.NameSpaceValue.ToString() + "-CIMOperation", "MethodCall");
                cimReq.Headers.Add(cimReq.NameSpaceValue.ToString() + "-CIMMethod", cimOperation);
                cimReq.Headers.Add(cimReq.NameSpaceValue.ToString() + "-CIMObject", this.DefaultNamespace.ToString());

                // Put the message in the send buffer.
                cimReq.Message = cimMessage;

                // Send the buffer, and get the response.
                cimResp = cimReq.GetResponse();

                // Grab the message from the response buffer.
                response = cimResp.Message;

                // See if an error occurred
                if ((int)cimResp.StatusCode != 200)
                    return null;
            }

            return response;
        }

        private SingleResponse MakeSingleRequest(string cimOperation, SingleRequest operation)
        {
            ParseResponse pr = new ParseResponse();

            string reqXml = CimXml.CreateRequest.ToXml(operation, this.DefaultNamespace);
            string respXml = ExecuteRequest(cimOperation, reqXml);

            BatchResponse responses = pr.ParseXml(respXml);

            if (responses == null)
            {
                return new SingleResponse();
            }

            if (responses.Count != 1)
                throw (new Exception("Not a single response to a single request"));

            return responses[0];
        }

        private void CheckSingleResponse(SingleResponse response, Type expectedType)
        {            
            if (response.Value.GetType() == typeof(CimomError))
            {
                string desc = ((CimomError)response.Value).Description;

                if (desc == string.Empty)
                    desc = ((CimomError)response.Value).DescriptionFromSpec;

                // We might want to create a CimomError Exception that incorporates the CimomError object
                throw (new Exception("Cimom Error [" + ((CimomError)response.Value).ErrorCode + "]: " + desc));
            }

            if (response.Value.GetType() != expectedType)
                throw (new Exception("Response didn't match a " + expectedType.ToString()));
        }

        private CimInstance CombineInstanceAndInstanceName(CimInstance instance, CimInstanceName instanceName)
        {
            if (instance.ClassName != instanceName.ClassName)
                throw new Exception("InstanceName.ClassName != Instance.ClassName");

            instance.InstanceName = instanceName;

            return instance;
        }
        #endregion

        #endregion
    }
}
