REM LineNo: 20
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Int16' is not defined.

REM LineNo: 21
REM ExpectedError: BC30451
REM ErrorMessage: Name 'A' is not declared.

REM LineNo: 22
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

REM LineNo: 23
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

Module IntegerLiteralTestC4
    Sub Main()
        Try
            Dim i As Int16
            i = A
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
