<?xml version="1.0"?>

<!--
	mdoc-html-utils.xsl: ECMA-style docs to HTML stylesheet transformation utils

	Author: Joshua Tauberer (tauberer@for.net)
	Author: Jonathan Pryor (jpryor@novell.com)

	This file requires that including files define the following callable
	templates:
		- CreateCodeBlock (language, content)
		- CreateEnumerationTable (content)
		- CreateHeader (content)
		- CreateListTable (header, content)
		- CreateMembersTable (content)
		- CreateSignature (content)
		- CreateTypeDocumentationTable (content)
		- GetLinkTarget (type, cref)
		- CreateEditLink (e)

-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	exclude-result-prefixes="msxsl"
	>
	<xsl:import href="mdoc-html-format.xsl" />
	
	<!-- TEMPLATE PARAMETERS -->
	<xsl:param name="language" select="'C#'"/>
	<xsl:param name="index" />
	<xsl:param name="source-id"/>
	
	<xsl:variable name="ThisType" select="/Type"/>

	<!-- The namespace that the current type belongs to. -->
	<xsl:variable name="TypeNamespace" select="substring(/Type/@FullName, 1, string-length(/Type/@FullName) - string-length(/Type/@Name) - 1)"/>		

	<!-- THE MAIN RENDERING TEMPLATE -->

	<!-- TYPE OVERVIEW -->
		
	<xsl:template name="CreateTypeOverview">
		<xsl:param name="implemented" />
		<xsl:param name="show-members-link" />

		<xsl:attribute name="id">
			<xsl:text>T:</xsl:text>
			<xsl:call-template name="GetEscapedTypeName">
				<xsl:with-param name="typename" select="@FullName" />
			</xsl:call-template>
			<xsl:text>:Summary</xsl:text>
		</xsl:attribute>
		<!-- summary -->
		<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
		<xsl:apply-templates select="Docs/summary" mode="editlink"/>

		<xsl:if test="$implemented">
			<p><b>Mono Implementation Note: </b></p>
			<blockquote>
				<xsl:value-of disable-output-escaping="yes" select="$implemented"/>
			</blockquote>
		</xsl:if>

		<xsl:if test="$show-members-link and not(Base/BaseTypeName='System.Enum' or Base/BaseTypeName='System.Delegate' or Base/BaseTypeName='System.MulticastDelegate') and count(Members)">
			<p>
				See Also:
				<a>
					<xsl:attribute name="href">
						<xsl:text>T</xsl:text>
						<xsl:call-template name="GetLinkId">
							<xsl:with-param name="type" select="." />
							<xsl:with-param name="member" select="." />
						</xsl:call-template>
						<xsl:text>/*</xsl:text>
					</xsl:attribute>
					<xsl:value-of select="translate(@Name, '+', '.')"/>
					<xsl:value-of select="' '" />
					<xsl:text>Members</xsl:text>
				</a>
			</p>
		</xsl:if>
		
		<!--
		Inheritance tree, but only for non-standard classes and not for interfaces
		-->
		<xsl:if test="not(Base/BaseTypeName='System.Enum' or Base/BaseTypeName='System.Delegate' or Base/BaseTypeName='System.ValueType' or Base/BaseTypeName='System.Object' or Base/BaseTypeName='System.MulticatDelegate' or count(Base/ParentType)=0)">
			<p>
			<xsl:for-each select="Base/ParentType">
				<xsl:sort select="@Order" order="descending"/>
				<xsl:variable name="p" select="position()" />
				<xsl:for-each select="parent::Base/ParentType[position() &lt; $p]">
					<xsl:value-of select="'&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;'" disable-output-escaping="yes"/>
				</xsl:for-each>
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="GetLinkTargetHtml">
							<xsl:with-param name="type" select="@Type" />
							<xsl:with-param name="cref">
								<xsl:text>T:</xsl:text>
								<xsl:call-template name="GetEscapedTypeName">
									<xsl:with-param name="typename" select="@Type" />
								</xsl:call-template>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@Type"/>
				</a>
				<br/>
			</xsl:for-each>

			<xsl:for-each select="Base/ParentType">
				<xsl:value-of select="'&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;'" disable-output-escaping="yes"/>
			</xsl:for-each>
			<xsl:value-of select="@FullName"/>
			</p>
		</xsl:if>
		<!--
		<xsl:if test="Base/BaseTypeName='System.Enum'">
			<br/>
			The type of the values in this enumeration is 
			<xsl:apply-templates select="Members/Member[@MemberName='value__']/ReturnValue/ReturnType" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>.
		</xsl:if>
		-->
	</xsl:template>

	<xsl:template name="CreateTypeSignature">
			<xsl:call-template name="CreateSignature">
			    <xsl:with-param name="id">
				  <xsl:text>T:</xsl:text>
				  <xsl:call-template name="GetEscapedTypeName">
					<xsl:with-param name="typename" select="@FullName" />
				  </xsl:call-template>
				  <xsl:text>:Signature</xsl:text>
				</xsl:with-param>
				<xsl:with-param name="content">
			<!-- signature -->
					<xsl:choose>
					<xsl:when test="$language='C#'">

						<xsl:for-each select="Attributes/Attribute">
							<xsl:text>[</xsl:text>
							<xsl:value-of select="AttributeName"/>
							<xsl:text>]</xsl:text>
							<br/>
						</xsl:for-each>

						<xsl:for-each select="ReturnValue/Attributes/Attribute">
							<xsl:text>[return:</xsl:text>
							<xsl:value-of select="AttributeName"/>
							<xsl:text>]</xsl:text>
							<br/>
						</xsl:for-each>	
	
						<xsl:choose>

						<xsl:when test="Base/BaseTypeName='System.Enum'">
							<xsl:call-template name="getmodifiers">
								<xsl:with-param name="sig" select="TypeSignature[@Language='C#']/@Value"/>
							</xsl:call-template>

							<xsl:text>enum </xsl:text>
	
							<!-- member name, argument list -->
							<b>
							<xsl:value-of select="translate (@Name, '+', '.')"/>
							</b>
						</xsl:when>
	
						<xsl:when test="Base/BaseTypeName='System.Delegate' or Base/BaseTypeName='System.MulticastDelegate'">
							<xsl:choose>

							<xsl:when test="count(Parameters) &gt; 0 and count(ReturnValue) &gt; 0">
							<!-- Only recreate the delegate signature if the appropriate information
								is present in the XML file. -->

							<xsl:call-template name="getmodifiers">
								<xsl:with-param name="sig" select="TypeSignature[@Language='C#']/@Value"/>
							</xsl:call-template>

							<xsl:text>delegate </xsl:text>
	
							<xsl:apply-templates select="ReturnValue/ReturnType" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>
	
							<!-- hard space -->
							<xsl:value-of select="' '"/>
	
							<!-- member name, argument list -->
							<b>
								<xsl:call-template name="GetDefinitionName">
									<xsl:with-param name="name" select="translate (@Name, '+', '.')" />
									<xsl:with-param name="TypeParameters" select="TypeParameters" />
								</xsl:call-template>
							</b>

							<!-- hard space -->
							<xsl:value-of select="' '"/>

							<xsl:value-of select="'('"/> <!-- prevents whitespace issues -->
							
							<xsl:for-each select="Parameters/Parameter">
								<xsl:call-template name="ShowParameter">
									<xsl:with-param name="Param" select="."/>
									<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
								</xsl:call-template>

								<xsl:if test="not(position()=last())">, </xsl:if>
							</xsl:for-each>
							
							<xsl:value-of select="')'"/>

							</xsl:when>
							
							<xsl:otherwise>
								<xsl:apply-templates select="TypeSignature[@Language=$language]/@Value"/>	
							</xsl:otherwise>

							</xsl:choose>

							
						</xsl:when>

						<xsl:otherwise>
							<xsl:call-template name="getmodifiers">
								<xsl:with-param name="sig" select="TypeSignature[@Language='C#']/@Value"/>
								<xsl:with-param name="typetype" select="true()"/>
							</xsl:call-template>
		
							<xsl:value-of select="' '"/>
		
							<b>
								<xsl:call-template name="GetDefinitionName">
									<xsl:with-param name="name" select="translate (@Name, '+', '.')" />
									<xsl:with-param name="TypeParameters" select="TypeParameters" />
								</xsl:call-template>
							</b>
		
							<xsl:variable name="HasStandardBaseType" select="Base/BaseTypeName='System.Object' or Base/BaseTypeName='System.ValueType'"/>
							<xsl:variable name="HasBaseType" select="count(Base/BaseTypeName)>0"/>
							<xsl:if test="(($HasBaseType) and not($HasStandardBaseType)) or not(count(Interfaces/Interface)=0)">
								<xsl:text> : </xsl:text>
		
								<xsl:if test="$HasBaseType and not($HasStandardBaseType)">
									<xsl:apply-templates select="Base/BaseTypeName" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>
									<xsl:if test="not(count(Interfaces/Interface)=0)">,	</xsl:if>
								</xsl:if>
		
								<xsl:for-each select="Interfaces/Interface">
									<xsl:if test="not(position()=1)">, </xsl:if>
									<xsl:apply-templates select="InterfaceName" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>
								</xsl:for-each>
							
							</xsl:if>
						</xsl:otherwise>

						</xsl:choose>

						<xsl:call-template name="CreateGenericConstraints">
							<xsl:with-param name="TypeParameters" select="TypeParameters" />
						</xsl:call-template>

					</xsl:when>

					<xsl:otherwise>
						<xsl:apply-templates select="TypeSignature[@Language=$language]/@Value"/>
					</xsl:otherwise>
					
					</xsl:choose>
				</xsl:with-param>
			</xsl:call-template>
	</xsl:template>

	<xsl:template name="GetDefinitionName">
		<xsl:param name="name" />
		<xsl:param name="TypeParameters" />

		<xsl:choose>
			<!-- do NOT process explicitly implemented generic interface members
			     unless they're actually generic methods. -->
			<xsl:when test="contains ($name, '&gt;') and
					'&gt;' = substring ($name, string-length ($name), 1)">
				<xsl:value-of select="substring-before ($name, '&lt;')" />
				<xsl:text>&lt;</xsl:text>
				<xsl:for-each select="$TypeParameters/TypeParameter">
					<xsl:for-each select="Attributes/Attribute">
						<xsl:text>[</xsl:text>
						<xsl:value-of select="AttributeName"/>
						<xsl:text>] </xsl:text>
					</xsl:for-each>
					<xsl:choose>
						<xsl:when test="@Name">
							<xsl:value-of select="@Name" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="." />
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="not(position()=last())">, </xsl:if>
				</xsl:for-each>
				<xsl:text>&gt;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="CreateGenericConstraints">
		<xsl:param name="TypeParameters" />

		<xsl:for-each select="$TypeParameters/TypeParameter">
			<xsl:variable name="constraintsCount" select="count(Constraints/*)" />
			<xsl:if test="$constraintsCount > 0 and count(Constraints/*[.='Contravariant' or .='Covariant']) != $constraintsCount">
				<xsl:call-template name="CreateGenericParameterConstraints">
					<xsl:with-param name="constraints" select="Constraints" />
				</xsl:call-template>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="CreateGenericParameterConstraints">
		<xsl:param name="constraints" />

		<br />
		<xsl:text> where </xsl:text>
		<xsl:value-of select="@Name" />
		<xsl:text> : </xsl:text>
		<xsl:variable name="kind" 
			select="count($constraints[ParameterAttribute='ReferenceTypeConstraint'])+
				count($constraints[ParameterAttribute='NotNullableValueTypeConstraint'])" />
		<xsl:variable name="base" select="count($constraints/BaseTypeName)" />
		<xsl:variable name="iface" select="count($constraints/InterfaceName)" />
		<xsl:variable name="struct" select="$constraints/ParameterAttribute='NotNullableValueTypeConstraint'" />
		<xsl:if test="$constraints/ParameterAttribute='ReferenceTypeConstraint'">
			<xsl:text>class</xsl:text>
		</xsl:if>
		<xsl:if test="$constraints/ParameterAttribute='NotNullableValueTypeConstraint'">
			<xsl:text>struct</xsl:text>
		</xsl:if>
		<xsl:if test="$constraints/BaseTypeName and not($struct)">
			<xsl:if test="$kind">, </xsl:if>
			<xsl:apply-templates select="$constraints/BaseTypeName" mode="typelink" />
		</xsl:if>
		<xsl:for-each select="$constraints/InterfaceName">
			<xsl:if test="position()=1">
				<xsl:if test="$kind or $base">, </xsl:if>
			</xsl:if>
			<xsl:apply-templates select="." mode="typelink" />
			<xsl:if test="not(position()=last())">, </xsl:if>
		</xsl:for-each>
		<xsl:if test="$constraints/ParameterAttribute='DefaultConstructorConstraint' and not($struct)">
			<xsl:if test="$base or $iface">, </xsl:if>
			<xsl:text>new()</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CreateMemberOverview">
		<xsl:param name="implemented" />

		<p class="Summary">
			<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
			<xsl:apply-templates select="Docs/summary" mode="editlink"/>
		</p>

		<xsl:if test="$implemented">
			<p><b>Mono Implementation Note: </b></p>
			<blockquote>
				<xsl:value-of disable-output-escaping="yes" select="$implemented"/>
			</blockquote>
		</xsl:if>

		<!-- member value -->
		<xsl:if test="MemberValue">
		<p><b>Value: </b>
			<xsl:value-of select="MemberValue"/>
		</p>
		</xsl:if>

	</xsl:template>

	<xsl:template name="CreateRelatedSection">
	  <xsl:param name="section" />
	  <xsl:param name="type" />
	  <xsl:if test="count(Docs/related[@type=$type])">
		<h3 class="{$type}"><xsl:value-of select="$section" /></h3>
		<ul class="{$type}">
		  <xsl:for-each select="Docs/related[@type=$type]">
			<li><a href="{@href}" target="_blank"><xsl:value-of select="." /></a></li>
		  </xsl:for-each>
		</ul>
	  </xsl:if>
	</xsl:template>

	<xsl:template name="CreatePlatformRequirements">
	  <!-- For now we only have that information in MonoTouch so only process that -->
	  <xsl:if test="starts-with(/Type/@FullName, 'MonoTouch')">
		<xsl:choose>
		  <!-- We first check if we have a [Since] at the member level -->
		  <xsl:when test="count(Attributes/Attribute/AttributeName[starts-with(text(), 'MonoTouch.ObjCRuntime.Since')])">
			<b>Minimum iOS version: </b>
			<xsl:value-of select="translate(substring-before (substring-after (Attributes/Attribute/AttributeName[starts-with(text(), 'MonoTouch.ObjCRuntime.Since')], 'MonoTouch.ObjCRuntime.Since('), ')'), ', ', '.')" />
			<br />
		  </xsl:when>
		  <!-- If not, we then check at the type level -->
		  <xsl:when test="count(/Type/Attributes/Attribute/AttributeName[starts-with(text(), 'MonoTouch.ObjCRuntime.Since')])">
			<b>Minimum iOS version: </b> 
			<xsl:value-of select="translate(substring-before (substring-after (/Type/Attributes/Attribute/AttributeName[starts-with(text(), 'MonoTouch.ObjCRuntime.Since')], 'MonoTouch.ObjCRuntime.Since('), ')'), ', ', '.')" />
			<br />
		  </xsl:when>
		</xsl:choose>
	  </xsl:if>
	</xsl:template>

	<xsl:template name="CreateMemberSignature">
		<xsl:param name="linkid" select="''" />

		<xsl:call-template name="CreateSignature">
			<xsl:with-param name="content">
			<xsl:if test="contains(MemberSignature[@Language='C#']/@Value,'this[')">
				<p><i>This is the default property for this class.</i></p>
			</xsl:if>

			<!-- recreate the signature -->
		
			<xsl:for-each select="Attributes/Attribute[AttributeName != 'System.Runtime.CompilerServices.Extension']">
				<xsl:text>[</xsl:text>
				<xsl:value-of select="AttributeName"/>
				<xsl:text>]</xsl:text>
				<br/>
			</xsl:for-each>	

			<xsl:for-each select="ReturnValue/Attributes/Attribute">
				<xsl:text>[return:</xsl:text>
				<xsl:value-of select="AttributeName"/>
				<xsl:text>]</xsl:text>
				<br/>
			</xsl:for-each>	

			<xsl:call-template name="getmodifiers">
				<xsl:with-param name="sig" select="MemberSignature[@Language='C#']/@Value"/>
			</xsl:call-template>

			<xsl:if test="MemberType = 'Event'">
				<xsl:text>event </xsl:text>

				<xsl:if test="ReturnValue/ReturnType=''">
					<xsl:value-of select="substring-before(substring-after(MemberSignature[@Language='C#']/@Value, 'event '), concat(' ', @MemberName))"/>
				</xsl:if>
			</xsl:if>

			<!-- return value (comes out "" where not applicable/available) -->
			<xsl:choose>
			<xsl:when test="@MemberName='op_Implicit'">
				<xsl:text>implicit operator</xsl:text>
			</xsl:when>
			<xsl:when test="@MemberName='op_Explicit'">
				<xsl:text>explicit operator</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="ReturnValue/ReturnType" mode="typelink">
					<xsl:with-param name="wrt" select="$TypeNamespace"/>
				</xsl:apply-templates>
			</xsl:otherwise>					
			</xsl:choose>

			<!-- hard space -->
			<xsl:value-of select="' '"/>

			<!-- member name -->
			<xsl:choose>
			
			<!-- Constructors get the name of the class -->
			<xsl:when test="MemberType='Constructor'">
				<b>
					<xsl:call-template name="GetConstructorName">
						<xsl:with-param name="type" select="../.." />
						<xsl:with-param name="ctor" select="." />
					</xsl:call-template>
				</b>
			</xsl:when>
			
			<!-- Conversion operators get the return type -->
			<xsl:when test="@MemberName='op_Implicit' or @MemberName='op_Explicit'">
				<xsl:apply-templates select="ReturnValue/ReturnType" mode="typelink">
					<xsl:with-param name="wrt" select="$TypeNamespace"/>
				</xsl:apply-templates>
			</xsl:when>
			
			<!-- Regular operators get their symbol -->
			<xsl:when test="@MemberName='op_UnaryPlus'">operator+</xsl:when>
			<xsl:when test="@MemberName='op_UnaryNegation'">operator-</xsl:when>
			<xsl:when test="@MemberName='op_LogicalNot'">operator!</xsl:when>
			<xsl:when test="@MemberName='op_OnesComplement'">operator~</xsl:when>
			<xsl:when test="@MemberName='op_Increment'">operator++</xsl:when>
			<xsl:when test="@MemberName='op_Decrement'">operator--</xsl:when>
			<xsl:when test="@MemberName='op_True'">operator true</xsl:when>
			<xsl:when test="@MemberName='op_False'">operator false</xsl:when>
			<xsl:when test="@MemberName='op_Addition'">operator+</xsl:when>
			<xsl:when test="@MemberName='op_Subtraction'">operator-</xsl:when>
			<xsl:when test="@MemberName='op_Multiply'">operator*</xsl:when>
			<xsl:when test="@MemberName='op_Division'">operator/</xsl:when>
			<xsl:when test="@MemberName='op_Modulus'">operator%</xsl:when>
			<xsl:when test="@MemberName='op_BitwiseAnd'">operator&amp;</xsl:when>
			<xsl:when test="@MemberName='op_BitwiseOr'">operator|</xsl:when>
			<xsl:when test="@MemberName='op_ExclusiveOr'">operator^</xsl:when>
			<xsl:when test="@MemberName='op_LeftShift'">operator&lt;&lt;</xsl:when>
			<xsl:when test="@MemberName='op_RightShift'">operator&gt;&gt;</xsl:when>
			<xsl:when test="@MemberName='op_Equality'">operator==</xsl:when>
			<xsl:when test="@MemberName='op_Inequality'">operator!=</xsl:when>
			<xsl:when test="@MemberName='op_GreaterThan'">operator&gt;</xsl:when>
			<xsl:when test="@MemberName='op_LessThan'">operator&lt;</xsl:when>
			<xsl:when test="@MemberName='op_GreaterThanOrEqual'">operator&gt;=</xsl:when>
			<xsl:when test="@MemberName='op_LessThanOrEqual'">operator&lt;=</xsl:when>

			<xsl:when test="MemberType='Property' and count(Parameters/Parameter) &gt; 0">
				<!-- C# only permits indexer properties to have arguments -->
				<xsl:text>this</xsl:text>
			</xsl:when>
			
			<!-- Everything else just gets its name -->
			<xsl:when test="contains (@MemberName, '&lt;')">
				<b>
					<xsl:call-template name="GetDefinitionName">
						<xsl:with-param name="name" select="@MemberName" />
						<xsl:with-param name="TypeParameters" select="TypeParameters" />
					</xsl:call-template>
				</b>
			</xsl:when>

			<xsl:otherwise>
				<b><xsl:value-of select="@MemberName"/></b>
			</xsl:otherwise>
			</xsl:choose>

			<!-- hard space -->
			<xsl:value-of select="' '"/>

			<!-- argument list -->
			<xsl:if test="MemberType='Method' or MemberType='Constructor' or (MemberType='Property' and count(Parameters/Parameter))">
				<xsl:if test="not(MemberType='Property')">(</xsl:if>
				<xsl:if test="MemberType='Property'">[</xsl:if>

				<xsl:for-each select="Parameters/Parameter">
					<xsl:call-template name="ShowParameter">
						<xsl:with-param name="Param" select="."/>
						<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
					</xsl:call-template>

					<xsl:if test="not(position()=last())">, </xsl:if>
				</xsl:for-each>
				<xsl:if test="not(MemberType='Property')">)</xsl:if>
				<xsl:if test="MemberType='Property'">]</xsl:if>
			</xsl:if>

			<xsl:if test="MemberType='Property'">
				<xsl:value-of select="' '"/>
				<xsl:text>{</xsl:text>
				<xsl:value-of select="substring-before(substring-after(MemberSignature[@Language='C#']/@Value, '{'), '}')"/>
				<xsl:text>}</xsl:text>
			</xsl:if>
			<xsl:call-template name="CreateGenericConstraints">
				<xsl:with-param name="TypeParameters" select="TypeParameters" />
			</xsl:call-template>
			</xsl:with-param>
		</xsl:call-template>
		
	</xsl:template>

	<xsl:template name="GetConstructorName">
		<xsl:param name="type" />
		<xsl:param name="ctor" />

		<xsl:choose>
			<xsl:when test="contains($type/@Name, '&lt;')">
				<xsl:value-of select="translate (substring-before ($type/@Name, '&lt;'), '+', '.')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="translate ($type/@Name, '+', '.')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="ShowParameter">
		<xsl:param name="Param"/>
		<xsl:param name="TypeNamespace"/>
		<xsl:param name="prototype" select="false()"/>

		<xsl:if test="not($prototype)">
			<xsl:for-each select="$Param/Attributes/Attribute[not(Exclude='1') and not(AttributeName='ParamArrayAttribute' or AttributeName='System.ParamArray')]">
				<xsl:text>[</xsl:text>
				<xsl:value-of select="AttributeName"/>
				<xsl:text>]</xsl:text>
				<xsl:value-of select="' '"/>
			</xsl:for-each>
		</xsl:if>

		<xsl:if test="count($Param/Attributes/Attribute/AttributeName[.='ParamArrayAttribute' or .='System.ParamArray'])">
			<b>params</b>
			<xsl:value-of select="' '"/>
		</xsl:if>

		<xsl:if test="$Param/@RefType">
			<i><xsl:value-of select="$Param/@RefType"/></i>
			<!-- hard space -->
			<xsl:value-of select="' '"/>
		</xsl:if>

		<!-- parameter type link -->
		<xsl:apply-templates select="$Param/@Type" mode="typelink">
			<xsl:with-param name="wrt" select="$TypeNamespace"/>
		</xsl:apply-templates>

		<xsl:if test="not($prototype)">
			<!-- hard space -->
			<xsl:value-of select="' '"/>
	
			<!-- parameter name -->
			<xsl:value-of select="$Param/@Name"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="DisplayDocsInformation">
		<xsl:param name="linkid" />

		<!-- The namespace that the current type belongs to. -->
		<xsl:variable name="TypeNamespace" select="substring(@FullName, 1, string-length(@FullName) - string-length(@Name) - 1)"/>

		<!-- alt member: not sure what these are for, actually -->

		<xsl:if test="count(Docs/altmember)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'See Also'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':See Also')" />
				<xsl:with-param name="content">
					<xsl:for-each select="Docs/altmember">
						<div><xsl:apply-templates select="@cref" mode="cref"/></div>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- parameters & return & value -->

		<xsl:if test="count(Docs/typeparam)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'Type Parameters'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Type Parameters')" />
				<xsl:with-param name="content">
					<dl>
					<xsl:for-each select="Docs/typeparam">
						<dt><i><xsl:value-of select="@name"/></i></dt>
						<dd>
							<xsl:apply-templates select="." mode="notoppara"/>
							<xsl:apply-templates select="." mode="editlink"/>
						</dd>
					</xsl:for-each>
					</dl>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="count(Docs/param)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'Parameters'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Parameters')" />
				<xsl:with-param name="content">
					<dl>
					<xsl:for-each select="Docs/param">
						<dt><i><xsl:value-of select="@name"/></i></dt>
						<dd>
							<xsl:apply-templates select="." mode="notoppara"/>
							<xsl:apply-templates select="." mode="editlink"/>
						</dd>
					</xsl:for-each>
					</dl>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="count(Docs/returns)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'Returns'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Returns')" />
				<xsl:with-param name="content">
					<xsl:apply-templates select="Docs/returns" mode="notoppara"/>
					<xsl:apply-templates select="Docs/returns" mode="editlink"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="count(Docs/value)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'Value'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Value')" />
				<xsl:with-param name="content">
					<xsl:apply-templates select="Docs/value" mode="notoppara"/>
					<xsl:apply-templates select="Docs/value" mode="editlink"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- method/property/constructor exceptions -->

		<xsl:if test="count(Docs/exception)">
			<xsl:call-template name="CreateH4Section">
				<xsl:with-param name="name" select="'Exceptions'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Exceptions')" />
				<xsl:with-param name="content">
					<xsl:call-template name="CreateTypeDocumentationTable">
					<xsl:with-param name="content">
					<xsl:for-each select="Docs/exception">
						<tr valign="top">
						<td>
							<xsl:apply-templates select="@cref" mode="typelink">
								<xsl:with-param name="wrt" select="$TypeNamespace"/>
							</xsl:apply-templates>
						</td>
						<td>
							<xsl:apply-templates select="." mode="notoppara"/>
							<xsl:apply-templates select="." mode="editlink"/>
						</td>
						</tr>
					</xsl:for-each>
					</xsl:with-param>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- remarks -->

		<xsl:if test="count(Docs/remarks)">
			<xsl:call-template name="CreateH2Section">
				<xsl:with-param name="name" select="'Remarks'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Remarks')" />
				<xsl:with-param name="content">
					<xsl:apply-templates select="Docs/remarks" mode="notoppara"/>
					<xsl:apply-templates select="Docs/remarks" mode="editlink"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- thread safety -->

		<xsl:if test="count(ThreadingSafetyStatement)">
			<xsl:call-template name="CreateH2Section">
				<xsl:with-param name="name" select="'Thread Safety'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Thread Safety')" />
				<xsl:with-param name="content">
					<xsl:apply-templates select="ThreadingSafetyStatement" mode="notoppara"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>


		<!-- permissions -->

		<xsl:if test="count(Docs/permission)">
			<xsl:call-template name="CreateH2Section">
				<xsl:with-param name="name" select="'Permissions'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Permissions')" />
				<xsl:with-param name="content">
					<xsl:call-template name="CreateTypeDocumentationTable">
					<xsl:with-param name="content">
					<xsl:for-each select="Docs/permission">
						<tr valign="top">
						<td>
							<xsl:apply-templates select="@cref" mode="typelink">
								<xsl:with-param name="wrt" select="$TypeNamespace"/>
							</xsl:apply-templates>
							<xsl:apply-templates select="." mode="editlink"/>
						</td>
						<td>
							<xsl:apply-templates select="." mode="notoppara"/>
						</td>
						</tr>
					</xsl:for-each>
					</xsl:with-param>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- enumeration values -->

		<xsl:if test="Base/BaseTypeName = 'System.Enum'">
			<xsl:call-template name="CreateH2Section">
				<xsl:with-param name="name" select="'Members'"/>
				<xsl:with-param name="child-id" select="concat ($linkid, ':Members')" />
				<xsl:with-param name="content">
					<xsl:call-template name="CreateEnumerationTable">
					<xsl:with-param name="content">

						<xsl:for-each select="Members/Member[MemberType='Field']">
							<xsl:if test="not(@MemberName='value__')">
								<tr valign="top"><td>
									<xsl:attribute name="id">
										<xsl:text>F:</xsl:text>
										<xsl:value-of select="translate (/Type/@FullName, '+', '.')" />
										<xsl:text>.</xsl:text>
										<xsl:value-of select="@MemberName" />
									</xsl:attribute>
									<b>
										<xsl:value-of select="@MemberName"/>
									</b>
								</td>
								<td>
									<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
									<xsl:apply-templates select="Docs/summary" mode="editlink"/>
								</td>
								</tr>
							</xsl:if>
						</xsl:for-each>
					</xsl:with-param>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>

		<!-- examples -->

		<xsl:if test="count(Docs/example)">
			<xsl:for-each select="Docs/example">
				<xsl:call-template name="CreateH2Section">
					<xsl:with-param name="name" select="'Example'"/>
					<xsl:with-param name="child-id" select="concat ($linkid, ':Example:', position())" />
					<xsl:with-param name="content">
						<xsl:apply-templates select="." mode="notoppara"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:for-each>
		</xsl:if>

		<!-- related content -->
		<xsl:if test="count(Docs/related)">
		  <xsl:call-template name="CreateH2Section">
			<xsl:with-param name="name" select="'Related content'" />
			<xsl:with-param name="child-id" select="concat ($linkid, ':Related:')" />
			<xsl:with-param name="content">
			  <div class="related">
				<xsl:call-template name="CreateRelatedSection">
				  <xsl:with-param name="section" select="'Articles'" />
				  <xsl:with-param name="type" select="'article'" />
				</xsl:call-template>
				<xsl:call-template name="CreateRelatedSection">
				  <xsl:with-param name="section" select="'Recipes'" />
				  <xsl:with-param name="type" select="'recipe'" />
				</xsl:call-template>
				<xsl:call-template name="CreateRelatedSection">
				  <xsl:with-param name="section" select="'Samples'" />
				  <xsl:with-param name="type" select="'sample'" />
				</xsl:call-template>
				<xsl:call-template name="CreateRelatedSection">
				  <xsl:with-param name="section" select="'Related specifications'" />
				  <xsl:with-param name="type" select="'specification'" />
				</xsl:call-template>
				<xsl:call-template name="CreateRelatedSection">
				  <xsl:with-param name="section" select="'External Documentation'" />
				  <xsl:with-param name="type" select="'externalDocumentation'" />
				</xsl:call-template>
			  </div>
			</xsl:with-param>
		  </xsl:call-template>
		</xsl:if>

		<xsl:call-template name="CreateH2Section">
			<xsl:with-param name="name" select="'Requirements'"/>
			<xsl:with-param name="child-id" select="concat ($linkid, ':Version Information')" />
			<xsl:with-param name="content">
				<xsl:call-template name="CreatePlatformRequirements" />
				<b>Namespace: </b><xsl:value-of select="substring(/Type/@FullName, 1, string-length(/Type/@FullName) - string-length(/Type/@Name) - 1)" />
				<xsl:if test="count(/Type/AssemblyInfo/AssemblyName) &gt; 0">
					<br />
					<b>Assembly: </b>
					<xsl:value-of select="/Type/AssemblyInfo/AssemblyName" />
					<xsl:text> (in </xsl:text>
					<xsl:value-of select="/Type/AssemblyInfo/AssemblyName" />
					<xsl:text>.dll)</xsl:text>
				</xsl:if>
				<xsl:if test="count(AssemblyInfo/AssemblyVersion) &gt; 0">
					<br />
					<b>Assembly Versions: </b>
					<xsl:for-each select="AssemblyInfo/AssemblyVersion">
						<xsl:if test="not(position()=1)">, </xsl:if>
							<xsl:value-of select="."/>
					</xsl:for-each>
				</xsl:if>
				<xsl:if test="count(Docs/since) &gt; 0">
					<br />
					<b>Since: </b>
					<xsl:for-each select="Docs/since">
						<xsl:if test="not(position()=1)">; </xsl:if>
							<xsl:value-of select="@version"/>
					</xsl:for-each>
				</xsl:if>
				<xsl:if test="count(Docs/since)=0 and count(/Type/Docs/since) &gt; 0">
					<br />
					<b>Since: </b>
					<xsl:for-each select="/Type/Docs/since">
						<xsl:if test="not(position()=1)">; </xsl:if>
							<xsl:value-of select="@version"/>
					</xsl:for-each>
				</xsl:if>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	
	<!-- Transforms the contents of the selected node into a hyperlink to the type named by the node.  The node can contain a type name (eg System.Object) or a type link (eg T:System.String). Use wrt parameter to specify the current namespace. -->

	<xsl:template match="*|@*" mode="typelink">
		<xsl:param name="wrt" select="'notset'"/>
		
		<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="."/>
				<xsl:with-param name="wrt" select="$wrt"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="makenamespacelink">
		<xsl:param name="cref" select="''"/>

		<a>
			<xsl:attribute name="href">
				<xsl:call-template name="GetLinkTargetHtml">
					<xsl:with-param name="cref" select="$cref" />
				</xsl:call-template>
			</xsl:attribute>
	
			<xsl:value-of select="substring-after ($cref, 'N:')" />
		</a>
	</xsl:template>

	<xsl:template name="maketypelink">
		<xsl:param name="type" select="'notset'"/>
		<xsl:param name="wrt" select="'notset'"/>
		<xsl:param name="nested" select="0"/>

		<xsl:variable name="btype">
			<xsl:call-template name="ToBrackets">
				<xsl:with-param name="s" select="$type" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="array">
			<xsl:call-template name="GetArraySuffix">
				<xsl:with-param name="type" select="$type" />
			</xsl:call-template>
		</xsl:variable>
		
		<xsl:choose>

		<!-- chop off T: -->
		<xsl:when test="starts-with($type, 'T:')">
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="substring($type, 3)"/>
				<xsl:with-param name="wrt" select="$wrt"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:when test="contains ($type, '&amp;') and 
				'&amp;' = substring ($type, string-length ($type), 1)">
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="substring($type, 1, string-length($type)-1)"/>
				<xsl:with-param name="wrt" select="$wrt"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:when test="string($array)">
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="substring($type, 1, string-length($type) - string-length ($array))"/>
				<xsl:with-param name="wrt" select="$wrt"/>
			</xsl:call-template>
			<xsl:value-of select="$array"/>
		</xsl:when>

		<xsl:when test="contains ($type, '*') and
				'*' = substring ($type, string-length ($type), 1)">
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="substring($type, 1, string-length($type)-1)"/>
				<xsl:with-param name="wrt" select="$wrt"/>
			</xsl:call-template>
			<xsl:value-of select="'*'"/>
		</xsl:when>
		
		<!-- if this is a generic type parameter, don't make a link but italicize it and give it a tooltip instead -->
		<xsl:when test="count($ThisType/TypeParameters/TypeParameter[@Name=$type] | 
				$ThisType/TypeParameters/TypeParameter[child::text()=$type] |
				ancestor::Member/Docs/typeparam[@name=$type]) = 1">
			<!-- note that we check if it is a generic type using /Type/TypeParameters because that will have type parameters declared in an outer class if this is a nested class, but then we get the tooltip text from the type parameters documented in this file -->
			<i title="{$ThisType/Docs/typeparam[@name=$type] | ancestor::Member/Docs/typeparam[@name=$type]}"><xsl:value-of select="$type"/></i>
		</xsl:when>
		
		<!-- if this is a generic type parameter of a base type, replace it with the type that it was instantiated with -->
		<xsl:when test="count(ancestor::Members/BaseTypeArgument[@TypeParamName=$type]) = 1">
			<!-- note that an overridden type parameter may be referenced in a type parameter within $type, but we can't replace that nicely since we can't parse generic type names here -->
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="ancestor::Members/BaseTypeArgument[@TypeParamName=$type]"/>
				<xsl:with-param name="wrt" select="$wrt"/>
			</xsl:call-template>
		</xsl:when>
		

		<xsl:otherwise>
			<xsl:variable name="escaped-type">
				<xsl:call-template name="GetEscapedTypeName">
					<xsl:with-param name="typename" select="$btype" />
				</xsl:call-template>
			</xsl:variable>
			<a>
				<xsl:attribute name="href">
					<xsl:call-template name="GetLinkTargetHtml">
						<xsl:with-param name="type" select="$escaped-type" />
						<xsl:with-param name="cref" select="concat ('T:', $escaped-type)" />
					</xsl:call-template>
				</xsl:attribute>
	
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T" select="$btype"/>
					<xsl:with-param name="wrt" select="$wrt"/>
				</xsl:call-template>
			</a>
		</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetArraySuffix">
		<xsl:param name="type" />

		<xsl:if test="contains ($type, ']') and 
				']' = substring ($type, string-length ($type), 1)">
			<xsl:variable name="start">
				<xsl:call-template name="GetArraySuffixStart">
					<xsl:with-param name="type" select="$type" />
					<xsl:with-param name="i" select="string-length ($type) - 1" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:value-of select="substring ($type, $start)" />
		</xsl:if>
	</xsl:template>

	<xsl:template name="GetArraySuffixStart">
		<xsl:param name="type" />
		<xsl:param name="i" />

		<xsl:choose>
			<xsl:when test="substring ($type, $i, 1) = '['">
				<xsl:value-of select="$i" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="GetArraySuffixStart">
					<xsl:with-param name="type" select="$type" />
					<xsl:with-param name="i" select="$i - 1" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetTypeDisplayName">
		<xsl:param name="T"/>
		<xsl:param name="wrt"/>

				<!-- use C#-style names -->
				<xsl:choose>
					<xsl:when test="$T='System.Object'">object</xsl:when>
					<xsl:when test="$T='System.Boolean'">bool</xsl:when>
					<xsl:when test="$T='System.Byte'">byte</xsl:when>
					<xsl:when test="$T='System.Char'">char</xsl:when>
					<xsl:when test="$T='System.Decimal'">decimal</xsl:when>
					<xsl:when test="$T='System.Double'">double</xsl:when>
					<xsl:when test="$T='System.Int16'">short</xsl:when>
					<xsl:when test="$T='System.Int32'">int</xsl:when>
					<xsl:when test="$T='System.Int64'">long</xsl:when>
					<xsl:when test="$T='System.SByte'">sbyte</xsl:when>
					<xsl:when test="$T='System.Single'">float</xsl:when>
					<xsl:when test="$T='System.String'">string</xsl:when>
					<xsl:when test="$T='System.UInt16'">ushort</xsl:when>
					<xsl:when test="$T='System.UInt32'">uint</xsl:when>
					<xsl:when test="$T='System.UInt64'">ulong</xsl:when>
					<xsl:when test="$T='System.Void'">void</xsl:when>

					<xsl:when test="contains($T, '&lt;')">
						<xsl:call-template name="GetTypeDisplayName">
							<xsl:with-param name="T" select="substring-before ($T, '&lt;')" />
							<xsl:with-param name="wrt" select="$wrt" />
						</xsl:call-template>
						<xsl:text>&lt;</xsl:text>
						<xsl:call-template name="GetMemberArgList">
							<xsl:with-param name="arglist" select="substring-after ($T, '&lt;')" />
							<xsl:with-param name="wrt" select="$wrt" />
						</xsl:call-template>
						<!-- don't need to append &gt; as GetMemberArgList (eventually) appends it -->
					</xsl:when>
	
					<!-- if the type is in the wrt namespace, omit the namespace name -->
					<xsl:when test="not($wrt='') and starts-with($T, concat($wrt,'.')) and not(contains(substring-after($T,concat($wrt,'.')), '.'))">
						<xsl:value-of select="translate (substring-after($T,concat($wrt,'.')), '+', '.')"/>
					</xsl:when>
	
					<!-- if the type is in the System namespace, omit the namespace name -->
					<xsl:when test="starts-with($T, 'System.') and not(contains(substring-after($T, 'System.'), '.'))">
						<xsl:value-of select="translate (substring-after($T,'System.'), '+', '.')"/>
					</xsl:when>
	
					<!-- if the type is in the System.Collections namespace, omit the namespace name -->
					<xsl:when test="starts-with($T, 'System.Collections.') and not(contains(substring-after($T, 'System.Collections.'), '.'))">
						<xsl:value-of select="translate (substring-after($T,'System.Collections.'), '+', '.')"/>
					</xsl:when>

					<!-- if the type is in the System.Collections.Generic namespace, omit the namespace name -->
					<xsl:when test="starts-with($T, 'System.Collections.Generic.') and not(contains(substring-after($T, 'System.Collections.Generic.'), '.'))">
						<xsl:value-of select="translate (substring-after($T,'System.Collections.Generic.'), '+', '.')"/>
					</xsl:when>

					<xsl:otherwise>
						<xsl:value-of select="translate ($T, '+', '.')" />
					</xsl:otherwise>
				</xsl:choose>
	</xsl:template>

	<xsl:template name="GetMemberDisplayName">
		<xsl:param name="memberName" />
		<xsl:param name="isproperty" select="false()" />

		<xsl:choose>
			<xsl:when test="contains($memberName, '.')">
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T">
						<xsl:call-template name="GetTypeName">
							<xsl:with-param name="type" select="$memberName"/>
						</xsl:call-template>
					</xsl:with-param>
					<xsl:with-param name="wrt" select="''" />
				</xsl:call-template>
				<xsl:text>.</xsl:text>
				<xsl:call-template name="GetMemberName">
					<xsl:with-param name="type" select="$memberName" />
					<xsl:with-param name="isproperty" select="$isproperty"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$memberName" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="ToBrackets">
		<xsl:param name="s" />
		<xsl:value-of select="translate (translate ($s, '{', '&lt;'), '}', '&gt;')" />
	</xsl:template>

	<xsl:template name="ToBraces">
		<xsl:param name="s" />
		<xsl:value-of select="translate (translate ($s, '&lt;', '{'), '&gt;', '}')" />
	</xsl:template>
	
	<xsl:template name="memberlinkprefix">
		<xsl:param name="member" />
		<xsl:choose>
			<xsl:when test="$member/MemberType='Constructor'">C</xsl:when>
			<xsl:when test="$member/MemberType='Method'">M</xsl:when>
			<xsl:when test="$member/MemberType='Property'">P</xsl:when>
			<xsl:when test="$member/MemberType='Field'">F</xsl:when>
			<xsl:when test="$member/MemberType='Event'">E</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="makememberlink">
		<xsl:param name="cref"/>

		<xsl:variable name="bcref">
			<xsl:call-template name="ToBrackets">
				<xsl:with-param name="s" select="$cref" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="fullname">
			<xsl:choose>
				<xsl:when test="starts-with($bcref, 'C:') or starts-with($bcref, 'T:')">
					<xsl:choose>
						<xsl:when test="contains($bcref, '(')">
							<xsl:value-of select="substring (substring-before ($bcref, '('), 3)" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($bcref, 3)" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="GetTypeName">
						<xsl:with-param name="type" select="substring($bcref, 3)"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="memberName">
			<xsl:choose>
				<xsl:when test="starts-with($bcref, 'T:')" />
				<xsl:when test="starts-with($bcref, 'C:') and not(contains($bcref, '('))" />
				<xsl:when test="starts-with($bcref, 'C:') and contains($bcref, '(')">
					<xsl:text>(</xsl:text>
					<xsl:call-template name="GetMemberArgList">
						<xsl:with-param name="arglist" select="substring-before(substring-after($bcref, '('), ')')" />
						<xsl:with-param name="wrt" select="$TypeNamespace" />
					</xsl:call-template>
					<xsl:text>)</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>.</xsl:text>
					<xsl:call-template name="GetMemberName">
						<xsl:with-param name="type" select="substring($bcref, 3)" />
						<xsl:with-param name="wrt" select="$fullname"/>
						<xsl:with-param name="isproperty" select="starts-with($bcref, 'P:')"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="escaped-type">
			<xsl:call-template name="GetEscapedTypeName">
				<xsl:with-param name="typename">
					<xsl:call-template name="ToBrackets">
						<xsl:with-param name="s" select="$fullname" />
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="displayname">
			<xsl:call-template name="GetTypeDisplayName">
				<xsl:with-param name="T" select="$fullname" />
				<xsl:with-param name="wrt" select="$TypeNamespace"/>
			</xsl:call-template>
		</xsl:variable>
		<a>
			<xsl:attribute name="href">
				<xsl:call-template name="GetLinkTargetHtml">
					<xsl:with-param name="type" select="$escaped-type" />
					<xsl:with-param name="cref">
						<xsl:call-template name="ToBraces">
							<xsl:with-param name="s" select="$cref" />
						</xsl:call-template>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:attribute>
			<xsl:value-of select="translate (concat($displayname, $memberName), '+', '.')" />
		</a>
	</xsl:template>

	<xsl:template name="GetTypeName">
		<xsl:param name="type" />
		<xsl:variable name="prefix" select="substring-before($type, '.')" />
		<xsl:variable name="suffix" select="substring-after($type, '.')" />
		<xsl:choose>
			<xsl:when test="contains($type, '(')">
				<xsl:call-template name="GetTypeName">
					<xsl:with-param name="type" select="substring-before($type, '(')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="not(contains($suffix, '.'))">
				<xsl:value-of select="$prefix" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$prefix" />
				<xsl:text>.</xsl:text>
				<xsl:call-template name="GetTypeName">
					<xsl:with-param name="type" select="$suffix" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetMemberName">
		<xsl:param name="type" />
		<xsl:param name="isproperty" select="0"/>
		<xsl:variable name="prefix" select="substring-before($type, '.')" />
		<xsl:variable name="suffix" select="substring-after($type, '.')" />
		<xsl:choose>
			<xsl:when test="contains($type, '(')">
				<xsl:call-template name="GetMemberName">
					<xsl:with-param name="type" select="substring-before($type, '(')" />
				</xsl:call-template>
				<xsl:text>(</xsl:text>
				<xsl:call-template name="GetMemberArgList">
					<xsl:with-param name="arglist" select="substring-before(substring-after($type, '('), ')')" />
					<xsl:with-param name="wrt" select="$TypeNamespace" />
				</xsl:call-template>
				<xsl:text>)</xsl:text>
			</xsl:when>
			<xsl:when test="not(contains($suffix, '.'))">
				<xsl:value-of select="$suffix" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="GetMemberName">
					<xsl:with-param name="type" select="$suffix" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetMemberArgList">
		<xsl:param name="arglist" />
		<xsl:param name="wrt" select="''"/>

		<xsl:variable name="_arglist">
			<xsl:choose>
				<xsl:when test="starts-with ($arglist, ',')">
					<xsl:value-of select="substring-after ($arglist, ',')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$arglist" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:if test="starts-with ($arglist, ',')">
			<xsl:text>, </xsl:text>
		</xsl:if>

		<xsl:variable name="c"  select="substring-before ($_arglist, ',')" />
		<xsl:variable name="lt" select="substring-before ($_arglist, '&lt;')" />
		<xsl:variable name="gt" select="substring-before ($_arglist, '&gt;')" />

		<xsl:choose>
			<!-- Need to insert ',' between type arguments -->
			<xsl:when test="
					($c != '' and $lt != '' and $gt != '' and 
					 string-length ($c) &lt; string-length ($lt) and 
					 string-length ($c) &lt; string-length ($gt)) or
					($c != '' and $lt != '' and $gt = '' and
					 string-length ($c) &lt; string-length ($lt)) or
					($c != '' and $lt = '' and $gt != '' and
					 string-length ($c) &lt; string-length ($gt)) or
					($c != '' and $lt = '' and $gt = '')">
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T" select="$c"/>
					<xsl:with-param name="wrt" select="$wrt"/>
				</xsl:call-template>
				<xsl:text>, </xsl:text>
				<xsl:call-template name="GetMemberArgList">
					<xsl:with-param name="arglist" select="substring-after($_arglist, ',')" />
					<xsl:with-param name="wrt" select="$wrt" />
				</xsl:call-template>
			</xsl:when>

			<!-- start of nested type argument list < -->
			<xsl:when test="
					($c != '' and $lt != '' and $gt != '' and 
					 string-length ($lt) &lt; string-length ($c) and 
					 string-length ($lt) &lt; string-length ($gt)) or
					($c != '' and $lt != '' and $gt = '' and
					 string-length ($lt) &lt; string-length ($c)) or
					($c = '' and $lt != '' and $gt != '' and
					 string-length ($lt) &lt; string-length ($gt))">
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T" select="$lt"/>
					<xsl:with-param name="wrt" select="$wrt"/>
				</xsl:call-template>
				<xsl:text>&lt;</xsl:text>
				<xsl:call-template name="GetMemberArgList">
					<xsl:with-param name="arglist" select="substring-after($_arglist, '&lt;')" />
					<xsl:with-param name="wrt" select="$wrt" />
				</xsl:call-template>
			</xsl:when>

			<!-- end of (nested?) type argument list > -->
			<xsl:when test="
					($c != '' and $lt != '' and $gt != '' and 
					 string-length ($gt) &lt; string-length ($c) and 
					 string-length ($gt) &lt; string-length ($lt)) or
					($c != '' and $lt = '' and $gt = '' and
					 string-length ($gt) &lt; string-length ($c)) or
					($c = '' and $lt != '' and $gt != '' and
					 string-length ($gt) &lt; string-length ($lt)) or
					($c = '' and $lt = '' and $gt != '')">
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T" select="$gt"/>
					<xsl:with-param name="wrt" select="$wrt"/>
				</xsl:call-template>
				<xsl:text>&gt;</xsl:text>
				<xsl:call-template name="GetMemberArgList">
					<xsl:with-param name="arglist" select="substring-after($_arglist, '&gt;')" />
					<xsl:with-param name="wrt" select="$wrt" />
				</xsl:call-template>
			</xsl:when>

			<!-- nothing left to do -->
			<xsl:otherwise>
				<xsl:call-template name="GetTypeDisplayName">
					<xsl:with-param name="T" select="$_arglist"/>
					<xsl:with-param name="wrt" select="$wrt"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<!-- Transforms the contents of the selected node containing a cref into a hyperlink. -->
	<xsl:template match="*|@*" mode="cref">
		<xsl:call-template name="makememberlink">
			<xsl:with-param name="cref" select="."/>
		</xsl:call-template>
		<!--
		<a>
			<xsl:attribute name="href"><xsl:value-of select="."/></xsl:attribute>
			<xsl:value-of select="substring-after(., ':')"/></a>
			-->
	</xsl:template>

	<xsl:template name="membertypeplural">
		<xsl:param name="name"/>
		<xsl:choose>
		<xsl:when test="$name='ExtensionMethod'">Extension Methods</xsl:when>
		<xsl:when test="$name='Constructor'">Constructors</xsl:when>
		<xsl:when test="$name='Property'">Properties</xsl:when>
		<xsl:when test="$name='Method'">Methods</xsl:when>
		<xsl:when test="$name='Field'">Fields</xsl:when>
		<xsl:when test="$name='Event'">Events</xsl:when>
		<xsl:when test="$name='Operator'">Operators</xsl:when>
		<xsl:when test="$name='Explicit'">Explicitly Implemented Interface Members</xsl:when>
		</xsl:choose>
	</xsl:template>
	<xsl:template name="membertypeplurallc">
		<xsl:param name="name"/>
		<xsl:choose>
		<xsl:when test="$name='ExtensionMethod'">extension methods</xsl:when>
		<xsl:when test="$name='Constructor'">constructors</xsl:when>
		<xsl:when test="$name='Property'">properties</xsl:when>
		<xsl:when test="$name='Method'">methods</xsl:when>
		<xsl:when test="$name='Field'">fields</xsl:when>
		<xsl:when test="$name='Event'">events</xsl:when>
		<xsl:when test="$name='Operator'">operators</xsl:when>
		<xsl:when test="$name='Explicit'">explicitly implemented interface members</xsl:when>
		</xsl:choose>
	</xsl:template>
	<xsl:template name="gettypetype">
		<xsl:variable name="sig" select="concat(' ', TypeSignature[@Language='C#']/@Value, ' ')"/>
		<xsl:choose>
		<xsl:when test="contains($sig,'class')">Class</xsl:when>
		<xsl:when test="contains($sig,'enum')">Enumeration</xsl:when>
		<xsl:when test="contains($sig,'struct')">Structure</xsl:when>
		<xsl:when test="contains($sig,'delegate')">Delegate</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- Ensures that the resuting node is not surrounded by a para tag. -->
	<xsl:template match="*|@*" mode="editlink">
		<xsl:call-template name="CreateEditLink">
			<xsl:with-param name="e" select="." />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="*" mode="notoppara">
		<xsl:choose>
		<xsl:when test="starts-with (string(.), 'To be added')">
			<span class="NotEntered">Documentation for this section has not yet been entered.</span>
		</xsl:when>
		<xsl:when test="count(*) = 1 and count(para)=1">
			<xsl:apply-templates select="para/node()"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:apply-templates select="."/>
		</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="para">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>

	<xsl:template match="paramref">
		<i><xsl:value-of select="@name"/>
				<xsl:apply-templates/>
		</i>
	</xsl:template>

	<xsl:template match="typeparamref">
		<i><xsl:value-of select="@name"/>
				<xsl:apply-templates/>
		</i>
	</xsl:template>

	<xsl:template match="block[@type='note']">
		<div>
		<i>Note: </i>
				<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="block[@type='behaviors']">
		<h5 class="Subsection">Operation</h5>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="block[@type='overrides']">
		<h5 class="Subsection">Note to Inheritors</h5>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="block[@type='usage']">
		<h5 class="Subsection">Usage</h5>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="c">
		<tt>
			<xsl:apply-templates/>	
		</tt>
	</xsl:template>
	<xsl:template match="c//para">
		<xsl:apply-templates/><br/>	
	</xsl:template>
	
	<xsl:template match="code">
		<xsl:call-template name="CreateCodeBlock">
			<xsl:with-param name="language" select="@lang" />
			<xsl:with-param name="content" select="string(descendant-or-self::text())" />
		</xsl:call-template>
	</xsl:template>
	<xsl:template match="img">
	  <p>
		<img>
		  <xsl:attribute name="src">
			<!-- we recognize two types of images:
				   - those with src attribute that reference directly an external image
				   - those with a href attributes which are internally available as part of the doc bundle
			-->
			<xsl:choose>
			  <xsl:when test="count(@src)&gt;0">
				<xsl:value-of select="@src" />
			  </xsl:when>
			  <xsl:when test="count(@href)&gt;0">
				<xsl:value-of select="concat('source-id:', $source-id, ':', @href)" />
			  </xsl:when>
			</xsl:choose>
		  </xsl:attribute>
		  <xsl:attribute name="class">
			<xsl:choose>
			  <xsl:when test="count(@class)&gt;0">
				<xsl:value-of select="@class" />
			  </xsl:when>
			  <xsl:otherwise>picture</xsl:otherwise>
			</xsl:choose>
		  </xsl:attribute>
		</img>
	  </p>
	</xsl:template>

	<xsl:template match="onequarter">¼</xsl:template>
	<xsl:template match="pi">π</xsl:template>
	<xsl:template match="theta">θ</xsl:template>
	<xsl:template match="leq">≤</xsl:template>
	<xsl:template match="geq">≥</xsl:template>
	<xsl:template match="subscript">
		<sub><xsl:value-of select="@term"/></sub>
	</xsl:template>
	<xsl:template match="superscript">
		<sup><xsl:value-of select="@term"/></sup>
	</xsl:template>

	<!-- tabular data
		example:

			<list type="table">
				<listheader>
					<term>First Col Header</term>
					<description>Second Col Header</description>
					<description>Third Col Header</description>
				</listheader>
				<item>
					<term>First Row First Col</term>
					<description>First Row Second Col</description>
					<description>First Row Third Col</description>
				</item>
				<item>
					<term>Second Row First Col</term>
					<description>Second Row Second Col</description>
					<description>Second Row Third Col</description>
				</item>
			</list>
	-->

	<xsl:template match="list[@type='table']">
		<xsl:call-template name="CreateListTable">
		<xsl:with-param name="header">
			<th><xsl:apply-templates select="listheader/term" mode="notoppara"/></th>
			<xsl:for-each select="listheader/description">
				<th><xsl:apply-templates mode="notoppara"/></th>
			</xsl:for-each>
		</xsl:with-param>

		<xsl:with-param name="content">
		<xsl:for-each select="item">
			<tr valign="top">
			<td>
				<xsl:apply-templates select="term" mode="notoppara"/>
			</td>
			<xsl:for-each select="description">
				<td>
					<xsl:apply-templates mode="notoppara"/>
				</td>
			</xsl:for-each>
			</tr>
		</xsl:for-each>
		</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="list[@type='bullet']">
		<ul>
			<xsl:for-each select="item">
				<li>
					<xsl:apply-templates select="term" mode="notoppara"/>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>
	<xsl:template match="list[@type='number']">
		<ol>
			<xsl:for-each select="item">
				<li>
					<xsl:apply-templates select="term" mode="notoppara"/>
				</li>
			</xsl:for-each>
		</ol>
	</xsl:template>

	<xsl:template match="list">
		[<i>The '<xsl:value-of select="@type"/>' type of list has not been implemented in the ECMA stylesheet.</i>]
		
		<xsl:message>
		[<i>The '<xsl:value-of select="@type"/>' type of list has not been implemented in the ECMA stylesheet.</i>]
		</xsl:message>
	</xsl:template>

	<xsl:template match="see[@cref]">
		<xsl:choose>
		<xsl:when test="not(substring-after(@cref, 'T:')='')">
			<xsl:call-template name="maketypelink">
				<xsl:with-param name="type" select="normalize-space (@cref)"/>
			</xsl:call-template>
		</xsl:when>
		<xsl:when test="not(substring-after(@cref, 'N:')='')">
			<xsl:call-template name="makenamespacelink">
				<xsl:with-param name="cref" select="normalize-space (@cref)"/>
			</xsl:call-template>
		</xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="makememberlink">
				<xsl:with-param name="cref" select="normalize-space (@cref)"/>
			</xsl:call-template>
		</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@langword]">
		<tt><xsl:value-of select="@langword"/></tt>
	</xsl:template>
	
	<xsl:template name="GetInheritedMembers">
		<xsl:param name="declaringtype"/>
		<xsl:param name="generictypereplacements"/>
		<xsl:param name="listmembertype"/>
		<xsl:param name="showprotected"/>
		<xsl:param name="overloads-mode" select="false()" />
		<xsl:param name="showstatic" select='1'/>

		<xsl:choose>
		<xsl:when test="$listmembertype='ExtensionMethod' and $showprotected=false()">
			<xsl:for-each select="$declaringtype/Members/Member[MemberType=$listmembertype]">
				<Members Name="Link/@Type" FullName="Link/@Type">
					<Member MemberName="{@MemberName}">
						<xsl:attribute name="ExplicitMemberName">
							<xsl:call-template name="GetMemberNameWithoutGenericTypes">
								<xsl:with-param name="m" select="@MemberName" />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="TypeParameters">
							<xsl:call-template name="GetTypeParameterNames">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="Parameters">
							<xsl:call-template name="GetParameterTypes">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:copy-of select="./*" />
					</Member>
				</Members>
			</xsl:for-each>
		</xsl:when>
		<xsl:otherwise>
		<Members Name="{$declaringtype/@Name}" FullName="{$declaringtype/@FullName}">
		
		<xsl:copy-of select="$generictypereplacements"/>

		<!-- Get all members in this type that are of listmembertype and are either
			protected or not protected according to showprotected. -->
		<xsl:choose>
			<xsl:when test="$listmembertype = 'Explicit'">
				<xsl:for-each select="$declaringtype/Members/Member
						[MemberType != 'Constructor']
						[contains (@MemberName, '.')]">
					<Member MemberName="{@MemberName}">
						<xsl:attribute name="ExplicitMemberName">
							<xsl:call-template name="GetMemberName">
								<xsl:with-param name="type" select="@MemberName" />
								<xsl:with-param name="isproperty" select="$listmembertype = 'Property'"/>
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="TypeParameters">
							<xsl:call-template name="GetTypeParameterNames">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="Parameters">
							<xsl:call-template name="GetParameterTypes">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:copy-of select="./*" />
					</Member>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="$declaringtype/Members/Member
					[(MemberType=$listmembertype or ($listmembertype='Operator' and MemberType='Method'))]
					[(not($overloads-mode) or @MemberName=$index or 
						($index='Conversion' and (@MemberName='op_Implicit' or @MemberName='op_Explicit'))) ]
					[$showprotected=starts-with(MemberSignature[@Language='C#']/@Value, 'protected ')]
					[($listmembertype='Method' and not(starts-with(@MemberName,'op_')))
						or ($listmembertype='Operator' and starts-with(@MemberName,'op_'))
						or (not($listmembertype='Method') and not($listmembertype='Operator'))]
					[$showstatic or not(contains(MemberSignature[@Language='C#']/@Value,' static '))]
					[$listmembertype = 'Constructor' or not(contains(@MemberName, '.'))]
					">
					<Member MemberName="{@MemberName}">
						<xsl:attribute name="ExplicitMemberName">
							<xsl:call-template name="GetMemberNameWithoutGenericTypes">
								<xsl:with-param name="m" select="@MemberName" />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="TypeParameters">
							<xsl:call-template name="GetTypeParameterNames">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:attribute name="Parameters">
							<xsl:call-template name="GetParameterTypes">
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:copy-of select="./*" />
					</Member>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>

		<Docs>
			<xsl:copy-of select="$declaringtype/Docs/typeparam" />
		</Docs>
			
		</Members>
		</xsl:otherwise>
		</xsl:choose>

		<xsl:if test="not($listmembertype='Constructor') and count($declaringtype/Base/BaseTypeName)=1">
			<xsl:variable name="basedocsfile">
				<xsl:call-template name="GetLinkTarget">
					<xsl:with-param name="type">
						<xsl:call-template name="GetEscapedTypeName">
							<xsl:with-param name="typename" select="$declaringtype/Base/BaseTypeName" />
						</xsl:call-template>
					</xsl:with-param>
					<xsl:with-param name="cref">
					</xsl:with-param>
					<xsl:with-param name="local-suffix" />
					<xsl:with-param name="remote"/>
					<xsl:with-param name="xmltarget" select='1'/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:if test="not(string($basedocsfile) = '')">
				<xsl:call-template name="GetInheritedMembers">
					<xsl:with-param name="listmembertype" select="$listmembertype"/>
					<xsl:with-param name="showprotected" select="$showprotected"/>
					<xsl:with-param name="declaringtype" select="document(string($basedocsfile),.)/Type"/>
					<xsl:with-param name="generictypereplacements" select="$declaringtype/Base/BaseTypeArguments/*"/>
					<xsl:with-param name="showstatic" select='0'/>
				</xsl:call-template>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GetMemberNameWithoutGenericTypes">
		<xsl:param name="m" />
		<xsl:choose>
			<xsl:when test="contains ($m, '&lt;')">
				<xsl:value-of select="substring-before ($m, '&lt;')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$m" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template name="GetTypeParameterNames">
		<xsl:param name="member" />

		<xsl:for-each select="$member/TypeParameters/TypeParameter">
			<xsl:if test="not(position()=1)">, </xsl:if>
			<xsl:value-of select="@Name" />
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="GetParameterTypes">
		<xsl:param name="member" />

		<xsl:for-each select="$member/Parameters/Parameter">
			<xsl:if test="not(position()=1)">, </xsl:if>
			<xsl:value-of select="@Type" />
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="ListAllMembers">
		<xsl:param name="html-anchor" select="false()" />

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Constructor'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Constructor'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Field'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Field'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Property'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Property'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Method'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Method'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Event'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Event'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Operator'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'Explicit'"/>
			<xsl:with-param name="showprotected" select="true()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>

		<xsl:call-template name="ListMembers">
			<xsl:with-param name="listmembertype" select="'ExtensionMethod'"/>
			<xsl:with-param name="showprotected" select="false()"/>
			<xsl:with-param name="html-anchor" select="$html-anchor" />
		</xsl:call-template>
	</xsl:template>

	<!-- Lists the members in the current Type node.
		 Only lists members of type listmembertype.
		 Displays the signature in siglanguage.
		 showprotected = true() or false()
	-->
	<xsl:template name="ListMembers">
		<xsl:param name="listmembertype"/>
		<xsl:param name="showprotected"/>
		<xsl:param name="overloads-mode" select="false()" />
		<xsl:param name="html-anchor" select="false()" />

		<!-- get name and namespace of current type -->
		<xsl:variable name="TypeFullName" select="@FullName"/>
		<xsl:variable name="TypeName" select="@Name"/>		
		<xsl:variable name="TypeNamespace" select="substring-before(@FullName, concat('.',@Name))"/>
		
		<xsl:variable name="MEMBERS-rtf">
			<xsl:call-template name="GetInheritedMembers">
				<xsl:with-param name="listmembertype" select="$listmembertype"/>
				<xsl:with-param name="showprotected" select="$showprotected"/>
				<xsl:with-param name="declaringtype" select="."/>
				<xsl:with-param name="overloads-mode" select="$overloads-mode" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="MEMBERS" select="msxsl:node-set($MEMBERS-rtf)" />
		
		<!--
		<xsl:variable name="MEMBERS" select="
			$ALLMEMBERS/Member
			[(MemberType=$listmembertype or ($listmembertype='Operator' and MemberType='Method'))]
			[$showprotected=contains(MemberSignature[@Language='C#']/@Value,'protected')]
			[($listmembertype='Method' and not(starts-with(@MemberName,'op_')))
				or ($listmembertype='Operator' and starts-with(@MemberName,'op_'))
				or (not($listmembertype='Method') and not($listmembertype='Operator'))]
			"/>
		-->
		
		<!-- if there aren't any, skip this -->
		<xsl:if test="count($MEMBERS//Member)">

		<xsl:variable name="SectionName">
			<xsl:if test="$listmembertype != 'Explicit' and $listmembertype != 'ExtensionMethod'">
				<xsl:if test="$showprotected">Protected </xsl:if>
				<xsl:if test="not($showprotected)">Public </xsl:if>
			</xsl:if>
			<xsl:call-template name="membertypeplural"><xsl:with-param name="name" select="$listmembertype"/></xsl:call-template>
		</xsl:variable>

		<!-- header -->
		<xsl:call-template name="CreateH2Section">
			<xsl:with-param name="name" select="$SectionName" />
			<xsl:with-param name="child-id" select="$SectionName" />
			<xsl:with-param name="content">
				<div class="SubsectionBox">
				<xsl:call-template name="CreateMembersTable">
				<xsl:with-param name="content">

				<xsl:for-each select="$MEMBERS/Members/Member">
					<!--<xsl:sort select="contains(MemberSignature[@Language='C#']/@Value,' static ')" data-type="text"/>-->
					<xsl:sort select="@MemberName = 'op_Implicit' or @MemberName = 'op_Explicit'"/>
					<xsl:sort select="@ExplicitMemberName" data-type="text"/>
					<xsl:sort select="count(TypeParameters/TypeParameter)"/>
					<xsl:sort select="@TypeParameters"/>
					<xsl:sort select="count(Parameters/Parameter)"/>
					<xsl:sort select="@Parameters"/>
					
					<xsl:variable name="local-id">
						<xsl:choose>
							<xsl:when test="count(Link) = 1">
								<xsl:value-of select="Link/@Member" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="GetLinkId" >
									<xsl:with-param name="type" select="parent::Members" />
									<xsl:with-param name="member" select="." />
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>

					<xsl:variable name="linkfile">
						<xsl:if test="not(parent::Members/@FullName = $TypeFullName)">
							<xsl:call-template name="GetLinkTargetHtml">
								<xsl:with-param name="type">
									<xsl:choose>
										<xsl:when test="count(Link) = 1">
											<xsl:value-of select="Link/@Type"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:call-template name="GetEscapedTypeName">
												<xsl:with-param name="typename" select="parent::Members/@FullName" />
											</xsl:call-template>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:with-param>
								<xsl:with-param name="cref" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>

					<xsl:variable name="linkid">
						<xsl:if test="$html-anchor">
							<xsl:value-of select="$linkfile" />
							<xsl:text>#</xsl:text>
						</xsl:if>
						<xsl:value-of select="$local-id" />
					</xsl:variable>
					
					<xsl:variable name="isinherited">
						<xsl:if test="$listmembertype != 'ExtensionMethod' and not(parent::Members/@FullName = $TypeFullName)">
							<xsl:text> (</xsl:text>
							<i>
							<xsl:text>Inherited from </xsl:text>
							<xsl:call-template name="maketypelink">
								<xsl:with-param name="type" select="parent::Members/@FullName"/>
								<xsl:with-param name="wrt" select="$TypeNamespace"/>
							</xsl:call-template>
							<xsl:text>.</xsl:text>
							</i>
							<xsl:text>)</xsl:text>
						</xsl:if>
					</xsl:variable>

					<tr valign="top">
						<td>
							<!-- random info -->

							<!-- check if it has get and set accessors -->
							<xsl:if test="MemberType='Property' and not(contains(MemberSignature[@Language='C#']/@Value, 'set;'))">
								<xsl:text>[read-only]</xsl:text>
							</xsl:if>
							<xsl:if test="MemberType='Property' and not(contains(MemberSignature[@Language='C#']/@Value, 'get;'))">
								<xsl:text>[write-only]</xsl:text>
							</xsl:if>

							<xsl:if test="contains(MemberSignature[@Language='C#']/@Value,'this[')">
								<div><i>default property</i></div>
							</xsl:if>

							<div>
							<xsl:call-template name="getmodifiers">
								<xsl:with-param name="sig" select="MemberSignature[@Language='C#']/@Value"/>
								<xsl:with-param name="protection" select="false()"/>
								<xsl:with-param name="inheritance" select="true()"/>
								<xsl:with-param name="extra" select="false()"/>
							</xsl:call-template>
							</div>
						</td>

					<xsl:choose>
						<!-- constructor listing -->
						<xsl:when test="MemberType='Constructor'">
							<!-- link to constructor page -->
							<td>
							<div>
							<b>
							<a href="{$linkid}">
								<xsl:call-template name="GetConstructorName">
									<xsl:with-param name="type" select="parent::Members" />
									<xsl:with-param name="ctor" select="." />
								</xsl:call-template>
							</a>
							</b>

							<!-- argument list -->
							<xsl:value-of select="'('"/>
								<xsl:for-each select="Parameters/Parameter">
									<xsl:if test="not(position()=1)">, </xsl:if>
									
									<xsl:call-template name="ShowParameter">
										<xsl:with-param name="Param" select="."/>
										<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
										<xsl:with-param name="prototype" select="true()"/>
									</xsl:call-template>
								</xsl:for-each>
							<xsl:value-of select="')'"/>
							</div>

							<!-- TODO: $implemented? -->

							</td>
						</xsl:when>

						<xsl:when test="$listmembertype = 'Explicit'">
							<td>
								<a href="{$linkid}">
									<b>
										<xsl:call-template name="GetMemberDisplayName">
											<xsl:with-param name="memberName" select="@MemberName" />
											<xsl:with-param name="isproperty" select="MemberType='Property'" />
										</xsl:call-template>
									</b>
								</a>
							</td>
						</xsl:when>

						<!-- field, property and event listing -->
						<xsl:when test="MemberType='Field' or MemberType='Property' or MemberType='Event'">
							<td>

							<!-- link to member page -->
							<b>
							<a href="{$linkid}">
								<xsl:call-template name="GetMemberDisplayName">
									<xsl:with-param name="memberName" select="@MemberName" />
									<xsl:with-param name="isproperty" select="MemberType='Property'" />
								</xsl:call-template>
							</a>
							</b>

							<!-- argument list for accessors -->
							<xsl:if test="Parameters/Parameter">
							<xsl:value-of select="'('"/>
								<xsl:for-each select="Parameters/Parameter">
									<xsl:if test="not(position()=1)">, </xsl:if>
									
									<xsl:call-template name="ShowParameter">
										<xsl:with-param name="Param" select="."/>
										<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
										<xsl:with-param name="prototype" select="true()"/>
									</xsl:call-template>

								</xsl:for-each>
							<xsl:value-of select="')'"/>
							</xsl:if>

							</td>
						</xsl:when>

						<!-- method listing -->
						<xsl:when test="$listmembertype='Method' or $listmembertype = 'ExtensionMethod'">
							<td colspan="2">

							<!-- link to method page -->
							<b>
							<a href="{$linkid}">
								<xsl:call-template name="GetMemberDisplayName">
									<xsl:with-param name="memberName" select="@MemberName" />
									<xsl:with-param name="isproperty" select="MemberType='Property'" />
								</xsl:call-template>
							</a>
							</b>

							<!-- argument list -->
							<xsl:value-of select="'('"/>
								<xsl:for-each select="Parameters/Parameter">
									<xsl:if test="not(position()=1)">, </xsl:if>
									
									<xsl:call-template name="ShowParameter">
										<xsl:with-param name="Param" select="."/>
										<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
										<xsl:with-param name="prototype" select="true()"/>
									</xsl:call-template>

								</xsl:for-each>
							<xsl:value-of select="')'"/>

							<!-- return type -->
							<xsl:if test="not(ReturnValue/ReturnType='System.Void')">
								<nobr>
								<xsl:text> : </xsl:text>
								<xsl:apply-templates select="ReturnValue/ReturnType" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>
								</nobr>
							</xsl:if>

							<blockquote>
								<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
								<xsl:copy-of select="$isinherited"/>
							</blockquote>
							</td>
						</xsl:when>

						<xsl:when test="$listmembertype='Operator'">
							<td>

							<!-- link to operator page -->
							<xsl:choose>
							<xsl:when test="@MemberName='op_Implicit' or @MemberName='op_Explicit'">
								<b>
								<a href="{$linkid}">
									<xsl:text>Conversion</xsl:text>
									<xsl:choose>
									<xsl:when test="ReturnValue/ReturnType = //Type/@FullName">
										<xsl:text> From </xsl:text>
										<xsl:value-of select="Parameters/Parameter/@Type"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:text> to </xsl:text>
										<xsl:value-of select="ReturnValue/ReturnType"/>
									</xsl:otherwise>
									</xsl:choose>
								</a>
								</b>						

								<xsl:choose>
								<xsl:when test="@MemberName='op_Implicit'">
									<xsl:text>(Implicit)</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>(Explicit)</xsl:text>
								</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:when test="count(Parameters/Parameter)=1">
								<b>
								<a href="{$linkid}">
									<xsl:value-of select="substring-after(@MemberName, 'op_')"/>
								</a>
								</b>
							</xsl:when>
							<xsl:otherwise>
								<b>
								<a href="{$linkid}">
									<xsl:value-of select="substring-after(@MemberName, 'op_')"/>
								</a>
								</b>
								<xsl:value-of select="'('"/>
									<xsl:for-each select="Parameters/Parameter">
										<xsl:if test="not(position()=1)">, </xsl:if>
										
										<xsl:call-template name="ShowParameter">
											<xsl:with-param name="Param" select="."/>
											<xsl:with-param name="TypeNamespace" select="$TypeNamespace"/>
											<xsl:with-param name="prototype" select="true()"/>
										</xsl:call-template>
			
									</xsl:for-each>
								<xsl:value-of select="')'"/>
							</xsl:otherwise>
							</xsl:choose>
							</td>
						</xsl:when>
						
						<xsl:otherwise>
							<!-- Other types: just provide a link -->
							<td>
							<a href="{$linkid}">
								<xsl:call-template name="GetMemberDisplayName">
									<xsl:with-param name="memberName" select="@MemberName" />
									<xsl:with-param name="isproperty" select="MemberType='Property'" />
								</xsl:call-template>
							</a>
							</td>
						</xsl:otherwise>
					</xsl:choose>

					<xsl:if test="$listmembertype != 'Method' and $listmembertype != 'ExtensionMethod'">
						<td>
							<!-- description -->
							<xsl:if test="MemberType='Field' or MemberType = 'Property'">
								<i><xsl:apply-templates select="ReturnValue/ReturnType" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates></i>
								<xsl:if test="MemberValue"> (<xsl:value-of select="MemberValue"/>)</xsl:if>
								<xsl:text>. </xsl:text>
							</xsl:if>

							<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
							<xsl:copy-of select="$isinherited"/>
						</td>
					</xsl:if>
					
					</tr>
				</xsl:for-each>

				</xsl:with-param>
				</xsl:call-template>
				</div>
			</xsl:with-param>
		</xsl:call-template>

		</xsl:if>

	</xsl:template>

	<xsl:template name="GetLinkName">
		<xsl:param name="type"/>
		<xsl:param name="member"/>
		<xsl:call-template name="memberlinkprefix">
			<xsl:with-param name="member" select="$member"/>
		</xsl:call-template>
		<xsl:text>:</xsl:text>
		<xsl:call-template name="GetEscapedTypeName">
			<xsl:with-param name="typename" select="$type/@FullName" />
		</xsl:call-template>
		<xsl:if test="$member/MemberType != 'Constructor'">
			<xsl:text>.</xsl:text>
			<xsl:variable name="memberName">
				<xsl:call-template name="GetGenericName">
					<xsl:with-param name="membername" select="$member/@MemberName" />
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:call-template name="Replace">
				<xsl:with-param name="s">
					<xsl:call-template name="ToBraces">
						<xsl:with-param name="s" select="$memberName" />
					</xsl:call-template>
				</xsl:with-param>
				<xsl:with-param name="from">.</xsl:with-param>
				<xsl:with-param name="to">#</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GetGenericName">
		<xsl:param name="membername" />
		<xsl:param name="member" />
		<xsl:variable name="numgenargs" select="count($member/Docs/typeparam)" />
		<xsl:choose>
			<xsl:when test="$numgenargs = 0">
				<xsl:value-of select="$membername" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:if test="contains($membername, '&lt;')">
					<xsl:value-of select="substring-before ($membername, '&lt;')" />
				</xsl:if>
				<xsl:text>``</xsl:text>
				<xsl:value-of select="$numgenargs" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetEscapedTypeName">
		<xsl:param name="typename" />
		<xsl:variable name="base" select="substring-before ($typename, '&lt;')" />

		<xsl:choose>
			<xsl:when test="$base != ''">
				<xsl:value-of select="translate ($base, '+', '.')" />
				<xsl:text>`</xsl:text>
				<xsl:call-template name="GetGenericArgumentCount">
					<xsl:with-param name="arglist" select="substring-after ($typename, '&lt;')" />
					<xsl:with-param name="count">1</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise><xsl:value-of select="translate ($typename, '+', '.')" /></xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetGenericArgumentCount">
		<xsl:param name="arglist" />
		<xsl:param name="count" />

		<xsl:variable name="rest-rtf">
			<xsl:call-template name="SkipTypeArgument">
				<xsl:with-param name="s" select="$arglist" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="rest" select="string($rest-rtf)" />

		<xsl:choose>
			<xsl:when test="$arglist != '' and $rest = ''">
				<xsl:value-of select="$count" />
			</xsl:when>
			<xsl:when test="$arglist = '' and $rest = ''">
				<xsl:message terminate="yes">
!WTF? arglist=<xsl:value-of select="$arglist" />; rest=<xsl:value-of select="$rest" />
				</xsl:message>
			</xsl:when>
			<xsl:when test="starts-with ($rest, '>')">
				<xsl:value-of select="$count" />
				<xsl:call-template name="GetEscapedTypeName">
					<xsl:with-param name="typename" select="substring-after ($rest, '>')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="starts-with ($rest, ',')">
				<xsl:call-template name="GetGenericArgumentCount">
					<xsl:with-param name="arglist" select="substring-after ($rest, ',')" />
					<xsl:with-param name="count" select="$count+1" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:message terminate="yes">
!WTF 2? arglist=<xsl:value-of select="$arglist" />; rest=<xsl:value-of select="$rest" />
				</xsl:message>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="SkipTypeArgument">
		<xsl:param name="s" />

		<xsl:variable name="p-rtf">
			<xsl:call-template name="GetCLtGtPositions">
				<xsl:with-param name="s" select="$s" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="p" select="msxsl:node-set($p-rtf)"/>

		<xsl:choose>
			<!--
			Have to select between three `s' patterns:
			A,B>: need to return ",B>"
			Foo<A,B>>: Need to forward to SkipGenericArgument to eventually return ">"
			Foo<A,B>+C>: Need to forward to SkipGenericArgument to eventually return ">"
			-->
			<xsl:when test="starts-with ($s, '>')">
				<xsl:message terminate="yes">
SkipTypeArgument: invalid type substring '<xsl:value-of select="$s" />'
				</xsl:message>
			</xsl:when>
			<xsl:when test="$p/Comma/@Length > 0 and 
					($p/Lt/@Length = 0 or $p/Comma/@Length &lt; $p/Lt/@Length) and 
					($p/Gt/@Length > 0 and $p/Comma/@Length &lt; $p/Gt/@Length)">
				<xsl:text>,</xsl:text>
				<xsl:value-of select="substring-after ($s, ',')" />
			</xsl:when>
			<xsl:when test="$p/Lt/@Length > 0 and $p/Lt/@Length &lt; $p/Gt/@Length">
				<xsl:variable name="r">
					<xsl:call-template name="SkipGenericArgument">
						<xsl:with-param name="s" select="substring-after ($s, '&lt;')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="starts-with ($r, '>') or starts-with ($r, '+')">
						<xsl:value-of select="substring-after ($r, '&gt;')" />
					</xsl:when>
					<xsl:when test="starts-with ($r, ',')">
						<xsl:value-of select="$r" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:message>
! WTF3: s=<xsl:value-of select="$s" />; r=<xsl:value-of select="$r" />
						</xsl:message>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="$p/Gt/@Length > 0">
				<xsl:text>&gt;</xsl:text>
				<xsl:value-of select="substring-after ($s, '&gt;')" />
			</xsl:when>
			<xsl:otherwise><xsl:value-of select="$s" /></xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetCLtGtPositions">
		<xsl:param name="s" />

		<xsl:variable name="c"  select="substring-before ($s, ',')" />
		<xsl:variable name="lt" select="substring-before ($s, '&lt;')" />
		<xsl:variable name="gt" select="substring-before ($s, '&gt;')" />

			<Comma String="{$c}" Length="{string-length ($c)}" />
			<Lt String="{$lt}" Length="{string-length ($lt)}" />
			<Gt String="{$gt}" Length="{string-length ($gt)}" />
	</xsl:template>

	<!--
	when given 'Foo<A,Bar<Baz<C,D,E>>>>', returns '>'
	when given 'Bar<C>+Nested>', returns '>'
	when given 'Foo<A,Bar<Baz<C,D,E>>>,', returns ','
	(basically, it matches '<' to '>' and "skips" the intermediate type-name contents.
	  -->
	<xsl:template name="SkipGenericArgument">
		<xsl:param name="s" />

		<xsl:variable name="p-rtf">
			<xsl:call-template name="GetCLtGtPositions">
				<xsl:with-param name="s" select="$s" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="p" select="msxsl:node-set($p-rtf)" />

		<xsl:choose>
			<xsl:when test="starts-with ($s, '>')">
				<xsl:message terminate="yes">
SkipGenericArgument: invalid type substring '<xsl:value-of select="$s" />'
				</xsl:message>
			</xsl:when>
			<xsl:when test="$p/Lt/@Length > 0 and $p/Lt/@Length &lt; $p/Gt/@Length">
				<!-- within 'Foo<A...'; look for matching '>' -->
				<xsl:variable name="r">
					<xsl:call-template name="SkipGenericArgument">
						<xsl:with-param name="s" select="substring-after ($s, '&lt;')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:value-of select="substring-after ($r, '&gt;')" />
			</xsl:when>
			<xsl:when test="$p/Gt/@Length > 0">
				<!--<xsl:value-of select="substring ($s, string-length ($gt)+1)" />-->
				<xsl:value-of select="substring-after ($s, '&gt;')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$s" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetEscapedParameter">
		<xsl:param name="orig-parameter-type" />
		<xsl:param name="parameter-type" />
		<xsl:param name="parameter-types" />
		<xsl:param name="escape" />
		<xsl:param name="index" />

		<xsl:choose>
			<xsl:when test="$index &gt; count($parameter-types)">
				<xsl:if test="$parameter-type != $orig-parameter-type">
					<xsl:value-of select="$parameter-type" />
				</xsl:if>
				<!-- ignore -->
			</xsl:when>
			<xsl:when test="$parameter-types[position() = $index]/@name = $parameter-type">
				<xsl:value-of select="concat ($escape, $index - 1)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="typeparam" select="$parameter-types[position() = $index]/@name" />
				<xsl:call-template name="GetEscapedParameter">
					<xsl:with-param name="orig-parameter-type" select="$orig-parameter-type" />
					<xsl:with-param name="parameter-type">
						<xsl:call-template name="Replace">
							<xsl:with-param name="s">
								<xsl:call-template name="Replace">
									<xsl:with-param name="s">
										<xsl:call-template name="Replace">
											<xsl:with-param name="s">
												<xsl:call-template name="Replace">
													<xsl:with-param name="s" select="$parameter-type"/>
													<xsl:with-param name="from" select="concat('&lt;', $typeparam, '&gt;')" />
													<xsl:with-param name="to" select="concat('&lt;', $escape, $index - 1, '&gt;')" />
												</xsl:call-template>
											</xsl:with-param>
											<xsl:with-param name="from" select="concat('&lt;', $typeparam, ',')" />
											<xsl:with-param name="to" select="concat('&lt;', $escape, $index - 1, ',')" />
										</xsl:call-template>
									</xsl:with-param>
									<xsl:with-param name="from" select="concat (',', $typeparam, '&gt;')" />
									<xsl:with-param name="to" select="concat(',', $escape, $index - 1, '&gt;')" />
								</xsl:call-template>
							</xsl:with-param>
							<xsl:with-param name="from" select="concat (',', $typeparam, ',')" />
							<xsl:with-param name="to" select="concat(',', $escape, $index - 1, ',')" />
						</xsl:call-template>
					</xsl:with-param>
					<xsl:with-param name="parameter-types" select="$parameter-types" />
					<xsl:with-param name="typeparam" select="$typeparam" />
					<xsl:with-param name="escape" select="$escape" />
					<xsl:with-param name="index" select="$index + 1" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetLinkId">
		<xsl:param name="type"/>
		<xsl:param name="member"/>
		<xsl:call-template name="GetLinkName">
			<xsl:with-param name="type" select="$type" />
			<xsl:with-param name="member" select="$member" />
		</xsl:call-template>
		<xsl:if test="count($member/Parameters/Parameter) &gt; 0 or $member/MemberType='Method' or $member/MemberType='Constructor'">
			<xsl:text>(</xsl:text>
			<xsl:for-each select="Parameters/Parameter">
				<xsl:if test="not(position()=1)">,</xsl:if>
				<xsl:call-template name="GetParameterType">
					<xsl:with-param name="type" select="$type" />
					<xsl:with-param name="member" select="$member" />
					<xsl:with-param name="parameter" select="." />
				</xsl:call-template>
			</xsl:for-each>
			<xsl:text>)</xsl:text>
		</xsl:if>
		<xsl:if test="$member/@MemberName='op_Implicit' or $member/@MemberName='op_Explicit'">
			<xsl:text>~</xsl:text>
			<xsl:variable name="parameter-rtf">
				<Parameter Type="{$member/ReturnValue/ReturnType}" />
			</xsl:variable>
			<xsl:call-template name="GetParameterType">
				<xsl:with-param name="type" select="$type" />
				<xsl:with-param name="member" select="$member" />
				<xsl:with-param name="parameter" select="msxsl:node-set($parameter-rtf)/Parameter" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- 
	  - what should be <xsl:value-of select="@Type" /> becomes a nightmare once
		- generics enter the picture, since a parameter type could come from the
		- type itelf (becoming `N) or from the method (becoming ``N).
	  -->
	<xsl:template name="GetParameterType">
		<xsl:param name="type" />
		<xsl:param name="member" />
		<xsl:param name="parameter" />

		<!-- the actual parameter type -->
		<xsl:variable name="ptype">
			<xsl:choose>
				<xsl:when test="contains($parameter/@Type, '[')">
					<xsl:value-of select="substring-before ($parameter/@Type, '[')" />
				</xsl:when>
				<xsl:when test="contains($parameter/@Type, '&amp;')">
					<xsl:value-of select="substring-before ($parameter/@Type, '&amp;')" />
				</xsl:when>
				<xsl:when test="contains($parameter/@Type, '*')">
					<xsl:value-of select="substring-before ($parameter/@Type, '*')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$parameter/@Type" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<!-- parameter modifiers -->
		<xsl:variable name="pmodifier">
			<xsl:call-template name="Replace">
				<xsl:with-param name="s" select="substring-after ($parameter/@Type, $ptype)" />
				<xsl:with-param name="from">&amp;</xsl:with-param>
				<xsl:with-param name="to">@</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="gen-type">
			<xsl:call-template name="GetEscapedParameter">
				<xsl:with-param name="orig-parameter-type" select="$ptype" />
				<xsl:with-param name="parameter-type">
					<xsl:variable name="nested">
						<xsl:call-template name="GetEscapedParameter">
							<xsl:with-param name="orig-parameter-type" select="$ptype" />
							<xsl:with-param name="parameter-type" select="$ptype" />
							<xsl:with-param name="parameter-types" select="$type/Docs/typeparam" />
							<xsl:with-param name="escape" select="'`'" />
							<xsl:with-param name="index" select="1" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="$nested != ''">
							<xsl:value-of select="$nested" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$ptype" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
				<xsl:with-param name="parameter-types" select="$member/Docs/typeparam" />
				<xsl:with-param name="escape" select="'``'" />
				<xsl:with-param name="index" select="1" />
			</xsl:call-template>
		</xsl:variable>

		<!-- the actual parameter type -->
		<xsl:variable name="parameter-type">
			<xsl:choose>
				<xsl:when test="$gen-type != ''">
					<xsl:value-of select="$gen-type" />
					<xsl:value-of select="$pmodifier" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat($ptype, $pmodifier)" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<!-- s/</{/g; s/>/}/g; so that less escaping is needed. -->
		<xsl:call-template name="Replace">
			<xsl:with-param name="s">
				<xsl:call-template name="Replace">
					<xsl:with-param name="s" select="translate ($parameter-type, '+', '.')" />
					<xsl:with-param name="from">&gt;</xsl:with-param>
					<xsl:with-param name="to">}</xsl:with-param>
				</xsl:call-template>
			</xsl:with-param>
			<xsl:with-param name="from">&lt;</xsl:with-param>
			<xsl:with-param name="to">{</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="Replace">
		<xsl:param name="s" />
		<xsl:param name="from" />
		<xsl:param name="to" />
		<xsl:choose>
			<xsl:when test="not(contains($s, $from))">
				<xsl:value-of select="$s" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="prefix" select="substring-before($s, $from)"/>
				<xsl:variable name="suffix" select="substring-after($s, $from)" />
				<xsl:value-of select="$prefix" />
				<xsl:value-of select="$to" />
				<xsl:call-template name="Replace">
					<xsl:with-param name="s" select="$suffix" />
					<xsl:with-param name="from" select="$from" />
					<xsl:with-param name="to" select="$to" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="getmodifiers">
		<xsl:param name="sig"/>
		<xsl:param name="protection" select="true()"/>
		<xsl:param name="inheritance" select="true()"/>
		<xsl:param name="extra" select="true()"/>
		<xsl:param name="typetype" select="false()"/>

		<xsl:variable name="Sig">
			<xsl:text> </xsl:text>
			<xsl:choose>
				<xsl:when test="contains($sig, '{')">
					<xsl:value-of select="substring-before ($sig, '{')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$sig" />
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text> </xsl:text>
		</xsl:variable>

		<xsl:if test="$protection">
			<xsl:if test="contains($Sig, ' public ')">public </xsl:if>
			<xsl:if test="contains($Sig, ' private ')">private </xsl:if>
			<xsl:if test="contains($Sig, ' protected ')">protected </xsl:if>
			<xsl:if test="contains($Sig, ' internal ')">internal </xsl:if>
		</xsl:if>

		<xsl:if test="contains($Sig, ' static ')">static </xsl:if>
		<xsl:if test="contains($Sig, ' abstract ')">abstract </xsl:if>
		<xsl:if test="contains($Sig, ' operator ')">operator </xsl:if>

		<xsl:if test="contains($Sig, ' const ')">const </xsl:if>
		<xsl:if test="contains($Sig, ' readonly ')">readonly </xsl:if>

		<xsl:if test="$inheritance">
			<xsl:if test="contains($Sig, ' override ')">override </xsl:if>
			<xsl:if test="contains($Sig, ' new ')">new </xsl:if>
		</xsl:if>

		<xsl:if test="$extra">
			<xsl:if test="contains($Sig, ' sealed ')">sealed </xsl:if>
			<xsl:if test="contains($Sig, ' virtual ')">virtual </xsl:if>

			<xsl:if test="contains($Sig, ' extern ')">extern </xsl:if>
			<xsl:if test="contains($Sig, ' checked ')">checked </xsl:if>
			<xsl:if test="contains($Sig, ' unsafe ')">unsafe </xsl:if>
			<xsl:if test="contains($Sig, ' volatile ')">volatile </xsl:if>
			<xsl:if test="contains($Sig, ' explicit ')">explicit </xsl:if>
			<xsl:if test="contains($Sig, ' implicit ')">implicit </xsl:if>
		</xsl:if>

		<xsl:if test="$typetype">
			<xsl:if test="contains($Sig, ' class ')">class </xsl:if>
			<xsl:if test="contains($Sig, ' interface ')">interface </xsl:if>
			<xsl:if test="contains($Sig, ' struct ')">struct </xsl:if>
			<xsl:if test="contains($Sig, ' delegate ')">delegate </xsl:if>
			<xsl:if test="contains($Sig, ' enum ')">enum </xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GetTypeDescription">
		<xsl:variable name="sig" select="TypeSignature[@Language='C#']/@Value"/>
		<xsl:choose>
			<xsl:when test="contains($sig, ' class ')">Class</xsl:when>
			<xsl:when test="contains($sig, ' interface ')">Interface</xsl:when>
			<xsl:when test="contains($sig, ' struct ')">Struct</xsl:when>
			<xsl:when test="contains($sig, ' delegate ')">Delegate</xsl:when>
			<xsl:when test="contains($sig, ' enum ')">Enum</xsl:when>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="since">
		<p>
			<i>Note: This namespace, class, or member is supported only in version <xsl:value-of select="@version" />
			and later.</i>
		</p>
	</xsl:template>

	<xsl:template name="GetLinkTargetHtml">
		<xsl:param name="type" />
		<xsl:param name="cref" />

		<xsl:variable name="href">
			<xsl:call-template name="GetLinkTarget">
				<xsl:with-param name="type" select="$type" />
				<xsl:with-param name="cref" select="$cref" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="string($href) = ''">
				<xsl:text>javascript:alert("Documentation not found.")</xsl:text>
			</xsl:when>
			<xsl:otherwise><xsl:value-of select="$href" /></xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
