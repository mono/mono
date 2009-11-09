// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace System.UnitTesting
{
    public class ExpectationCollection<TInputAndOutput> : Collection<Expectation<TInputAndOutput>>
    {
        public void Add(TInputAndOutput inputAndOutput)
        {
            Add(inputAndOutput, inputAndOutput);
        }

        public void AddRange(IEnumerable<TInputAndOutput> inputAndOutputs)
        {
            foreach (TInputAndOutput inputAndOutput in inputAndOutputs)
            {
                Add(inputAndOutput);
            }
        }

        public void Add(TInputAndOutput input, TInputAndOutput output)
        {
            Add(new Expectation<TInputAndOutput>(input, output));
        }
    }
}
