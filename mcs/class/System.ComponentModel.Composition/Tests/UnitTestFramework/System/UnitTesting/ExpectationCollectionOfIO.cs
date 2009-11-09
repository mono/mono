// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;

namespace System.UnitTesting
{
    public class ExpectationCollection<TInput, TOutput> : Collection<Expectation<TInput, TOutput>>
    {
        public void Add(TInput input, TOutput output)
        {
            Add(new Expectation<TInput, TOutput>(input, output));
        }
    }
}
