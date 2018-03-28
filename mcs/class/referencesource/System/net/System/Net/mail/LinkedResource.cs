using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Runtime.Versioning;

namespace System.Net.Mail
{
    public class LinkedResource : AttachmentBase
    {
        
        internal LinkedResource()
        { }
        
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public LinkedResource(string fileName) :
            base(fileName)
        { }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public LinkedResource(string fileName, string mediaType) :
            base(fileName, mediaType)
        { }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public LinkedResource(string fileName, ContentType contentType) :
            base(fileName, contentType)
        { }

        public LinkedResource(Stream contentStream) :
            base(contentStream)
        { }

        public LinkedResource(Stream contentStream, string mediaType) :
            base(contentStream, mediaType)
        { }

        public LinkedResource(Stream contentStream, ContentType contentType) :
            base(contentStream, contentType)
        { }

        public Uri ContentLink
        {
            get
            {
                return ContentLocation;
            }

            set
            {
                ContentLocation = value;
            }
        }

        public static LinkedResource CreateLinkedResourceFromString(string content){
            LinkedResource a = new LinkedResource();
            a.SetContentFromString(content, null, String.Empty);
            return a;
        }

        public static LinkedResource CreateLinkedResourceFromString(string content, Encoding contentEncoding, string mediaType){
            LinkedResource a = new LinkedResource();
            a.SetContentFromString(content, contentEncoding, mediaType);
            return a;
        }

        public static LinkedResource CreateLinkedResourceFromString(string content, ContentType contentType){
            LinkedResource a = new LinkedResource();
            a.SetContentFromString(content, contentType);
            return a;
        }
    }
}
