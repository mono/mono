'Error Line:13
'Error BC30359
'Error: 'Default' can be applied to only one property name in a class.

Imports System

Class base
	Public Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
	Public Default ReadOnly Property Item1(ByVal i as Integer)As Integer
		Get			
			Return 2*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
	End Sub
End Module