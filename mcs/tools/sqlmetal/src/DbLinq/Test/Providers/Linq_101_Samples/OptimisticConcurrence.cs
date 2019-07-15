using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using nwind;
using Test_NUnit.Linq_101_Samples;

// test ns Linq_101_Samples
#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    [TestFixture]
    public class OptimisticConcurrence:TestBase
    {
        [Test(Description="Get conflict information. This sample demonstrates how to retrieve the changes that lead to an optimistic concurrency exception.")]

        public void LinqToSqlOptimistic01()
        {
            Northwind db=CreateDb();

    Console.WriteLine("YOU:  ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~");
    var product = db.Products.First(p=>p.ProductID = 1);
    Console.WriteLine("~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~");
    Console.WriteLine();
    Console.WriteLine("OTHER USER: ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~");
    // Open a second connection to the database to simulate another user
    // who is going to make changes to the Products table                

    var otherUser_db = Northwind(My.Settings.NORTHWINDConnectionString1) With {.Log = db.Log};
    var otherUser_product = otherUser_db.Products.First(p=>p.ProductID = 1);
    otherUser_product.UnitPrice = 999.99D;
    otherUser_product.UnitsOnOrder = 10;
    otherUser_db.SubmitChanges();
    Console.WriteLine("~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~");
    Console.WriteLine("YOU (continued): ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~");
    product.UnitPrice = 777.77D;

    var conflictOccurred = False;
    try{
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
    catch(ChangeConflictException c)
    {
        Console.WriteLine("* * * OPTIMISTIC CONCURRENCY EXCEPTION * * *")
        For Each aConflict In db.ChangeConflicts
            var prod = CType(aConflict.Object, Product)
            Console.WriteLine("The conflicting product has ProductID {0}", prod.ProductID)
            Console.WriteLine()
            Console.WriteLine("Conflicting members:")
            Console.WriteLine()
            For Each memConflict In aConflict.MemberConflicts
                var name = memConflict.Member.Name
                var yourUpdate = memConflict.CurrentValue.ToString()
                var original = memConflict.OriginalValue.ToString()
                var theirUpdate = memConflict.DatabaseValue.ToString()
                If (memConflict.IsModified) Then

                    Console.WriteLine("//{0}// was updated from {1} to {2} while you updated it to {3}", _
                                          name, original, theirUpdate, yourUpdate)
                Else
                    Console.WriteLine("//{0}// was updated from {1} to {2}, you did not change it.", _
                                                                        name, original, theirUpdate)
                End If
                Console.WriteLine()
            Next
            conflictOccurred = True
        Next

        Console.WriteLine()
        If (Not conflictOccurred) Then

            Console.WriteLine("* * * COMMIT SUCCESSFUL * * *")
            Console.WriteLine("Changes to Product 1 saved.")
        End If
        Console.WriteLine("~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ")

        ResetProducts() // clean up
    }
}



[Test(Description="Resolve conflicts: Overwrite current values. This sample demonstrates how to automatically resolve concurrency conflicts. The //overwrite current values// option writes the new database values to the client objects.")]

public void LinqToSqlOptimistic02()
            {
            Northwind db=CreateDb();


    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    db.Log = null

    var product = db.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("You retrieve the product 1, it costs {0}", product.UnitPrice)
    Console.WriteLine("There are {0} units in stock, {1} units on order", product.UnitsInStock, product.UnitsOnOrder)
    Console.WriteLine()

    Console.WriteLine("Another user changes the price to 22.22 and UnitsInStock to 22")
    var otherUser_product = otherUser_db.Products.First(p=>p.ProductID = 1)
    otherUser_product.UnitPrice = 22.22D
    otherUser_product.UnitsInStock = 22
    otherUser_db.SubmitChanges()

    Console.WriteLine("You set the price of product 1 to 1.01 and UnitsOnOrder to 11")
    product.UnitPrice = 1.01D
    product.UnitsOnOrder = 11
    Try
        Console.WriteLine("You submit")
        Console.WriteLine()
        db.SubmitChanges()
    Catch c As ChangeConflictException
        WriteConflictDetails(db.ChangeConflicts)  // write changed objects / members to console
        Console.WriteLine()
        Console.WriteLine("Resolve by overwriting current values")
        db.ChangeConflicts.ResolveAll(RefreshMode.OverwriteCurrentValues)
        db.SubmitChanges()
    End Try

    Console.WriteLine()
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    var result = dbResult.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("Now product 1 has price={0}, UnitsInStock={1}, UnitsOnOrder={2}", _
        result.UnitPrice, result.UnitsInStock, result.UnitsOnOrder)
    Console.WriteLine()
    ResetProducts() // clean up
}



[Test(Description="Resolve conflicts: Keep current values. This sample demonstrates how to automatically resolve concurrency conflicts. The //keep current values// option changes everything to the values of this client.")]
public void LinqToSqlOptimistic03()
            {
            Northwind db=CreateDb();

    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    db.Log = null

    var Prod = db.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("You retrieve the product 1, it costs {0}", Prod.UnitPrice)
    Console.WriteLine("There are {0} units in stock, {1} units on order", Prod.UnitsInStock, Prod.UnitsOnOrder)
    Console.WriteLine()

    Console.WriteLine("Another user changes the price to 22.22 and UnitsInStock to 22")
    var otherUser_product = otherUser_db.Products.First(p=>p.ProductID = 1)
    otherUser_product.UnitPrice = 22.22D
    otherUser_product.UnitsInStock = 22
    otherUser_db.SubmitChanges()

    Console.WriteLine("You set the price of product 1 to 1.01 and UnitsOnOrder to 11")
    Prod.UnitPrice = 1.01D
    Prod.UnitsOnOrder = 11
    Try
        Console.WriteLine("You submit")
        Console.WriteLine()
        db.SubmitChanges()
    Catch c As ChangeConflictException
        WriteConflictDetails(db.ChangeConflicts) // write changed objects / members to console
        Console.WriteLine()
        Console.WriteLine("Resolve by keeping current values")
        db.ChangeConflicts.ResolveAll(RefreshMode.KeepCurrentValues)
        db.SubmitChanges()
    End Try
    Console.WriteLine()
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    var result = dbResult.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("Now product 1 has price={0}, UnitsInStock={1}, UnitsOnOrder={2}", _
        result.UnitPrice, result.UnitsInStock, result.UnitsOnOrder)
    Console.WriteLine()
    ResetProducts() // clean up
}



[Test(Description="Resolve conflicts: Keep changes. This sample demonstrates how to automatically resolve concurrency conflicts. The //keep changes// option keeps all changes from the current user and merges changes from other users if the corresponding field was not changed by the current user.")]

public void LinqToSqlOptimistic04()
            {
            Northwind db=CreateDb();


    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    db.Log = null

    var prod = db.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("You retrieve the product 1, it costs {0}", prod.UnitPrice)
    Console.WriteLine("There are {0} units in stock, {1} units on order", prod.UnitsInStock, prod.UnitsOnOrder)
    Console.WriteLine()

    Console.WriteLine("Another user changes the price to 22.22 and UnitsInStock to 22")
    var otherUser_product = otherUser_db.Products.First(p=>p.ProductID = 1)
    otherUser_product.UnitPrice = 22.22D
    otherUser_product.UnitsInStock = 22
    otherUser_db.SubmitChanges()

    Console.WriteLine("You set the price of product 1 to 1.01 and UnitsOnOrder to 11")
    prod.UnitPrice = 1.01D
    prod.UnitsOnOrder = 11D
    Try
        Console.WriteLine("You submit")
        Console.WriteLine()
        db.SubmitChanges()
    Catch c As ChangeConflictException
        WriteConflictDetails(db.ChangeConflicts) //write changed objects / members to console
        Console.WriteLine()
        Console.WriteLine("Resolve by keeping changes")
        db.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges)
        db.SubmitChanges()
    End Try
    Console.WriteLine()
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    var result = dbResult.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("Now product 1 has price={0}, UnitsInStock={1}, UnitsOnOrder={2}", _
        result.UnitPrice, result.UnitsInStock, result.UnitsOnOrder)
    Console.WriteLine()
    ResetProducts() // clean up
}



[Test(Description="Custom resolve rule. Demonstrates using MemberConflict.Resolve to write a custom resolve rule.")]

public void LinqToSqlOptimistic05()
            {
            Northwind db=CreateDb();


    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    db.Log = null

    var prod = db.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("You retrieve the product 1, it costs {0}", prod.UnitPrice)
    Console.WriteLine("There are {0} units in stock, {1} units on order", prod.UnitsInStock, prod.UnitsOnOrder)
    Console.WriteLine()

    Console.WriteLine("Another user changes the price to 22.22 and UnitsOnOrder to 2")
    var otherUser_product = otherUser_db.Products.First(p=>p.ProductID = 1)
    otherUser_product.UnitPrice = 22.22D
    otherUser_product.UnitsOnOrder = 2
    otherUser_db.SubmitChanges()

    Console.WriteLine("You set the price of product 1 to 1.01 and UnitsOnOrder to 11")
    prod.UnitPrice = 1.01D
    prod.UnitsOnOrder = 11
    var needsSubmit = True
    While needsSubmit
        Try
            Console.WriteLine("You submit")
            Console.WriteLine()
            needsSubmit = False
            db.SubmitChanges()
        Catch c As ChangeConflictException
            needsSubmit = True
            WriteConflictDetails(db.ChangeConflicts) // write changed objects / members to console
            Console.WriteLine()
            Console.WriteLine("Resolve by higher price / order")
            For Each conflict In db.ChangeConflicts
                conflict.Resolve(RefreshMode.KeepChanges)
                For Each memConflict In conflict.MemberConflicts
                    If (memConflict.Member.Name = "UnitPrice") Then
                        //always use the highest price
                        var theirPrice = CDec(memConflict.DatabaseValue)
                        var yourPrice = CDec(memConflict.CurrentValue)
                        memConflict.Resolve(Math.Max(theirPrice, yourPrice))
                    ElseIf (memConflict.Member.Name = "UnitsOnOrder") Then
                        //always use higher order
                        var theirOrder = CShort(memConflict.DatabaseValue)
                        var yourOrder = CShort(memConflict.CurrentValue)
                        memConflict.Resolve(Math.Max(theirOrder, yourOrder))
                    End If
                Next
            Next
        End Try
    End While
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    var result = dbResult.Products.First(p=>p.ProductID = 1)
    Console.WriteLine("Now product 1 has price={0}, UnitsOnOrder={1}", _
        result.UnitPrice, result.UnitsOnOrder)
    Console.WriteLine()
    ResetProducts() //clean up
}



[Test(Description="Submit with FailOnFirstConflict. Submit(FailOnFirstConflict) throws an Optimistic Concurrency Exception when the first conflict is detected. Only one exception is handled at a time, you have to submit for each conflict.")]

public void LinqToSqlOptimistic06()
            {
            Northwind db=CreateDb();


    db.Log = null
    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)

    //you load 3 products
    var prod() = db.Products.OrderBy(p=>p.ProductID).Take(3).ToArray()
    For i = 0 To 2
        Console.WriteLine("You retrieve the product {0}, it costs {1}", i + 1, prod(i).UnitPrice)
    Next
    //other user changes these products
    var otherUserProd() = otherUser_db.Products.OrderBy(p=>p.ProductID).Take(3).ToArray()
    For i = 0 To 2
        var otherPrice = (i + 1) * 111.11D
        Console.WriteLine("Other user changes the price of product {0} to {1}", i + 1, otherPrice)
        otherUserProd(i).UnitPrice = otherPrice
    Next
    otherUser_db.SubmitChanges()
    Console.WriteLine("Other user submitted changes")

    //you change your loaded products
    For i = 0 To 2
        var yourPrice = (i + 1) * 1.01D
        Console.WriteLine("You set the price of product {0} to {1}", i + 1, yourPrice)
        prod(i).UnitPrice = yourPrice
    Next

    // submit
    var needsSubmit = True
    While needsSubmit
        Try
            Console.WriteLine("======= You submit with FailOnFirstConflict =======")
            needsSubmit = False
            db.SubmitChanges(ConflictMode.FailOnFirstConflict)
        Catch c As ChangeConflictException
            For Each conflict In db.ChangeConflicts

                DescribeConflict(conflict) //write changes to console
                Console.WriteLine("Resolve conflict with KeepCurrentValues")
                conflict.Resolve(RefreshMode.KeepCurrentValues)
            Next
            needsSubmit = True
        End Try
    End While
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    For i = 0 To 2
        //Creating a temporary since this will be used in a lambda
        var tmp = i
        var result = dbResult.Products.First(p=>p.ProductID = tmp + 1)
        Console.WriteLine("Now the product {0} has price {1}", i + 1, result.UnitPrice)
    Next
    ResetProducts() //clean up
}
c




[Test(Description="Submit with ContinueOnConflict. Submit(ContinueOnConflict) collects all concurrency conflicts and throws an exception when the last conflict is detected.\r\nAll conflicts are handled in one catch statement. It is still possible that another user updated the same objects before this update, so it is possible that another Optimistic Concurrency Exception is thrown which would need to be handled again.")]

public void LinqToSqlOptimistic07()
            {
            Northwind db=CreateDb();

    db.Log = null
    var otherUser_db = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)

    // you load 3 products
    var prod() = db.Products.OrderBy(p=>p.ProductID).Take(3).ToArray()
    For i = 0 To 2
        Console.WriteLine("You retrieve the product {0}, it costs {1}", i + 1, prod(i).UnitPrice)
    Next
    // other user changes these products
    var otherUserProd() = otherUser_db.Products.OrderBy(p=>p.ProductID).Take(3).ToArray()
    For i = 0 To 2
        var otherPrice = (i + 1) * 111.11D
        Console.WriteLine("Other user changes the price of product {0} to {1}", i + 1, otherPrice)
        otherUserProd(i).UnitPrice = otherPrice
    Next
    otherUser_db.SubmitChanges()
    Console.WriteLine("Other user submitted changes")

    // you change your loaded products
    For i = 0 To 2
        var yourPrice = (i + 1) * 1.01D
        Console.WriteLine("You set the price of product {0} to {1}", i + 1, yourPrice)
        prod(i).UnitPrice = yourPrice
    Next
    // submit
    var needsSubmit = True
    While needsSubmit
        Try
            Console.WriteLine("======= You submit with ContinueOnConflict =======")
            needsSubmit = False
            db.SubmitChanges(ConflictMode.ContinueOnConflict)
        Catch c As ChangeConflictException
            For Each conflict In db.ChangeConflicts
                DescribeConflict(conflict) // write changes to console
                Console.WriteLine("Resolve conflict with KeepCurrentValues")
                conflict.Resolve(RefreshMode.KeepCurrentValues)
            Next
            needsSubmit = True
        End Try
    End While
    var dbResult = New NorthwindDataContext(My.Settings.NORTHWINDConnectionString1)
    For i = 0 To 2
        var tmp = i
        var result = dbResult.Products.First(p=>p.ProductID = tmp + 1)
        Console.WriteLine("Now the product {0} has price {1}", i + 1, result.UnitPrice)
    Next

    ResetProducts() //clean up
}


    }
}
