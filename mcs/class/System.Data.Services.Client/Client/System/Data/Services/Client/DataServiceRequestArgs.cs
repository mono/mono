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


namespace System.Data.Services.Client
{
    using System.Collections.Generic;

    public class DataServiceRequestArgs
    {
        private readonly Dictionary<string, string> headers;

        public DataServiceRequestArgs()
        {
            this.headers = new Dictionary<string, string>(EqualityComparer<string>.Default);
        }

        public string AcceptContentType
        {
            get { return this.GetHeaderValue(XmlConstants.HttpRequestAccept); }
            set { this.SetHeaderValue(XmlConstants.HttpRequestAccept, value); }
        }

        public string ContentType
        {
            get { return this.GetHeaderValue(XmlConstants.HttpContentType); }
            set { this.SetHeaderValue(XmlConstants.HttpContentType, value);  }
        }

        public string Slug
        {
            get { return this.GetHeaderValue(XmlConstants.HttpSlug); }
            set { this.SetHeaderValue(XmlConstants.HttpSlug, value); }
        }

        public Dictionary<string, string> Headers
        {
            get { return this.headers; }
        }

        private string GetHeaderValue(string header)
        {
            string value;
            if (!this.headers.TryGetValue(header, out value))
            {
                return null;
            }

            return value;
        }

        private void SetHeaderValue(string header, string value)
        {
            if (value == null)
            {
                if (this.headers.ContainsKey(header))
                {
                    this.headers.Remove(header);
                }
            }
            else
            {
                this.headers[header] = value;
            }
        }
    }
}
