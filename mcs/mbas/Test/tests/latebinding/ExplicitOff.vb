Option Explicit Off

Class TestingHelper
	Shared public i as Integer
End Class

Module ExplicitOff
	private funCount as Integer = 0
	
	Sub Main()
		DoItExplicitly()
		DoItExplicitlyLateBound()
		DoItImplicitlyTypingLateBound()
		DoItImplicitlyLateBound()
	End Sub
	
	Sub DoItExplicitly()
		dim o as TestingHelper = new TestingHelper()
		o.i = o.i+1
		fun()
	End Sub
	
	Sub DoItExplicitlyLateBound()
		dim o as object = new TestingHelper()
		o.i = o.i+1
		fun()
	End Sub
	
	Sub DoItImplicitlyTypingLateBound()
		dim o = new TestingHelper()
		o.i = o.i+1
		fun()
	End Sub
	
	Sub DoItImplicitlyLateBound()
		anything = new TestingHelper()
		anything.i = anything.i+1
		fun()
	End Sub
	
	Sub fun()
		funCount = funCount + 1
		System.Console.WriteLine("FunCounting: {0} - TestingHelper.i {1}", funCount, TestingHelper.i)
		TestingHelper.i = TestingHelper.i+1
		if TestingHelper.i<>(2*funCount)
			Throw new System.Exception("Shared variable not working") 
		end if
	End Sub
End Module
