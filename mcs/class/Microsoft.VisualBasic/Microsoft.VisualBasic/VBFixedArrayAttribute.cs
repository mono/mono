//
// VBFixedArrayAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
//

using System;

namespace Microsoft.VisualBasic 
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)] 
	sealed public class VBFixedArrayAttribute : Attribute {

		// Declarations
		private int upperBound1;
		private int upperBound2;
		private bool bidimensional; 

		// Constructors
		public VBFixedArrayAttribute(int UpperBound1) { 
			upperBound1 = UpperBound1; 
			bidimensional = false;
		}

		public VBFixedArrayAttribute(int UpperBound1, int UpperBound2) {
			upperBound1 = UpperBound1; 
			upperBound2 = UpperBound2; 
			bidimensional = true;
		}

		// Properties
		public int Length { 
			get { 
				if (bidimensional)
					return (upperBound1+1)*(upperBound2+1);
				return upperBound1+1;
			} 
		}

		public int[] Bounds { 
			get { 
				if (bidimensional)
					return new int[] { upperBound1, upperBound2 };
				return new int[] { upperBound1 }; 
			} 
		}
	};
}
