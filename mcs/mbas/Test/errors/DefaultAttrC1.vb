'Checks if Default is working or not..
'Error Line: 9
'Error BC31048
'Error: Properties with no required parameters cannot be declared 'Default'.

Imports System

Class base
	Public Default ReadOnly Property Item()As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module