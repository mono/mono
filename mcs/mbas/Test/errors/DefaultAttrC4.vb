'Error Line:8
'Error BC30502
'Error: 'Shared' cannot be combined with 'Default' on a property declaration.

Imports System

Class base
	Shared Default ReadOnly Property Item(i as Integer)As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module