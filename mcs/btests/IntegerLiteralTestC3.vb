REM LineNo: 17
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Integer'.

REM LineNo: 18
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

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
