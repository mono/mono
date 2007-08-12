<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:param name="source-name" select="(fillme)" />
	<xsl:output method="html" indent="no"/>
	<!--	<xsl:output method="xml"/>-->
	<xsl:strip-space elements="*"/>

	<xsl:template match="/">
		<xsl:variable name="assembly-name" select="substring-before ($source-name, '.src')" />
		<html>
			<head>
				<title>
					Mono Class Library Status for <xsl:value-of select="$assembly-name" />
				</title>
				<script src="cm/cormissing.js"></script>
				<link rel="stylesheet" type="text/css" href="cm/cormissing.css"></link>
				<style type="text/css"><![CDATA[
				div.links {
					background-color: #CCFFCC;
				}
				div.links ul li {
					display: inline;
					padding: 5px;
					}
				]]></style>
			</head>
			<body>
<div class="links">
	<ul>
	<li><a href="System.Runtime.Serialization.html">Runtime.Serialization</a></li>
	<li><a href="System.IdentityModel.html">IdentityModel</a></li>
	<li><a href="System.IdentityModel.Selectors.html">IdentityModel.Selectors</a></li>
	<li><a href="System.ServiceModel.html">ServiceModel</a></li>
	</ul>
</div>
			<div class="main">
				<p>
					<h1>Mono Class Library Status for <xsl:value-of select="$assembly-name" /></h1>
				</p>
				<p>
					<table>
						<tr>
							<td> <input type="checkbox" ID="todo" onClick="selectTodo();" checked="1"/> </td>
							<td> <img src="cm/st.gif"/> </td>
							<td> TODO </td>
							<td width="20"/>
							<td> <input type="checkbox" ID="missing" onClick="selectMissing();" checked="1"/> </td>
							<td> <img src="cm/sm.gif"/> </td>
							<td> Missing </td>
						</tr>
						<tr>
							<td> <input type="checkbox" ID="extra" onClick="selectExtra();" checked="1"/> </td>
							<td> <img src="cm/sx.gif"/> </td>
							<td> Extra </td>
							<td width="20"/>
							<td> <input type="checkbox" ID="errors" onClick="selectErrors();" checked="1"/> </td>
							<td> <img src="cm/se.gif"/> </td>
							<td> Errors </td>
						</tr>
						<tr>
							<td> </td>
							<td> <img src="cm/sc.gif"/> </td>
							<td> Completed </td>
						</tr>
					</table>
				</p>

				<p>
					<select id="FilteredAttributes">
						<option>System.Runtime.InteropServices.ComVisibleAttribute</option>
						<option>System.Diagnostics.DebuggerDisplayAttribute</option>
						<option>System.Diagnostics.DebuggerTypeProxyAttribute</option>
					</select>
					<input type="button" onclick="removeAndFilter()" value="Unfilter attribute" />
				</p>
				<p>
					<input type="text" id="NewFilterTarget" length="60" />
					<input type="button" onclick="addAndFilter()" value="Add to attribute filter" />
				</p>
				<p>Target Reference:
					<input type="radio" id="TargetMonodoc" name="TargetDoc" value="0" />monodoc
					<input type="radio" id="TargetMsdn1" name="TargetDoc" value="1" />msdn
					<input type="radio" id="TargetMsdn2" name="TargetDoc" value="2" checked="checked" />msdn2
					<input type="radio" id="TargetWinsdk" name="TargetDoc" value="3" checked="checked" />Windows SDK
				</p>

				<DIV ID="ROOT">
					<xsl:apply-templates/>
				</DIV>
				<p>
					Legend :<BR/>
					<table>
						<tr>
							<td> <img src="cm/y.gif"/> </td>
							<td> Assembly </td>
							<td width="20"/>
							<td> <img src="cm/n.gif"/> </td>
							<td> Namespace </td>
							<td width="20"/>
							<td> <img src="cm/c.gif"/> </td>
							<td> Class </td>
							<td width="20"/>
							<td> <img src="cm/s.gif"/> </td>
							<td> Struct </td>
						<tr>
						</tr>
							<td> <img src="cm/i.gif"/> </td>
							<td> Interface </td>
							<td width="20"/>
							<td> <img src="cm/d.gif"/> </td>
							<td> Delegate </td>
							<td width="20"/>
							<td> <img src="cm/en.gif"/> </td>
							<td> Enum </td>
							<td width="20"/>
							<td> <img src="cm/m.gif"/> </td>
							<td> Method </td>
						</tr>
						<tr>
							<td> <img src="cm/f.gif"/> </td>
							<td> Field </td>
							<td width="20"/>
							<td> <img src="cm/p.gif"/> </td>
							<td> Property </td>
							<td width="20"/>
							<td> <img src="cm/e.gif"/> </td>
							<td> Event </td>
							<td width="20"/>
							<td> <img src="cm/r.gif"/> </td>
							<td> Attribute </td>
						</tr>
						<tr>
							<td> <img src="cm/w.gif" /> </td>
							<td colspan="10"> Generic Constraints</td>
						</tr>
					</table>

				</p>
			</div>
			</body>
		</html>
	</xsl:template>



	<!-- assembly -->
	<xsl:template match="/assemblies">
		<xsl:apply-templates select="assembly">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="assembly">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">y</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- namespace -->
	<xsl:template match="namespaces">
		<xsl:apply-templates select="namespace">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="namespace">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">n</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<xsl:template match="namespace/classes">
		<xsl:apply-templates select="class[@type='interface']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="class[@type='class']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="class[@type='struct']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="class[@type='delegate']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="class[@type='enum']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>


	<!-- class -->
	<xsl:template match="class[@type='class']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">c</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="(@missing_total or @todo_total or @extra_total or @warning_total or @error) and not(@presence)">
				<xsl:apply-templates select="attributes"/>
				<xsl:apply-templates select="interfaces"/>
				<xsl:apply-templates select="constructors"/>
				<xsl:apply-templates select="./*[local-name() != 'attributes' and local-name() != 'constructors' and local-name() != 'interfaces']"/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- struct -->
	<xsl:template match="class[@type='struct'][@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">s</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>



	<!-- interface types -->
	<xsl:template match="class[@type='interface']">
		<xsl:apply-templates select="class[@type='interface']">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="class[@type='interface'][@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">i</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>

	<!-- interfaces implemented by Types -->
	<xsl:template match="interface">
		<xsl:apply-templates select="interface">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="interface[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">i</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- generic constraints -->
	<xsl:template match="generic-type-constraints">
		<xsl:apply-templates select="generic-type-constraint">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="generic-type-constraint[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">w</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>



	<!-- delegate -->
	<xsl:template match="class[@type='delegate'][@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">d</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- enumeration -->
	<xsl:template match="class[@type='enum'][@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">en</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- method -->
	<xsl:template match="methods">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="method[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- property -->
	<xsl:template match="properties">
		<xsl:apply-templates select="property">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="property[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">p</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- event -->
	<xsl:template match="events">
		<xsl:apply-templates select="event">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="event[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">e</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- constructor -->
	<xsl:template match="constructors">
		<xsl:apply-templates select="constructor">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="constructor[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">x</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- field -->
	<xsl:template match="fields">
		<xsl:apply-templates select="field">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="field[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">f</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>

	<!-- accessor -->
	<xsl:template match="property/methods">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="property[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]/methods/method[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">o</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>


	<!-- attribute -->
	<xsl:template match="attributes">
		<xsl:apply-templates select="attribute">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="attribute[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">r</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
		</DIV>
	</xsl:template>



	<!-- support templates -->

	<xsl:template name="ELEMENT">
		<xsl:param name="class"/>
		<xsl:param name="image"/>
			<xsl:attribute name="class">
				<xsl:value-of select="$class"/>
				<xsl:if test="./node() and local-name() != 'assembly'">_</xsl:if>
			</xsl:attribute>
			<xsl:call-template name="toggle"/>
			<xsl:choose>
				<xsl:when test="@error and @error != 'todo'">
					<xsl:element name="img">
						<xsl:attribute name="src">cm/se.gif</xsl:attribute>
						<xsl:attribute name="class">t</xsl:attribute>
						<xsl:attribute name="title"><xsl:call-template name="warning-hover" /></xsl:attribute>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@error = 'todo'">
					<xsl:element name="img">
						<xsl:attribute name="src">cm/st.gif</xsl:attribute>
						<xsl:attribute name="class">t</xsl:attribute>
						<xsl:if test="@comment">
							<xsl:attribute name="title"><xsl:value-of select="@comment"/></xsl:attribute>
						</xsl:if>
						<xsl:if test="not(@comment)">
							<xsl:attribute name="title">No TODO description</xsl:attribute>
						</xsl:if>
					</xsl:element>
				</xsl:when>
				<xsl:when test="@presence = 'missing'">
					<img src="cm/sm.gif" class="t"/>
				</xsl:when>
				<xsl:when test="@presence = 'extra'">
					<img src="cm/sx.gif" class="t"/>
				</xsl:when>
				<xsl:otherwise>
					<img src="cm/sc.gif" class="t"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="$image">
					<img src="cm/{$image}.gif" class="t"/>
				</xsl:when>
				<xsl:otherwise>
					<img src="cm/{$class}.gif" class="t"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="name"/>
			<xsl:if test="not(@presence)">
				<xsl:call-template name="status"/>
			</xsl:if>
	</xsl:template>

	<xsl:template name="status">
		<xsl:if test="@complete_total and @complete_total != 0">
			<SPAN class="st">
				<img src="cm/sc.gif"/>
				<xsl:text>: </xsl:text>
				<xsl:value-of select="@complete_total"/>
				<xsl:text>%</xsl:text>
			</SPAN>
		</xsl:if>
		<xsl:if test="@todo_total">
			<SPAN class="st">
				<img src="cm/st.gif"/>: <xsl:value-of select="@todo_total"/>
			</SPAN>
		</xsl:if>
		<xsl:if test="@missing_total">
			<SPAN class="st">
				<img src="cm/sm.gif"/>: <xsl:value-of select="@missing_total"/>
			</SPAN>
		</xsl:if>
		<xsl:if test="@extra_total">
			<SPAN class="st">
				<img src="cm/sx.gif"/>: <xsl:value-of select="@extra_total"/>
			</SPAN>
		</xsl:if>
		<xsl:if test="@warning_total">
			<SPAN class="st">
				<img src="cm/se.gif"/>: <xsl:value-of select="@warning_total"/>
			</SPAN>
		</xsl:if>
	</xsl:template>

	<xsl:template name="toggle">
		<xsl:choose>
			<xsl:when test="not(@presence) and .//*[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
				<xsl:choose>
					<xsl:when test="local-name() != 'assembly'">
						<img src="cm/tp.gif" class="t"/>
					</xsl:when>
					<xsl:otherwise>
						<img src="cm/tm.gif" class="t"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<img src="cm/tb.gif"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="name">
		<xsl:if test="@name">
			<SPAN class="l"><xsl:value-of select="@name"/></SPAN>
		</xsl:if>
	</xsl:template>

	<xsl:template name="warning-hover">
		<xsl:for-each select="warnings/warning">
			<xsl:text>WARNING: </xsl:text>
			<xsl:value-of select="@text"/>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
