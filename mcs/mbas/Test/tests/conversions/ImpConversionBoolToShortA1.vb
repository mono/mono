Module ImpConversionofBooltoShort
	Sub Main()
		Dim b as Boolean = False
		Dim a as Short = b 
		if a <> 0 then 
			Throw New System.Exception("Implicit Conversion of Bool(False) to Short has Failed. Expected 0, but got " & a)
		End if		
	End Sub
End Module
