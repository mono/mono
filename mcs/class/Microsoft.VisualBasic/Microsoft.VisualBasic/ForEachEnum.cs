//
// Microsoft.VisualBasic.ForEachEnum.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//      
// (C) 2004 Novell Inc.
// 
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

namespace Microsoft.VisualBasic {
        internal class ForEachEnum : IEnumerator
        {
                Microsoft.VisualBasic.Collection collection;
                int index;

                public ForEachEnum (Collection Collection)
                {
                        this.collection = Collection;
                }

                public void AdjustIndex (int itemIndex, bool remove)
                {
                        if (itemIndex <= index) {
                                if (remove)
                                        index --;
                                else
                                        index ++;
                        }
                }

                public bool MoveNext ()
                {
                        index ++;
                        if (index > collection.Count)
                                return false;

                        return true;
                }

                public void Reset ()
                {
                        index = 0;
                }

                public object Current {

                        get {
                                if (index > collection.Count)
                                        return null;

                                return collection [index];
                        }
                }
        }
}
