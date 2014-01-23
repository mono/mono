/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public interface IInstructionProvider {
        void AddInstructions(LightCompiler compiler);
    }

    public abstract partial class Instruction {
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }
        public virtual int ConsumedContinuations { get { return 0; } }
        public virtual int ProducedContinuations { get { return 0; } }

        public int StackBalance {
            get { return ProducedStack - ConsumedStack; }
        }

        public int ContinuationsBalance {
            get { return ProducedContinuations - ConsumedContinuations; }
        }

        public abstract int Run(InterpretedFrame frame);

        public virtual string InstructionName {
            get { return GetType().Name.Replace("Instruction", ""); }
        }

        public override string ToString() {
            return InstructionName + "()";
        }

        public virtual string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects) {
            return ToString();
        }

        public virtual object GetDebugCookie(LightCompiler compiler) {
            return null;
        }
    }
}
