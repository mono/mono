Module ImpConversionofBooltoSingle
	Sub Main()
		Dim b as Boolean = False
		Dim a as Single = b  
		if a <> 0 then 
			Throw New System.Exception("Implicit Conversion of Bool(False) to Single has Failed. Expected 0, but got " & a)
		End if		
	End Sub
End Module
