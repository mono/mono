//
// VBMath.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
// (C) 2002 Tipic Inc
//

using System;

namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class VBMath {
		// Declarations
		// Constructors
		// Properties
		static Random rnd = new Random();
		static float last = (float)rnd.NextDouble();
		// Methods
		public static float Rnd () {
			last =  (float)rnd.NextDouble();
			return last; 
		}
		public static float Rnd (float Number) 
		{
			if (Number == 0.0)
			{
				return last;
			} 
			else if (Number < 0.0 )
			{
				//fd: What does this mean?
				//fd: ms-help://MS.VSCC/MS.MSDNVS/script56/html/vsstmRandomize
				//fd: ms-help://MS.VSCC/MS.MSDNVS/script56/html/vsfctrnd.htm
				Randomize(Number);
				return Rnd();
			}
			return Rnd();
		} 
		public static void Randomize () { 
			rnd = new Random();
		} 
		[MonoTODO("Rethink the double => int conversion")]
		public static void Randomize (double Number) 
		{ 
			rnd = new Random((int)Number);
		}
		// Events
	};
}
