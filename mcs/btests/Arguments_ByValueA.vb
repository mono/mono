'==========================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.0.0: Argument Passing by value, which means the procedure cannot modify the variable
' 		itself.
'==========================================================================================
Imports System
Module APV1_0
	Sub F(ByVal p As Integer)
      p += 1
   End Sub 
   
   Sub Main()
      Dim a As Integer = 1
      F(a)
	if a<>1
		Throw new System.Exception("#A1, Unexcepted behaviour")
	end if
   End Sub 
End Module
'============================================================================================