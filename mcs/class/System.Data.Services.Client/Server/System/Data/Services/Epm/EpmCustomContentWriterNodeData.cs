//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Common
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Data.Services.Client;

    internal sealed class EpmCustomContentWriterNodeData : IDisposable
    {
        private bool disposed;

        internal EpmCustomContentWriterNodeData(EpmTargetPathSegment segment, object element)
        {
            this.XmlContentStream = new MemoryStream();
            XmlWriterSettings customContentWriterSettings = new XmlWriterSettings();
            customContentWriterSettings.OmitXmlDeclaration = true;
            customContentWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
            this.XmlContentWriter = XmlWriter.Create(this.XmlContentStream, customContentWriterSettings);
            this.PopulateData(segment, element);
        }

        internal EpmCustomContentWriterNodeData(EpmCustomContentWriterNodeData parentData, EpmTargetPathSegment segment, object element)
        {
            this.XmlContentStream = parentData.XmlContentStream;
            this.XmlContentWriter = parentData.XmlContentWriter;
            this.PopulateData(segment, element);

        }

        internal MemoryStream XmlContentStream
        {
            get;
            private set;
        }

        internal XmlWriter XmlContentWriter
        {
            get;
            private set;
        }

        internal String Data
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                if (this.XmlContentWriter != null)
                {
                    this.XmlContentWriter.Close();
                    this.XmlContentWriter = null;
                }

                if (this.XmlContentStream != null)
                {
                    this.XmlContentStream.Dispose();
                    this.XmlContentStream = null;
                }

                this.disposed = true;
            }
        }

        internal void AddContentToTarget(XmlWriter target)
        {
            this.XmlContentWriter.Close();
            this.XmlContentWriter = null;
            this.XmlContentStream.Seek(0, SeekOrigin.Begin);
            XmlReaderSettings customContentReaderSettings = new XmlReaderSettings();
            customContentReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlReader reader = XmlReader.Create(this.XmlContentStream, customContentReaderSettings);
            this.XmlContentStream = null;
            target.WriteNode(reader, false);
        }

        private void PopulateData(EpmTargetPathSegment segment, object element)
        {
            if (segment.EpmInfo != null)
            {
                Object propertyValue;

                try
                {
                   propertyValue = segment.EpmInfo.PropValReader.DynamicInvoke(element);

                }
                catch
                (System.Reflection.TargetInvocationException)
                {
                    throw;
                }

               this.Data = propertyValue == null ? String.Empty : ClientConvert.ToString(propertyValue, false );

            }
        }
    }
}