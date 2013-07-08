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
using System.Collections;

namespace _root
{
	public static class Extensions
	{
//		public static string toString(this object o) 
//		{
//			return o.ToString ();
//		}

		public static string toLocaleString(this object o) 
		{
			return o.ToString ();
		}

		public static bool hasOwnProperty(this object o, string name) 
		{
			var t = o.GetType ();
			return t.GetProperty(name) != null || t.GetField(name) != null;
		}

		public static string toString(this uint o, int radix = 10) 
		{
			return Convert.ToString (o, radix);
		}

		//
		// IList extensions (for arrays, etc).
		//

//		public static int get_length(this IList list) 
//		{
//			return list.Count;
//		}

	}
}

