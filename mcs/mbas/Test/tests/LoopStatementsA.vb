Imports System

Module LoopStatementsA

    Sub main()
        Dim index As Integer = 0
        Dim count As Integer = 0

        Do
            count += 1
            index = 0
            While index <> 4
                index += 1
            End While
            If index <> 4 Then
                Throw New Exception("#LSA1 - Loop Statement failed")
            End If

            Do While index < 10
                index += 1
                If index = 8 Then
                    Exit Do
                End If
            Loop
            If index <> 8 Then
                Throw New Exception("#LSA2 - Loop Statement failed")
            End If

            Do
                index += 1
            Loop While index < 12
            If index <> 12 Then
                Throw New Exception("#LSA3 - Loop Statement failed")
            End If

            Do Until index <= 8
                index -= 1
            Loop
            If index <> 8 Then
                Throw New Exception("#LSA4 - Loop Statenment failed")
            End If

            Do
                index -= 1
                If index = 4 Then
                    Exit Do
                End If
            Loop Until index <= 3
            If index <> 4 Then
                Throw New Exception("#LSA5 - Loop Statenment failed")
            End If

            If count = 2 Then
                Exit Do
                Exit Do
            End If

        Loop

    End Sub

End Module