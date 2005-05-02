'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 16
REM ExpectedError: BC30431
REM ErrorMessage: 'End Property' must be preceded by a matching 'Property'.

option strict
Imports System

Module Test
Interface A
    Property Prop() as Integer
    End Property	
End Interface
    Public Sub Main()		
    End Sub
End Module

