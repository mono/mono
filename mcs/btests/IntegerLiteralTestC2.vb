REM LineNo: 17
REM ExpectedError: BC30035
REM ErrorMessage: Syntax error.

REM LineNo: 18
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

Module IntegerLiteralTestC2
    Sub Main()
        Try
            Dim i As Integer
            i = &O9
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
