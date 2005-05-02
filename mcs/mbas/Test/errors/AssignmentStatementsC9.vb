'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError:  BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'Byte'.

Option Strict On
Module Test
    Private b As Byte = 0
    Private i As Integer = 0
    Sub Main()
        b += i ' Error, b = I is not permitted.
    End Sub
End Module
