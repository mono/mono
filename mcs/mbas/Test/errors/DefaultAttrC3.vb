'Error Line:8
'Error BC30490
'Error: 'Default' cannot be combined with 'Private'.

Imports System

Class base
	Private Default ReadOnly Property Item(i as Integer)As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module