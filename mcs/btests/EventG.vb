Imports System

Class C
	Delegate Sub EH()
	Public Event E as EH

	Public Sub S()
		RaiseEvent E
	End Sub
End Class

Class C1
	dim x as C = new C()

	sub setHandler()
		AddHandler x.E, AddressOf xh
	end sub

	sub unsetHandler()
		RemoveHandler x.E, AddressOf xh
	end sub

	Sub call_S()
		x.S()
	End Sub

	Sub xh() 
		Console.WriteLine("event called")
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.setHandler ()
		y.call_S()
		y.unsetHandler()
		y.call_S()
	End Sub

End Module
