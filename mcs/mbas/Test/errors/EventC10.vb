'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 27
REM ExpectedError: BC30585
REM ErrorMessage: Event 'Fun' cannot be handled because it is not accessible from 'Module Test'.

Imports System

Class Raiser
    Public Event Constructed(ByVal i as Integer)
    Private Event Fun(ByVal i as Integer)
    Public Sub New(i as Integer)
        RaiseEvent Constructed(23)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Public Sub Constructed(ByVal i as integer) Handles x.Constructed
    End Sub

    Public Sub Fun(ByVal i as integer) Handles x.Fun
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
