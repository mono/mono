REM LineNo: 10
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Imports System
Module IntegerLiteralTestC1
    Sub Main()
        Try
            Dim i As Integer
            i = &H2G
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
