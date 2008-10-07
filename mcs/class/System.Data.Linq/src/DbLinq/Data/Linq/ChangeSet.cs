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

using System.Collections.Generic;
#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// Contains list of datacontext entities to be deleted, inserted and updated.
    /// Merges table separate lists into single one.
    /// Standard DLinq class defined in MSDN.
    /// Note: this is immutable and reflects a snapshot of the DataContext when calling GetChangeSet
    /// </summary>
    public sealed class ChangeSet
    {
        private readonly IList<object> inserts;
        public IList<object> Inserts { get { return inserts; } }

        private readonly IList<object> updates;
        public IList<object> Updates { get { return updates; } }

        private readonly IList<object> deletes;
        public IList<object> Deletes { get { return deletes; } }

        public override string ToString()
        {
            return string.Format("Total changes: {{Added: {0}, Removed: {1}, Modified: {2}}}",
                                 Inserts.Count, Deletes.Count, Updates.Count);
        }

        internal ChangeSet(List<object> inserts, List<object> updates, List<object> deletes)
        {
            this.inserts = inserts.AsReadOnly();
            this.updates = updates.AsReadOnly();
            this.deletes = deletes.AsReadOnly();
        }
    };
}
