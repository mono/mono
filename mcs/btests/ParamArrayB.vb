'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: ParamArray:
'APR-1.0.0: ParamArray can be used only on the last argument of argument list. it allows us to 'pass an arbitrary list. It allows us to pass an arbitrary number of argument to the procedure 
'=============================================================================================
Imports System
Module PA_1_0_0
   Sub F(ParamArray args() As Integer)
	Dim a as Integer
	a=args.Length
	if a=0
		Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
	end if
   End Sub
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
      F(a)
      F(10, 20, 30, 40)
   End Sub
End Module
'=============================================================================================