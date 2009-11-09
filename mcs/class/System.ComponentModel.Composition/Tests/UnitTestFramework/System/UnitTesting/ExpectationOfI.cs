// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.UnitTesting
{
    public class Expectation<TInputAndOutput> : Expectation<TInputAndOutput, TInputAndOutput>
    {
        public Expectation(TInputAndOutput input, TInputAndOutput output)
            : base(input, output)
        {
        }
    }

}
