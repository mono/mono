REM LineNo: 14
REM ExpectedError: BC30070
REM ErrorMessage: Next control variable does not match For loop control variable 'j'.

Imports System

Module ForC7

    Sub main()

        For i As Integer = 0 To 10 Step 2
            For j As Integer = 0 To 10 Step 4
                Console.WriteLine("Hello World")
            Next i
        Next

    End Sub

End Module