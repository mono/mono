Class C1
End Class

Class C2
	Inherits C1
	Public Sub S1()
		S2()
		C = 10
	End Sub
End Class

Class C3
	Public Sub S()
		Dim myC As New C1()
		myC.S2()
		myC.C = 20
	End Sub
End Class

Module Accessibility
	Sub Main()
	End Sub
End Module
