Imports System
Class C
	Function F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, Optional ByRef name As String="Sinha") As Boolean
		if (code = 080 and code1 = 091 and name="Manish")
			return true
		else
			return false
		end if		
   	End Function 
End Class

Module OP1_0_0
   Sub Main()
	Dim o as Object = new C()
      Dim telephoneNo As Long = 9886066432
	Dim name As String ="Manish"
	Dim status As Boolean
      status =o.F(telephoneNo,,,name) 
	if (status = false)
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
	end if
   End Sub 

End Module