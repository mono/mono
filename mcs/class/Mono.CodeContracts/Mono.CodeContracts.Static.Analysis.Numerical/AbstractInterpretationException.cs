using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    public class AbstractInterpretationException : Exception
    {
        public AbstractInterpretationException (string message)
            : base (message)
        {
        }
    }
}