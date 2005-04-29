Imports System
Module APV1_0
	Class C
		Sub F(ByVal p As Integer)
			p += 1
		End Sub 
	End Class
  	 
	Sub Main()
		Dim obj As Object = new C ()
		Dim a As Integer = 1
		obj.F(a)
		if a<>1
			Throw new System.Exception("#A1, Unexcepted behaviour")
		end if
	End Sub 
End Module
