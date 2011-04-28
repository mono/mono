ns0.xsd : MS default serialization namespace.
ns1.xsd : imports ns2.xsd, has duplicate name to ns2. Also check [KnownType].
ns2.xsd : used with ns1.xsd, not much special.
ns3.xsd : has simple content extension, seems like not supported in WCF.
ns4.xsd : has required attributes, not supported in WCF.
ns5.xsd : has optional attributes but not in MS serialization namespace,
	  not supported in WCF either.
ns6.xsd : derivation by extension example. Must be used with ns0.xsd.
ns7.xsd : derivation by restriction example. Must be used with ns0,xsd, but
	  not supported in WCF.
ns8.xsd : attempt to examine if complex type simple content restriction is
	  supported. There is no way to define it without simple DBE (and
	  simple DBE is not supported in WCF).
ns9.xsd : contains elements with an identical name, not supported in WCF.
ns10.xsd : contains element with substitutionGroup. E2 is resolved to be
	   mapped to T1. To verify it, Import() only E2 element.
ns11.xsd : contains xs:choice as the particle, not supported in WCF(!)
ns12.xsd : contains xs:all as the particle, not supported in WCF(!)
ns13.xsd : contains xs:any as the content of xs:sequence, not supported in WCF.
	   see also ns33.xsd which contains an xs:element and thus OK.
ns14.xsd : contains simple content restriction by enum. Mapped to CLI enum.
ns15.xsd : contains simple list by string, not supported in WCF.
ns16.xsd : contains simple list by embedded enumeration string type.
ns17.xsd : contains simple list by external enumeration string type, not
	   supported in WCF.
ns18.xsd : variation of ns14, replaced xs:string with xs:int. No output.
ns19.xsd : variation of ns16, replaced xs:string with xs:int. Error.
ns20.xsd : variation of ns16, added maxLength facet. Error.
ns21.xsd : array of int, gives no output.
ns22.xsd : use of Id/Ref attributes, makes DataContract(IsReference=true).
ns23.xsd : collection contract, to prove that xmlns does not matter (rrays
	   instead of Arrays).
	   cf. it matters when it comes to primitive type, see ns27.
ns24.xsd : ArrayOfint type contains two elements, not supported in WCF.
ns25.xsd : uses ArrayOfint as a member type. Gives int[] instead of ArrayOfint.
ns26.xsd : variation of ns25, uses custom simpleType. Gives ArrayOfint.
ns27.xsd : variation of ns25, uses non-Arrays xmlns. Gives ArrayOfint.
ns28.xsd : dictionary collection type.
ns29.xsd : variation of ns28, removed Value. Error.
ns30.xsd : variation of ns28, customized name. Shown in [DataMember].
ns31.xsd : variation of ns28, removed IsDictionary appInfo. Becomes List<T>.
ns32.xsd : variation of ns13, moved xs:complexType under xs:element and thus
	   became OK. (despite the error message explicitly prohibits this!)
ns33.xsd : variation of ns13, replaced xs:any with xs:element and became OK
	   (despite the error message explicitly prohibits this!)
