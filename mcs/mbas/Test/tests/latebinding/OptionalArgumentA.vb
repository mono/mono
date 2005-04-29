Imports System
Class C
	Sub F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, Optional ByRef name As String="Sinha")
		if (code <> 080 and code1 <> 091 and name="Sinha")
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
		end if
   	End Sub 
End Class
Module OP1_0_0
   Sub Main()
	dim o as Object = new C()
      Dim telephoneNo As Long = 9886066432
	Dim name As String ="Manish"
      o.F(telephoneNo,,,name)
   End Sub 

End Module