'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 26
REM ExpectedError:  BC31029
REM ErrorMessage: Method 'Constructed' cannot handle Event 'Constructed' because they do not have the same signature.

Imports System

Class Raiser
    Public Event Constructed(ByVal Count As Integer)

    Public Sub New()
        Static CreationCount As Integer = 0

        CreationCount += 1
        RaiseEvent Constructed(CreationCount)
    End Sub
End Class

Module Test
    Private WithEvents x As Raiser

    Private Sub Constructed(ByVal Count As Long) Handles x.Constructed
        Console.WriteLine("Constructed instance #" & Count)
    End Sub

    Public Sub Main()
        x = New Raiser   ' Causes "Constructed instance #1" to be printed.    
    End Sub
End Module
