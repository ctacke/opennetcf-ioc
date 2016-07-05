// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://oncfext.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;

namespace System.Xml.Linq
{
    public static class XElementExtensions
    {
        public static XElement AddChildElement(this XElement e, XElement child)
        {
            e.Add(child);
            return e;
        }

        public static XElement AddAttribute(this XElement e, string name, string value)
        {
            e.SetAttributeValue(name, value);
            return e;
        }

        public static XElement AddAttribute(this XElement e, string name, int value)
        {
            e.SetAttributeValue(name, value);
            return e;
        }

        public static XElement AddAttribute(this XElement e, XAttribute arribute)
        {
            e.SetAttributeValue(arribute.Name, arribute.Value);
            return e;
        }

        public static XElement AddAttributeIfHasValue(this XElement e, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                e.SetAttributeValue(name, value);
            }
            return e;
        }

        public static XElement AddAttributeIfHasValue<T>(this XElement e, string name, T? value)
            where T : struct
        {
            if (value.HasValue)
            {
                e.SetAttributeValue(name, value.Value.ToString());
            }
            return e;
        }

        public static XElement AddAttributeIfHasValue<T>(this XElement e, string name, T value)
            where T : class
        {
            if (value != null)
            {
                e.SetAttributeValue(name, value.ToString());
            }
            return e;
        }

        public static string AttributeValue(this XElement e, string attributeName)
        {
            var attr = e.Attribute(attributeName);
            if (attr == null) return null;
            return attr.Value;
        }

        public static TimeSpan? AttributeValueAsNullableTimeSpan(this XElement e, string attributeName)
        {
            var attr = e.Attribute(attributeName);
            if (attr == null) return null;
            return TimeSpan.Parse(attr.Value);
        }

        public static string ToString(this XDocument d, bool includeDeclaration)
        {
            if (!includeDeclaration) return d.ToString();

            StringBuilder sb = new StringBuilder(2048);
            using (var sw = new StringWriter<UTF8Encoding>(sb))
            {
                try
                {
                    d.Save(sw);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return sb.ToString();

        }

        private class StringWriter<T> : StringWriter
            where T : Encoding, new()
        {
            public StringWriter(StringBuilder builder)
                : base(builder)
            {
            }

            public override Encoding Encoding
            {
                get { return new T(); }
            }
        }

#if!PCL
        public static T XmlDeserialize<T>(this XElement element)
        {
            if (element == null) return default(T);

            return (element.ToString()).DeserializeFromXml<T>();
        }
#endif
#if !(WINDOWS_PHONE || PCL)
        public static XmlNode AsXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
#endif
    }
}
