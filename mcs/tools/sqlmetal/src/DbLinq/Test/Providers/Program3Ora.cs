using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
#if ODP
using Test_NUnit_OracleODP;
#else
using Test_NUnit_Oracle;
#endif

namespace Test_NUnit
{
    /// <summary>
    /// when a problem crops up in NUnit, 
    /// you can convert the project from DLL into EXE, 
    /// and debug into the offending method.
    /// </summary>
    class Program2
    {
        static void Main()
        {
            //new ReadTest_Complex().F1_ProductCount();
            //new ReadTest().D04_SelectProducts_OrderByName();
            //new Linq_101_Samples.AdvancedTest().LinqToSqlAdvanced06();
            //new WriteTest().G2_DeleteTest();
            //new WriteTest().G1_InsertProduct();
            new ReadTest().C1_SelectProducts();
        }
    }
    //class Column { public string table_name; }
    //class Table { public string table_name; }
}
