Imports System
Module APV1_1_0
	Class C
		Sub F(p As String)
			p = "Sinha"
		End Sub 
	End Class

	Sub Main()
		Dim obj As Object = new C ()
		Dim a As String = "Manish"
		obj.F(a)
		if a<>"Manish"
			Throw New System.Exception("#A1, Unexcepted behaviour in string of APV1_1_0")
		end if
	End Sub 
End Module
