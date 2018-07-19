// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.InteropServices.TCEAdapterGen {

    using System;
    internal static class NameSpaceExtractor
    {
        private static char NameSpaceSeperator = '.';
        
        public static String ExtractNameSpace(String FullyQualifiedTypeName)
        {
            int TypeNameStartPos = FullyQualifiedTypeName.LastIndexOf(NameSpaceSeperator);
            if (TypeNameStartPos == -1)
                return "";
            else
                return FullyQualifiedTypeName.Substring(0, TypeNameStartPos);
         }
    }
}
