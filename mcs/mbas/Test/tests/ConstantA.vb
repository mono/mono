Imports System

Module ConstantA
    Public Const a As Integer = 10
    Const b As Boolean = True, c As Long = 20
    Const d = 20
    Const e% = 10
    Const f% = 10, g# = 20
    Sub Main()
        If a.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A1, Type mismatch found")
        End If
        If b.GetTypeCode() <> TypeCode.Boolean Then
            Throw New System.Exception("#A2, Type mismatch found")
        End If
        If c.GetTypeCode() <> TypeCode.Int64 Then
            Throw New System.Exception("#A3, Type mismatch found")
        End If
        If d.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A4, Type mismatch found")
        End If
        If e.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A5, Type mismatch found")
        End If
        If f.GetTypeCode() <> TypeCode.Int32 Then
            Throw New System.Exception("#A6, Type mismatch found")
        End If
        If g.GetTypeCode() <> TypeCode.Double Then
            Throw New System.Exception("#A7, Type mismatch found")
        End If
    End Sub
End Module
