<?xml version="1.0"?>

<!--
	mono-ecma-impl.xsl: ECMA-style docs to HTML stylesheet trasformation

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

-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:monodoc="monodoc:///extensions"
	exclude-result-prefixes="monodoc"
	>
	<xsl:include href="mdoc-html-utils.xsl" />
	
	<!-- TEMPLATE PARAMETERS -->

	<xsl:param name="show"/>
	<xsl:param name="membertype"/>
	<xsl:param name="namespace"/>

	<!-- THE MAIN RENDERING TEMPLATE -->

	<xsl:template match="Type|elements">
		<!-- The namespace that the current type belongs to. -->
		<xsl:variable name="TypeNamespace" select="substring(@FullName, 1, string-length(@FullName) - string-length(@Name) - 1)"/>

		<!-- HEADER -->

		<xsl:variable name="typename" select="translate (@FullName, '+', '.')" />
		<xsl:variable name="typelink">
			<xsl:call-template name="GetEscapedTypeName">
				<xsl:with-param name="typename" select="@FullName" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="basename">
			<xsl:if test="Base/BaseTypeName">
				<xsl:value-of select="translate (Base/BaseTypeName, '+', '.')" />
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="baselink">
			<xsl:if test="Base/BaseTypeName">
				<xsl:call-template name="GetEscapedTypeName">
					<xsl:with-param name="typename" select="Base/BaseTypeName" />
				</xsl:call-template>
			</xsl:if>
		</xsl:variable>

		<xsl:call-template name="CreateHeader">
			<xsl:with-param name="content">
			  <ul class="breadcrumb">
				<xsl:choose>
					<xsl:when test="$show='masteroverview'">
						<li class="namespace">
						  <xsl:text>Namespaces in this Collection</xsl:text>
						</li>
					</xsl:when>
					<xsl:when test="$show='typeoverview'">
						<li class="namespace">
						<a>
							<xsl:attribute name="href">N:<xsl:value-of select="$TypeNamespace"/></xsl:attribute>
							<xsl:value-of select="$TypeNamespace"/></a>
						</li>
						<li class="pubclass">
							<xsl:value-of select="@Name"/>
						</li>
					</xsl:when>
					<xsl:when test="$show='members'">
						<li class="namespace">
						  <a>
						    <xsl:attribute name="href">N:<xsl:value-of select="$TypeNamespace"/></xsl:attribute>
						    <xsl:value-of select="$TypeNamespace"/>
						  </a>
						</li>
						<li class="pubclass">
						  <a>
							<xsl:attribute name="href">
								<xsl:text>T:</xsl:text>
								<xsl:value-of select="$typelink" />
							</xsl:attribute>						  
							<xsl:value-of select="@Name"/>
						  </a>
						</li>
						<li class="members">
						  Members
						</li>
					</xsl:when>
					<xsl:when test="$show='member' or $show='overloads'">
						<li class="namespace">
						<a>
							<xsl:attribute name="href">N:<xsl:value-of select="$TypeNamespace"/></xsl:attribute>
							<xsl:value-of select="$TypeNamespace"/></a>
						</li>
						<li class="pubclass">
						  <a>
							<xsl:attribute name="href">
								<xsl:text>T:</xsl:text>
								<xsl:value-of select="$typelink" />
							</xsl:attribute>						  
							<xsl:value-of select="@Name"/>
						  </a>
						</li>
						<li class="pubproperty">
						  <xsl:choose>
						  <xsl:when test="$membertype='Operator'">
						  	<xsl:value-of select="$typename"/>
						  	<xsl:value-of select="' '"/> <!-- hard space -->
						  	<xsl:value-of select="substring-after(Members/Member[MemberType='Method'][position()=$index+1]/@MemberName, 'op_')"/>
						  </xsl:when>
						  <xsl:when test="$membertype='Constructor'">
						  	<xsl:value-of select="$typename"/>
						  </xsl:when>
						  <xsl:otherwise>
						  	<xsl:value-of select="Members/Member[MemberType=$membertype][position()=$index+1]/@MemberName"/>
						  </xsl:otherwise>
						  </xsl:choose>
						</li>
					</xsl:when>
					<xsl:when test="$show='namespace'">
						<li class="namespace">
						  <xsl:value-of select="$namespace"/>
						</li>
					</xsl:when>
				</xsl:choose>
			</ul>
			<div class="named-header">
				<xsl:choose>
					<xsl:when test="$show='masteroverview'">
						<xsl:text>Master Overview</xsl:text>
					</xsl:when>
					<xsl:when test="$show='typeoverview'">
						<xsl:value-of select="$typename"/>
						<xsl:value-of select="' '"/>
						<xsl:call-template name="gettypetype"/>
					</xsl:when>
					<xsl:when test="$show='members' and $membertype='All'">
						<xsl:value-of select="$typename"/>
						<xsl:text> Members</xsl:text>
					</xsl:when>
					<xsl:when test="$show='members'">
						<xsl:value-of select="$typename"/>
						<xsl:text>: </xsl:text>
						<xsl:value-of select="$membertype"/>
						<xsl:text> Members</xsl:text>
					</xsl:when>
					<xsl:when test="$show='member'">
						<xsl:choose>
						<xsl:when test="$membertype='Operator'">
							<xsl:value-of select="$typename"/>
							<xsl:value-of select="' '"/> <!-- hard space -->
							<xsl:value-of select="substring-after(Members/Member[MemberType='Method'][position()=$index+1]/@MemberName, 'op_')"/>
						</xsl:when>
						<xsl:when test="$membertype='Constructor'">
							<xsl:value-of select="$typename"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$typename"/>.<xsl:value-of select="Members/Member[MemberType=$membertype][position()=$index+1]/@MemberName"/>
						</xsl:otherwise>
						</xsl:choose>
						<xsl:value-of select="' '"/>
						<xsl:value-of select="$membertype"/>
					</xsl:when>

					<xsl:when test="$show='namespace'">
						<xsl:value-of select="$namespace"/>
						<xsl:text> Namespace</xsl:text>
					</xsl:when>
					
					<xsl:when test="$show='overloads'">
						<xsl:value-of select="$typename"/>.<xsl:value-of select="$index"/> Overloads
					</xsl:when>

				</xsl:choose>
			</div>
			</xsl:with-param>
		</xsl:call-template>

		<!-- SELECT WHAT TYPE OF VIEW:
				typeoverview
				members
				member
				-->
		<div class="Content">
		<xsl:choose>
		<xsl:when test="$show='masteroverview'">
		
			<xsl:for-each select="namespace">
				<xsl:sort select="@ns"/>
				
				<!-- Don't display the namespace if it is a sub-namespace of another one.
				     But don't consider namespaces without periods, e.g. 'System', to be
					 parent namespaces because then most everything will get grouped under it. -->
				<xsl:variable name="ns" select="@ns"/>
				<xsl:if test="count(parent::*/namespace[not(substring-before(@ns, '.')='') and starts-with($ns, concat(@ns, '.'))])=0">

				<p>
					<b><a href="N:{@ns}"><xsl:value-of select="@ns"/></a></b>
				</p>
				<blockquote>
					<div>
					<xsl:apply-templates select="summary" mode="notoppara"/>
					</div>
					
					<!-- Display the sub-namespaces of this namespace -->
					<xsl:if test="not(substring-before($ns, '.')='')">
					<xsl:for-each select="parent::*/namespace[starts-with(@ns, concat($ns, '.'))]">
						<br/>
						<div><a href="N:{@ns}"><xsl:value-of select="@ns"/></a></div>
						<div><xsl:apply-templates select="summary" mode="notoppara"/></div>						
					</xsl:for-each>
					</xsl:if>
				</blockquote>
				
				</xsl:if>
			</xsl:for-each>
			
		</xsl:when>
		<!-- TYPE OVERVIEW -->
		<xsl:when test="$show='typeoverview'">
			<xsl:variable name="implemented" select="monodoc:MonoImpInfo(string(AssemblyInfo/AssemblyName), string(@FullName), true())" />
			<xsl:call-template name="CreateTypeOverview">
				<xsl:with-param name="implemented" select="$implemented" />
				<xsl:with-param name="show-members-link" select="true()" />
			</xsl:call-template>
			

			<!-- signature -->
			<xsl:call-template name="CreateTypeSignature" />

			<xsl:call-template name="DisplayDocsInformation">
				<xsl:with-param name="linkid" select="concat ('T:', @FullName)" />
			</xsl:call-template>
		</xsl:when>

		<!-- MEMBER LISTING -->
		<xsl:when test="$show='members'">
			<xsl:if test="$membertype='All'">
				<p>
					The members of <xsl:value-of select="$typename"/> are listed below.
				</p>

				<xsl:if test="Base/BaseTypeName">
					<p>
						<xsl:text>See Also: </xsl:text>
						<a>
							<xsl:attribute name="href">T:<xsl:value-of select="$baselink"/>/*</xsl:attribute>
							<xsl:text>Inherited members from </xsl:text>
							<xsl:value-of select="$basename"/>
						</a>
					</p>
				</xsl:if>

				<ul class="TypeMembersIndex">
					<xsl:if test="count(Members/Member[MemberType='Constructor'])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/C</xsl:attribute>Constructors</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='Field'])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/F</xsl:attribute>Fields</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='Property'])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/P</xsl:attribute>Properties</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='Method' and not(starts-with(@MemberName,'op_'))])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/M</xsl:attribute>Methods</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='Event'])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/E</xsl:attribute>Events</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='Method' and starts-with(@MemberName,'op_')])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/E</xsl:attribute>Events</a>
						</li>
					</xsl:if>
					<xsl:if test="count(Members/Member[MemberType='ExtensionMethod'])">
						<li>
							<a><xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/X</xsl:attribute>Extension Methods</a>
						</li>
					</xsl:if>
				</ul>

				<!-- list each type of member (public, then protected) -->

				<xsl:call-template name="ListAllMembers" />
			</xsl:if>

			<xsl:if test="not($membertype='All')">
				<!-- list the members of this type (public, then protected) -->

				<p>
					The
					<xsl:call-template name="membertypeplurallc"><xsl:with-param name="name" select="$membertype"/></xsl:call-template>
					of <xsl:value-of select="$typename"/> are listed below.  For a list of all members, see the <a>
					<xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/*</xsl:attribute>
					<xsl:value-of select="@Name"/> Members</a> list.
				</p>
				
				<xsl:if test="Base/BaseTypeName">
					<p>
						<xsl:text>See Also: </xsl:text>
						<a>
							<xsl:attribute name="href">T:<xsl:value-of select="$baselink"/>/*</xsl:attribute>
							<xsl:text>Inherited members from </xsl:text>
							<xsl:value-of select="$basename"/>
						</a>
					</p>
				</xsl:if>

				<xsl:call-template name="ListMembers">
					<xsl:with-param name="listmembertype" select="$membertype"/>
					<xsl:with-param name="showprotected" select="false()"/>
				</xsl:call-template>

				<xsl:call-template name="ListMembers">
					<xsl:with-param name="listmembertype" select="$membertype"/>
					<xsl:with-param name="showprotected" select="true()"/>
				</xsl:call-template>
			</xsl:if>

		</xsl:when>
		
		<xsl:when test="$show='overloads'">
				<p>
					The overloads of <xsl:value-of select="$index"/>
					are listed below.  For a list of all members, see the <a>
					<xsl:attribute name="href">T:<xsl:value-of select="$typelink"/>/*</xsl:attribute>
					<xsl:value-of select="@Name"/> Members</a> list.
				</p>
				
				<!-- TODO: can we make this actually test if there are any overloads
				<xsl:if test="Base/BaseTypeName">
					<p>
						See Also: <a>
					<xsl:attribute name="href">T:<xsl:value-of select="Base/BaseTypeName"/>/*</xsl:attribute>
					Inherited members</a> from <xsl:value-of select="Base/BaseTypeName"/>
					</p>
				</xsl:if>
				 -->
				 
				<xsl:call-template name="ListMembers">
					<xsl:with-param name="listmembertype" select="$membertype"/>
					<xsl:with-param name="showprotected" select="false()"/>
					<xsl:with-param name="overloads-mode" select="true()"/>
				</xsl:call-template>

				<xsl:call-template name="ListMembers">
					<xsl:with-param name="listmembertype" select="$membertype"/>
					<xsl:with-param name="showprotected" select="true()"/>
					<xsl:with-param name="overloads-mode" select="true()"/>
				</xsl:call-template>
		</xsl:when>
		<!-- MEMBER DETAILS -->
		<xsl:when test="$show='member'">
			<xsl:variable name="Type" select="."/>

			<!-- select the member, this just loops through the one member that we are to display -->
			<xsl:for-each select="Members/Member[MemberType=$membertype or ($membertype='Operator' and MemberType='Method')][position()=$index+1]">

				<!-- summary -->
				
				<xsl:call-template name="CreateMemberOverview">
					<xsl:with-param name="implemented" select="monodoc:MonoImpInfo(string(AssemblyInfo/AssemblyName), string(@FullName), true())" />
				</xsl:call-template>

				<xsl:call-template name="CreateMemberSignature">
					<xsl:with-param name="linkid" select="concat ('T:', @FullName)" />
				</xsl:call-template>

				<div class="MemberBox">
					<xsl:call-template name="DisplayDocsInformation">
						<xsl:with-param name="linkid" select="concat ('T:', @FullName)" />
					</xsl:call-template>
				</div>

			</xsl:for-each>

		</xsl:when>

		<!-- NAMESPACE SUMMARY -->
		<xsl:when test="$show='namespace'">

			<!-- summary -->

			<p>
				<xsl:apply-templates select="summary" mode="notoppara"/>
				<xsl:if test="monodoc:MonoEditing()">
					<xsl:value-of select="' '" />
					[<a href="{monodoc:EditUrlNamespace (., $namespace, 'summary')}">Edit</a>]
				</xsl:if>
			</p>

			<!-- remarks -->

			<xsl:if test="not(remarks = '')">
				<h2>Remarks</h2>
				<div class="SectionBox">
					<xsl:apply-templates select="remarks"/>
					<xsl:if test="monodoc:MonoEditing()">
						<xsl:value-of select="' '" />
						[<a href="{monodoc:EditUrlNamespace (., $namespace, 'remarks')}">Edit</a>]
					</xsl:if>
				</div>
			</xsl:if>
		
			<xsl:call-template name="namespacetypes">
				<xsl:with-param name="typetype" select="'class'"/>
				<xsl:with-param name="typetitle" select="'Classes'"/>
			</xsl:call-template>

			<xsl:call-template name="namespacetypes">
				<xsl:with-param name="typetype" select="'interface'"/>
				<xsl:with-param name="typetitle" select="'Interfaces'"/>
			</xsl:call-template>

			<xsl:call-template name="namespacetypes">
				<xsl:with-param name="typetype" select="'struct'"/>
				<xsl:with-param name="typetitle" select="'Structs'"/>
			</xsl:call-template>

			<xsl:call-template name="namespacetypes">
				<xsl:with-param name="typetype" select="'delegate'"/>
				<xsl:with-param name="typetitle" select="'Delegates'"/>
			</xsl:call-template>

			<xsl:call-template name="namespacetypes">
				<xsl:with-param name="typetype" select="'enum'"/>
				<xsl:with-param name="typetitle" select="'Enumerations'"/>
			</xsl:call-template>

			
		</xsl:when>

		<!-- don't know what kind of page this is -->
		<xsl:otherwise>
			Don't know what to do!
		</xsl:otherwise>

		</xsl:choose>
		</div>
		
		<!-- FOOTER -->
		
		<div class="Footer">
		<hr/>
			This documentation is part of the <a target="_top" title="Mono Project" href="http://www.mono-project.com/">Mono Project</a>.
		</div>

	</xsl:template>

	<xsl:template name="GetLinkTarget">
		<xsl:param name="type" />
		<xsl:param name="cref" />

		<xsl:value-of select="$cref" />
	</xsl:template>

	<xsl:template name="namespacetypes">
		<xsl:param name="typetype"/>
		<xsl:param name="typetitle"/>

		<xsl:variable name="NODES" select="*[name()=$typetype]"/>

		<xsl:if test="count($NODES)">

		<xsl:call-template name="CreateH2Section">
			<xsl:with-param name="name" select="$typetitle" />
			<xsl:with-param name="child-id" select="$typetitle" />
			<xsl:with-param name="content">
		
		<xsl:call-template name="CreateTypeDocumentationTable">
		<xsl:with-param name="content">
			<xsl:for-each select="$NODES">
				<xsl:sort select="@name"/>

				<tr>
					<td>
						<a>
							<xsl:attribute name="href">
								<xsl:text>T:</xsl:text>
								<xsl:call-template name="GetEscapedTypeName">
									<xsl:with-param name="typename" select="@fullname" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:value-of select="@name"/>
						</a>

						<xsl:variable name="containingtype" select="substring-before(@fullname, concat('+',@name))"/>
						<xsl:if test="$containingtype">
						<br/>(in
							<xsl:call-template name="maketypelink">
								<xsl:with-param name="type" select="$containingtype"/>
								<xsl:with-param name="wrt" select="$namespace"/>
							</xsl:call-template>)
						</xsl:if>
					</td>
					<td>
						<xsl:apply-templates select="summary" mode="notoppara"/>

						<xsl:variable name="MonoImplInfo" select="monodoc:MonoImpInfo(string(@assembly), string(@fullname), false())"/>
						<xsl:if test="$MonoImplInfo"><br/><b><xsl:value-of disable-output-escaping="yes" select="$MonoImplInfo"/></b></xsl:if>
					</td>
				</tr>
			</xsl:for-each>
		</xsl:with-param>
		</xsl:call-template>
			</xsl:with-param>
		</xsl:call-template>

		</xsl:if>
	</xsl:template>
	
	<xsl:template name="CreateEditLink">
		<xsl:param name="e" />
		<xsl:if test="monodoc:MonoEditing()">
			<xsl:value-of select="' '" />
			[<a href="{monodoc:EditUrl ($e)}">Edit</a>]
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
