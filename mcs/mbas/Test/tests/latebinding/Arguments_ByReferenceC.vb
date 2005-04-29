Imports System
Module APR_1_1_0
	Class C1
		Sub F(ByRef p As String)
			p = "Sinha"
   		End Sub 
	End Class
   
   	Sub Main()
		Dim obj As Object = new C1()
      		Dim a As String = "Manish"
      		obj.F(a)
		if (a="Manish")
			Throw New System.Exception("#A1, Unexcepted behaviour of ByRef of String Datatype")
		end if
   	End Sub 
End Module
