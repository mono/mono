' Unhandled Exception: System.IndexOutOfRangeException: Index was outside
' the bounds of the array.
                                                                                
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class IndexOutOfRange
                                                                                
                <Test, ExpectedException (GetType (System.IndexOutOfRangeException))> _
                Public Sub TestindexOutOfRange ()
                        Dim arr As Integer(,) = {{1, 2, 3}, {3, 4, 7}}
                        arr(0, 2) = arr(0, 0) * arr(0, 1)
                        arr(1, 2) = arr(1, 0) * arr(1, 1)
                        If arr(0, 2) <> 2 Or arr(1, 2) <> 12 Then
                          Throw New Exception("#A1 - Array Handling Statement failed")
                        End If
                        ReDim Preserve arr(1, 1)
                        arr(1, 2) = 2
            End Sub
End Class







