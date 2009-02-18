#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbLinq.Util;

namespace DbLinq.Schema.Dbml.Adapter
{
#if MONO_STRICT
    internal
#else
    public
#endif
    class EnumType : IDictionary<string, int>, INamedType
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                UpdateMember();
            }
        }

        private readonly IDictionary<string, int> dictionary;
        private readonly object owner;
        private readonly MemberInfo memberInfo;

        internal static bool IsEnum(string literalType)
        {
            string enumName;
            IDictionary<string, int> values;
            return Extract(literalType, out enumName, out values);
        }

        /// <summary>
        /// Extracts enum name and value from a given string.
        /// The string is in the following form:
        /// enumName key1[=value1]{,keyN[=valueN]}
        /// if enumName is 'enum', then the enum is anonymous
        /// </summary>
        /// <param name="literalType"></param>
        /// <param name="enumName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static bool Extract(string literalType, out string enumName, out IDictionary<string, int> values)
        {
            enumName = null;
            values = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(literalType))
                return false;

            var nameValues = literalType.Split(new[] { ' ' }, 2);
            if (nameValues.Length == 2)
            {
                // extract the name
                string name = nameValues[0].Trim();
                if (!name.IsIdentifier())
                    return false;

                // now extract the values
                IDictionary<string, int> readValues = new Dictionary<string, int>();
                int currentValue = 1;
                var keyValues = nameValues[1].Split(',');
                foreach (var keyValue in keyValues)
                {
                    // a value may indicate its numeric equivalent, or not (in this case, we work the same way as C# enums, with an implicit counter)
                    var keyValueParts = keyValue.Split(new[] { '=' }, 2);
                    var key = keyValueParts[0].Trim();

                    if (!key.IsIdentifier())
                        return false;

                    if (keyValueParts.Length > 1)
                    {
                        if (!int.TryParse(keyValueParts[1], out currentValue))
                            return false;
                    }
                    readValues[key] = currentValue++;
                }
                if (name == "enum")
                    enumName = string.Empty;
                else
                    enumName = name;
                values = readValues;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Does the opposite: creates a literal string from values
        /// </summary>
        private void UpdateMember()
        {
            var pairs = from kvp in dictionary orderby kvp.Value select kvp;
            int currentValue = 1;
            var keyValues = new List<string>();
            foreach (var pair in pairs)
            {
                string keyValue;
                if (pair.Value == currentValue)
                    keyValue = pair.Key;
                else
                {
                    currentValue = pair.Value;
                    keyValue = string.Format("{0}={1}", pair.Key, pair.Value);
                }
                keyValues.Add(keyValue);
                currentValue++;
            }
            string literalType = string.IsNullOrEmpty(Name) ? "enum" : Name;
            literalType += " ";
            literalType += string.Join(", ", keyValues.ToArray());
            MemberInfoExtensions.SetMemberValue(memberInfo, owner, literalType);
        }

        internal EnumType(object owner, MemberInfo memberInfo)
        {
            this.owner = owner;
            this.memberInfo = memberInfo;
            string name;
            Extract((string)memberInfo.GetMemberValue(owner), out name, out dictionary);
            Name = name;
        }

        #region IDictionary implementation

        public void Add(KeyValuePair<string, int> item)
        {
            dictionary.Add(item);
            UpdateMember();
        }

        public void Clear()
        {
            dictionary.Clear();
            UpdateMember();
        }

        public bool Contains(KeyValuePair<string, int> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, int> item)
        {
            bool removed = dictionary.Remove(item);
            UpdateMember();
            return removed;
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return dictionary.IsReadOnly; }
        }

        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Add(string key, int value)
        {
            dictionary.Add(key, value);
            UpdateMember();
        }

        public bool Remove(string key)
        {
            bool removed = dictionary.Remove(key);
            UpdateMember();
            return removed;
        }

        public bool TryGetValue(string key, out int value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public int this[string key]
        {
            get { return dictionary[key]; }
            set
            {
                dictionary[key] = value;
                UpdateMember();
            }
        }

        public ICollection<string> Keys
        {
            get { return dictionary.Keys; }
        }

        public ICollection<int> Values
        {
            get { return dictionary.Values; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, int>>)this).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion
    }
}