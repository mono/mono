Imports System

Module LocalDeclarationA

    Sub main()
        Dim i, sum As Integer
        For i = 0 To 4
            sum = sum + f2()
        Next
        If sum <> 15 Then
            Throw New Exception("#LD1: Static locals error")
        End If
    End Sub

    Sub f1()

        ' Various kinds of local decalations statements
        Const a1 As Integer = 2
        Static a2 As Double
        Dim a3 As Integer ' Default value is initialized to 0 
        Dim a4 As Boolean = True
        Dim arr1() As Integer = {0, 1, 2, 3}
        Dim arr2(,) As Integer = {{1, 2, 3}, {1 + 1, 1 + 2, 1 + 3}}
        Dim arr3(10) As Integer ' An array of 11 integers indexed from 0 through 10
        Dim arr4(2, 3, 4) As Date
        Dim arr5() As Object
        Dim b1, b2, b3, b4 As Integer
        Dim c1 As Double, c2 As Boolean, c3 As DateTime
        Dim d1 As New Date(2004, 8, 11)

    End Sub

    Function f2() As Integer
        Static a As Integer
        a = a + 1
        Return a
    End Function

End Module