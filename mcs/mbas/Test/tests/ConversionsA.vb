
Imports System
Imports Microsoft.VisualBasic

Module ConversionsA

    Sub f1(ByRef a As Object)
    End Sub

    Sub f2(ByVal array() As Object, ByVal index As Integer, ByVal count As Integer, ByVal value As Object)
        Dim i As Integer
        For i = index To (index + count) - 1
            array(i) = value
        Next i
    End Sub


    Sub Main()
        On Error GoTo ErrorHandler

        Dim a(10) As Object
        Dim b() As Object = New String(10) {}
        f1(a(0))
        f1(b(1)) ' ArrayTypeMismatchException

        Dim str(100) As String
        f2(str, 0, 101, "Undefined")
        f2(str, 0, 10, Nothing)
        f2(str, 91, 10, 0) ' ArrayTypeMismatchException
        Exit Sub

ErrorHandler:
        If Err.Number <> 5 Then   ' System.ArrayTypeMismatchException
            Throw New Exception("#CA1 - Conversion Statement failed")
        End If
        Resume Next

    End Sub

End Module