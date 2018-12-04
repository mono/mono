using System;
using System.Collections.Generic;
using System.Text;

namespace MS.Internal
{
    public static class MultiTargetUtilities
    {
        public static void ExecuteIfNetDesktop(Action net4xCode, Action alternateCode=null)
        {
#if NET4X
            net4xCode.Invoke();
#else
            alternateCode?.Invoke();
#endif
        }
    }
}
