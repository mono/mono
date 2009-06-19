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
using System.Linq;
using DbLinq.Util;

namespace DbLinq.Schema.Dbml.Adapter
{
    /// <summary>
    /// Wraps a CSV string to an array
    /// </summary>
#if !MONO_STRICT
    public
#endif
    class CsvArrayAdapter : ArrayAdapter<string>
    {
        public CsvArrayAdapter(object o, string fieldName)
            : base(o, fieldName)
        {
        }

        /// <summary>
        /// The value here comes from a CSV string, ie "A,B,C"
        /// So we split around the commas and trim
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable GetValue()
        {
            var values = MemberInfo.GetMemberValue(Owner) as string;
            if (values == null)
                return new string[0];
            var splitValues = values.Split(',');
            var trimmedSplitValues = from v in splitValues select v.Trim();
            return trimmedSplitValues;
        }

        /// <summary>
        /// The value type is an array of string
        /// </summary>
        /// <returns></returns>
        protected override System.Type GetValueType()
        {
            return typeof(string[]);
        }

        /// <summary>
        /// To set a unique value, we just join the parts
        /// </summary>
        /// <param name="value"></param>
        protected override void SetValue(IEnumerable value)
        {
            var values = value.Cast<string>().ToArray();
            var joinedValues = string.Join(",", values);
            MemberInfo.SetMemberValue(Owner, joinedValues);
        }
    }
}
