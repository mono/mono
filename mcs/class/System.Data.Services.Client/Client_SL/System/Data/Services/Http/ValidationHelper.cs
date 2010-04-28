//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Http
{
    using System;
    using System.Diagnostics;

    internal static class ValidationHelper
    {
        private static readonly char[] HttpTrimCharacters = new char[] { '\t', '\n', '\v', '\f', '\r', ' ' };

        private static readonly char[] InvalidParamChars = new char[]
        { 
            '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '\'', '/', '[', ']', '?', '=',  '{', '}', ' ', '\t', '\r', '\n'
        };

        internal static string CheckBadChars(string name, bool isHeaderValue)
        {
            if (String.IsNullOrEmpty(name))
            {
                if (!isHeaderValue)
                {
                    if (name == null)
                    {
                        throw new ArgumentNullException("name");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.7", name));
                    }
                }
                
                return string.Empty;
            }
            
            if (isHeaderValue)
            {
                name = name.Trim(HttpTrimCharacters);
                int crlf = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    char c = (char)('\x00ff' & name[i]);
                    switch (crlf)
                    {
                        case 0:
                        {
                            if (c != '\r')
                            {
                                break;
                            }

                            crlf = 1;
                            continue;
                        }
                        
                        case 1:
                        {
                            if (c != '\n')
                            {
                                throw new InvalidOperationException(
                                    System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars", name));
                            }
                            
                            crlf = 2;
                            continue;
                        }
                        
                        case 2:
                        {
                            if ((c != ' ') && (c != '\t'))
                            {
                                throw new InvalidOperationException(
                                    System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.2", name));
                            }

                            crlf = 0;
                            continue;
                        }
                        
                        default:
                        {
                            continue;
                        }
                    }
                    
                    if (c == '\n')
                    {
                        crlf = 2;
                    }
                    else if ((c == '\x007f') || ((c < ' ') && (c != '\t')))
                    {
                        throw new InvalidOperationException(
                            System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.3", name));                        
                    }
                }
                
                if (crlf != 0)
                {
                    throw new InvalidOperationException(
                        System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.4", name));                    
                }
                
                return name;
            }
            
            if (name.IndexOfAny(InvalidParamChars) != -1)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.5", name));
            }

            if (ContainsNonAsciiChars(name))
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_InternalArgument("ValidationHelper.CheckBadChars.6", name));                
            }
            
            return name;
        }

        private static bool ContainsNonAsciiChars(string token)
        {
            Debug.Assert(token != null, "token != null");
            for (int i = 0; i < token.Length; i++)
            {
                if ((token[i] < ' ') || (token[i] > '~'))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
