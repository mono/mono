'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 21
REM ExpectedError: BC30234
REM ErrorMessage:  'ReadOnly' is not valid on a WithEvents declaration

Imports System

Class Raiser
    Public Event Constructed()

    Public Sub New()
        RaiseEvent Constructed
    End Sub
End Class

Module Test
    Private Readonly WithEvents x As Raiser

    Private Sub HandleConstructed() Handles x.Constructed
    End Sub

    Public Sub Main()
        x = New Raiser()
    End Sub
End Module
