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

namespace _root {

	public static class Number {
	
		//
		// Extension Methods
		//
	
 		public static string toExponential(this double d, uint fractionDigits) {
			throw new System.NotImplementedException();
 		}
 	 	
		public static string toFixed(this double d, uint fractionDigits) {
			return d.ToString ( "F" + fractionDigits.ToString() );
		}
 	 	
		public static string toPrecision(this double d, uint precision) {
			throw new System.NotImplementedException();
		}
 	 	
		public static string toString(this double d) {
			return d.ToString();
		}

		public static string toString(this double d, double radix) {
			throw new System.NotImplementedException();
		}
 	 	
		public static double valueOf(this double d) {
			return d;
		}

 	 	//
 	 	// Constants
 	 	//
 	 	
 	 	public const double MAX_VALUE = System.Double.MaxValue;
 	 		
		public const double MIN_VALUE = System.Double.MinValue;

 	 	public const double @NaN = System.Double.NaN;

 	 	public const double NEGATIVE_INFINITY = System.Double.NegativeInfinity;

 	 	public const double POSITIVE_INFINITY = System.Double.PositiveInfinity;
	
	}

}
