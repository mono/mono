//
// Regex.jvm.cs
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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

using RegularExpression = System.Text.RegularExpressions.Syntax.RegularExpression;
using Parser = System.Text.RegularExpressions.Syntax.Parser;

using System.Diagnostics;


namespace System.Text.RegularExpressions
{
	public partial class Regex : ISerializable
	{
		IMachineFactory _monoFactory;
		readonly object _monoFactoryLock = new object ();

		internal bool SameGroupNamesFlag {
			get {
				return GetJvmMachine ().PatternData.SameGroupsFlag;
			}
		}

		internal IMachine GetMonoMachine () {

			lock (_monoFactoryLock) {
				if (_monoFactory != null) 
					return _monoFactory.NewInstance ();
				
				_monoFactory = CreateMachineFactory (this.pattern, this.Options);
			}

			return _monoFactory.NewInstance ();
		}

		internal JvmReMachine GetJvmMachine () {
			if (machineFactory is InterpreterFactory)
				return null;

			return (JvmReMachine) ((JvmReMachineFactory) machineFactory).GetMachine ();
		}

		internal int GetJavaNumberByNetNumber (int netNumber) {
			if (netNumber < 0 || netNumber > group_count) {
				return netNumber;
			}

			return GetJvmMachine ().PatternData.NetToJavaNumbersMap [netNumber];
		}

		private void Init () {
			
			this.machineFactory = cache.Lookup (this.pattern, this.roptions);
			
			if (this.machineFactory != null) {
		
				this.group_count = this.machineFactory.GroupCount;
				this.mapping = this.machineFactory.Mapping;
				this._groupNumberToNameMap = this.machineFactory.NamesMapping;

				return;
			}

			PatternData patternData = PatternDataBuilder.GetPatternData (pattern, roptions);

			if (patternData == null)
				InitNewRegex ();
			else
				InitJvmRegex (patternData);

			cache.Add (this.pattern, this.roptions, this.machineFactory);
			return;
		}

		private void InitJvmRegex (PatternData patternData) {
			
			machineFactory = new JvmReMachineFactory (patternData);
			this.group_count = this.machineFactory.GroupCount;
			this.mapping = this.machineFactory.Mapping;
			this._groupNumberToNameMap = this.machineFactory.NamesMapping;
		}

	}
}