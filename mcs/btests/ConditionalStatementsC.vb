Imports System

Module ConditionalStatementsC

    Sub Main()

        Dim i As Integer
        Dim arr(10) As Integer

        For i = 0 To 10

            Select Case i
                Case 0 To 2
                    arr(i) = 1
                Case Is < 2, 3, 6 To 7
                    arr(i) = 2
                Case <= 8, >= 7
                    arr(i) = 3
                Case Else
                    arr(i) = 4
            End Select

        Next

        If arr(0) <> 1 Or arr(1) <> 1 Or arr(2) <> 1 Then
            Throw New Exception("#CSC1 - Switch Statement failed")
        ElseIf arr(3) <> 2 Or arr(6) <> 2 Or arr(7) <> 2 Then
            Throw New Exception("#CSC2 - Switch Statement failed")
        ElseIf arr(4) <> 3 Or arr(5) <> 3 Or arr(8) <> 3 Or arr(9) <> 3 Or arr(10) <> 3 Then
            Throw New Exception("#CSC3 - Switch Statement failed")
        Else
            Console.WriteLine("OK")
        End If

    End Sub

End Module