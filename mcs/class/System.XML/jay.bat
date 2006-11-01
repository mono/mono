echo on
@echo "processing Parser.jay..."
"%1\..\..\jay\jay.exe" -ct < "%1..\..\jay\skeleton.cs" "%1System.Xml.XPath\Parser.jay" > "%1System.Xml.XPath\Parser.cs"

@echo "generating PatternParser.jay..."
sed "s/\%%start Expr/\%%start Pattern/" "%1\System.Xml.XPath\Parser.jay" >"%1\Mono.Xml.Xsl\PatternParser.jay"

@echo "processing PatternParser.jay..."
@echo #define XSLT_PATTERN > "%1\Mono.Xml.Xsl\PatternParser.cs"
"%1\..\..\jay\jay.exe" -ct < "%1..\..\jay\skeleton.cs" "%1Mono.Xml.Xsl\PatternParser.jay" >> "%1Mono.Xml.Xsl\PatternParser.cs"

@echo "generating PatternTokenizer.cs"
@echo #define XSLT_PATTERN > "%1\Mono.Xml.Xsl\PatternTokenizer.cs"
type "%1\System.Xml.XPath\Tokenizer.cs" >> "%1\Mono.Xml.Xsl\PatternTokenizer.cs"