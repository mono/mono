using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CoreClr.Tools;
using Mono.Cecil;

namespace DetectMethodPrivileges
{
	public class HtmlWriter : MethodPrivilegePropagationReportBuilder.ReportWriter
	{
		override public void BeginReport(TextWriter writer)
		{
			writer.WriteLine(@"<html>
				<header>
				<style TYPE='text/css'> <!-- 
				{0}
				--> </style>
				</header>
				<body>
				<ul>", Stylesheet());
		}

		override public void PublicMethod(TextWriter writer, MethodDefinition method, string comment)
		{
			writer.WriteLine("<li><a href='{0}' class='{3}' name='{1}'>{1}</a> - {2}<p></li>",
			                 HRefFor(method),
			                 HtmlSignatureFor(method),
			                 comment,
			                 CssClassFor(comment));
		}

		private string HtmlSignatureFor(MethodDefinition method)
		{
			return HttpUtility.HtmlEncode(SignatureFor(method));
		}

		private string HRefFor(MethodDefinition method)
		{
			return "api:" + HttpUtility.UrlEncode(method.DeclaringType.Module.Assembly.SimpleName() + " " + SignatureFor(method));
		}

		override public string PropagationGraphStringFor(IEnumerable<PropagationReason> stack)
		{
			var sb = new StringBuilder("<br>");
			foreach (var reason in stack)
			{
				if (reason.MethodThatTaintedMe == null)
					sb.AppendFormat("{0}<br>", reason.Explanation);
				else
					sb.AppendFormat("{2} <a class='{3}' href='{0}'>{1}</a>(ML:{4})<br>which ",
					                HRefFor(reason.MethodThatTaintedMe),
					                HtmlSignatureFor(reason.MethodThatTaintedMe),
					                reason.Explanation,
									CssClassFor(reason),
									Moonlight.GetSecurityStatusFor(reason.MethodThatTaintedMe));

			}
			return sb.ToString();
		}

		private string SignatureFor(MethodDefinition method)
		{
			return MethodSignatureProvider.SignatureFor(method);
		}

		string Stylesheet()
		{
			return
				@"body { font-family: lucida console, courier new;}
				a { text-decoration: none; }
				a.available { color: green; }
				a.reviewed { color: lightgray; }
				a.unavailable { color: red; }
				a.criticalTypeMember { color: gray; }
				span.call {}
				span.override { color: blue; }
				span.overriden { color: purle; }";
		}

		string CssClassFor(string comment)
		{
			return "available";
			/*
			if (comment.StartsWith("#unavailable_butreviewed"))
				return "reviewed";
			if (comment.StartsWith("#available"))
				return "available";
			if (comment.Contains(PropagationReasonIsInCriticalType.Default.Explanation))
				return "criticalTypeMember";
			return "unavailable";*/
		}

		string CssClassFor(PropagationReason reason)
		{
			return "";
			/*
			if (reason == PropagationReasonIsInCriticalType.Default)
				return "criticalTypeMember";
			return "";*/
		}

		override public void EndReport(TextWriter writer)
		{
			writer.WriteLine(@"</ul>
				</body>
				</html>");
		}
	}
}