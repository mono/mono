'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError:  BC30452
REM ErrorMessage: Operator '+' is not defined for types 'Char' and 'Integer'.

Option Strict On
Module Test
    Private ch As Char = Nothing
    Private i As Integer = 0
    Sub Main()
        ch += 1 ' Error, ch = 1 is not permitted.
    End Sub
End Module
