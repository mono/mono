Module APV1_1_0
	Class C
		Function F(p As String) As String
			p = "Sinha"
			return p
		End Function
	End Class
	
	Sub Main()
		Dim obj As Object = new C()
		Dim a As String = "Manish"
		Dim b as String = ""
		b = obj.F(a)
		if a=b
			Throw New System.Exception("#A1, Unexcepted behaviour in string of APV1_1_0")
		end if
	End Sub 
End Module
