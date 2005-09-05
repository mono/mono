/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal class GlobalizationHelper
	{
		#region Constructors

		private GlobalizationHelper()
		{
		}

		#endregion

		#region Static Methods

		public static bool CultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(
				strA,
				strB,
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth |
				CompareOptions.IgnoreCase) == 0 ? true : false;
		}

		#endregion
	}
}
