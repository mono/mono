REM LineNo: 6
REM ExpectedError: BC30671
REM ErrorMessage: Explicit initialization is not permitted with multiple variables declared with a single type specifier.

Module VariableC2
    Dim c, d, e As Long = 10
    Sub Main()
    End Sub
End Module
