'==========================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.0.0: Argument Passing by value, which means the procedure cannot modify the variable
' 		itself.
'==========================================================================================
Imports System
Module APV1_0
	Function  F(ByVal p As Integer) As Integer
		p += 1
		return p
	End Function 
   
   Sub Main()
      Dim a As Integer = 1
      Dim b As Integer = 0 
	b = F(a)
	if b=a
		Throw new System.Exception("#A1, Unexcepted behaviour")
	end if
   End Sub 
End Module
'==========================================================================================