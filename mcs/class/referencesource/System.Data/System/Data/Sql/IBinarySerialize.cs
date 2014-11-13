//------------------------------------------------------------------------------
//  <copyright file="IBinarySerialize.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">daltudov</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">beysims</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">vadimt</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection.Emit;
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Server {
    
    // This interface is used by types that want full control over the
    // binary serialization format.
    public interface IBinarySerialize {
        // Read from the specified binary reader.
        void Read(BinaryReader r);
        // Write to the specified binary writer.
        void Write(BinaryWriter w);
    }
}

