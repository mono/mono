Class AA
	Shared public i as Integer
End Class

Module M
	Sub Main()
		dim o as Object = new AA()
		o.i = o.i+1
		fun()
	End Sub
	Sub fun()
		AA.i = AA.i+1
		if AA.i<>2
			Throw new System.Exception("Shared variable not workin") 
		end if
	End Sub
End Module
