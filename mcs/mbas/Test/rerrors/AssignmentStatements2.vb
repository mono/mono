' System.InvalidCastException: Cast from string "Hello World" to type 'Integer' is not valid.
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class FormatExcep
Inherits SystemException
                <Test, ExpectedException (GetType (System.FormatException))> _
                Public Sub TestForFormat ()
                        Dim a As Integer
                        a = "Hello"  + "World"
                        Console.WriteLine(a)
                                                                                
            End Sub
End Class

