'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ImpConversionStringtoBooleanA
	Sub Main()
			Dim a as Boolean
			Dim b as String= "True"
			a = b
			if a <> True
				Throw new System.Exception("Conversion of String to Boolean not working. Expected True but got " &a) 
			End if		
	End Sub
End Module
