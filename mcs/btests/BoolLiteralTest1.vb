REM LineNo: 14
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

REM LineNo: 15
REM ExpectedError: BC30451
REM ErrorMessage: Name 'Console' is not declared.

Module BoolLiteralTest1
    Sub Main()
        Try
            Dim b As Boolean
            b = Not True
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
