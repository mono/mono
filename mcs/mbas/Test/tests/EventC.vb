Imports System

Class C
	Public Event E (ByVal i as integer)

	Public Sub S()
		RaiseEvent E(10)
	End Sub
End Class

Class C1
	dim WithEvents x as C = new C()	

	Sub call_S()
		x.S()
	End Sub

	Sub EH(i as Integer) Handles x.E
		Console.WriteLine("event called : " + i)
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.call_S()
	End Sub
End Module


