Module APR_1_1_0
	Class C
		Function F(ByRef p As String) as String
			p = "Sinha"
			return p
   		End Function 
	End Class 

   	Sub Main()
		Dim obj As Object = new C()
      		Dim a As String = "Manish"
      		Dim b As String = ""
      		b=obj.F(a)
		if (b<>a)
			Throw New System.Exception("#A1, Unexcepted behaviour of ByRef of String Datatype")
		end if
   	End Sub 
End Module
