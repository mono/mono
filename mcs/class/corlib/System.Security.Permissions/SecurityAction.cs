// SecurityAction.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// <para> Specifies security actions that can be performed using declarative security.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="ECMAOnly" type="note">For information about using declarative security
	///  and security actions, see Partition II of the CLI Specification.</block>
	/// </para>
	/// <block subset="none" type="note">
	/// <para>Declarative security is specified using types derived
	///  from <see cref="!:System.Security.SecurityAttribute" />. The following table describes the attribute targets supported by each of the
	///  security actions.</para>
	/// <list type="table">
	/// <listheader>
	/// <term>Security action</term>
	/// <description>Attribute Targets</description>
	/// </listheader>
	/// <item>
	/// <term> Assert</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> Demand</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> Deny</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> InheritanceDemand</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> LinkDemand</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> PermitOnly</term>
	/// <description>Class, Method</description>
	/// </item>
	/// <item>
	/// <term> RequestMinimum</term>
	/// <description>Assembly</description>
	/// </item>
	/// <item>
	/// <term> RequestOptional</term>
	/// <description>Assembly</description>
	/// </item>
	/// <item>
	/// <term> RequestRefuse</term>
	/// <description>Assembly</description>
	/// </item>
	/// </list>
	/// <para> For additional information on attribute targets, see <see cref="T:System.Attribute" />.</para>
	/// </block>
	/// </remarks>
	public enum SecurityAction {

		/// <summary><para> Specified that all callers are required
		///       to have the permissions specified by the current security attribute.</para><para>This action can be applied to classes and methods.</para></summary>
		Demand = 2,

		/// <summary><para> Specifies that callers of the code performing the assert
		///  need not have the permissions specified by the current security attribute, and
		///  that a check for any such permission can stop after the code that asserted it.
		///  <block subset="none" type="note">An assert can change the default behavior of a security check (such as
		///  that caused by a Demand, LinkDemand, etc.).</block></para><para>This action can be applied to classes and methods.</para><para><block subset="none" type="note">This action should only be used by code that can
		///  assure that its callers cannot manipulate it to abuse the asserted
		///  permission.</block></para></summary>
		Assert = 3,

		/// <summary><para> Specifies that access to the resource or
		///  operation described by the current security attribute be denied to callers, even if they
		///  have been granted permission to access it. <block subset="none" type="note"><see cref="F:System.Security.Permissions.SecurityAction.Deny" />
		///  causes a security check
		///  for the permissions specified by the current security attribute to fail even when
		///  it would otherwise succeed.</block></para><para>This action can be applied to classes and methods.</para></summary>
		Deny = 4,

		/// <summary><para> Specifies that access is limited to only those resources 
		///       or operations specified by the current security attribute, even if the code has been
		///       granted permission to access others. A security check for a permission not
		///       described by the current security attribute fails regardless of whether or not
		///       callers have been granted this permission. </para><para>This action can be applied to classes and methods.</para></summary>
		PermitOnly = 5,

		/// <summary><para> Specifies that the immediate caller be required
		///  to have the specified permissions.</para><para>This action can be applied to classes and methods.</para></summary>
		LinkDemand = 6,

		/// <summary><para> 
		///       Specifies the permissions that a derived class is required to have. When the
		///       target is a class, classes inheriting from the target are required to have the
		///       permissions specified by the current security attribute. When the target is
		///       a method, classes overriding the target are required to have the
		///       permissions specified by the current security attribute.</para><para>This action can be applied to classes and methods. </para></summary>
		InheritanceDemand = 7,

		/// <summary><para>Specifies that the current security attribute describes the minimum permissions required for an assembly
		///       to run.</para><para>This action can be applied to assemblies.</para></summary>
		RequestMinimum = 8,

		/// <summary><para> Specifies that the current security attribute
		///       describes
		///       optional permissions that an assembly can be granted.</para><para>This action can be applied to assemblies.</para></summary>
		RequestOptional = 9,

		/// <summary><para> Specifies that the current security attribute
		///       describes resources or operations that an assembly cannot access.</para><para>This action can be applied to assemblies.</para></summary>
		RequestRefuse = 10,
	} // SecurityAction

} // System.Security.Permissions
