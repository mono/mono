REM LineNo: 17
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

REM LineNo: 18
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

REM LineNo: 19
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

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
