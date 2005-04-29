Imports System
Class C
	Sub F(ByVal telephoneNo as Long, Optional ByRef code as Integer = 080)
		if (code <> 080)
			Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_0")
		end if
   	End Sub 
End Class

Module OP1_0_0
   Sub Main()
	Dim o as object = new C()
      Dim telephoneNo As Long = 9886066432
      o.F(telephoneNo)
   End Sub 
End Module