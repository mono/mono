REM LineNo: 29
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

REM LineNo: 34
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

REM LineNo: 39
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'String'.

REM LineNo: 39
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'String'.

' BC30201: Expression expected 
' BC30512: Option Strict On disallows implicit conversions from 'Integer' to 'String'

Option Strict On

Imports System

Module LikeOperatorC1
    Sub main()

        Dim a As Boolean

        a = "HELLO" Like 
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Failed")
        End If

        a =  Like "H*O"
        If a <> True Then
            Console.WriteLine("#A2-LikeOperator:Failed")
        End If

        a = 123 Like 123

    End Sub

End Module
