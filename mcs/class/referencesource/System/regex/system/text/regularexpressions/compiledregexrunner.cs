//------------------------------------------------------------------------------
// <copyright file="CompiledRegexRunner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;

#if !SILVERLIGHT && !FULL_AOT_RUNTIME
using System.Reflection.Emit;

namespace System.Text.RegularExpressions {

    internal sealed class CompiledRegexRunner : RegexRunner {
        NoParamDelegate goMethod;
        FindFirstCharDelegate findFirstCharMethod;
        NoParamDelegate initTrackCountMethod;

        internal CompiledRegexRunner() {}

        internal void SetDelegates(NoParamDelegate go, FindFirstCharDelegate firstChar, NoParamDelegate trackCount) {
            goMethod = go;
            findFirstCharMethod = firstChar;
            initTrackCountMethod = trackCount;
        }
        
        protected override void Go() {
            goMethod(this);
        }

        protected override bool FindFirstChar() {
            return findFirstCharMethod(this);
        }

        protected override void InitTrackCount() {
            initTrackCountMethod(this);
        }
    }

    internal delegate void NoParamDelegate(RegexRunner r);
    internal delegate bool FindFirstCharDelegate(RegexRunner r);
    
}

#endif

