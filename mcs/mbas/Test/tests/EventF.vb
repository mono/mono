Imports System

Class C
	Delegate Sub EH()
	Public Event E as EH

	Public Sub S()
		RaiseEvent E
	End Sub

	Sub bxh() 
		Console.WriteLine("event called from other class")
	End Sub
End Class

Class C1
	Sub call_S()
		dim x as C = new C()
		AddHandler x.E, AddressOf Me.xh
		AddHandler x.E, AddressOf x.bxh
		x.S()
	End Sub

	Sub xh() 
		Console.WriteLine("event called")
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.call_S()
	End Sub

End Module
