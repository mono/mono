//
// Microsoft.VisualBasic.OpenModeTypes.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//      
// (C) 2004 Novell Inc.
// 
//

using System;

namespace Microsoft.VisualBasic {
        public enum OpenModeTypes
        {
                Input = 1,
                Output = 2,
                Random = 4,
                Append = 8,
                Binary = 32,
                Any = -1
        }
}
