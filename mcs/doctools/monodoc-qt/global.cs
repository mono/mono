// global.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;

	public class Global
	{
		public static string HomeDir = QDir.HomeDirPath ();
		public static string MonoDocDir = QDir.HomeDirPath ()+"/.monodoc";
		public static QPixmap IMono = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/mono.png");
		public static QPixmap IName = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/red.gif");
		public static QPixmap IClass = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/class.gif");
		public static QPixmap IDelegate = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/delegate.gif");
		public static QPixmap IEnum = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/enum.gif");
		public static QPixmap IInterface = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/interface.gif");
		public static QPixmap IStructure = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/structure.gif");
		public static QPixmap IMethod = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/method.gif");
		public static QPixmap IEvent = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/event.gif");
		public static QPixmap IField = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/field.gif");
		public static QPixmap IOperator = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/operator.gif");
		public static QPixmap IProperty = new QPixmap (QDir.HomeDirPath ()+"/.monodoc/images/property.gif");
		public static string Summary = "<summary> Provide a brief (usually one sentence) description of a member or type.";
		public static string Remarks = "<remarks> Provide verbose information for a type or member.";
		
		static string init;

		public static string InitDir
		{
			get { return init; }
			set { init = value; }
		}
		
		static string lastopened;

		public static string LastOpened
		{
			get { return lastopened; }
			set { lastopened = value; }
		}

		static string doc;

		public static string DocDir
		{
			get { return doc; }
			set { doc = value; }
		}
	}
}
