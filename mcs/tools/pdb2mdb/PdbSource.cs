//-----------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//-----------------------------------------------------------------------------
using System;

namespace Microsoft.Cci.Pdb {
  internal class PdbSource {
    internal uint index;
    internal string name;

    internal PdbSource(uint index, string name) {
      this.index = index;
      this.name = name;
    }
  }
}
