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

using System.Linq;
using DbLinq.Schema.Dbml;

namespace DbLinq.Util
{
    /// <summary>
    /// Executes a given SQL command, with parameter and delegate
    /// </summary>
#if MONO_STRICT
    internal
#else
    // TODO: once R# is fixed with internal extension methods problems, switch to full internal
    public // DataCommand is used by vendors
#endif
    static class DbmlExtensions
    {
        /// <summary>
        /// Returns the reverse association
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="association"></param>
        /// <returns></returns>
        public static Association GetReverseAssociation(this Database schema, Association association)
        {
            // we use Single() because we NEED the opposite association
            // (and it must be here
            var reverseTable = (from t in schema.Table where t.Type.Name == association.Type select t).Single();
            // same thing for association
            var reverseAssociation = (from a in reverseTable.Type.Associations
                                      where a.Name == association.Name && a != association
                                      select a).Single();
            return reverseAssociation;
        }
    }
}
