'Error Line:13
'Error BC30361
'Error: 'Public ReadOnly Property Item(i As Integer, j As Integer) As Integer' and 'Public ReadOnly Default Property Item(i As Integer) As Integer' cannot overload each other because only one is declared 'Default'.

Imports System

Class base
	Public Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
	Public ReadOnly Property Item(ByVal i as Integer, ByVal j as Integer)As Integer
		Get			
			Return 2*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
	End Sub
End Module