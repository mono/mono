REM LineNo: 6
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected.

Module IdentifierFail2
    Sub _() ' not a valid identifier
    End Sub

    Sub Main()
    End Sub

End Module
