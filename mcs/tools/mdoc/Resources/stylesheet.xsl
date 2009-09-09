<?xml version="1.0"?>

<!--
	Based on Mono's /monodoc/browser/mono-ecma.xsl file.
-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	>
	<xsl:include href="mdoc-html-utils.xsl" />
	<xsl:include href="mdoc-sections-css.xsl" />
	
	<!-- TEMPLATE PARAMETERS -->

	<xsl:param name="language" select="'C#'"/>
	<xsl:param name="ext" select="'xml'"/>
	<xsl:param name="basepath" select="'./'"/>
	
	<xsl:param name="Index" />

	<!-- The namespace that the current type belongs to. -->
	<xsl:variable name="mono-docs">http://www.go-mono.com/docs/monodoc.ashx?link=</xsl:variable>

	<!-- THE MAIN RENDERING TEMPLATE -->

	<xsl:template match="Type">
		<xsl:variable name="cref">
			<xsl:text>T:</xsl:text>
			<xsl:call-template name="GetEscapedTypeName">
				<xsl:with-param name="typename" select="@FullName" />
			</xsl:call-template>
		</xsl:variable>

		<Page>
		
		<Title>
			<xsl:value-of select="translate (@FullName, '+', '.')" />
		</Title>
		
		<CollectionTitle>
			<xsl:variable name="namespace" select="substring-before (@FullName, @Name)" />
			<a>
				<xsl:attribute name="href">
					<xsl:if test="string-length($namespace)">
						<xsl:value-of select="$basepath" />
					</xsl:if>
					<xsl:text>index.</xsl:text>
					<xsl:value-of select="$ext" />
				</xsl:attribute>
				<xsl:value-of select="AssemblyInfo/AssemblyName" />
			</a>
			<xsl:text> : </xsl:text>
			<a href="index.{$ext}"><xsl:value-of select="$TypeNamespace"/> Namespace</a>
		</CollectionTitle>
		
		<PageTitle>
			<xsl:attribute name="id">
				<xsl:value-of select="$cref" />
			</xsl:attribute>
			<xsl:value-of select="translate (@Name, '+', '.')"/>
			<xsl:text xml:space="preserve"> </xsl:text>
			<xsl:if test="count(Docs/typeparam) &gt; 0">Generic</xsl:if>
			<xsl:text xml:space="preserve"> </xsl:text>
			<xsl:call-template name="GetTypeDescription" />
		</PageTitle>
		
		<!--
		<SideBar>
			<p style="font-weight: bold; border-bottom: thin solid black"><a href="index.{$ext}"><xsl:value-of select="$TypeNamespace"/></a></p>

			<xsl:for-each select="document('index.xml',.)/Overview/Types/Namespace[@Name=$TypeNamespace]/Type">
				<xsl:sort select="@Name"/>
				<div>
					<a href="../{parent::Namespace/@Name}/{@Name}.{$ext}">
						<xsl:value-of select="@Name"/>
					</a>
				</div>
			</xsl:for-each>
		</SideBar>
		-->

		<!-- TYPE OVERVIEW -->
		
		<Summary id="Summary">
			<xsl:attribute name="id">
				<xsl:value-of select="concat ($cref, ':Summary')" />
			</xsl:attribute>
			<!-- summary -->
			<xsl:apply-templates select="Docs/summary" mode="notoppara"/>
		</Summary>

		<Signature>
			<xsl:call-template name="CreateTypeSignature" />
		</Signature>
			
		<Remarks>
			<xsl:attribute name="id">
				<xsl:value-of select="concat ($cref, ':Docs')" />
			</xsl:attribute>
			<xsl:call-template name="DisplayDocsInformation">
				<xsl:with-param name="linkid" select="concat ($cref, ':Docs')" />
			</xsl:call-template>

			<!-- MEMBER LISTING -->
			<xsl:if test="not(Base/BaseTypeName='System.Delegate' or Base/BaseTypeName='System.MulticastDelegate' or Base/BaseTypeName='System.Enum')">
				<xsl:call-template name="CreateH2Section">
					<xsl:with-param name="name" select="'Members'"/>
					<xsl:with-param name="id" select="'Members'"/>
					<xsl:with-param name="child-id" select="'_Members'"/>
					<xsl:with-param name="content">
						<xsl:if test="Base/BaseTypeName">
							<p>
								See Also: Inherited members from
								<xsl:apply-templates select="Base/BaseTypeName" mode="typelink"><xsl:with-param name="wrt" select="$TypeNamespace"/></xsl:apply-templates>.
							</p>
						</xsl:if>

						<!-- list each type of member (public, then protected) -->

						<xsl:call-template name="ListAllMembers">
							<xsl:with-param name="html-anchor" select="true()" />
						</xsl:call-template>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:if>
			
		</Remarks>
			
		<Members>
		<!-- MEMBER DETAILS -->
			<xsl:attribute name="id">
				<xsl:value-of select="concat ($cref, ':Members')" />
			</xsl:attribute>
			<xsl:if test="not(Base/BaseTypeName='System.Delegate' or Base/BaseTypeName='System.MulticastDelegate' or Base/BaseTypeName='System.Enum')">
			<xsl:variable name="Type" select="."/>
			
			<xsl:call-template name="CreateH2Section">
				<xsl:with-param name="name" select="'Member Details'"/>
				<xsl:with-param name="id" select="'MemberDetails'"/>
				<xsl:with-param name="child-id" select="'_MemberDetails'"/>
				<xsl:with-param name="content">
					<xsl:for-each select="Members/Member[MemberType != 'ExtensionMethod']">
					
						<xsl:variable name="linkid">
							<xsl:call-template name="GetLinkId">
								<xsl:with-param name="type" select="../.." />
								<xsl:with-param name="member" select="." />
							</xsl:call-template>
						</xsl:variable>

						<xsl:call-template name="CreateH3Section">
							<xsl:with-param name="id" select="$linkid" />
							<xsl:with-param name="child-id" select="concat($linkid, ':member')" />
							<xsl:with-param name="class" select="MemberName" />
							<xsl:with-param name="name">
								<xsl:choose>
									<xsl:when test="MemberType='Constructor'">
										<xsl:call-template name="GetConstructorName">
											<xsl:with-param name="type" select="../.." />
											<xsl:with-param name="ctor" select="." />
										</xsl:call-template>
									</xsl:when>
									<xsl:when test="@MemberName='op_Implicit' or @MemberName='op_Explicit'">
										<xsl:text>Conversion</xsl:text>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="@MemberName"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text xml:space="preserve"> </xsl:text>
								<xsl:if test="count(Docs/typeparam) &gt; 0">
									<xsl:text>Generic </xsl:text>
								</xsl:if>
								<xsl:value-of select="MemberType" />
							</xsl:with-param>
							<xsl:with-param name="ref" select="." />
							<xsl:with-param name="content">
								<xsl:call-template name="CreateMemberOverview" />
								<xsl:call-template name="CreateMemberSignature">
									<xsl:with-param name="linkid" select="$linkid" />
								</xsl:call-template>
								<xsl:call-template name="DisplayDocsInformation">
									<xsl:with-param name="linkid" select="$linkid" />
								</xsl:call-template>
								<hr size="1"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
			</xsl:if>
			
			</Members>
			
			<Copyright>
			</Copyright>
			
		</Page>
	</xsl:template>

	<xsl:template name="GetLinkTarget">
		<xsl:param name="type" />
		<xsl:param name="cref" />
		<!-- Search for type in the index.xml file. -->
		<xsl:variable name="typeentry">
			<xsl:call-template name="FindTypeInIndex">
				<xsl:with-param name="type" select="$type" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="count($typeentry)">
				<xsl:if test="string-length ($typeentry/@Namespace)">
					<xsl:value-of select="$basepath" />
					<xsl:value-of select="$typeentry/@Namespace" />
					<xsl:text>/</xsl:text>
				</xsl:if>
				<xsl:value-of select="$typeentry/@Name"/>
				<xsl:text>.</xsl:text>
				<xsl:value-of select="$ext" />
				<xsl:if test="string-length ($cref) > 0 and substring ($cref, 1, 2) != 'T:'">
					<xsl:text>#</xsl:text>
					<xsl:call-template name="GetActualCref">
						<xsl:with-param name="cref" select="$cref" />
					</xsl:call-template>
				</xsl:if>
			</xsl:when>

			<xsl:when test="starts-with($type, 'System.') or 
				starts-with($type, 'Cairo.') or starts-with ($type, 'Commons.Xml.') or
				starts-with($type, 'Mono.GetOptions.') or starts-with($type,'Mono.Math.') or
				starts-with($type, 'Mono.Posix.') or starts-with($type, 'Mono.Remoting.') or
				starts-with($type, 'Mono.Security.') or starts-with($type, 'Mono.Unix.') or
				starts-with($type, 'Mono.Xml.')">
				<xsl:value-of select="$mono-docs" />
				<xsl:value-of select="$cref" />
			</xsl:when>
			<xsl:otherwise>javascript:alert("Documentation not found.")</xsl:otherwise>
			<!--<xsl:otherwise>javascript:alert("Documentation not found for <xsl:value-of select="$type"/>.")</xsl:otherwise>-->
		</xsl:choose>
	</xsl:template>

	<xsl:template name="FindTypeInIndex">
		<xsl:param name="type" />

		<xsl:for-each select="$Index/Types/Namespace/Type">
			<xsl:variable name="nsp">
				<xsl:choose>
					<xsl:when test="string-length (parent::Namespace/@Name) = 0" />
					<xsl:otherwise>
						<xsl:value-of select="parent::Namespace/@Name" />
						<xsl:text>.</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:if test="concat($nsp, translate(@Name, '+', '.')) = $type">
				<Type Name="{@Name}" Namespace="{parent::Namespace/@Name}" />
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="GetActualCref">
		<xsl:param name="cref" />

		<xsl:variable name="fullname">
			<xsl:choose>
				<xsl:when test="starts-with($cref, 'C:') or starts-with($cref, 'T:')">
					<xsl:choose>
						<xsl:when test="contains($cref, '(')">
							<xsl:value-of select="substring (substring-before ($cref, '('), 3)" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="substring($cref, 3)" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="GetTypeName">
						<xsl:with-param name="type" select="substring($cref, 3)"/>
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

		<xsl:value-of select="substring ($cref, 1, 2)" />
		<xsl:value-of select="$escaped-type" />
		<xsl:value-of select="substring-after ($cref, $fullname)" />
	</xsl:template>
	
	<xsl:template name="CreateCodeBlock">
		<xsl:param name="language" />
		<xsl:param name="content" />
		<table class="CodeExampleTable">
		<tr><td><b><font size="-1"><xsl:value-of select="$language"/> Example</font></b></td></tr>
		<tr><td>
			<pre>
				<xsl:attribute name="class">
					<xsl:call-template name="GetCodeClass">
						<xsl:with-param name="lang" select="$language" />
					</xsl:call-template>
				</xsl:attribute>
				<xsl:value-of select="$content" />
			</pre>
		</td></tr>
		</table>
	</xsl:template>

	<xsl:template name="CreateEditLink">
		<!-- ignore -->
	</xsl:template>

	<xsl:template name="CreateExpandedToggle">
		<xsl:text>⊟</xsl:text>
	</xsl:template>

	<xsl:template name="GetCodeClass">
		<xsl:param name="lang" />

		<xsl:choose>
			<xsl:when test="$lang = 'C#' or $lang = 'csharp'">
				<xsl:text>code-csharp</xsl:text>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
