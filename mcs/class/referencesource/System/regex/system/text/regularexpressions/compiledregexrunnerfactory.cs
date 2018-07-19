//------------------------------------------------------------------------------
// <copyright file="CompiledRegexRunnerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Security.Permissions;

#if !SILVERLIGHT && !FULL_AOT_RUNTIME
using System.Reflection.Emit;

namespace System.Text.RegularExpressions {

    
    internal sealed class CompiledRegexRunnerFactory : RegexRunnerFactory {
        DynamicMethod goMethod;
        DynamicMethod findFirstCharMethod;
        DynamicMethod initTrackCountMethod;

        internal CompiledRegexRunnerFactory (DynamicMethod go, DynamicMethod firstChar, DynamicMethod trackCount) {
            this.goMethod = go;
            this.findFirstCharMethod = firstChar;
            this.initTrackCountMethod = trackCount;
            //Debug.Assert(goMethod != null && findFirstCharMethod != null && initTrackCountMethod != null, "can't be null");
        }
        
        protected internal override RegexRunner CreateInstance() {
            CompiledRegexRunner runner = new CompiledRegexRunner();

#if MONO_FEATURE_CAS
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            runner.SetDelegates((NoParamDelegate)       goMethod.CreateDelegate(typeof(NoParamDelegate)),
                                (FindFirstCharDelegate) findFirstCharMethod.CreateDelegate(typeof(FindFirstCharDelegate)),
                                (NoParamDelegate)       initTrackCountMethod.CreateDelegate(typeof(NoParamDelegate)));

            return runner;
        }
    }

    internal delegate RegexRunner CreateInstanceDelegate();
}

#endif

