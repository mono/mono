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
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class NameValueFromDictionary : Dictionary<string, List<string>>
    {
        public NameValueFromDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
            Debug.Assert(comparer != null, "comparer != null");
        }

        public void Add(string key, string value)
        {
            Debug.Assert(key != null, "key != null");
            Debug.Assert(value != null, "value != null");

            List<string> valueArray;
            if (this.ContainsKey(key))
            {
                valueArray = base[key];
            }
            else
            {
                valueArray = new List<string>();
            }

            valueArray.Add(value);
            this[key] = valueArray;
        }

        public string Get(string name)
        {
            Debug.Assert(name != null, "name != null");
            string retString = null;
            if (this.ContainsKey(name))
            {
                List<string> valueArray = base[name];
                for (int i = 0; i < valueArray.Count; i++)
                {
                    if (i == 0)
                    {
                        retString = valueArray[i];
                    }
                    else
                    {
                        retString = retString + valueArray[i];
                    }

                    if (i != (valueArray.Count - 1))
                    {
                        retString = retString + ",";
                    }
                }
            }

            return retString;
        }

        public void Set(string key, string value)
        {
            Debug.Assert(key != null, "key != null");
            Debug.Assert(value != null, "value != null");

            List<string> valueArray = new List<string>();
            valueArray.Add(value);
            this[key] = valueArray;
        }
    }
}
