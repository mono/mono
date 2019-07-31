#region HEADER
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;
using Test_NUnit_MySql;

#endregion

namespace Test_NUnit
{
#region HEADER
    /// <summary>
    /// when a problem crops up in NUnit, you can convert the project from DLL into EXE, 
    /// and debug into the offending method.
    /// </summary>
#endregion
    class Program2
    {
        static void Main()
        {
            //new ReadTest_GroupBy().G01_SimpleGroup_Count();
            //new ReadTest_GroupBy().G05_Group_Into();
            //new ReadTest().D04_SelectProducts_OrderByName();
            //new Join().Join_Laurent();
            //new ReadTest().C2_SelectPenId();
            //new ReadTest_Complex().D2_ArrayContains();
            //new ReadTest_Complex().O3_OperatorUnion();
            //new StoredProcTest().SPB_GetOrderCount_Having();
            //new ReadTest().E2_ConnectionClosedTest();
            //new ReadTest_AllTypes().AT1_SelectRow();
            //new ReadTest_Operands().H1_SelectConcat();
            //rc.F11_ConcatString();
            new WriteTest().G5_SetFieldToNull();
            //new WriteTest_BulkInsert().BI01_InsertProducts();
            //new NullTest().NullableT_Value();
            //new Count_Sum_Min_Max_Avg().LiqnToSqlCount02();
            //new Top_Bottom().LinqToSqlTop03_Ex_Andrus();
            //new Object_Identity().LinqToSqlObjectIdentity01();
            //new String_Date_functions().LinqToSqlString02();
        }
    }

}
