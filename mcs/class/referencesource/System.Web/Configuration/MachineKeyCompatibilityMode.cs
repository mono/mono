//------------------------------------------------------------------------------
// <copyright file="MachineKeyCompatibilityMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;

    public enum MachineKeyCompatibilityMode {

        // 2.0 SP1 mode encryption doesn't use IVs when encrypting data and is included only for legacy reasons.
        Framework20SP1 = 0,

        // 2.0 SP2 mode encryption uses IVs when encrypting data.
        // See: DevDiv Bugs #137864 (http://bugcheck/bugs/DevDivBugs/137864)
        Framework20SP2 = 1,

        // 4.5 mode encryption uses IVs, signing, and a purpose (subkey derivation) to encrypt data.
        // See: DevDiv #48244 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=48244), which the overall Crypto DCR is a reaction to
        // See: DevDiv #87406 (http://vstfdevdiv:8080/DevDiv2/web/wi.aspx?id=87406)
        Framework45 = 2,

    }
}
