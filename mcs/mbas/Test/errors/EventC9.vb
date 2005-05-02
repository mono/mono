'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 21
REM ExpectedError: BC30029
REM ErrorMessage: Derived classes cannot raise base class events.

Imports System

Class A
    Public Event Fun(ByVal i as Integer)
End Class

Class Raiser
    Inherits A
    Public Event Constructed(ByVal i as Integer)
    Public Sub New(i as Integer)
        RaiseEvent Constructed(23)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Public Sub Constructed(ByVal i as integer) Handles x.Constructed
    End Sub

    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
