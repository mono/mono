using System;
using System.Reflection;
using System.Windows;
using System.Windows.Serialization;
using System.Xml;
using System.Diagnostics;

namespace Mono.Windows.Serialization {
	internal abstract class ParserConsumerBase {
		internal void crunch (XmlTextReader reader) {
			int justClosed = 0;
			XamlParser p = new XamlParser(reader);
			XamlNode n;
			while (true) {
				n = p.GetNextNode();
				if (n == null)
					break;
				Debug.WriteLine(this.GetType() + ": INCOMING " + n.GetType());
				if (n is XamlDocumentStartNode) {
					Debug.WriteLine(this.GetType() + ": document begins");
					// do nothing
				} else if (n is XamlElementStartNode && n.Depth == 0) {
					Debug.WriteLine(this.GetType() + ": element begins as top-level");
					CreateTopLevel(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name);
				} else if (n is XamlElementStartNode && ((XamlElementStartNode)n).propertyObject) {
					Debug.WriteLine(this.GetType() + ": element begins as property value");
					string key = getKeyFromNode(n);
					CreatePropertyObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name, key);
				} else if (n is XamlElementStartNode && ((XamlElementStartNode)n).depPropertyObject) {
					Debug.WriteLine(this.GetType() + ": element begins as dependency property value");
					string key = getKeyFromNode(n);
					CreateDependencyPropertyObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name, key);

				} else if (n is XamlElementStartNode) {
					Debug.WriteLine(this.GetType() + ": element begins");
					string key = getKeyFromNode(n);
					CreateObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name, key);
				} else if (n is XamlPropertyNode && ((XamlPropertyNode)n).PropInfo != null) {
					Debug.WriteLine(this.GetType() + ": normal property begins");
					CreateProperty(((XamlPropertyNode)n).PropInfo);
				} else if (n is XamlPropertyNode && ((XamlPropertyNode)n).DP != null) {
					Debug.WriteLine(this.GetType() + ": dependency property begins");
					DependencyProperty dp = ((XamlPropertyNode)n).DP;
					Type typeAttachedTo = dp.OwnerType;
					string propertyName = ((XamlPropertyNode)n).PropertyName;
					
					CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
				} else if (n is XamlClrEventNode && !(((XamlClrEventNode)n).EventMember is EventInfo)) {
					Debug.WriteLine(this.GetType() + ": delegate property");
					CreatePropertyDelegate(((XamlClrEventNode)n).Value, ((PropertyInfo)((XamlClrEventNode)n).EventMember).PropertyType);
					EndProperty();


				} else if (n is XamlClrEventNode) {
					Debug.WriteLine(this.GetType() + ": event");
					CreateEvent((EventInfo)((XamlClrEventNode)n).EventMember);
					CreateEventDelegate(((XamlClrEventNode)n).Value, ((EventInfo)((XamlClrEventNode)n).EventMember).EventHandlerType);
					EndEvent();

				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.Object){
					Debug.WriteLine(this.GetType() + ": text for object");
					CreateObjectText(((XamlTextNode)n).TextContent);
				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.Property){
					Debug.WriteLine(this.GetType() + ": text for property");
					if (((XamlTextNode)n).keyText != null)
						CreatePropertyReference(((XamlTextNode)n).keyText);
					else
						CreatePropertyText(((XamlTextNode)n).TextContent, ((XamlTextNode)n).finalType);
					EndProperty();
				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.DependencyProperty){
					Debug.WriteLine(this.GetType() + ": text for dependency property");
					if (((XamlTextNode)n).keyText != null)
						CreateDependencyPropertyReference(((XamlTextNode)n).keyText);
					else
						CreateDependencyPropertyText(((XamlTextNode)n).TextContent, ((XamlTextNode)n).finalType);
					EndDependencyProperty();
				} else if (n is XamlPropertyComplexEndNode) {
					Debug.WriteLine(this.GetType() + ": end complex property");
					if (justClosed == 2) {
						EndProperty();
					} else if (justClosed == 1) {
						EndDependencyProperty();
					} else {
						throw new NotImplementedException("justClosed of " + justClosed);
					}
					justClosed = 0;
				} else if (n is XamlLiteralContentNode) {
					Debug.WriteLine(this.GetType() + ": literal content");
					CreateCode(((XamlLiteralContentNode)n).Content);
				} else if (n is XamlElementEndNode) {
					Debug.WriteLine(this.GetType() + ": end element");
					Type ft = ((XamlElementEndNode)n).finalType;
					if (((XamlElementEndNode)n).propertyObject) {
						EndPropertyObject(ft);
						justClosed = 2;
					} else if (((XamlElementEndNode)n).depPropertyObject) {
						EndDependencyPropertyObject(ft);
						justClosed = 1;
					} else {
						EndObject();
					}
				} else if (n is XamlDocumentEndNode) {
					Debug.WriteLine(this.GetType() + ": end document");
					Finish();
				} else {
					throw new Exception("Unknown node " + n.GetType());
				}
			}

		}

		private string getKeyFromNode(XamlNode n)
		{
			// we know that n is a XamlElementStartNode, but don't need that knowledge
			if (n is XamlKeyElementStartNode)
				return ((XamlKeyElementStartNode)n).key;
			else
				return null;
		}

		
		public abstract void CreateTopLevel(Type parent, string className);
		public abstract void CreateObject(Type type, string varName, string key);
		public abstract void CreateProperty(PropertyInfo property);
		public abstract void CreateEvent(EventInfo evt);
		public abstract void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType);
		public abstract void EndDependencyProperty();
		public abstract void CreateObjectText(string text);
		public abstract void CreateEventDelegate(string functionName, Type eventDelegateType);
		public abstract void CreatePropertyDelegate(string functionName, Type propertyType);
		public abstract void CreatePropertyText(string text, Type propertyType);
		public abstract void CreatePropertyReference(string key);
		public abstract void CreateDependencyPropertyText(string text, Type propertyType);
		public abstract void CreateDependencyPropertyObject(Type type, string varName, string key);
		public abstract void CreateDependencyPropertyReference(string key);
		public abstract void CreatePropertyObject(Type type, string varName, string key);
		public abstract void EndDependencyPropertyObject(Type destType);
		public abstract void EndPropertyObject(Type destType);
		public abstract void EndObject();
		public abstract void EndProperty();
		public abstract void EndEvent();
		public abstract void CreateCode(string code);
		public abstract void Finish();

	}
}
