' Logical Operators

Imports System

Module M

    Sub Main()
        Console.WriteLine(f())
    End Sub

    Function f1() As Boolean
        Console.WriteLine("Function f1() is called")
        Return True
    End Function

    Function f2() As Boolean
        Console.WriteLine("Function f2() is called")
        Return False
    End Function

    Function f() As Integer
        Dim arr(35) As Boolean

        Dim a1, a2, a3, a4 As Boolean
        a1 = True : a2 = True : a3 = False : a4 = False

        If a1 And a2 Then arr(0) = True
        If a1 And a3 Then arr(1) = False Else arr(1) = True
        If a3 And a1 Then arr(2) = False Else arr(2) = True
        If a4 And a3 Then arr(3) = False Else arr(3) = True
        If f1() And (a1 = True) Then arr(4) = True
        If f2() And f1() Then arr(5) = False Else arr(5) = True

        If a1 Or a2 Then arr(6) = True
        If a1 Or a3 Then arr(7) = True
        If a3 Or a1 Then arr(8) = True
        If a4 Or a3 Then arr(9) = False Else arr(9) = True
        If f1() Or (a1 = True) Then arr(10) = True
        If f2() Or f1() Then arr(11) = True

        If a1 Xor a2 Then arr(12) = False Else arr(12) = True
        If a1 Xor a3 Then arr(13) = True
        If a3 Xor a1 Then arr(14) = True
        If a4 Xor a3 Then arr(15) = False Else arr(15) = True
        If f1() Xor (a1 = True) Then arr(16) = False Else arr(16) = True
        If f2() Xor f1() Then arr(17) = True

        If f1() AndAlso f2() Then arr(18) = False Else arr(18) = True
        If f2() AndAlso f1() Then arr(19) = False Else arr(19) = True
        If f1() AndAlso (a1 = True) Then arr(20) = True

        If f1() OrElse f2() Then arr(21) = True
        If f2() OrElse f1() Then arr(22) = True
        If (a1 = False) OrElse f2() Then arr(23) = False Else arr(23) = True

        Dim b1 As Long = 2
        Dim b2 As Byte = 5
        
        If (b1 And System.Int64.MaxValue) = b1 Then arr(24) = True
        If (b1 And 0) = 0 Then arr(25) = True
        If (b1 Or System.Int64.MaxValue) = System.Int64.MaxValue Then arr(26) = True
        If (b1 Or 0) = b1 Then arr(27) = True
        If (b1 Xor System.Int64.MaxValue) = (System.Int64.MaxValue - b1) Then arr(28) = True
        If (b1 Xor 0) = b1 Then arr(29) = True

        If (b2 And System.Byte.MaxValue) = b2 Then arr(30) = True
        If (b2 And 0) = 0 Then arr(31) = True
        If (b2 Or System.Byte.MaxValue) = System.Byte.MaxValue Then arr(32) = True
        If (b2 Or 0) = b2 Then arr(33) = True
        If (b2 Xor System.Byte.MaxValue) = (System.Byte.MaxValue - b2) Then arr(34) = True
        If (b2 Xor 0) = b2 Then arr(35) = True

        'Dim i As Integer
        'For i = 0 To arr.GetUpperBound(0)
        'Console.WriteLine("{0}: {1}", i, arr(i))
        'Next

        Dim bval As Boolean
        For Each bval In arr
            If Not bval Then
                Return 1
            End If
        Next

        Return 0

    End Function

End Module
