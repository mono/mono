Imports System

Module M
Enum E
    A
    B
End Enum
Sub InInt(ByVal i As Integer)
End Sub
Sub InLong(ByVal i As Long)
End Sub
Sub InEnum(ByVal e As E)
End Sub
Sub Main
    Dim e1 As E

    e1 = E.A
    If e1.GetType().ToString() <> GetType(E).ToString() Then
        Throw New Exception("#A1: wrong type")
    End If
    If E.A.GetType().ToString() <> GetType(E).ToString() Then
        Throw New Exception("#A2: wrong type")
    End If
    Dim e2 As E
    e2 = e1
    Dim i As Integer
    i = e2
    InInt(e2)
    InInt(E.B)
    InLong(e2)
    InLong(E.B)
    InEnum(e2)
    InEnum(0)
End Sub
End Module
