//
// System.Data.OleDb.OleDbSchemaGuid
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbSchemaGuid
	{
		#region Fields

		public static readonly Guid Assertions = new Guid ("df855bea-fb95-4abc-8932-e57e45c7ddae");
		public static readonly Guid Catalogs = new Guid ("e4a67334-f03c-45af-8b1d-531f99268045");
		public static readonly Guid Character_Sets = new Guid ("e4533bdb-0b55-48ee-986d-17d07143657d");
		public static readonly Guid Check_Constraints = new Guid ("fedf7f5d-cfb4-4635-af02-45eb4bb4e8f3");
		public static readonly Guid Check_Constraints_By_Table = new Guid ("d76547ef-837d-413c-8d76-bab1d7bb014a");
		public static readonly Guid Collations = new Guid ("5145b85c-c448-4b9e-8929-4c2de31ffa30");
		public static readonly Guid Columns = new Guid ("86dcd6e2-9a8c-4c6d-bc1c-e0e334c727c9");
		public static readonly Guid Column_Domain_Usage = new Guid ("058acb5e-eb1d-4b6e-8e98-a7d59a959ff1");
		public static readonly Guid Column_Privileges = new Guid ("43152796-f3b4-4342-9647-008f1060e352");
		public static readonly Guid Constraint_Column_Usage = new Guid ("3a39f999-f481-4293-8b9f-af7e91b4ee7d");
		public static readonly Guid Constraint_Table_Usage = new Guid ("d689719b-24b0-4963-a635-097c480edcd2");
		public static readonly Guid DbInfoLiterals = new Guid ("7a564da6-f3bc-474b-9e66-71cb47bde5b0");
		public static readonly Guid Foreign_Keys = new Guid ("d9e547ce-e62d-4200-b849-566bc3dc29de");
		public static readonly Guid Indexes = new Guid ("69d8523c-96ad-40cb-a89a-ee98d2d6fcec");
		public static readonly Guid Key_Column_Usage = new Guid ("65423211-805e-4822-8eb4-f4f6d540056e");
		public static readonly Guid Primary_Keys = new Guid ("c6e5b174-fbd8-4055-b757-8585040e463f");
		public static readonly Guid Procedures = new Guid ("61f276ad-4f25-4c26-b4ae-8238e06d56db");
		public static readonly Guid Procedure_Columns = new Guid ("7148080d-e053-4ada-b79a-9a2ff614a3d4");
		public static readonly Guid Procedure_Parameters = new Guid ("984af700-8fe7-476f-81c2-4b814df67907");
		public static readonly Guid Provider_Types = new Guid ("0bc2da44-d834-4136-9ff0-3cef477784b9");
		public static readonly Guid Referential_Constraints = new Guid ("d2eab85e-49a7-462d-aa22-1d97c74178ae");
		public static readonly Guid Schemata = new Guid ("2fbd7503-0af3-43d2-92c6-51e78b84dd37");
		public static readonly Guid Sql_Languages = new Guid ("d60a511d-a07f-4e59-aac2-71c25fab5b02");
		public static readonly Guid Statistics = new Guid ("03ed9f7d-35bc-45fe-993f-ee7a5f29fb74");
		public static readonly Guid Tables = new Guid ("ceac88ba-240c-4bb4-821e-4a49fc013371");
		public static readonly Guid Tables_Info = new Guid ("9ff81c59-2b1e-4371-a08b-3a2d373189fa");
		public static readonly Guid Table_Constraints = new Guid ("62883c55-082d-42cb-bb00-747985ca6047");
		public static readonly Guid Table_Privileges = new Guid ("1a73f478-8c8e-4ede-b3ec-22ba13ab55a0");
		public static readonly Guid Table_Statistics = new Guid ("9c944744-cd51-448a-8be4-7095f039d0ef");
		public static readonly Guid Translations = new Guid ("5578b57e-a682-4f1b-bdb4-f8a14ad6f61e");
		public static readonly Guid Trustee = new Guid ("521207e2-3a23-42b6-ac78-810a3fce3271");
		public static readonly Guid Usage_Privileges = new Guid ("f8113a2b-2934-4c67-ab7b-adbe3ab74973");
		public static readonly Guid Views = new Guid ("9a6345b6-61a0-40fd-9b45-402f3c9c9c3e");
		public static readonly Guid View_Column_Usage = new Guid ("2c91ef91-02d8-4d38-ae5a-1e826873d6ea");
		public static readonly Guid View_Table_Usage = new Guid ("7dcb7f53-1045-4fdf-86a1-d3caaf27c7f5");

		#endregion

		#region Constructors

		public OleDbSchemaGuid ()
		{
		}

		#endregion
	}
}
