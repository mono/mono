REM LineNo: 11
REM ExpectedError: BC30269
REM ErrorMessage: Method 'S' has multiple definitions with identical signatures.

Imports System

Class C
	Delegate Sub EH()
	Public Event E as EH

	Public Sub S
		RaiseEvent E
	End Sub

	Sub xh 
		Console.WriteLine("event called")
	End Sub

	shared sub s
	end sub
End Class

Class C1
	Inherits C

	Delegate Sub EH1()
	Public Event E1 as EH1

	Sub xh1()
		Console.WriteLine("event called 1")
	End Sub

	Sub call_S()
		AddHandler MyBase.E, AddressOf MyBase.xh
		MyBase.S()
		RemoveHandler MyBase.E, AddressOf MyBase.xh
			
	
		AddHandler Me.E1, AddressOf Me.xh1
		RaiseEvent E1
		RemoveHandler Me.E1, AddressOf Me.xh1
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.call_S()
	End Sub

End Module
