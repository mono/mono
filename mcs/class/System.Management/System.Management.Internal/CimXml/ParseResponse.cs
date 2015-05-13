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
using System.Text;
using System.Management.Internal.Batch;


namespace System.Management.Internal.CimXml
{
    internal class ParseResponse
    {
        BatchResponse responses = null;

        private void InternalCallBack(CimXmlHeader header, object CimObject)
        {
            if (responses == null)
            {
                if (header.IsMultipleResponse)
                    responses = new BatchResponse(header);
                else
                    responses = new BatchResponse(header, 1);   // Save Mem
            }

            while (responses.Count <= header.ResponseNumber)
            {
                responses.Add(new SingleResponse());
                responses.LastResponse.MethodName = header.MethodName;
            }

            if (CimObject is CimName)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimNameList();
                }

                ((CimNameList)responses.LastResponse.Value).Add((CimName)CimObject);
            }
            else if (CimObject is CimClass)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimClassList();
                }

                ((CimClassList)responses.LastResponse.Value).Add((CimClass)CimObject);
            }
            else if (CimObject is CimInstance)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimInstanceList();
                }

                ((CimInstanceList)responses.LastResponse.Value).Add((CimInstance)CimObject);
            }
            else if (CimObject is CimInstanceName)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimInstanceNameList();
                }

                ((CimInstanceNameList)responses.LastResponse.Value).Add((CimInstanceName)CimObject);
            }
            else if (CimObject is string)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = (string)CimObject;
                }
                ((CimValueList)responses.LastResponse.Value).Add((string)CimObject);
            }
            else if (CimObject is CimQualifierDeclaration)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimQualifierDeclarationList();
                }

                ((CimQualifierDeclarationList)responses.LastResponse.Value).Add((CimQualifierDeclaration)CimObject);
            }
            else if (CimObject is CimClassPath)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimClassPathList();
                }

                ((CimClassPathList)responses.LastResponse.Value).Add((CimClassPath)CimObject);
            }
            else if (CimObject is CimInstancePath)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimInstancePathList();
                }

                ((CimInstancePathList)responses.LastResponse.Value).Add((CimInstancePath)CimObject);
            }
            else if (CimObject is CimClassNamePath)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimClassNamePathList();
                }

                ((CimClassNamePathList)responses.LastResponse.Value).Add((CimClassNamePath)CimObject);
            }
            else if (CimObject is CimInstanceNamePath)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = new CimInstanceNamePathList();
                }

                ((CimInstanceNamePathList)responses.LastResponse.Value).Add((CimInstanceNamePath)CimObject);
            }
            else if (CimObject is CimMethodResponse)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = (CimMethodResponse)CimObject;
                }
            }
            else if (CimObject is CimomError)
            {
                if (responses.LastResponse.Value == null)
                {
                    responses.LastResponse.Value = (CimomError)CimObject;
                }
            }
            else if (CimObject == null)
            {
                // Do nothing, take the defaults
            }
            else
            {
                throw new Exception("Not implemented yet");
            }
        }

        public void ParseXml(string xml, CimDataTypeHandler callBack)
        {            
            CimXmlReader cxr = new CimXmlReader(xml);
            
            cxr.ReadCim(callBack);
        }

        public BatchResponse ParseXml(string xml)
        {            
            ParseXml(xml, InternalCallBack);

            return responses;
        }
    }
}
