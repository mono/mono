'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class Raiser 
	Public Event Constructed() 
		Public Sub New() 
		RaiseEvent Constructed 
	End Sub 
End Class 


Module Test
    Private _x As Raiser

    Public Property x() As Raiser
        Get
            Return _x
        End Get

        Set (ByVal Value As Raiser)
            ' Unhook any existing handlers.
            If Not _x Is Nothing Then
                RemoveHandler _x.Constructed, AddressOf HandleConstructed
            End If

            ' Change value.
            _x = Value

            ' Hook-up new handlers.
            If Not _x Is Nothing Then
                AddHandler _x.Constructed, AddressOf HandleConstructed
            End If
        End Set
    End Property

    Sub HandleConstructed()
        Console.WriteLine("Constructed")
    End Sub

    Sub Main()
        x = New Raiser()
    End Sub
End Module
