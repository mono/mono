
' Relational Operators

Imports System

Module M3

    Sub main()
        Console.WriteLine(f3())
    End Sub

    Function f3() As Integer

        Dim arr(16) As Boolean

        Dim a1, a2, a3, a4 As Boolean
        a1 = False : a2 = False : a3 = True : a4 = True

        If a1 = a2 Then arr(0) = True Else arr(0) = False
        If a3 = a4 Then arr(1) = True Else arr(1) = False
        If a1 <> a2 Then arr(2) = False Else arr(2) = True
        If a3 <> a4 Then arr(3) = False Else arr(3) = True
        If a1 = a3 Then arr(4) = False Else arr(4) = True
        If a1 <> a3 Then arr(5) = True Else arr(5) = False


        Dim b1 As String = "a"
        Dim b2 As String = "b"
        ' Only the equality (=) and inequality (<>) operators are 
        ' defined for Strings 

        If b1 = "a" Then arr(6) = True Else arr(6) = False
        If b1 <> "a" Then arr(7) = False Else arr(7) = True
        If b1 = b2 Then arr(8) = False Else arr(8) = True
        If b1 <> b2 Then arr(9) = True Else arr(9) = False

        Dim c1 As Date = New Date(2004, 7, 29)
        Dim c2 As Date = New Date(2004, 7, 28)

        If c1 = c2 Then arr(10) = False Else arr(10) = True
        If c1 <> c2 Then arr(11) = True Else arr(11) = False
        If c1 = "#7/29/2004#" Then arr(12) = True Else arr(12) = False
        If c1 <> "#7/29/2004#" Then arr(12) = False Else arr(12) = True

        Dim d1 As Char = "a"c
        Dim d2 As Char = "b"c

        If d1 = "a"c Then arr(13) = True Else arr(13) = False
        If d1 <> "a"c Then arr(14) = False Else arr(14) = True
        If d1 = d2 Then arr(15) = False Else arr(15) = True
        If d1 <> d2 Then arr(16) = True Else arr(16) = False

        For i As Integer = 0 To arr.GetUpperBound(0) 
            Console.WriteLine("{0}: {1}", i, arr(i))
        Next

        For Each bval As Boolean In arr
            If Not bval Then
                Return 1
            End If
        Next

        Return 0

    End Function

End Module