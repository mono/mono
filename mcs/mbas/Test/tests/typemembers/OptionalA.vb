Imports System

Module Test
    Enum E
        A
        B
    End Enum
    Function F(Optional i As Integer = 42) As Integer
        F = i + 1
    End Function
    Function F2(ByVal Optional i As Integer = 42) As Integer
        F2 = i + 1
    End Function
    Function G(i As Integer, Optional j As Integer = 42) As Integer
        G = i + j
    End Function
    Function G(e As E) As Integer
        G = e
    End Function
    Function H(i As Integer, Optional j As Integer = 42, Optional k As Integer = 3) As Integer
        H = i + j + k
    End Function
    Function K(ByRef Optional i As Integer = 3) As Integer
        K = i
        i = i + 3
    End Function
    Sub Main
        If F() <> 43 Then
            Throw New Exception("#A1: unexpected return value")
        End If
        If F(99) <> 100 Then
            Throw New Exception("#A2: unexpected return value")
        End If
        If F2() <> 43 Then
            Throw New Exception("#A3: unexpected return value")
        End If
        If G(1) <> 43 Then
            Throw New Exception("#A4: unexpected return value")
        End If
        If G(E.A) <> 0 Then
            Throw New Exception("#A5: unexpected return value")
        End If
        If G(1,99) <> 100 Then
            Throw New Exception("#A6: unexpected return value")
        End If
        If G(E.A,99) <> 99 Then
            Throw New Exception("#A7: unexpected return value")
        End If
        If H(1) <> 46 Then
            Throw New Exception("#A8: unexpected return value")
        End If
        If H(1,0) <> 4 Then
            Throw New Exception("#A9: unexpected return value")
        End If
        If H(E.A) <> 45 Then
            Throw New Exception("#A10: unexpected return value")
        End If
        If K() <> 3 Then
            Throw New Exception("#A11: unexpected return value")
        End If
        Dim i As Integer = 9
        If K(i) <> 9 OrElse i <> 12 Then
            Throw New Exception("#A12: unexpected return value")
        End If
    End Sub
End Module
