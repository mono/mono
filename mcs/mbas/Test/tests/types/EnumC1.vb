Imports System

Module M
	Public Enum E1 As Long
		A = 2.3
		B = 2.5
	End Enum

   Sub Main()
	if E1.A<> 2
		throw new System.Exception("#A1 Enum not working")
	end if
    End Sub

	

End Module
