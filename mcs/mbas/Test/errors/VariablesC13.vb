'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC31049
REM ErrorMessage: Initializers on structure members are valid only for constants.

Imports System

Module Test
    Structure AB
	    Public i as Integer = 10
    End Structure

    Public Sub Main()
    End Sub
End Module
