'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 22
REM ExpectedError: BC30287
REM ErrorMessage:'.' expected.

Imports System

Class Raiser
    Public Event Fun(ByVal i as String)
    Public Sub New(i as Integer)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Public Sub Constructed(ByVal i as integer) Handles X_Fun1
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
