'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To Check if Methods Accebility has anything to do with Events

Imports System

Class Raiser
    Public Event Fun(ByVal i as Integer)
    Public Sub New(i as Integer)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Private Sub Fun(ByVal i as integer) Handles x.Fun
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
