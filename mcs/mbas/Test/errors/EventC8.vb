'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 23
REM ExpectedError: BC30219
REM ErrorMessage: Method 'Constructed' cannot handle Event 'Constructed' because they do not have the same signature.

Imports System

Class Raiser
    Public Event Constructed(ByVal i as Integer)

    Public Sub New(i as Integer)
        RaiseEvent Constructed(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Public Sub Constructed(ByRef i as integer) Handles x.Constructed
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
