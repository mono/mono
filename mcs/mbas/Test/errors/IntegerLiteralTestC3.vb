REM LineNo: 10
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Integer'.

Imports System
Module IntegerLiteralTestC3
    Sub Main()
        Try
            Dim i As Integer
            i = System.Int64.MaxValue
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
