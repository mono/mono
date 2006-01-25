'To Check if WithEvents members can appear after they are used by 
'Handles clauses

Imports System

Class Raiser
    Public Event Fun(ByVal i as Integer)
    Public Sub New(i as Integer)
        RaiseEvent Fun(23)
    End Sub
End Class

Module Test
    Private Sub Fun(ByVal i as integer) Handles x.Fun
    End Sub

    Private WithEvents x As Raiser
    
    Public Sub Main()
        x = New Raiser(10)
    End Sub
End Module
