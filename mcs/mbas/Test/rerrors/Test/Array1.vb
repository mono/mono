'Unhandled Exception: System.ArrayTypeMismatchException: 'ReDim' can
' only change the rightmost dimension.
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class Array1
                                                                                
                <Test, ExpectedException (GetType (ArrayTypeMismatchException))> _
                Public Sub TestMismatchException ()
                        Dim arr As Integer(,) = {{1, 2}, {3, 4}}
                        ReDim Preserve arr(3, 3)
                        arr(2, 2) = 12
                End Sub
End Class



