REM LineNo: 13
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'String'.

Option Strict On

Imports System

Module LikeOperatorC1
    Sub main()

        Dim a As Boolean
        a = 123 Like 123

    End Sub

End Module
