REM LineNo: 25
REM ExpectedError: BC30192
REM ErrorMessage: ParamArray parameter must be last in parameter list.

REM LineNo: 29
REM ExpectedError: BC30451
REM ErrorMessage: Name 'args1' is not declared.

REM LineNo: 44
REM ExpectedError: BC30311
REM ErrorMessage: Value of type '1-dimensional array of Integer' cannot be converted to 'Integer'.

REM LineNo: 44
REM ExpectedError: BC30311
REM ErrorMessage: Value of type '1-dimensional array of Integer' cannot be converted to 'Integer'.

'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: ParamArray:
'APR-1.0.0: ParamArray can be used only on the last argument of argument list. it allows us to 'pass an arbitrary list. It allows us to pass an arbitrary number of argument to the procedure 
'=============================================================================================
Imports System
Module PA_1_0_0
   Function F(ParamArray args() As Integer,ParamArray args1() As Integer, ByVal a as Integer, ByVal b as Integer)as Boolean
      Dim a as integer
	Dim b as integer
	a = args.Length
	b = args1.Length
	if a<>b
	return true
	else 
	return false
	end if
   End Function
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
	Dim b As Integer() = { 1, 2, 3,4 }
	Dim c as Integer = 0
	Dim d as Integer = 0
	c = a.Length
	d = b.Length
      Dim e as Boolean
	e= F(a,b,c,d)
	if e<>true
		Throw New System.Exception("#A1, Unexcepted Behaviour in F(a)")
	end if
   End Sub
End Module
'=============================================================================================