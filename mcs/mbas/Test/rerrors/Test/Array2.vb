'Expected: System.FormatException  '
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class Array2
                                                                                
                <Test, ExpectedException (GetType (System.FormatException))> _
                Public Sub TestArrayFormat ()
                        Dim arr As Integer(,) = {{1, "Hello"}, {3, 4}}
                        If arr(0, 0) <> 1 Then
                        Throw New Exception("#A1")
                End if
                End Sub
End Class


