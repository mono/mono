'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 22
REM ExpectedError: BC30435
REM ErrorMessage:  Members in a Structure cannot be declared 'WithEvents'

Imports System

Class Raiser
    Public Event Constructed()

    Public Sub New()
        RaiseEvent Constructed
    End Sub
End Class

Module Test
    Structure AB
	    WithEvents x As Raiser
	    Public i as Integer
    End Structure

    Public Sub Main()
    End Sub
End Module
