//
// JvmReMachineFactory.jvm.cs
//
// Author:
//	Arina Itkes  <arinai@mainsoft.com>
//
// Copyright (C) 2007 Mainsoft, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Text;

namespace System.Text.RegularExpressions
{
	sealed class JvmReMachineFactory : IMachineFactory
	{
		readonly JvmReMachine _machine;

		public JvmReMachineFactory (PatternData patternData) {
			_machine = new JvmReMachine (patternData);
		}

		public IMachine NewInstance () {
			return _machine;
		}

		public JvmReMachine GetMachine () {
			return _machine;
		}

		public IDictionary Mapping {
			get { return _machine.Mapping; }
			set { throw new NotImplementedException ("Mapping setter of JvmReMachineFactory should not be called."); }//We must implement the setter of interface but it is not in use
		}

		public string [] NamesMapping {
			get { return _machine.NamesMapping; }
			set { throw new NotImplementedException ("NamesMapping setter of JvmReMachineFactory should not be called."); }//We must implement the setter of interface but it is not in use
		}

		public int GroupCount {
			get { return _machine.GroupCount; }
		}

		public int Gap {
			// FIXME: fix definition once JvmMachine is updated
			get { return 1 + GroupCount; }
			set { throw new NotImplementedException ("Gap setter of JvmReMachineFactory should not be called."); }//We must implement the setter of interface but it is not in use
		}
	}
}