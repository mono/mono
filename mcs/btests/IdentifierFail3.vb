REM LineNo: 6
REM ExpectedError: BC30203
REM ErrorMessage: Identifier expected.

Module IdentifierFail3
    sub 2sd() ' not a valid identifier
    End Sub

    Sub Main()
    End Sub
End Module
