// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.UnitTesting
{
    public class Expectation<TInput, TOutput>
    {
        public Expectation(TInput input, TOutput output)
        {
            Input = input;
            Output = output;
        }

        public TInput Input
        {
            get;
            private set;
        }

        public TOutput Output
        {
            get;
            private set;
        }
    }
}
