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
using System.Text;

namespace System.Management.Internal
{
    /// <summary>
    /// Represents an erro thrown by the cimom
    /// </summary>
    internal class CimomError
    {
        #region DTD
        /*
             <!ELEMENT ERROR (INSTANCE*) 
             <!ATTLIST ERROR 
                    CODE CDATA #REQUIRED 
                    DESCRIPTION CDATA #IMPLIED>
        */
        #endregion

        #region CimErrorType
        /// <summary>
        /// Errors defined by the DMTF spec
        /// </summary>
        public enum CimErrorType : int
        {
            CIM_ERR_UNKNOWN = 0,
            CIM_ERR_FAILED = 1,
            CIM_ERR_ACCESS_DENIED = 2,
            CIM_ERR_INVALID_NAMESPACE =	3,
            CIM_ERR_INVALID_PARAMETER = 4,
            CIM_ERR_INVALID_CLASS = 5,
            CIM_ERR_NOT_FOUND = 6,
            CIM_ERR_NOT_SUPPORTED = 7,
            CIM_ERR_CLASS_HAS_CHILDREN = 8,
            CIM_ERR_CLASS_HAS_INSTANCES = 9,
            CIM_ERR_INVALID_SUPERCLASS = 10,
            CIM_ERR_ALREADY_EXISTS = 11,
            CIM_ERR_NO_SUCH_PROPERTY = 12,
            CIM_ERR_TYPE_MISMATCH = 13,
            CIM_ERR_QUERY_LANGUAGE_NOT_SUPPORTED = 14,
            CIM_ERR_INVALID_QUERY = 15,
            CIM_ERR_METHOD_NOT_AVAILABLE = 16,
            CIM_ERR_METHOD_NOT_FOUND = 17,
            CIM_ERR_INVALID_RESPONSE_DESTINATION = 19,
            CIM_ERR_NAMESPACE_NOT_EMPTY = 20
        }
        #endregion

        string _errorCode = null;
        string _description = null;
        string _xmlResponse = null;
        CimInstanceList _instances = null;

        #region constructors
        /// <summary>
        /// Creates a new CimomError with the given error code and description
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="description"></param>
        public CimomError(string errorCode, string description)
        {
            Instances = null;
            Description = description;
            ErrorCode = errorCode;
        }
        #endregion

        #region Properties and Indexers
        public string ErrorCode
        {
            get 
            {
                if (_errorCode == null)
                    _errorCode = string.Empty;
                return _errorCode; 
            }
            set { _errorCode = value; }
        }

        public CimErrorType ErrorType
        {
            get
            {
                switch (ErrorCode)
                {
                    case "1":
                        return CimErrorType.CIM_ERR_FAILED;

                    case "2":
                        return CimErrorType.CIM_ERR_ACCESS_DENIED;

                    case "3":
                        return CimErrorType.CIM_ERR_INVALID_NAMESPACE;

                    case "4":
                        return CimErrorType.CIM_ERR_INVALID_PARAMETER;

                    case "5":
                        return CimErrorType.CIM_ERR_INVALID_CLASS;

                    case "6":
                        return CimErrorType.CIM_ERR_NOT_FOUND;

                    case "7":
                        return CimErrorType.CIM_ERR_NOT_SUPPORTED;

                    case "8":
                        return CimErrorType.CIM_ERR_CLASS_HAS_CHILDREN;

                    case "9":
                        return CimErrorType.CIM_ERR_CLASS_HAS_INSTANCES;

                    case "10":
                        return CimErrorType.CIM_ERR_INVALID_SUPERCLASS;

                    case "11":
                        return CimErrorType.CIM_ERR_ALREADY_EXISTS;

                    case "12":
                        return CimErrorType.CIM_ERR_NO_SUCH_PROPERTY;

                    case "13":
                        return CimErrorType.CIM_ERR_TYPE_MISMATCH;

                    case "14":
                        return CimErrorType.CIM_ERR_QUERY_LANGUAGE_NOT_SUPPORTED;

                    case "15":
                        return CimErrorType.CIM_ERR_INVALID_QUERY;

                    case "16":
                        return CimErrorType.CIM_ERR_METHOD_NOT_AVAILABLE;

                    case "17":
                        return CimErrorType.CIM_ERR_METHOD_NOT_FOUND;

                    case "19":
                        return CimErrorType.CIM_ERR_INVALID_RESPONSE_DESTINATION;

                    case "20":
                        return CimErrorType.CIM_ERR_NAMESPACE_NOT_EMPTY;

                    default:
                        return CimErrorType.CIM_ERR_UNKNOWN;
                }
            }
        }

        /// <summary>
        /// Gets or sets the description of the error
        /// </summary>
        public string Description
        {
            get 
            {
                if (_description == null)
                    _description = string.Empty;
                return _description; 
            }
            set { _description = value; }
        }

        /// <summary>
        /// Gets the description of the error as defined by the DMTF spec
        /// </summary>
        public string DescriptionFromSpec
        {
            get
            {
                // From: http://www.dmtf.org/standards/published_documents/DSP0200.html
                switch (ErrorType)
                {
                    case CimErrorType.CIM_ERR_FAILED:
                        return "A general error occurred that is not covered by a more specific error code.";

                    case CimErrorType.CIM_ERR_ACCESS_DENIED:
                        return "Access to a CIM resource is not available to the client.";

                    case CimErrorType.CIM_ERR_INVALID_NAMESPACE:
                        return "The target namespace does not exist.";

                    case CimErrorType.CIM_ERR_INVALID_PARAMETER:
                        return "One or more parameter values passed to the method are not valid.";

                    case CimErrorType.CIM_ERR_INVALID_CLASS:
                        return "The specified class does not exist.";

                    case CimErrorType.CIM_ERR_NOT_FOUND:
                        return "The requested object cannot be found.";

                    case CimErrorType.CIM_ERR_NOT_SUPPORTED:
                        return "The requested operation is not supported.";

                    case CimErrorType.CIM_ERR_CLASS_HAS_CHILDREN:
                        return "The operation cannot be invoked on this class because it has subclasses.";

                    case CimErrorType.CIM_ERR_CLASS_HAS_INSTANCES:
                        return "The operation cannot be invoked on this class because one or more instances of this class exist.";

                    case CimErrorType.CIM_ERR_INVALID_SUPERCLASS:
                        return "The operation cannot be invoked because the specified superclass does not exist.";

                    case CimErrorType.CIM_ERR_ALREADY_EXISTS:
                        return "The operation cannot be invoked because an object already exists.";

                    case CimErrorType.CIM_ERR_NO_SUCH_PROPERTY:
                        return "The specified property does not exist.";

                    case CimErrorType.CIM_ERR_TYPE_MISMATCH:
                        return "The value supplied is not compatible with the type.";

                    case CimErrorType.CIM_ERR_QUERY_LANGUAGE_NOT_SUPPORTED:
                        return "The query language is not recognized or supported.";

                    case CimErrorType.CIM_ERR_INVALID_QUERY:
                        return "The query is not valid for the specified query language.";

                    case CimErrorType.CIM_ERR_METHOD_NOT_AVAILABLE:
                        return "The extrinsic method cannot be invoked.";

                    case CimErrorType.CIM_ERR_METHOD_NOT_FOUND:
                        return "The specified extrinsic method does not exist.";

                    case CimErrorType.CIM_ERR_INVALID_RESPONSE_DESTINATION:
                        return "The specified destination for the asynchrounous response is not valid.";

                    case CimErrorType.CIM_ERR_NAMESPACE_NOT_EMPTY:
                        return "The specified Namespace is not empty.";                        

                    case CimErrorType.CIM_ERR_UNKNOWN:
                        return "Unknown";

                    default:
                        throw(new Exception("Not implemented yet"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the XML response of the error
        /// </summary>
        public string XmlResponse
        {
            get 
            {
                if (_xmlResponse == null)
                    _xmlResponse = string.Empty;
                return _xmlResponse; 
            }
            set { _xmlResponse = value; }
        }       

        public CimInstanceList Instances
        {
            get 
            {
                if (_instances == null)
                    Instances = new CimInstanceList();

                return _instances; 
            }
            set { _instances = value; }
        }

        /// <summary>
        /// Returns true if the description is not empty
        /// </summary>
        public bool IsSet
        {
            get { return (Description != string.Empty); }
        }
        #endregion
    }
}
