REM LineNo: 14
REM ExpectedError: BC30390 ????
REM ErrorMessage: 'C1.b' is not accessible in this context because it is 'Protected'. ????

Option Explicit On

Class TestingHelper
	Shared public i as Integer
End Class

Module ExplicitOn
	Sub Main()
		' MUST generate error as Explicit is on
		anything = new TestingHelper()
		anything.i = anything.i+1
		TestingHelper.i = TestingHelper.i+1
		if TestingHelper.i<>2
			Throw new System.Exception("Shared variable not working") 
		end if
	End Sub
End Module
