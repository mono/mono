Imports System

Class cls1
End Class

Module VariableB
    Dim a As Integer, b As Boolean
    Dim c, d, e As Long
    Dim f As String
    Dim g(5) As Integer
    Dim h(5, 7) As Long
    Dim i As New Integer()
    Dim j(5) As Integer
    Dim k(5) As Integer
    Dim l(5), m(6), n(7) As Integer
    Dim o
    Dim p As New cls1()
    Dim q%, r&, s@, t!, u#
    Sub main()
	If a.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A1, Type mismatch found")
        End If
        If b.GetTypeCode() <> TypeCode.Boolean Then
            Throw New System.Exception("#A2, Type mismatch found")
        End If
        If c.GetTypeCode() <> TypeCode.Int64 Then
            Throw New System.Exception("#A3, Type mismatch found")
        End If
        If d.GetTypeCode() <> TypeCode.Int64 Then
            Throw New System.Exception("#A4, Type mismatch found")
        End If
        If e.GetTypeCode() <> TypeCode.Int64 Then
            Throw New System.Exception("#A5, Type mismatch found")
        End If
        f = "abc"
        If f.GetTypeCode() <> TypeCode.String Then
            Throw New System.Exception("#A6, Type mismatch found")
        End If
        If i.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A7, Type mismatch found")
        End If
	If q.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A8, Type mismatch found")
        End If
	If s.GetTypeCode() <> TypeCode.Decimal Then
            Throw New System.Exception("#A9, Type mismatch found")
        End If
	If u.GetTypeCode() <> TypeCode.Double Then
            Throw New System.Exception("#A10, Type mismatch found")
        End If
    End Sub
End Module
