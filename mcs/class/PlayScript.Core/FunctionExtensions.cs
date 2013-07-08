// Copyright 2013 Zynga Inc.
//	
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//		
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.

using System;
using System.Reflection;

namespace _root {

	public static class FunctionExtensions {

		public static dynamic apply(this Delegate d, dynamic thisArg, Array argArray) {
			return d.DynamicInvoke(argArray != null ? argArray.ToArray() : null);
		}

		public static dynamic call(this Delegate d, dynamic thisArg, params object[] args) {
			return d.DynamicInvoke(args);
		}

		// this returns the number of arguments to the delegate method
		public static int get_length(this Delegate d) {
			return d.Method.GetParameters().Length;
		}

	}


}
