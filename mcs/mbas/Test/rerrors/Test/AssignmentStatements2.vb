' System.InvalidCastException: Cast from string "Hello World" to type 'Integer' is not valid.
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class AssignmentStatements2
Inherits SystemException
                <Test, ExpectedException (GetType (System.FormatException))> _
                Public Sub TestForFormat ()
                        Dim a As Integer
                        a = "Hello"  + "World"
                                                                                
            End Sub
End Class

