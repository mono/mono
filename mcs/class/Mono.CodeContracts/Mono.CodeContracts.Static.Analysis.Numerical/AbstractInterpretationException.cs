using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class AbstractInterpretationException : Exception {
                public AbstractInterpretationException (string message)
                        : base (message)
                {
                }
        }
}