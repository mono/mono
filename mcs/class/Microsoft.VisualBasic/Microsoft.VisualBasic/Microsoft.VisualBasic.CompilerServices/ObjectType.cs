//
// ObjectType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
//     2002 Tipic, Inc (http://www.tipic.com)
//

using System;

namespace Microsoft.VisualBasic.CompilerServices {
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class ObjectType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static System.Int32 ObjTst (System.Object o1, System.Object o2, System.Boolean TextCompare) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object PlusObj (System.Object obj) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object NegObj (System.Object obj) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object NotObj (System.Object obj) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object BitAndObj (System.Object obj1, System.Object obj2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object BitOrObj (System.Object obj1, System.Object obj2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object BitXorObj (System.Object obj1, System.Object obj2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object AddObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object SubObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object MulObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object DivObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object PowObj (System.Object obj1, System.Object obj2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object ModObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object IDivObj (System.Object o1, System.Object o2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object XorObj (System.Object obj1, System.Object obj2) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean LikeObj (System.Object vLeft, System.Object vRight, Microsoft.VisualBasic.CompareMethod CompareOption) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object StrCatObj (System.Object vLeft, System.Object vRight) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object GetObjectValuePrimitive (System.Object o) { throw new NotImplementedException (); }
		// Events
	};
}


// //
//// ObjectType.cs
////
//// Author:
////   Chris J Breisch (cjbreisch@altavista.net) 
////   Francesco Delfino (pluto@tipic.com)
////
//// (C) 2002 Chris J Breisch
////     2002 Tipic, Inc (http://www.tipic.com)
////
//
///*
//  * Copyright (c) 2002-2003 Mainsoft Corporation.
//  *
//  * Permission is hereby granted, free of charge, to any person obtaining a
//  * copy of this software and associated documentation files (the "Software"),
//  * to deal in the Software without restriction, including without limitation
//  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  * and/or sell copies of the Software, and to permit persons to whom the
//  * Software is furnished to do so, subject to the following conditions:
//  * 
//  * The above copyright notice and this permission notice shall be included in
//  * all copies or substantial portions of the Software.
//  * 
//  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  * DEALINGS IN THE SOFTWARE.
//  */
//
//using System;
//
//namespace Microsoft.VisualBasic.CompilerServices {
//	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
//	sealed public class ObjectType {
//
//		private static int[,] ConversionClassTable = new int[13,13];
//		private static int[,] WiderType = new int[12,12];
//		private static int [] VType2FromTypeCode  = new int[19];
//		//private static Type [] TypeFromTypeCode  = new Type[19];
//		private static int [] VTypeFromTypeCode  = new int[19];
//		private static int [] TypeCodeFromVType  = new int[13];
//
//		//    static {
//		//        WiderType[VType.t_bad][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_bool] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_ui1] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_i2] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_i4] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_i8] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_dec] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_r4] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_r8] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_str] = VType.t_bad;
//		//        WiderType[VType.t_bad][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_bool][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_bool][VType.t_bool] = VType.t_bool;
//		//        WiderType[VType.t_bool][VType.t_ui1] = VType.t_bool;
//		//        WiderType[VType.t_bool][VType.t_i2] = VType.t_i2;
//		//        WiderType[VType.t_bool][VType.t_i4] = VType.t_i4;
//		//        WiderType[VType.t_bool][VType.t_i8] = VType.t_i8;
//		//        WiderType[VType.t_bool][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_bool][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_bool][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_bool][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_bool][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_bool][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_ui1][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_ui1][VType.t_bool] = VType.t_bool;
//		//        WiderType[VType.t_ui1][VType.t_ui1] = VType.t_ui1;
//		//        WiderType[VType.t_ui1][VType.t_i2] = VType.t_i2;
//		//        WiderType[VType.t_ui1][VType.t_i4] = VType.t_i4;
//		//        WiderType[VType.t_ui1][VType.t_i8] = VType.t_i8;
//		//        WiderType[VType.t_ui1][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_ui1][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_ui1][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_ui1][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_ui1][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_ui1][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_i2][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_i2][VType.t_bool] = VType.t_i2;
//		//        WiderType[VType.t_i2][VType.t_ui1] = VType.t_i2;
//		//        WiderType[VType.t_i2][VType.t_i2] = VType.t_i2;
//		//        WiderType[VType.t_i2][VType.t_i4] = VType.t_i4;
//		//        WiderType[VType.t_i2][VType.t_i8] = VType.t_i8;
//		//        WiderType[VType.t_i2][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_i2][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_i2][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_i2][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_i2][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_i2][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_i4][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_i4][VType.t_bool] = VType.t_i4;
//		//        WiderType[VType.t_i4][VType.t_ui1] = VType.t_i4;
//		//        WiderType[VType.t_i4][VType.t_i2] = VType.t_i4;
//		//        WiderType[VType.t_i4][VType.t_i4] = VType.t_i4;
//		//        WiderType[VType.t_i4][VType.t_i8] = VType.t_i8;
//		//        WiderType[VType.t_i4][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_i4][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_i4][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_i4][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_i4][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_i4][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_i8][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_i8][VType.t_bool] = VType.t_i8;
//		//        WiderType[VType.t_i8][VType.t_ui1] = VType.t_i8;
//		//        WiderType[VType.t_i8][VType.t_i2] = VType.t_i8;
//		//        WiderType[VType.t_i8][VType.t_i4] = VType.t_i8;
//		//        WiderType[VType.t_i8][VType.t_i8] = VType.t_i8;
//		//        WiderType[VType.t_i8][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_i8][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_i8][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_i8][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_i8][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_i8][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_dec][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_dec][VType.t_bool] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_ui1] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_i2] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_i4] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_i8] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_dec] = VType.t_dec;
//		//        WiderType[VType.t_dec][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_dec][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_dec][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_dec][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_dec][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_r4][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_r4][VType.t_bool] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_ui1] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_i2] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_i4] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_i8] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_dec] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_r4] = VType.t_r4;
//		//        WiderType[VType.t_r4][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_r4][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_r4][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_r4][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_r8][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_r8][VType.t_bool] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_ui1] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_i2] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_i4] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_i8] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_dec] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_r4] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_r8][VType.t_str] = VType.t_r8;
//		//        WiderType[VType.t_r8][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_char][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_bool] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_ui1] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_i2] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_i4] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_i8] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_dec] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_r4] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_r8] = VType.t_bad;
//		//        WiderType[VType.t_char][VType.t_char] = VType.t_char;
//		//        WiderType[VType.t_char][VType.t_str] = VType.t_str;
//		//        WiderType[VType.t_char][VType.t_date] = VType.t_bad;
//		//
//		//        WiderType[VType.t_str][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_str][VType.t_bool] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_ui1] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_i2] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_i4] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_i8] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_dec] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_r4] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_r8] = VType.t_r8;
//		//        WiderType[VType.t_str][VType.t_char] = VType.t_str;
//		//        WiderType[VType.t_str][VType.t_str] = VType.t_str;
//		//        WiderType[VType.t_str][VType.t_date] = VType.t_date;
//		//
//		//        WiderType[VType.t_date][VType.t_bad] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_bool] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_ui1] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_i2] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_i4] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_i8] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_dec] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_r4] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_r8] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_char] = VType.t_bad;
//		//        WiderType[VType.t_date][VType.t_str] = VType.t_date;
//		//        WiderType[VType.t_date][VType.t_date] = VType.t_date;
//		//
//		//        ConversionClassTable[VType2.t_bad][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_bool] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_ui1] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i2] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_r4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_r8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_date] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_ref] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_str] = VType2.t_bad;
//		//
//		//        ConversionClassTable[VType2.t_bool][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_bool] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_ui1] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_i2] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_i4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_i8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bool][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_ui1] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_i2] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_i4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_i8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_ui1][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_char][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_bool] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_ui1] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_char] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_char][VType2.t_i2] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_i4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_i8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_r4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_r8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_char][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_i2][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_i2] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_i4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_i8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i2][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_i4][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_i2] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_i4] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_i8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i4][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_i8][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_i2] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_i4] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_i8] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_i8][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_r4][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_i2] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_i4] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_i8] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_r4] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_dec] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r4][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_r8][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_i2] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_i4] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_i8] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_r4] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_r8] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_dec] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_r8][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_bad][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_bool] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_ui1] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i2] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_i8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_r4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_r8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_bad] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_bad][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_ui1] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i2] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i4] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i8] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_dec] = VType2.t_bool;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_str] = VType2.t_ui1;
//		//
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bool] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_ui1] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_char] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i2] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_i8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_r4] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_r8] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_dec][VType2.t_str] = VType2.t_bad;
//		//
//		//        ConversionClassTable[VType2.t_str][VType2.t_bad] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_str][VType2.t_bool] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_ui1] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_char] = VType2.t_char;
//		//        ConversionClassTable[VType2.t_str][VType2.t_i2] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_i4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_i8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_r4] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_r8] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_bad] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_dec] = VType2.t_ui1;
//		//        ConversionClassTable[VType2.t_str][VType2.t_dec] = VType2.t_bad;
//		//        ConversionClassTable[VType2.t_str][VType2.t_str] = VType2.t_bool;
//		//        
//		//        VType2FromTypeCode[TypeCode.bool] = VType2.t_bool;
//		//        VType2FromTypeCode[TypeCode.Byte] = VType2.t_ui1;
//		//        VType2FromTypeCode[TypeCode.Int16] = VType2.t_i2;
//		//        VType2FromTypeCode[TypeCode.Int32] = VType2.t_i4;
//		//        VType2FromTypeCode[TypeCode.Int64] = VType2.t_i8;
//		//        VType2FromTypeCode[TypeCode.Decimal] = VType2.t_dec;
//		//        VType2FromTypeCode[TypeCode.Single] = VType2.t_r4;
//		//        VType2FromTypeCode[TypeCode.Double] = VType2.t_r8;
//		//        VType2FromTypeCode[TypeCode.Char] = VType2.t_char;
//		//        VType2FromTypeCode[TypeCode.string] = VType2.t_str;
//		//        VType2FromTypeCode[TypeCode.DateTime] = VType2.t_date;
//		//        
//		//        VTypeFromTypeCode[TypeCode.bool] = VType.t_bool;
//		//        VTypeFromTypeCode[TypeCode.Byte] = VType.t_ui1;
//		//        VTypeFromTypeCode[TypeCode.Int16] = VType.t_i2;
//		//        VTypeFromTypeCode[TypeCode.Int32] = VType.t_i4;
//		//        VTypeFromTypeCode[TypeCode.Int64] = VType.t_i8;
//		//        VTypeFromTypeCode[TypeCode.Decimal] = VType.t_dec;
//		//        VTypeFromTypeCode[TypeCode.Single] = VType.t_r4;
//		//        VTypeFromTypeCode[TypeCode.Double] = VType.t_r8;
//		//        VTypeFromTypeCode[TypeCode.Char] = VType.t_char;
//		//        VTypeFromTypeCode[TypeCode.string] = VType.t_str;
//		//        VTypeFromTypeCode[TypeCode.DateTime] = VType.t_date;
//		//        
//		//        TypeCodeFromVType[VType.t_bool] = TypeCode.bool; 
//		//        TypeCodeFromVType[VType.t_ui1] = TypeCode.Byte;
//		//        TypeCodeFromVType[VType.t_i2] = TypeCode.Int16;
//		//        TypeCodeFromVType[VType.t_i4] = TypeCode.Int32;
//		//        TypeCodeFromVType[VType.t_i8] = TypeCode.Int64;
//		//        TypeCodeFromVType[VType.t_dec] = TypeCode.Decimal;
//		//        TypeCodeFromVType[VType.t_r4] = TypeCode.Single;
//		//        TypeCodeFromVType[VType.t_r8] = TypeCode.Double;
//		//        TypeCodeFromVType[VType.t_char] = TypeCode.Char;
//		//        TypeCodeFromVType[VType.t_str] = TypeCode.string;
//		//        TypeCodeFromVType[VType.t_date] = TypeCode.DateTime;
//		//        
//		//        TypeFromTypeCode[TypeCode.bool] = Type.BooleanType; 
//		//        TypeFromTypeCode[TypeCode.Byte] = Type.ByteType; 
//		//        TypeFromTypeCode[TypeCode.Int16] = Type.Int16Type; 
//		//        TypeFromTypeCode[TypeCode.Int32] = Type.Int32Type; 
//		//        TypeFromTypeCode[TypeCode.Int64] = Type.Int64Type; 
//		//        TypeFromTypeCode[TypeCode.Decimal] = Type.DecimalType;         
//		//        TypeFromTypeCode[TypeCode.Single] = Type.SingleType;         
//		//        TypeFromTypeCode[TypeCode.Double] = Type.DoubleType;         
//		//        TypeFromTypeCode[TypeCode.Char] = Type.CharType;         
//		//        TypeFromTypeCode[TypeCode.string] = Type.StringType;         
//		//        TypeFromTypeCode[TypeCode.DateTime] = Type.DateTimeType;         
//		//        TypeFromTypeCode[TypeCode.SByte] = Type.SByteType;         
//		//        TypeFromTypeCode[TypeCode.UInt16] = Type.UInt16Type;         
//		//        TypeFromTypeCode[TypeCode.UInt32] = Type.UInt32Type;   
//		//        TypeFromTypeCode[TypeCode.UInt64] = Type.UInt64Type;   
//		//        TypeFromTypeCode[TypeCode.object] = Type.ObjectType;
//		//        TypeFromTypeCode[TypeCode.DBNull] = Type.DBNullType;   
//		//    }
//    
//		private static void checkIfAddValidObjects(
//			object obj1,
//			object obj2,
//			int tc1,
//			int tc2) {
//			if (tc1 == TypeCode.Object    || tc1 == TypeCode.SByte
//				|| tc1 == TypeCode.UInt16 || tc1 == TypeCode.UInt32
//				|| tc1 == TypeCode.UInt64)
//				throwNoValidOperator(obj1, obj2);
//			if (tc2 == TypeCode.Object    || tc2 == TypeCode.SByte
//				|| tc2 == TypeCode.UInt16 || tc2 == TypeCode.UInt32
//				|| tc2 == TypeCode.UInt64)
//				throwNoValidOperator(obj1, obj2);
//		}
//
//		//checked
//		public static object AddObj(object obj1, object obj2) {
//			int tc1 = getTypeCode(obj1);
//			if (obj1 != null && (obj1 is char[]))
//				obj1 = new string((char[]) obj1);
//
//			int tc2 = getTypeCode(obj2);
//			if (obj2 != null && (obj2 is char[]))
//				obj2 = new string((char[]) obj2);
//        
//			checkIfAddValidObjects(obj1,obj2,tc1,tc2);
//            
//			if (tc1 == TypeCode.DBNull || tc2 == TypeCode.DBNull)
//				return getAddDBNull(obj1,obj2,tc1,tc2);
//			else if (tc1 == TypeCode.DateTime || tc2 == TypeCode.DateTime)
//				return getAddDateTime(obj1,obj2,tc1,tc2);
//			else if (tc1 == TypeCode.Char || tc2 == TypeCode.Char)
//				return getAddChar(obj1,obj2,tc1,tc2);
//        
//			if (tc1 == TypeCode.Empty)
//				return obj2;              
//			else if (tc2 == TypeCode.Empty)
//				return obj1;               
//   
//			if (tc1 == TypeCode.String && tc2 == TypeCode.String)
//				return StringType.FromObject(obj1) + StringType.FromObject(obj2);
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String )
//				return getAddStringObject(obj1,obj2,tc1,tc2);                 
//        
//			if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)
//				return getAddDouble(obj1,obj2,tc1,tc2);                 
//			else if (tc1 == TypeCode.Single || tc2 == TypeCode.Single)
//				return getAddSingle(obj1,obj2,tc1,tc2);                               
//			else if(tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal)
//				return getAddDecimal(obj1,obj2,tc1,tc2);              
//			else if (tc1 == TypeCode.Int64 || tc2 == TypeCode.Int64)
//				return getAddInt64(obj1,obj2,tc1,tc2);                                 
//			else if (tc1 == TypeCode.Int32 || tc2 == TypeCode.Int32)
//				return getAddInt32(obj1,obj2,tc1,tc2);                                  
//			else if (tc1 == TypeCode.Int16 || tc2 == TypeCode.Int16)
//				return getAddInt16(obj1,obj2,tc1,tc2);                               
//			else if (tc1 == TypeCode.Boolean || tc2 == TypeCode.Boolean)
//				return getAddInt16(obj1,obj2,tc1,tc2);                                 
//			else if (tc1 == TypeCode.Byte || tc2 == TypeCode.Byte)
//				return getAddByte(obj1,obj2,tc1,tc2);                                           
//
//			throwNoValidOperator(obj1, obj2);
//			return null;
//		}
//    
//		private static object getAddDBNull(object obj1,object obj2,int tc1,int tc2) {
//			if (tc1 == TypeCode.DBNull) {
//				if (tc2 == TypeCode.String)
//					return obj2;
//				else
//					throwNoValidOperator(obj1, obj2);        
//			}
//			else if (tc1 == TypeCode.String)
//				return obj1;
//			else
//				throwNoValidOperator(obj1, obj2);
//			return null;        
//		}
//    
//		private static object getAddChar(object obj1,object obj2,int tc1,int tc2) {
//			if (tc1 == TypeCode.Char) {
//				if (tc2 == TypeCode.Char || tc2 == TypeCode.String)
//					return StringType.FromObject(obj1) + StringType.FromObject(obj2);
//				else
//					throwNoValidOperator(obj1, obj2);                
//			}
//			else if (tc2 == TypeCode.Char) {
//				if (tc1 == TypeCode.Char || tc1 == TypeCode.String)
//					return StringType.FromObject(obj1) + StringType.FromObject(obj2);
//				else
//					throwNoValidOperator(obj1, obj2);                
//			}
//			return null;        
//		}
//    
//		private static object getAddDateTime(object obj1,object obj2,int tc1,int tc2) {
//			if (tc1 == TypeCode.DateTime) {
//				if (tc2 == TypeCode.DateTime || tc2 == TypeCode.String)
//					return obj1.toString() + obj2.toString();
//				else
//					throwNoValidOperator(obj1, obj2);                
//			}
//			else if (tc2 == TypeCode.DateTime) {
//				if (tc1 == TypeCode.DateTime || tc1 == TypeCode.String)
//					return obj1.toString() + obj2.toString();
//				else
//					throwNoValidOperator(obj1, obj2);                
//			}        
//			return null;        
//		}
//    
//		private static object getAddByte(object obj1,object obj2,int tc1,int tc2) {
//			byte d1 = convertObjectToByte(obj1,tc1);
//			byte d2 = convertObjectToByte(obj2,tc2);
//			short s = (short) (d1 + d2);
//			if (s >= 0 && s <= 255)
//				return new ClrByte((byte) s);
//			return new ClrInt16(s);       
//		}
//    
//		private static object getAddDouble(object obj1,object obj2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(obj1,tc1);
//			double d2 = convertObjectToDouble(obj2,tc2); 
//			return new ClrDouble(d1+d2);      
//		}
//    
//		private static object getAddDecimal(object obj1,object obj2,int tc1,int tc2) {
//			Decimal d1 = convertObjectToDecimal(obj1,tc1);
//			Decimal d2 = convertObjectToDecimal(obj2,tc2);
//			return Decimal.Add(d1,d2);      
//		}
//    
//		private static object getAddInt64(object obj1,object obj2,int tc1,int tc2) {
//			Decimal d1 = convertObjectToDecimal(obj1,tc1);
//			Decimal d2 = convertObjectToDecimal(obj2,tc2);
//			Decimal sum = Decimal.Add(d1,d2);
//			if (Decimal.Compare(sum, new Decimal(Long.MAX_VALUE)) <= 0
//				&& Decimal.Compare(sum, new Decimal(Long.MIN_VALUE)) >= 0)
//				return new ClrInt64(Decimal.ToInt64(sum));
//
//			return sum;              
//		}
//    
//		private static object getAddInt32(object obj1,object obj2,int tc1,int tc2) {
//			long d1 = convertObjectToLong(obj1,tc1); 
//			long d2 = convertObjectToLong(obj2,tc2); 
//			long sum = d1+ d2;
//			if (sum >= ClrInt32.MinValue && sum <= ClrInt32.MaxValue)
//				return new ClrInt32((int) sum);
//			return new ClrInt64(sum);              
//		}
//    
//		private static object getAddInt16(object obj1,object obj2,int tc1,int tc2) {
//			int d1 = convertObjectToInt(obj1,tc1);
//			int d2 = convertObjectToInt(obj2,tc2); 
//			int sum = d1+ d2;
//			if (sum >= ClrInt16.MinValue && sum <= ClrInt16.MaxValue)
//				return new ClrInt16((short) sum);
//			return new ClrInt32(sum);              
//		}
//    
//    
//		private static object getAddSingle(object obj1,object obj2,int tc1,int tc2) {
//			float d1 = convertObjectToFloat(obj1,tc1);
//			float d2 = convertObjectToFloat(obj2,tc2);
//   
//			double sum = (double) d1 + (double) d2;
//			if (sum <= 3.40282e+038 &&  sum>= -3.40282e+038)
//				return new ClrSingle((float) sum);
//			if (ClrDouble.IsInfinity(sum) &&
//				(ClrSingle.IsInfinity(d1) || ClrSingle.IsInfinity(d2)))
//				return new ClrSingle((float) sum);
//			return new ClrDouble(sum);
//		}
//
//		//checked
//		private static int toVBBool(IConvertible conv) {
//			if (conv.ToBoolean(null))
//				return -1;
//			return 0;
//		}
//
//
//		// checked
//		private static object getAddStringObject(object s1, object s2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(s1,tc1);
//			double d2 = convertObjectToDouble(s1,tc2);
//			return new ClrDouble(d1 + d2);
//		}
//
//		//checked !!
//		public static object BitAndObj(object obj1, object obj2) {
//			Type type1 = ObjectStaticWrapper.GetType(obj1);;
//			Type type2 = ObjectStaticWrapper.GetType(obj2);
//			int typeCode = GetWidestType(obj1, obj2, false);
//        
//			if (obj1 == null && obj2 == null)
//				return new ClrInt32(0);
//			else if (typeCode == TypeCode.Boolean) {
//				if (type1 == type2)
//					return new ClrBoolean(BooleanType.FromObject(obj1)& BooleanType.FromObject(obj2));
//				return new ClrInt16(
//					(short) (ShortType.FromObject(obj1) & ShortType.FromObject(obj2)));
//			}
//			else if (typeCode == TypeCode.Byte) {
//				byte val = (byte) (ByteType.FromObject(obj1)
//					& ByteType.FromObject(obj2));
//				return createByteAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int16) {
//				short val =
//					(short) (ShortType.FromObject(obj1)
//					& ShortType.FromObject(obj2));
//				return createShortAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int32) {
//				int val = IntegerType.FromObject(obj1) & IntegerType.FromObject(obj2);
//				return createIntAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int64) {
//				long val = LongType.FromObject(obj1) & LongType.FromObject(obj2);
//				return createLongAccordingToEnumCond(val, type1, type2, obj1, obj2);
//			}
//			else if (typeCode == TypeCode.Single  || typeCode == TypeCode.Double
//				|| typeCode == TypeCode.Decimal || typeCode == TypeCode.String) {
//				long val = LongType.FromObject(obj1) & LongType.FromObject(obj2);
//				return new ClrInt64(val);
//			}
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//		}
//    
//		private static object createByteAccordingToEnumCond(
//			byte val,
//			Type type1,
//			Type type2,
//			object obj1,
//			object obj2) {
//			bool isEnum1 = type1.get_IsEnum();
//			bool isEmum2 = type2.get_IsEnum();
//			if (isEnum1 && isEmum2 && type1 != type2)
//				return new ClrByte(val);
//			if (isEnum1)
//				return Enum.ToObject(type1, val);
//			if (!(isEmum2))
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//			return Enum.ToObject(type2, val);
//		}
//    
//		private static object createShortAccordingToEnumCond(
//			short val,
//			Type type1,
//			Type type2,
//			object obj1,
//			object obj2) {
//			bool isEnum1 = type1.get_IsEnum();
//			bool isEmum2 = type2.get_IsEnum();
//			if (isEnum1 && isEmum2 && type1 != type2)
//				return new ClrInt16(val);
//			if (isEnum1)
//				return Enum.ToObject(type1, val);
//			if (!(isEmum2))
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//			return Enum.ToObject(type2, val);
//		}
//    
//		private static object createIntAccordingToEnumCond(
//			int val,
//			Type type1,
//			Type type2,
//			object obj1,
//			object obj2) {
//			bool isEnum1 = type1.get_IsEnum();
//			bool isEmum2 = type2.get_IsEnum();
//			if (isEnum1 && isEmum2 && type1 != type2)
//				return new ClrInt32(val);
//			if (isEnum1)
//				return Enum.ToObject(type1, val);
//			if (!(isEmum2))
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//			return Enum.ToObject(type2, val);
//		}
//    
//		private static object createLongAccordingToEnumCond(
//			long val,
//			Type type1,
//			Type type2,
//			object obj1,
//			object obj2) {
//			bool isEnum1 = type1.get_IsEnum();
//			bool isEmum2 = type2.get_IsEnum();
//			if (isEnum1 && isEmum2 && type1 != type2)
//				return new ClrInt64(val);
//			if (isEnum1)
//				return Enum.ToObject(type1, val);
//			if (!(isEmum2))
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//			return Enum.ToObject(type2, val);
//		}
//
//		//checked !!
//		public static object BitOrObj(object obj1, object obj2) {
//			Type type1 = ObjectStaticWrapper.GetType(obj1);;
//			Type type2 = ObjectStaticWrapper.GetType(obj2);
//			int typeCode = GetWidestType(obj1, obj2, false);
//        
//			if (obj1 == null && obj2 == null)
//				return new ClrInt32(0);
//			else if (typeCode == TypeCode.Boolean) {
//				if (type1 == type2)
//					return new ClrBoolean(BooleanType.FromObject(obj1)| BooleanType.FromObject(obj2));
//				return new ClrInt16(
//					(short) (ShortType.FromObject(obj1) | ShortType.FromObject(obj2)));
//			}
//			else if (typeCode == TypeCode.Byte) {
//				byte val = (byte) (ByteType.FromObject(obj1)
//					| ByteType.FromObject(obj2));
//				return createByteAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int16) {
//				short val = (short) (ShortType.FromObject(obj1)
//					| ShortType.FromObject(obj2));
//				return createShortAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int32) {
//				int val = IntegerType.FromObject(obj1) | IntegerType.FromObject(obj2);
//				return createIntAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int64) {
//				long val = LongType.FromObject(obj1) | LongType.FromObject(obj2);
//				return createLongAccordingToEnumCond(val, type1, type2, obj1, obj2);
//			}
//			else if (typeCode == TypeCode.Single  || typeCode == TypeCode.Double
//				|| typeCode == TypeCode.Decimal || typeCode == TypeCode.String) {
//				long val = LongType.FromObject(obj1) | LongType.FromObject(obj2);
//				return new ClrInt64(val);
//			}
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//		}
//
//		//checked !!
//		public static object BitXorObj(object obj1, object obj2) {
//			Type type1 = ObjectStaticWrapper.GetType(obj1);;
//			Type type2 = ObjectStaticWrapper.GetType(obj2);
//			int typeCode = GetWidestType(obj1, obj2, false);
//        
//			if (obj1 == null && obj2 == null)
//				return new ClrInt32(0);
//			else if (typeCode == TypeCode.Boolean) {
//				if (type1 == type2)
//					return new ClrBoolean(BooleanType.FromObject(obj1)^ BooleanType.FromObject(obj2));
//				return new ClrInt16(
//					(short) (ShortType.FromObject(obj1) ^ ShortType.FromObject(obj2)));
//			}
//			else if (typeCode == TypeCode.Byte) {
//				byte val = (byte) (ByteType.FromObject(obj1)
//					^ ByteType.FromObject(obj2));
//				return createByteAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int16) {
//				short val =
//					(short) (ShortType.FromObject(obj1)
//					^ ShortType.FromObject(obj2));
//				return createShortAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int32) {
//				int val = IntegerType.FromObject(obj1) ^ IntegerType.FromObject(obj2);
//				return createIntAccordingToEnumCond(val,type1,type2,obj1,obj2);                    
//			}
//			else if (typeCode == TypeCode.Int64) {
//				long val = LongType.FromObject(obj1) ^ LongType.FromObject(obj2);
//				return createLongAccordingToEnumCond(val, type1, type2, obj1, obj2);
//			}
//			else if (typeCode == TypeCode.Single  || typeCode == TypeCode.Double
//				|| typeCode == TypeCode.Decimal || typeCode == TypeCode.String) {
//				long val = LongType.FromObject(obj1) ^ LongType.FromObject(obj2);
//				return new ClrInt64(val);
//			}
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//		}
//
//		//checked + string
//		public static object CTypeHelper(object obj, TypeCode toType) {
//			return CTypeHelper(obj, toType.getValue());
//		}
//
//		//checked + string
//		public static object CTypeHelper(object obj, int toType) {
//			if (obj == null)
//				return null;
//			switch (toType) {
//				case TypeCode.Boolean :
//					return new ClrBoolean(BooleanType.FromObject(obj));
//				case TypeCode.Byte :
//					return new ClrByte((byte)ByteType.FromObject(obj));
//				case TypeCode.Int16 :
//					return new ClrInt16(ShortType.FromObject(obj));
//				case TypeCode.Int32 :
//					return new ClrInt32(IntegerType.FromObject(obj));
//				case TypeCode.Int64 :
//					return new ClrInt64(LongType.FromObject(obj));
//				case TypeCode.Decimal :
//					return DecimalType.FromObject(obj);
//				case TypeCode.Single :
//					return new ClrSingle(SingleType.FromObject(obj));
//				case TypeCode.Double :
//					return new ClrDouble(DoubleType.FromObject(obj));
//				case TypeCode.String :
//					return StringType.FromObject(obj);
//				case TypeCode.Char :
//					return new ClrChar(CharType.FromObject(obj));
//				case TypeCode.DateTime :
//					return DateType.FromObject(obj);
//
//			}
//
//			throw new ClassCastException(
//				Utils.GetResourceString(
//				"InvalidCast_FromTo",
//				Utils.VBFriendlyName(obj),
//				Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(toType))));
//
//		}
//
//		//checked + string
//		public static object CTypeHelper(object obj, Type toType) {
//			if (obj == null)
//				return null;
//
//			if (toType == Type.ObjectType)
//				return obj;
//
//			Type type = ObjectStaticWrapper.GetType(obj);
//			//    if (toType.IsByRef) {
//			//        toType = toType.GetElementType();
//			//        local2 = true;
//			//    }
//			//    if (type.IsByRef)
//			//        local1 = local1.GetElementType();
//			if (type == toType)
//				return obj;
//
//			int tc = Type.GetTypeCode(toType);
//			if (tc == TypeCode.Object) {
//				if (toType.IsAssignableFrom(type))
//					return obj;
//				//TODO:
//				//            if (obj is string
//				//                && toType == ObjectStaticWrapper.GetType(char[].class))
//				//                return CharArrayType.FromString((string) obj);
//				//
//				//            throw new ClassCastException(
//				//                Utils.GetResourceString(
//				//                    "InvalidCast_FromTo",
//				//                    Utils.VBFriendlyName(type),
//				//                    Utils.VBFriendlyName(toType)));
//			}
//
//			object retVal = CTypeHelper(obj, tc);
//			if (toType.IsEnum())
//				return Enum.ToObject(toType,retVal);
//			return retVal;
//		}
//
//		//checked + string
//		public static int GetWidestType(int type1, int type2) {
//			int index1 = getVTypeFromTypeCode(type1);
//			int index2 = getVTypeFromTypeCode(type2);
//			return getTypeCodeFromVType(WiderType[index1][index2]);
//		}
//    
//		public static int GetWidestType(object obj1, int typeCode2) {
//			if (obj1 == null)
//				return typeCode2;
//			int tc1 = getTypeCode(obj1);     
//			return GetWidestType(tc1,typeCode2);
//		}
//    
//		private static int getTypeCode(object obj) {
//			IConvertible iConvertible = null;
//			int typeCode;
//			if (obj is IConvertible) {
//				iConvertible = (IConvertible)obj;
//			}
//			if (iConvertible != null) {
//				typeCode = iConvertible.GetTypeCode();
//			}
//			else if (obj == null) {
//				typeCode = TypeCode.Empty;
//			}
//			else if (obj is string) {
//				typeCode = TypeCode.String;
//			}
//			else if (obj is char[]) {
//				typeCode = TypeCode.String;
//			}
//			else {
//				typeCode = TypeCode.Object;
//			}
//			return typeCode;
//		}
//
//		//checked + string
//		public static int GetWidestType(object obj1, object obj2, bool IsAdd) {
//			int typeCode1 = getTypeCode(obj1);
//			int typeCode2 = getTypeCode(obj1);
//        
//			if (obj1 == null)
//				return typeCode2;
//			if (obj2 == null)
//				return typeCode1;
//			if ( IsAdd &&
//				((typeCode1 == TypeCode.DBNull && typeCode2 == TypeCode.String) || 
//				(typeCode1 == TypeCode.String && typeCode2 == TypeCode.DBNull)))
//				return TypeCode.DBNull;
//			else
//				return GetWidestType(typeCode1,typeCode2);
//		}
//
//		//checked !!
//		private static int getTypeCodeFromVType(int vartype) {
//			return TypeCodeFromVType[vartype];
//		}
//
//
//		//checked !!
//		private static int getVTypeFromTypeCode(int type) {
//			return VTypeFromTypeCode[type];
//		}
//
//		//checked !!
//		public static object GetObjectValuePrimitive(object o) {
//			if (o == null || !(o is IConvertible))
//				return o;
//			IConvertible iConv = IConvertibleStaticWrapper.GetIConvertibleWrapper(o);
//			int tc = getTypeCode(o);
//			switch (tc) {
//					//this four type code are converted in this way since they can also
//					//be enum.
//				case TypeCode.Byte :
//					return new ClrByte((byte)iConv.ToByte(null));
//				case TypeCode.Int16 :
//					return new ClrInt16((short)iConv.ToInt16(null));
//				case TypeCode.Int32 :
//					return new ClrInt32(iConv.ToInt32(null));
//				case TypeCode.Int64 :
//					return new ClrInt64(iConv.ToInt64(null));
//				default :
//					return o;
//			}
//		}
//
//		//checked !!
//		private static object getDivDecimal(object obj1, object obj2,int tc1,int tc2) {
//			Decimal dec1 = convertObjectToDecimal(obj1,tc1);
//			Decimal dec2 = convertObjectToDecimal(obj2,tc2);
//			try {
//				return Decimal.Divide(dec1, dec2);
//			}
//			catch(OverflowException e) {
//				float val1 = Convert.ToSingle(dec1);
//				float val2 = Convert.ToSingle(dec2);
//				float val3 = val1/val2;
//				return new ClrSingle(val3);
//			}
//
//		}
//
//		//checked !!
//		private static object getDivDouble(object s1, object s2, int tc1 ,int tc2) {
//			double d1 = convertObjectToDouble(s1,tc1);
//			double d2 = convertObjectToDouble(s2,tc2);
//			return new ClrDouble(d1 / d2);
//		}
//
//		//checked !!
//		private static object getDivSingle(object obj1, object obj2,int tc1 , int tc2) {
//			float d1 = convertObjectToFloat(obj1,tc1);
//			float d2 = convertObjectToFloat(obj2,tc2);
//
//			float sum = d1 / d2;
//			if (ClrSingle.IsInfinity(d1)) {
//				if (ClrSingle.IsInfinity(d1) || ClrSingle.IsInfinity(d2))
//					return new ClrSingle(sum);
//				return new ClrDouble((double) d1 / (double) d2);
//			}
//			return new ClrSingle(sum);
//		}
//    
//		private static void checkIfValidObjects(
//			object obj1,
//			object obj2,
//			int tc1,
//			int tc2) {
//			if (tc1 == TypeCode.Object    || tc1 == TypeCode.DBNull
//				|| tc1 == TypeCode.Char   || tc1 == TypeCode.SByte
//				|| tc1 == TypeCode.UInt16 || tc1 == TypeCode.UInt32
//				|| tc1 == TypeCode.UInt64 || tc1 == TypeCode.DateTime)
//				throwNoValidOperator(obj1, obj2);
//			if (tc2 == TypeCode.Object    || tc2 == TypeCode.DBNull
//				|| tc2 == TypeCode.Char   || tc2 == TypeCode.SByte
//				|| tc2 == TypeCode.UInt16 || tc2 == TypeCode.UInt32
//				|| tc2 == TypeCode.UInt64 || tc2 == TypeCode.DateTime)
//				throwNoValidOperator(obj1, obj2);
//		}
//
//		//checked !!
//		public static object DivObj(object o1, object o2) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//        
//			checkIfValidObjects(o1,o2,tc1,tc2);
//        
//			if (tc2 == TypeCode.Empty)
//				throw new DivideByZeroException(
//					Environment.GetResourceString("Arg_DivideByZero"));
//                
//			if (tc1 == TypeCode.String || tc2 == TypeCode.String)
//				return getDivDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Empty)
//				return getDivDouble(new ClrDouble(0), o2,TypeCode.Double,tc2);
//			else if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)                
//				return getDivDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Single || tc2 == TypeCode.Single)                
//				return getDivSingle(o1, o2,tc1,tc2);
//			else if (tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal)                
//				return getDivDecimal(o1, o2,tc1,tc2);
//			else
//				return getDivDouble(o1, o2,tc1,tc2);
//		}
//
//		//checked
//		private static IConvertible toVBBoolConv(IConvertible conv) {
//			if (conv.ToBoolean(null))
//				return new ClrInt32(-1);
//			return new ClrInt32(0);
//		}
//
//		private static object getIDivideByte(object o1, object o2,int tc1,int tc2) {
//			byte d1 = convertObjectToByte(o1,tc1);
//			byte d2 = convertObjectToByte(o2,tc2);
//			return new ClrByte((byte) (d1 / d2));
//		}
//
//		private static object getIDivideInt16(object o1, object o2,int tc1,int tc2) {
//			short d1 = convertObjectToShort(o1,tc1);
//			short d2 = convertObjectToShort(o2,tc2);
//			return new ClrInt32((short) (d1 / d2));
//		}
//
//		private static object getIDivideInt32(object o1, object o2,int tc1,int tc2) {
//			int d1 = convertObjectToInt(o1,tc1);
//			int d2 = convertObjectToInt(o2,tc2);
//			return new ClrInt32(d1 / d2);
//		}
//
//		private static object getIDivideInt64(object o1, object o2,int tc1,int tc2) {
//			long d1 = convertObjectToLong(o1,tc1);
//			long d2 = convertObjectToLong(o2,tc2);
//			return new ClrInt64(d1 / d2);
//		}
//
//
//		public static object IDivObj(object o1, object o2) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//			checkIfValidObjects(o1,o2,tc1,tc2);
//        
//			if (tc2 == TypeCode.Empty)
//				throw new DivideByZeroException(
//					Environment.GetResourceString("Arg_DivideByZero"));
//			else if (tc1 == TypeCode.Empty) {
//				if (tc2 == TypeCode.Byte)
//					return new ClrByte((byte)0);
//				else if (tc2 == TypeCode.Int16)
//					return new ClrInt16((short)0);
//				else if (tc2 == TypeCode.Int32)
//					return new ClrInt32(0);
//				else
//					return new ClrInt64(0);            
//			}
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String ||
//				tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal ||
//				tc1 == TypeCode.Double || tc2 == TypeCode.Double ||
//				tc1 == TypeCode.Single || tc2 == TypeCode.Single ||
//				tc1 == TypeCode.Int64 || tc2 == TypeCode.Int64)
//				return getIDivideInt64(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int32 || tc2 == TypeCode.Int32)    
//				return getIDivideInt32(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int16 || tc2 == TypeCode.Int16)    
//				return getIDivideInt16(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Byte || tc2 == TypeCode.Byte)    
//				return getIDivideByte(o1,o2,tc1,tc2);
//			else    
//				return getIDivideInt16(o1,o2,tc1,tc2);
//		}
//
//		private static object negObj(
//			object obj,
//			int tc) {
//			IConvertible conv;
//
//			switch (tc) {
//				case TypeCode.Empty :
//					return new ClrInt32(0);
//				case TypeCode.Boolean :
//					int boolValue = (((ClrBoolean)obj).ToBoolean(null) == true)? 1 :0;
//					return new ClrInt16((short)boolValue); 
//				case TypeCode.Byte :
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj); 
//					short byteValue = (short) (0 - conv.ToByte(null));
//					return new ClrInt16(byteValue);
//				case TypeCode.Int16 :
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj); 
//					int shortValue =  (0 - conv.ToInt16(null));
//					return new ClrInt32(shortValue);
//				case TypeCode.Int32 :
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					long intValue = (0 - conv.ToInt32(null));
//					return new ClrInt64(intValue);
//				case TypeCode.Int64 :
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					long longValue = conv.ToInt64(null);
//					if (longValue == ClrInt64.MinValue)
//						return Decimal.Negate(conv.ToDecimal(null));
//					else
//						return new ClrInt64(-longValue);     
//				case TypeCode.Decimal :
//					return Decimal.Negate((Decimal) obj);
//				case TypeCode.Single :
//					return new ClrSingle(-((ClrSingle) obj).getValue());
//				case TypeCode.Double :
//					return new ClrDouble(0 - ((ClrDouble) obj).getValue());
//				case TypeCode.String :
//					return new ClrDouble(0 - DoubleType.FromString((string) obj));
//			}
//
//			throw new ClassCastException(
//				Utils.GetResourceString(
//				"NoValidOperator_OneOperand",
//				Utils.VBFriendlyName(obj)));
//
//		}
//
//		//checked
//		public static bool IsTypeOf(Type typSrc, Type typParent) {
//			if (typSrc == typParent)
//				return true;
//			return typSrc.IsSubclassOf(typParent);
//		}
//
//		//checked
//		public static bool IsWideningConversion(Type FromType, Type ToType) {
//			int typeCode1 = Type.GetTypeCode(FromType);
//			int typeCode2 = Type.GetTypeCode(ToType);
//			if (typeCode1 == TypeCode.Object) {
//				//TODO:
//				//            if ((FromType == ObjectStaticWrapper.GetType(char[].class)) &&
//				//               (typeCode2 == TypeCode.String || ToType == ObjectStaticWrapper.GetType(char[].class)))
//				//            {
//				//                return true;
//				//            }
//				//            if (typeCode2 != TypeCode.Object)
//				//            {
//				//                return false;
//				//            }
//				//            if (!FromType.get_IsArray() || !ToType.get_IsArray())
//				//            {
//				//                return ToType.IsAssignableFrom(FromType);
//				//            }
//				//            //if (FromType.GetArrayRank() == ToType.GetArrayRank())
//				//            //{
//				//            //    return ToType.GetElementType().IsAssignableFrom(FromType.GetElementType());
//				//            //}
//				//            //else
//				//            //{
//				//                return false;
//				//            //}
//			}
//			if (typeCode2 == TypeCode.Object) {
//				//TODO:
//				//            if (ToType == ObjectStaticWrapper.GetType(char[].class) && typeCode1 == TypeCode.string)
//				//            {
//				//                return false;
//				//            }
//				return ToType.IsAssignableFrom(FromType);
//			}
//			if (ToType.IsEnum) {
//				return false;
//			}
//			int index1 = getVType2FromTypeCode(typeCode2);
//			int index2 = getVType2FromTypeCode(typeCode1);
//			int cC = ConversionClassTable[index1][index2];
//			if (cC != CC.Wide && cC != CC.Same) {
//				return false;
//			}
//			else {
//				return true;
//			}
//		}
//
//		//checked
//		public static bool IsWiderNumeric(Type Type1, Type Type2) {
//			int typeCode1 = Type.GetTypeCode(Type1);
//			int typeCode2 = Type.GetTypeCode(Type2);
//			if (!Utils.IsNumericType(Type1) || !Utils.IsNumericType(Type2)) {
//				return false;
//			}
//			if (typeCode1 == TypeCode.Boolean || typeCode2 == TypeCode.Boolean) {
//				return false;
//			}
//			if (Type1.get_IsEnum()) {
//				return false;
//			}
//
//			int index1 = getVTypeFromTypeCode(typeCode1);
//			int index2 = getVTypeFromTypeCode(typeCode2);
//			return WiderType[index1][index2] == getVTypeFromTypeCode(typeCode1);
//		}
//
//		//checked
//		public static bool LikeObj(
//			object vLeft,
//			object vRight,
//			CompareMethod compareOption) {
//			return StringType.StrLike(
//				StringType.FromObject(vLeft),
//				StringType.FromObject(vRight),
//				compareOption);
//		}
//
//		private static object getModByte(object o1,object o2,int tc1,int tc2) {
//			byte i1 = convertObjectToByte(o1,tc1);
//			byte i2 = convertObjectToByte(o2,tc2);
//			return new ClrByte((byte) (i1 % i2));
//		}
//
//		private static object getModDecimal(object o1,object o2,int tc1,int tc2) {
//			Decimal dec1 = convertObjectToDecimal(o1,tc1);
//			Decimal dec2 = convertObjectToDecimal(o2,tc2);
//			return Decimal.Remainder(dec1, dec2);
//		}
//
//		private static object getModDouble(object o1, object o2 , int tc1,int tc2) {
//			double d1 = convertObjectToDouble(o1,tc1);
//			double d2 = convertObjectToDouble(o1,tc1);
//			return new ClrDouble(d1 % d2);
//		}
//
//		private static object getModInt16(object o1, object o2 , int tc1,int tc2) {
//			short i1 = convertObjectToShort(o1,tc1);
//			short i2 = convertObjectToShort(o2,tc2);
//			int mod = i1 % i2;
//			if (mod < -32768 || mod > 32767)
//				return new ClrInt32(mod);
//			return new ClrInt16((short) mod);
//		}
//
//		private static object getModInt32(object o1, object o2 , int tc1,int tc2) {
//			int i1 = convertObjectToInt(o1,tc1);
//			int i2 = convertObjectToInt(o1,tc1);
//			long mod = (long) i1 % (long) i2;
//			if (mod < 2147483648L || mod > 2147483647L)
//				return new ClrInt64(mod);
//			return new ClrInt32((int) mod);
//		}
//
//		private static object getModInt64(object o1, object o2 , int tc1,int tc2) {
//			long l1 = convertObjectToLong(o1,tc1);
//			long l2 = convertObjectToLong(o2,tc2);
//			long mod = l1 % l2;
//			return new ClrInt64(mod);
//		}
//
//		public static object ModObj(object o1, object o2) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//        
//			checkIfValidObjects(o1,o2,tc1,tc2);
//        
//			if (tc2 == TypeCode.Empty)
//				throw new DivideByZeroException(
//					Environment.GetResourceString("Arg_DivideByZero"));
//			else if (tc1 == TypeCode.Empty)
//				return createZero(tc2);
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String)
//				return getModDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)
//				return getModDouble(o1,o2,tc1,tc2);   
//			else if (tc1 == TypeCode.Single || tc2 == TypeCode.Single)
//				return getModSingle(o1,o2,tc1,tc2);    
//			else if (tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal)
//				return getModDecimal(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int64 || tc2 == TypeCode.Int64)
//				return getModInt64(o1,o2,tc1,tc2); 
//			else if (tc1 == TypeCode.Int32 || tc2 == TypeCode.Int32)
//				return getModInt32(o1,o2,tc1,tc2);   
//			else if (tc1 == TypeCode.Int16 || tc2 == TypeCode.Int16)
//				return getModInt16(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Byte && tc2 == TypeCode.Byte)
//				return getModByte(o1,o2,tc1,tc2); 
//			else 
//				return getModInt16(o1,o2,tc1,tc2);
//		}
//
//
//		//checked
//		private static int getVType2FromTypeCode(int type) {
//			return VType2FromTypeCode[type];
//		}
//
//		private static object getModSingle(object o1, object o2,int tc1,int tc2) {
//			float sng1 = convertObjectToFloat(o1,tc1);
//			float sng2 = convertObjectToFloat(o2,tc2);
//			return new ClrSingle(sng1 % sng2);
//		}
//
//		private static object getMulByte(object o1, object o2,int tc1,int tc2) {
//			byte b1 = convertObjectToByte(o1,tc1);
//			byte b2 = convertObjectToByte(o2,tc2);
//			int i1 = b1 * b2;
//			if (i1 >= 0 && i1 <= 255)
//				return new ClrByte((byte) i1);
//			if (i1 >= -32768 && i1 <= 32767)
//				return new ClrInt16((short) i1);
//			return new ClrInt32(i1);
//		}
//
//		private static object getMulDouble(object o1, object o2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(o1,tc1);
//			double d2 = convertObjectToDouble(o2,tc2);
//			return new ClrDouble(d1 * d2);
//		}
//
//		private static object getMulInt16(object o1,object o2,int tc1,int tc2) {
//			short s1 = convertObjectToShort(o1,tc1);
//			short s2 = convertObjectToShort(o2,tc2);
//			int i1 = s1 * s2;
//			if (i1 >= -32768 && i1 <= 32767)
//				return new ClrInt16((short) i1);
//			return new ClrInt32(i1);
//		}
//
//		private static object getMulInt32(object o1,object o2,int tc1,int tc2) {
//			int i1 = convertObjectToInt(o1,tc1);
//			int i2 = convertObjectToInt(o2,tc2);
//			long l1 = (long) i1 * (long) i2;
//			if (l1 >= 2147483648L && l1 <= 2147483647L)
//				return new ClrInt32((int) l1);
//			return new ClrInt64(l1);
//		}
//
//		private static object getMulSingle(object o1, object o2 , int tc1,int tc2) {
//			float f1 = convertObjectToFloat(o1,tc1);
//			float f2 = convertObjectToFloat(o2,tc2);
//			double d = (double) f1 * (double) f2;
//			if (d <= 3.40282e+038 && d >= -3.40282e+038)
//				return new ClrSingle((float) d);
//			if (ClrDouble.IsInfinity(d)
//				&& ClrSingle.IsInfinity(f1)
//				|| ClrSingle.IsInfinity(f2))
//				return new ClrSingle((float) d);
//			return new ClrDouble(d);
//		}
//
//		private static object getMulDecimal(object o1, object o2,int tc1,int tc2) {
//			Decimal dec1 = convertObjectToDecimal(o1,tc1);
//			Decimal dec2 = convertObjectToDecimal(o1,tc1);
//
//			try {
//				Decimal dec3 = Decimal.Multiply(dec1, dec2);
//				return dec3;
//			}
//			catch (OverflowException exp) {
//				double d1 = Convert.ToDouble(dec1);
//				double d2 = Convert.ToDouble(dec2);
//
//				return new ClrDouble(d1 * d2);
//			}
//		}
//
//		private static object getMulInt64(object o1,object o2 , int tc1,int tc2) {
//			long l1 = convertObjectToLong(o1,tc1);
//			long l2 = convertObjectToLong(o2,tc2);
//			if (Long.MAX_VALUE / java.lang.Math.abs(l1) >= java.lang.Math.abs(l2))
//				return new ClrInt64(l1 * l2);
//
//			Decimal dec1 = new Decimal(l1);
//			Decimal dec2 = new Decimal(l2);
//
//			try {
//				Decimal dec3 = Decimal.Multiply(dec1, dec2);
//				return dec3;
//			}
//			catch (OverflowException exp) {
//				double d1 = (double) l1 * (double) l2;
//
//				return new ClrDouble(d1);
//			}
//		}
//    
//		private static object createZero(int tc) {
//			switch(tc) {
//				case TypeCode.Boolean:
//					return new ClrInt16((short)0);
//				case TypeCode.Byte:
//					return new Byte((byte)0);    
//				case TypeCode.Int16:
//					return new ClrInt16((short)0);
//				case TypeCode.Int32:
//					return new ClrInt32(0);
//				case TypeCode.Int64:
//					return new ClrInt64(0);
//				case TypeCode.Single:
//					return new ClrSingle(0.0f);
//				case TypeCode.Double:
//					return new ClrDouble(0.0);
//				case TypeCode.Decimal:
//					return Decimal.Zero;
//				case TypeCode.string:
//										return new ClrDouble(0.0);
//				default:
//					return null;     
//			}        
//		}
//
//		public static object MulObj(object o1, object o2) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//			checkIfValidObjects(o1,o2,tc1,tc2);
//        
//			if (tc1 == TypeCode.Empty && tc2 == TypeCode.Empty)
//				return new ClrInt32(0);
//			else if (tc1 == TypeCode.Empty)
//				return createZero(tc2);
//			else if (tc2 == TypeCode.Empty)
//				return createZero(tc1);
//			else if (tc1 == TypeCode.String || tc1 == TypeCode.String)
//				return getMulDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Double || tc1 == TypeCode.Double)
//				return getMulDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Single || tc1 == TypeCode.Single)
//				return getMulSingle(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Decimal || tc1 == TypeCode.Decimal)
//				return getMulDecimal(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int64 || tc1 == TypeCode.Int64)
//				return getMulInt64(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int32 || tc1 == TypeCode.Int32)
//				return getMulInt32(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int16 || tc1 == TypeCode.Int16)
//				return getMulInt16(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Byte || tc1 == TypeCode.Byte)
//				return getMulByte(o1,o2,tc1,tc2);
//			else 
//				return getMulInt16(o1,o2,tc1,tc2);
//		}
//
//		//checked !!
//		public static object NegObj(object obj) {
//			int tc = getTypeCode(obj);
//			return negObj(obj, tc);
//		}
//
//		public static object NotObj(object obj) {        
//			IConvertible conv;
//			Type type;
//			int tc = getTypeCode(obj);        
//
//			if (obj == null)
//				return new ClrInt32(-1);
//
//			switch (tc) {
//				case TypeCode.Boolean :
//					return new ClrBoolean(!((ClrBoolean)obj).ToBoolean(null));
//				case TypeCode.Byte :
//					type = ObjectStaticWrapper.GetType(obj);
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					byte byteVal = (byte) ~(conv.ToByte(null));
//					if (type.get_IsEnum())
//						return Enum.ToObject(type, byteVal);
//					return new ClrByte(byteVal);
//				case TypeCode.Int16 :
//					type = ObjectStaticWrapper.GetType(obj);
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					short shortVal = (short) ~(conv.ToInt16(null));
//					if (type.get_IsEnum())
//						return Enum.ToObject(type, shortVal);
//					return new ClrInt16(shortVal);
//				case TypeCode.Int32 :
//					type = ObjectStaticWrapper.GetType(obj);
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					int intVal = ~(conv.ToInt32(null));
//					if (type.get_IsEnum())
//						return Enum.ToObject(type, intVal);
//					return new ClrInt32(intVal);
//				case TypeCode.Int64 :
//					type = ObjectStaticWrapper.GetType(obj);
//					conv = IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//					long longVal = ~(conv.ToInt64(null));
//					if (type.get_IsEnum())
//						return Enum.ToObject(type, longVal);
//					return new ClrInt64(longVal);
//				case TypeCode.Single :
//				case TypeCode.Double :
//				case TypeCode.Decimal :
//				case TypeCode.String :
//					long val = LongType.FromObject(obj);
//					return new ClrInt64(~val);
//			}
//
//			throw new ClassCastException(
//				Utils.GetResourceString(
//				"NoValidOperator_OneOperand",
//				Utils.VBFriendlyName(obj)));
//		}
//    
//		private static object createZeroForCompare(int tc) {
//			switch(tc) {
//				case TypeCode.Boolean:
//					return new ClrInt32(0);
//				case TypeCode.Char:
//					return new ClrChar((char)0);
//				case TypeCode.Byte:
//					return new Byte((byte)0);    
//				case TypeCode.Int16:
//					return new ClrInt16((short)0);
//				case TypeCode.Int32:
//					return new ClrInt32(0);
//				case TypeCode.Int64:
//					return new ClrInt64(0);
//				case TypeCode.Single:
//					return new ClrSingle(0.0f);
//				case TypeCode.Double:
//					return new ClrDouble(0.0);
//				case TypeCode.Decimal:
//					return Decimal.Zero;
//				case TypeCode.DateTime:
//					return null;
//				case TypeCode.String:
//					return null;
//				default:
//					return null;     
//			}        
//		}
//
//		public static int ObjTst(object o1, object o2, bool textCompare) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//			IComparable icomp1 = IComparableStaticWrapper.GetIComparableWrapper(o1); 
//			IComparable icomp2 = IComparableStaticWrapper.GetIComparableWrapper(o2); 
//        
//			if (tc1 == TypeCode.Empty && tc2 == TypeCode.Empty)
//				return 0;
//			else if (tc1 == TypeCode.Empty)
//				return icomp2.CompareTo(createZeroForCompare(tc2));      
//			else if (tc2 == TypeCode.Empty)
//				return icomp1.CompareTo(createZeroForCompare(tc1));      
//			else if (tc1 == TypeCode.DateTime || tc2 == TypeCode.DateTime)
//				return getObjTstDateTime(o1,o2,tc1,tc2);      
//			else if (tc1 == TypeCode.Char && tc2 == TypeCode.Char)
//				return getObjTstChar(o1,o2,tc1,tc2);      
//			else if ((tc1 == TypeCode.Boolean && tc2 == TypeCode.String)||
//				(tc2 == TypeCode.Boolean && tc1 == TypeCode.String) )
//				return getObjTstBoolean(o1,o2,tc1,tc2);      
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String)
//				return getObjTstStringObject(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)
//				return getObjTstStringObject(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String)
//				return getObjTstStringObject(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)
//				return getObjTstDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Single || tc2 == TypeCode.Single)
//				return getObjTstSingle(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal)
//				return getObjTstDecimal(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int64 || tc2 == TypeCode.Int64)
//				return getObjTstInt64(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int32 || tc2 == TypeCode.Int32)
//				return getObjTstInt32(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int16 || tc2 == TypeCode.Int16)
//				return getObjTstInt16(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Byte && tc2 == TypeCode.Byte)
//				return getObjTstByte(o1,o2,tc1,tc2);
//			else
//				return getObjTstInt16(o1,o2,tc1,tc2);
//		}
//
//		private static int getObjTstBoolean(object o1,object o2,int tc1,int tc2) {
//			IConvertible iconv1 = IConvertibleStaticWrapper.GetIConvertibleWrapper(o1);
//			IConvertible iconv2 = IConvertibleStaticWrapper.GetIConvertibleWrapper(o2);
//			bool b1 = iconv1.ToBoolean(null);
//			bool b2 = iconv2.ToBoolean(null);
//			if (b1 == b2)
//				return 0;
//			if (b1 == false)
//				return 1;
//			return -1;
//		}
//
//		private static int getObjTstByte(object o1,object o2,int tc1,int tc2) {
//			byte by1 = convertObjectToByte(o1,tc1);
//			byte by2 = convertObjectToByte(o2,tc2);
//			if (by1 < by2)
//				return -1;
//			if (by1 > by2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstChar(object o1,object o2,int tc1,int tc2) {
//			char ch1 = convertObjectToChar(o1,tc1);
//			char ch2 = convertObjectToChar(o2,tc2); 
//			if (ch1 < ch2)
//				return -1;
//			if (ch1 > ch2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstDateTime(object o1,object o2,int tc1,int tc2) {
//			DateTime var1 = convertObjectToDateTime(o1,tc1);
//			DateTime var2 = convertObjectToDateTime(o2,tc2);
//			long time1 = var1.getCalendar().getTime().getTime();
//			long time2 = var2.getCalendar().getTime().getTime();
//			if (time1 < time2)
//				return -1;
//			if (time1 > time2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstDecimal(object o1,object o2,int tc1,int tc2) {
//			Decimal dec1 = convertObjectToDecimal(o1,tc1);
//			Decimal dec2 = convertObjectToDecimal(o2,tc2);
//			int res = Decimal.Compare(dec1, dec2);
//			if (res < 0)
//				return -1;
//			if (res > 0)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstDouble(object o1,object o2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(o1,tc1);
//			double d2 = convertObjectToDouble(o2,tc2);
//			if (d1 < d2)
//				return -1;
//			if (d1 > d2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstInt16(object o1,object o2,int tc1,int tc2) {
//			short s1 = convertObjectToShort(o1,tc1);
//			short s2 = convertObjectToShort(o2,tc2);
//			if (s1 < s2)
//				return -1;
//			if (s1 > s2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstInt32(object o1,object o2,int tc1,int tc2) {
//			int i1 = convertObjectToInt(o1,tc1);
//			int i2 = convertObjectToInt(o2,tc2);
//			if (i1 < i2)
//				return -1;
//			if (i1 > i2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstInt64(object o1,object o2,int tc1,int tc2) {
//			long l1 = convertObjectToLong(o1,tc1);
//			long l2 = convertObjectToLong(o1,tc1);
//			if (l1 < l2)
//				return -1;
//			if (l1 > l2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstSingle(object o1,object o2,int tc1,int tc2) {
//			float f1 = convertObjectToFloat(o1,tc1);
//			float f2 = convertObjectToFloat(o2,tc2);
//			if (f1 < f2)
//				return -1;
//			if (f1 > f2)
//				return 1;
//			return 0;
//		}
//
//		private static int getObjTstStringObject(object s1, object s2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(s1,tc1);
//			double d2 = convertObjectToDouble(s2,tc2);
//			return ObjectType.getObjTstDouble(s1,s2,tc1, tc2);
//		}
//
//		public static object PlusObj(object obj) {
//			int tc = getTypeCode(obj);
//			if (tc == TypeCode.Empty)
//				return new ClrInt32(0);
//			else if (tc == TypeCode.Boolean) {
//				IConvertible conv =
//					IConvertibleStaticWrapper.GetIConvertibleWrapper(obj);
//				return new ClrInt16((short) conv.ToInt16(null));
//			}
//			else if (tc == TypeCode.String)
//				return new ClrDouble(DoubleType.FromObject(obj));
//			else if (tc == TypeCode.Byte || tc == TypeCode.Int16
//				|| tc == TypeCode.Int32  || tc == TypeCode.Int64
//				|| tc == TypeCode.Single || tc == TypeCode.Double
//				|| tc == TypeCode.Decimal)
//				return obj;
//			else
//				throw new ClassCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_OneOperand",
//					Utils.VBFriendlyName(obj)));
//		}
//    
//		public static object PowObj(object obj1, object obj2) {
//			if (obj1 == null || obj2 == null)
//				return new ClrInt32(1);
//			int tc1 = getTypeCode(obj1); 
//			int tc2 = getTypeCode(obj2);
//			int widestType =  GetWidestType(tc1,tc2);
//        
//			if (widestType == TypeCode.Byte      || widestType == TypeCode.Boolean
//				|| widestType == TypeCode.Int16  || widestType == TypeCode.Int32
//				|| widestType == TypeCode.Int64  || widestType == TypeCode.Single
//				|| widestType == TypeCode.Double || widestType == TypeCode.Decimal
//				|| widestType == TypeCode.String)
//				return new ClrDouble(
//					java.lang.Math.pow(
//					DoubleType.FromObject(obj1),
//					DoubleType.FromObject(obj2)));
//			else
//				throw new ClassCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//		}
//
//		//checked
//		public static object StrCatObj(object vLeft, object vRight) {
//			if (vLeft is DBNull && vRight is DBNull)
//				return DBNull.Value;
//			return StringType.FromObject(vLeft) + StringType.FromObject(vRight);
//		}
//
//		private static object getSubByte(object o1, object o2 , int tc1,int tc2) {
//			byte byte1 = convertObjectToByte(o1,tc1);
//			byte byte2 = convertObjectToByte(o2,tc2);
//			short s = (short) (byte1 - byte2);
//			if (s >= 0 && s <= 255)
//				return new ClrByte((byte) s);
//			return new ClrInt16(s);
//		}
//
//		private static object getSubDecimal(object o1, object o2,int tc1,int tc2) {
//			Decimal dec1 = convertObjectToDecimal(o1,tc1);
//			Decimal dec2 = convertObjectToDecimal(o2,tc2);
//			try {
//				Decimal dec3 = Decimal.Subtract(dec1, dec2);
//				return dec3;
//			}
//			catch (OverflowException exp) {
//				double d1 = Decimal.ToDouble(dec1);
//				double d2 = Decimal.ToDouble(dec2);
//
//				return new ClrDouble(d1 * d2);
//			}
//		}
//
//		private static object getSubInt16(object o1, object o2,int tc1,int tc2) {
//			short s1 = convertObjectToShort(o1,tc1);
//			short s2 = convertObjectToShort(o2,tc2);
//			int res = s1 - s2;
//			if (res >= -32768 && res <= 32767)
//				return new ClrInt16((short) res);
//			return new ClrInt32(res);
//		}
//
//		private static object getSubInt32(object o1, object o2,int tc1,int tc2) {
//			int i1 = convertObjectToInt(o1,tc1);
//			int i2 = convertObjectToInt(o2,tc2);
//			long res = (long) i1 - (long) i2;
//			if (res >= 2147483648L && res <= 2147483647L)
//				return new ClrInt32((int) res);
//			return new ClrInt64(res);
//		}
//
//		private static object getSubInt64(object o1, object o2,int tc1,int tc2) {
//			long l1 = convertObjectToLong(o1,tc1);
//			long l2 = convertObjectToLong(o2,tc2);
//        
//			if (Long.MAX_VALUE - java.lang.Math.abs(l1) <= java.lang.Math.abs(l2))
//				return new ClrInt64(l1 - l2);
//
//			Decimal dec1 = new Decimal(l1);
//			Decimal dec2 = new Decimal(l2);
//
//			Decimal dec3 = Decimal.Subtract(dec1, dec2);
//			return dec3;
//		}
//
//		private static object getSubSingle(object o1, object o2 ,int tc1 ,int tc2) {
//			float d1 = convertObjectToFloat(o1,tc1);
//			float d2 = convertObjectToFloat(o2,tc2);
//			double res = (double) d1 - (double) d2;
//			if (res <= 3.40282e+038 && res >= -3.40282e+038)
//				return new ClrSingle((float) res);
//			if (ClrDouble.IsInfinity(res)
//				&& ClrSingle.IsInfinity(d1)
//				|| ClrSingle.IsInfinity(d2))
//				return new ClrSingle((float) res);
//			return new ClrDouble(res);
//		}
//    
//		private static DateTime convertObjectToDateTime(object o1, int tc1) {
//			DateTime dateTime = null;
//			if (o1 != null) {
//				if (o1 is string)
//					dateTime = DateType.FromString((string) o1);
//				else if (o1 is IConvertible) {
//					dateTime = ((IConvertible) o1).ToDateTime(null);
//				}
//			}
//			return dateTime;
//		}
//    
//		private static char convertObjectToChar(object o1 , int tc1) {
//			char char1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					char1 = CharType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						char1 = (char)toVBBool((IConvertible)o1);
//					}
//					else
//						char1 = (char)((IConvertible)o1).ToChar(null);
//				}
//			}
//			return char1;
//		}
//    
//		private static byte convertObjectToByte(object o1 , int tc1) {
//			byte byte1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					byte1 = (byte)ByteType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						byte1 = (byte)toVBBool((IConvertible)o1);
//					}
//					else
//						byte1 = (byte)((IConvertible)o1).ToByte(null);
//				}
//			}
//			return byte1;
//		}
//    
//		private static short convertObjectToShort(object o1 , int tc1) {
//			short s1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					s1 = ShortType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						s1 = (short)toVBBool((IConvertible)o1);
//					}
//					else
//						s1 = ((IConvertible)o1).ToInt16(null);
//				}
//			}
//			return s1;
//		}
//    
//		private static int convertObjectToInt(object o1 , int tc1) {
//			int i1=0;
//			if (o1 != null) {
//				if (o1 is string)
//					i1 = IntegerType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						i1 = (int)toVBBool((IConvertible)o1);
//					}
//					else
//						i1 = ((IConvertible)o1).ToInt32(null);
//				}
//			}
//			return i1;
//		}
//    
//		private static Decimal convertObjectToDecimal(object o1 , int tc1) {
//			Decimal dec1 = null;
//			if (o1 != null) {
//				if (o1 is string)
//					dec1 = DecimalType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						dec1 = toVBBoolConv((IConvertible)o1).ToDecimal(null);
//					}
//					else
//						dec1 = ((IConvertible)o1).ToDecimal(null);
//				}
//			}
//			return dec1;
//		}
//    
//		private static long convertObjectToLong(object o1 , int tc1) {
//			long l1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					l1 = LongType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						l1 = (long)toVBBool((IConvertible)o1);
//					}
//					else
//						l1 = ((IConvertible)o1).ToInt64(null);
//				}
//			}
//			return l1;
//		}
//    
//		private static float convertObjectToFloat(object o1 , int tc1) {
//			float d1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					d1 = SingleType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						d1 = (float)toVBBool((IConvertible)o1);
//					}
//					else
//						d1 = ((IConvertible)o1).ToSingle(null);
//				}
//			}
//			return d1;
//		}
//    
//		private static double convertObjectToDouble(object o1 , int tc1) {
//			double d1 = 0;
//			if (o1 != null) {
//				if (o1 is string)
//					d1 = DoubleType.FromString((string)o1);
//				else if (o1 is IConvertible) {
//					if (tc1 == TypeCode.Boolean) {
//						d1 = (double) toVBBool((IConvertible)o1);
//					}
//					else
//						d1 = ((IConvertible)o1).ToDouble(null);
//				}
//			}
//			return d1;
//		}
//
//		private static object getSubDouble(object s1, object s2,int tc1,int tc2) {
//			double d1 = convertObjectToDouble(s1,tc1);
//			double d2 = convertObjectToDouble(s2,tc2);
//			return new ClrDouble(d1 - d2);
//		}
//
//
//		public static object SubObj(object o1, object o2) {
//			int tc1 = getTypeCode(o1);
//			int tc2 = getTypeCode(o2);
//			checkIfValidObjects(o1,o2,tc1,tc2);
//        
//			if (tc1 == TypeCode.Empty && tc2 == TypeCode.Empty)
//				return new ClrInt32(0);
//			else if (tc2 == TypeCode.Empty)
//				return o1;
//			else if (tc1 == TypeCode.Empty)
//				return  negObj(o2, tc2);
//			else if (tc1 == TypeCode.Double || tc2 == TypeCode.Double)
//				return getSubDouble(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.String || tc2 == TypeCode.String)
//				return getSubDouble(o1,o2,tc1,tc2);               
//			else if (tc1 == TypeCode.Single || tc2 == TypeCode.Single)
//				return getSubSingle(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Decimal || tc2 == TypeCode.Decimal)
//				return getSubDecimal(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int64 || tc2 == TypeCode.Int64)
//				return getSubInt64(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int32 || tc2 == TypeCode.Int32)
//				return getSubInt32(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Int16 || tc2 == TypeCode.Int16)
//				return getSubInt16(o1,o2,tc1,tc2);
//			else if (tc1 == TypeCode.Byte && tc2 == TypeCode.Byte)
//				return getSubByte(o1,o2,tc1,tc2);
//			else 
//				return getSubInt16(o1,o2,tc1,tc2);    
//
//		}
//
//		//checked
//		private static void throwNoValidOperator(object obj1, object obj2) {
//			string obj1MsgStr = "'Nothing'";
//			string obj2MsgStr = "'Nothing'";
//			string obj1Name = Utils.VBFriendlyName(obj1);
//			string obj2Name = Utils.VBFriendlyName(obj2);
//			if (obj1 is string)
//				obj1MsgStr =
//					Utils.GetResourceString(
//					"NoValidOperator_StringType1",
//					(string) obj1);
//			else if (obj1 != null)
//				obj1MsgStr =
//					Utils.GetResourceString(
//					"NoValidOperator_NonStringType1",
//					obj1Name);
//			if (obj2 is string)
//				obj2MsgStr =
//					Utils.GetResourceString(
//					"NoValidOperator_StringType1",
//					(string) obj2);
//			else if (obj2 != null)
//				obj2MsgStr =
//					Utils.GetResourceString(
//					"NoValidOperator_NonStringType1",
//					obj2Name);
//        
//			throw new ClassCastException(
//				Utils.GetResourceString(
//				"NoValidOperator_TwoOperands",
//				obj1MsgStr,
//				obj2MsgStr));
//		}
//    
//		public static object ShiftLeftObj (object o1, int amount) {
//			IConvertible convertible1 =
//				IConvertibleStaticWrapper.GetIConvertibleWrapper(o1);
//			int tc = getTypeCode(o1);
//			TypeCode code2;
//			if (tc == TypeCode.Empty)
//				return new ClrInt32(0);
//			else if (tc == TypeCode.Boolean)
//				return new ClrInt16(
//					(short) (((short) - convertible1.ToInt16(null))
//					<< (amount & 15)));
//			else if (tc == TypeCode.Byte)
//				return new Byte((byte) (convertible1.ToByte(null) << (amount & 7)));
//			else if (tc == TypeCode.Int16)
//				return new ClrInt16(
//					(short) (convertible1.ToInt16(null) << (amount & 15)));
//			else if (tc == TypeCode.Int32)
//				return new ClrInt32(convertible1.ToInt32(null) << (amount & 31));
//			else if (
//				tc == TypeCode.Int64
//				|| tc == TypeCode.Single
//				|| tc == TypeCode.Double
//				|| tc == TypeCode.Decimal)
//				return new ClrInt64(convertible1.ToInt64(null) << (amount & 63));
//			else if (tc == TypeCode.String)
//				return new ClrInt64(
//					LongType.FromString(convertible1.ToString(null))
//					<< (amount & 63));
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_OneOperand",
//					Utils.VBFriendlyName(o1)));
//		}
//    
//		public static object ShiftRightObj (object o1, int amount) {
//			IConvertible convertible1 =
//				IConvertibleStaticWrapper.GetIConvertibleWrapper(o1);
//			int tc = getTypeCode(o1);
//			TypeCode code2;
//			if (tc == TypeCode.Empty)
//				return new ClrInt32(0);
//			else if (tc == TypeCode.Boolean)
//				return new ClrInt16(
//					(short) (((short) - convertible1.ToInt16(null))
//					>> (amount & 15)));
//			else if (tc == TypeCode.Byte)
//				return new Byte((byte) (convertible1.ToByte(null) >> (amount & 7)));
//			else if (tc == TypeCode.Int16)
//				return new ClrInt16(
//					(short) (convertible1.ToInt16(null) >> (amount & 15)));
//			else if (tc == TypeCode.Int32)
//				return new ClrInt32(convertible1.ToInt32(null) >> (amount & 31));
//			else if (
//				tc == TypeCode.Int64
//				|| tc == TypeCode.Single
//				|| tc == TypeCode.Double
//				|| tc == TypeCode.Decimal)
//				return new ClrInt64(convertible1.ToInt64(null) >> (amount & 63));
//			else if (tc == TypeCode.String)
//				return new ClrInt64(
//					LongType.FromString(convertible1.ToString(null))
//					>> (amount & 63));
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_OneOperand",
//					Utils.VBFriendlyName(o1))); 
//		}
//
//
//		//checked
//		public static Type TypeFromTypeCode(TypeCode vartype) {
//			return TypeFromTypeCode(vartype.getValue());
//		}
//		//checked
//		public static Type TypeFromTypeCode(int vartype) {
//			return TypeFromTypeCode[vartype];
//		}
//
//		public static object XorObj(object obj1, object obj2) {
//			if (obj1 == null && obj2 == null)
//				return new ClrInt32(0);
//			int tc1 = getTypeCode(obj1);
//			int tc2 = getTypeCode(obj2);
//			int widestType = GetWidestType(tc1, tc2);
//			if (widestType == TypeCode.Boolean    || widestType == TypeCode.Byte
//				|| widestType == TypeCode.Int16   || widestType == TypeCode.Int32
//				|| widestType == TypeCode.Int64   || widestType == TypeCode.Single
//				|| widestType == TypeCode.Double  || widestType == TypeCode.Decimal
//				|| widestType == TypeCode.String) {
//				bool b1 = BooleanType.FromObject(obj1);
//				bool b2 = BooleanType.FromObject(obj2);
//				return new ClrBoolean(b1 ^ b2);
//			}
//			else
//				throw new InvalidCastException(
//					Utils.GetResourceString(
//					"NoValidOperator_TwoOperands",
//					Utils.VBFriendlyName(obj1),
//					Utils.VBFriendlyName(obj2)));
//		}
//		//TODO:
//		//    static class VType extends Enum implements IClrInt
//		//    {
//		//        public int value__;
//		//
//		//        public static final int t_bad = 0;
//		//        public static final VType _t_bad = new VType(0);
//		//
//		//        public static final int t_bool = 1;
//		//        public static final VType _t_bool = new VType(1);
//		//
//		//        public static final int t_ui1 = 2;
//		//        public static final VType _t_ui1 = new VType(2);
//		//
//		//        public static final int t_i2 = 3;
//		//        public static final VType _t_i2 = new VType(3);
//		//
//		//        public static final int t_i4 = 4;
//		//        public static final VType _t_i4 = new VType(4);
//		//
//		//        public static final int t_i8 = 5;
//		//        public static final VType _t_i8 = new VType(5);
//		//
//		//        public static final int t_dec = 6;
//		//        public static final VType _t_dec = new VType(6);
//		//
//		//        public static final int t_r4 = 7;
//		//        public static final VType _t_r4 = new VType(7);
//		//
//		//        public static final int t_r8 = 8;
//		//        public static final VType _t_r8 = new VType(8);
//		//
//		//        public static final int t_char = 9;
//		//        public static final VType _t_char = new VType(9);
//		//
//		//        public static final int t_str = 10;
//		//        public static final VType _t_str = new VType(10);
//		//
//		//        public static final int t_date = 11;
//		//        public static final VType _t_date = new VType(11);
//		//
//		//        public VType()
//		//        {}
//		//
//		//        private VType(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//        
//		//        public void __ZeroInit__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public void __RealCtor__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public VType __Clone__()
//		//        {
//		//            return new VType(value__);
//		//        }
//		//        
//		//
//		//        public int getValue()
//		//        {
//		//            return value__;
//		//        }
//		//
//		//        public void setValue(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//
//		//        public static VType getEnumForValue(int value)
//		//        {
//		//            switch(value)
//		//            {
//		//                case t_bad:	return _t_bad;
//		//                case t_bool:	return _t_bool;
//		//                case t_ui1:	return _t_ui1;
//		//                case t_i2:	return _t_i2;
//		//                case t_i4:	return _t_i4;
//		//                case t_i8:	return _t_i8;
//		//                case t_dec:	return _t_dec;
//		//                case t_r4:	return _t_r4;
//		//                case t_r8:	return _t_r8;
//		//                case t_char:	return _t_char;
//		//                case t_str:	return _t_str;
//		//                case t_date:	return _t_date;
//		//            }
//		//
//		//            throw new IllegalArgumentException("The value " + value + " is not" + 
//		//                " valid for enumeration VType");
//		//        }
//		//
//		//        protected object GetValue()
//		//        {
//		//            return new system.ClrInt32(getValue());
//		//        }
//		//    }
//		//
//		//    static class VType2 extends Enum implements IClrInt
//		//    {
//		//        public int value__;
//		//
//		//        public static final int t_bad = 0;
//		//        public static final VType2 _t_bad = new VType2(0);
//		//
//		//        public static final int t_bool = 1;
//		//        public static final VType2 _t_bool = new VType2(1);
//		//
//		//        public static final int t_ui1 = 2;
//		//        public static final VType2 _t_ui1 = new VType2(2);
//		//
//		//        public static final int t_char = 3;
//		//        public static final VType2 _t_char = new VType2(3);
//		//
//		//        public static final int t_i2 = 4;
//		//        public static final VType2 _t_i2 = new VType2(4);
//		//
//		//        public static final int t_i4 = 5;
//		//        public static final VType2 _t_i4 = new VType2(5);
//		//
//		//        public static final int t_i8 = 6;
//		//        public static final VType2 _t_i8 = new VType2(6);
//		//
//		//        public static final int t_r4 = 7;
//		//        public static final VType2 _t_r4 = new VType2(7);
//		//
//		//        public static final int t_r8 = 8;
//		//        public static final VType2 _t_r8 = new VType2(8);
//		//
//		//        public static final int t_date = 9;
//		//        public static final VType2 _t_date = new VType2(9);
//		//
//		//        public static final int t_dec = 10;
//		//        public static final VType2 _t_dec = new VType2(10);
//		//
//		//        public static final int t_ref = 11;
//		//        public static final VType2 _t_ref = new VType2(11);
//		//
//		//        public static final int t_str = 12;
//		//        public static final VType2 _t_str = new VType2(12);
//		//
//		//        public VType2()
//		//        {}        
//		//
//		//        private VType2(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//        
//		//        public void __ZeroInit__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public void __RealCtor__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public VType2 __Clone__()
//		//        {
//		//            return new VType2(value__);
//		//        }
//		//        
//		//
//		//        public int getValue()
//		//        {
//		//            return value__;
//		//        }
//		//
//		//        public void setValue(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//
//		//        public static VType2 getEnumForValue(int value)
//		//        {
//		//            switch(value)
//		//            {
//		//                case t_bad:	return _t_bad;
//		//                case t_bool:	return _t_bool;
//		//                case t_ui1:	return _t_ui1;
//		//                case t_char:	return _t_char;
//		//                case t_i2:	return _t_i2;
//		//                case t_i4:	return _t_i4;
//		//                case t_i8:	return _t_i8;
//		//                case t_r4:	return _t_r4;
//		//                case t_r8:	return _t_r8;
//		//                case t_date:	return _t_date;
//		//                case t_dec:	return _t_dec;
//		//                case t_ref:	return _t_ref;
//		//                case t_str:	return _t_str;
//		//            }
//		//
//		//            throw new IllegalArgumentException("The value " + value + " is not" + 
//		//                " valid for enumeration VType2");
//		//        }
//		//
//		//        protected object GetValue()
//		//        {
//		//            return new system.ClrInt32(getValue());
//		//        }
//		//    }
//		//
//		//    static class CC extends Enum implements IClrInt
//		//    {
//		//        public int value__;
//		//
//		//        public static final int Err = 0;
//		//        public static final CC _Err = new CC(0);
//		//
//		//        public static final int Same = 1;
//		//        public static final CC _Same = new CC(1);
//		//
//		//        public static final int Narr = 2;
//		//        public static final CC _Narr = new CC(2);
//		//
//		//        public static final int Wide = 3;
//		//        public static final CC _Wide = new CC(3);
//		//
//		//        public CC()
//		//        {}
//		//
//		//        private CC(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//        
//		//        public void __ZeroInit__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public void __RealCtor__()
//		//        {
//		//            value__ = 0;
//		//        }
//		//    
//		//        public CC __Clone__()
//		//        {
//		//            return new CC(value__);
//		//        }
//		//
//		//        public int getValue()
//		//        {
//		//            return value__;
//		//        }
//		//
//		//        public void setValue(int value)
//		//        {
//		//            value__ = value;
//		//        }
//		//
//		//        public static CC getEnumForValue(int value)
//		//        {
//		//            switch(value)
//		//            {
//		//                case Err:	return _Err;
//		//                case Same:	return _Same;
//		//                case Narr:	return _Narr;
//		//                case Wide:	return _Wide;
//		//            }
//		//
//		//            throw new IllegalArgumentException("The value " + value + " is not" + 
//		//                " valid for enumeration CC");
//		//        }
//		//
//		//        protected object GetValue()
//		//        {
//		//            return new system.ClrInt32(getValue());
//		//        }
//		//    }
//	}
//}
