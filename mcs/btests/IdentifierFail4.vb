REM LineNo: 7
REM ExpectedError: BC30183
REM ErrorMessage: Keyword is not valid as an identifier.

Module IdentifierFail4
    '' invalid identifier
    sub sub()
    End Sub

    Sub Main()
    End Sub
End Module
