Module ExpConversionDoubletoBool
	Sub Main()
		Dim a As Double = -4.94065645841247e-324 
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Double to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
