Imports System
	
Module M
	Enum E
		A = B
		B = C 
		C = 100
	End Enum

	Sub Main
		Dim e1 As E

		If E.A <> 100
			Throw New Exception ("#D1: Unexpected value for E.A")
	     	End If

		If E.B <> 100
			Throw New Exception ("#D2: Unexpected value for E.B")
	     	End If

		If E.C <> 100
			Throw New Exception ("#D3: Unexpected value for E.C")
	     	End If
	End Sub
End Module
