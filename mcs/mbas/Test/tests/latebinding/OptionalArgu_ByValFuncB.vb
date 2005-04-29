Imports System
Class C
	Function F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080) As Boolean
		if (code = 080)
		return false
		else 
			return true
		end if
   	End Function
End CLass

Module OP1_0_1
   Sub Main()
      Dim o as Object = new C()
	Dim telephoneNo As Long = 9886066432
	Dim code As Integer = 081
	Dim status as Boolean
      status = o.F(telephoneNo,code)
	if(status = false)
		Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
	end if
   End Sub 

End Module