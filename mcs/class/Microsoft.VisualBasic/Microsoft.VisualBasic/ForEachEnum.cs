//
// Microsoft.VisualBasic.ForEachEnum.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//      
// (C) 2004 Novell Inc.
// 
//

using System;
using System.Collections;

namespace Microsoft.VisualBasic {
        public class ForEachEnum : IEnumerator
        {
                Microsoft.VisualBasic.Collection collection;
                int index;

                public ForEachEnum (Collection Collection)
                {
                        this.collection = collection;
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
