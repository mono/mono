'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: ParamArray:
'APR-1.0.0: ParamArray can be used only on the last argument of argument list. it allows us to 'pass an arbitrary list. It allows us to pass an arbitrary number of argument to the procedure 
'=============================================================================================
Imports System
Module PA_1_0_0
   Function F(ParamArray args() As Integer)as Integer
      Dim a as integer
	a = args.Length
	return a
   End Function
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
      Dim b as Integer
	b= F(a)
	if b<>3
		Throw New System.Exception("#A1, Unexcepted Behaviour in F(a)")
	end if

      b = F(10, 20, 30, 40)
	if b<>4
		Throw New System.Exception("#A2, Unexcepted Behaviour in F(10,20,30,40)")
	end if
      b = F()
	if b<>0
		Throw New System.Exception("#A3, Unexcepted Behaviour in F()")
	end if
   End Sub
End Module
'=============================================================================================