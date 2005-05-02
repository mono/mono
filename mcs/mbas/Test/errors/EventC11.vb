'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 24
REM ExpectedError: BC31029
REM ErrorMessage: Method 'Constructed' cannot handle Event 'Fun' because they do not have the same signature. 

Imports System

Class Raiser
    Public Event Constructed(ByVal i as Integer)
    Private Event Fun(ByVal i as String)
    Public Sub New(i as Integer)
        RaiseEvent Constructed(23)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Public Sub Constructed(ByVal i as integer) Handles x.Constructed, X.Fun
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
