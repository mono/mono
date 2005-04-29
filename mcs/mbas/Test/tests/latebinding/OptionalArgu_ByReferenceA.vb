Imports System
Class c
	Sub F(ByVal telephoneNo as Long, Optional ByRef code as Integer = 080)
		if (code = 080)
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
		end if
   	End Sub 
End Class

Module OP1_0_1
   Sub Main()
	Dim o as Object = new c()
      Dim telephoneNo As Long = 9886066432
	Dim code As Integer = 081
      o.F(telephoneNo,code)
   End Sub 

End Module