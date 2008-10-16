<?xml version="1.0"?>

<!--
	Based on Mono's /monodoc/browser/mono-ecma.xsl file.
-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	>
	
	<xsl:output omit-xml-declaration="yes" />
	
	<xsl:include href="stylesheet.xsl"/>
	
	<xsl:param name="ext" select="'xml'"/>
	<xsl:param name="namespace" select="''"/>

	<xsl:variable name="max-types">50</xsl:variable>

	<xsl:template match="Overview">
		<Page>

			<Title>
				<xsl:value-of select="Title"/>
				<xsl:if test="not($namespace='' or $namespace='all' or count(Types/Namespace)=1)">
					<xsl:value-of select="': '"/>
					<xsl:value-of select="$namespace"/>
				</xsl:if>
			</Title>

			<CollectionTitle>
				<xsl:if test="not($namespace='' or $namespace='all')">
					<a href="../index.{$ext}"><xsl:value-of select="Title"/></a>
				</xsl:if>			
			</CollectionTitle>
			
			<PageTitle>
				<xsl:choose>
				<xsl:when test="not($namespace='' or $namespace='all')">
					<xsl:call-template name="GetNamespaceName">
						<xsl:with-param name="ns" select="$namespace" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Title"/>
				</xsl:otherwise>
				</xsl:choose>
			</PageTitle>

			<!--
			<SideBar>
			<xsl:if test="not($namespace='')">
				<p style="font-weight: bold; border-bottom: thin solid black"><a href="../index.{$ext}"><xsl:value-of select="Assembly"/></a></p>
				<xsl:for-each select="Types/Namespace">
					<xsl:sort select="@Name"/>
					<div>
						<a href="../{@Name}/index.{$ext}">
							<xsl:value-of select="@Name"/>
						</a>
					</div>
				</xsl:for-each>
			</xsl:if>
			<xsl:if test="$namespace=''">
					<div class="AssemblyRemarks" style="margin-top: 1em; margin-bottom: 1em">
						<xsl:apply-templates select="Remarks" mode="notoppara"/>
					</div>
			</xsl:if>
			</SideBar>
			-->
			
			<Summary>
				<xsl:if test="$namespace=''">
					<div class="AssemblyRemarks" style="margin-top: 1em; margin-bottom: 1em">
						<xsl:apply-templates select="Remarks" mode="notoppara"/>
					</div>
				</xsl:if>
			</Summary>
			
			<Signature>
			</Signature>
			
			<Remarks>			
				<xsl:choose>
					<xsl:when test="Types/Namespace[@Name=$namespace][count(Type)>0] and $namespace != ''">
						<!-- show all types within namespace -->
						<h2 class="Section"><xsl:call-template name="GetNamespaceName" /></h2>
						<xsl:call-template name="CreateNamespaceDetails">
							<xsl:with-param name="ns" select="Types/Namespace[@Name=$namespace]" />
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="count(Types//Type) &lt; $max-types">
						<!-- index; show all types -->
						<xsl:for-each select="Types/Namespace">
							<xsl:sort select="@Name"/>
							<h2 class="Section"><xsl:call-template name="CreateNamespaceLink" /></h2>
							<xsl:call-template name="CreateNamespaceDetails">
								<xsl:with-param name="ns" select="." />
							</xsl:call-template>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<!-- index; show only namespaces -->
						<xsl:for-each select="Types/Namespace">
							<xsl:sort select="@Name"/>
							<h2 class="Section"><xsl:call-template name="CreateNamespaceLink" /></h2>
							<p><xsl:apply-templates select="document(concat('ns-',@Name,'.xml'), .)/Namespace/Docs/summary" mode="notoppara"/></p>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>

			</Remarks>
				
			<Members>
			</Members>
			
			<xsl:copy-of select="Copyright"/>
			
		</Page>

	</xsl:template>
	<xsl:template name="CreateNamespaceDetails">
		<xsl:param name="ns" />
					<p><xsl:apply-templates select="document(concat('ns-',$ns/@Name,'.xml'), .)/Namespace/Docs/remarks" mode="notoppara"/></p>

					<table class="TypesListing" style="margin-top: 1em">
			
					<tr>
						<th>Type</th>
						<th>Description</th>
					</tr>				
						
					<xsl:for-each select="$ns/Type">
						<xsl:sort select="@Name"/>
						<tr valign="top">
							<td>
								<xsl:variable name="path">
									<xsl:choose>
									<xsl:when test="$namespace=parent::Namespace/@Name">.</xsl:when>
									<xsl:otherwise><xsl:value-of select="parent::Namespace/@Name"/></xsl:otherwise>
									</xsl:choose>
								</xsl:variable>
							
								<a href="{$path}/{@Name}.{$ext}">
									<xsl:choose>
										<xsl:when test="@DisplayName != ''">
											<xsl:value-of select="translate (@DisplayName, '+', '.')"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="translate (@Name, '+', '.')"/>
										</xsl:otherwise>
									</xsl:choose>
								</a>
							</td>
							<td>
								<xsl:variable name="docdir">
									<xsl:choose>
										<xsl:when test="parent::Namespace/@Name = ''">.</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="parent::Namespace/@Name" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:variable>
								<xsl:apply-templates select="document(concat($docdir, '/', @Name, '.xml'), .)/Type/Docs/summary" mode="notoppara"/>
							</td>
						</tr>
					</xsl:for-each>
						
					</table>
	</xsl:template>

	<xsl:template name="GetNamespaceName">
		<xsl:param name="ns" select="@Name" />
		<xsl:choose>
			<xsl:when test="$ns = ''">
				<xsl:text>Root</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$ns" />
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> Namespace</xsl:text>
	</xsl:template>

	<xsl:template name="CreateNamespaceLink">
		<xsl:choose>
			<xsl:when test="@Name =''">
				<xsl:call-template name="GetNamespaceName" />
			</xsl:when>
			<xsl:otherwise>
				<a href="{@Name}/index.{$ext}"><xsl:call-template name="GetNamespaceName" /></a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
</xsl:stylesheet>

