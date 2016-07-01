using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics;
#if !XAMARIN
namespace System.Runtime.Serialization
{
    public class BasicXmlSerializer : BasicSerializer<XElement>
    {
        public override TOuptutType Deserialize<TOuptutType>(XElement element)
        {
            return (TOuptutType)Deserialize(typeof(TOuptutType), element);
        }

        public T Deserialize<T>(string xml)
        {
            return (T)Deserialize(typeof(T), XElement.Parse(xml));
        }

        public T Deserialize<T>(XDocument document)
        {
            return (T)Deserialize(typeof(T), document);
        }
        
        public object Deserialize(Type type, XDocument document)
        {
            var nodeName = type.Name;

            var element = document.Element(nodeName);
            if (element == null) throw new SerializationException(string.Format("No child node with name '{0}' found", nodeName));

            return Deserialize(type, element);
        }

        public object Deserialize(Type type, XElement element)
        {
            var nodeName = type.Name;

            if (element.Name != nodeName)
            {
                //throw new SerializationException(string.Format("Element is not named '{0}'", nodeName));
            }

            object item;

            try
            {
#if WindowsCE
                // CF doesn't support 'Activator.CreateInstance' with two parameters
                var ctor = type.GetDefaultConstructor(true);
                item = ctor.Invoke(null);
#else
                item = Activator.CreateInstance(type, true);
#endif
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to create instance of type '{0}'.  Is there a parameterless constructor?", type.Name), ex);
            }

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var props = item.GetType().GetProperties(flags);
            string intermediate;

            foreach (var prop in props)
            {
                var attrib = element.Attribute(prop.Name);

                if (attrib != null)
                {
                    if (prop.PropertyType.IsEnum)
                    {
                        prop.SetValue(item, Enum.Parse(prop.PropertyType, attrib.Value, true), null);
                    }
                    else
                    {
                        string typeName;

                        if (prop.PropertyType.IsNullable())
                        {
                            typeName = prop.PropertyType.GetGenericArguments()[0].Name;

                            if (attrib.Value.IsNullOrEmpty())
                            {
                                if (typeName == "String")
                                {
                                    prop.SetValue(item, attrib.Value, null);
                                }
                                else
                                {
                                    prop.SetValue(item, null, null);
                                }
                                continue;
                            }
                        }
                        else
                        {
                            typeName = prop.PropertyType.Name;
                        }

                        switch (typeName)
                        {
                            case "String":
                                prop.SetValue(item, attrib.Value, null);
                                break;
                            case "Boolean":
                                prop.SetValue(item, bool.Parse(attrib.Value), null);
                                break;
                            case "DateTime":
                                prop.SetValue(item, DateTime.Parse(attrib.Value), null);
                                break;
                            case "TimeSpan":
                                prop.SetValue(item, TimeSpan.Parse(attrib.Value), null);
                                break;
                            case "Byte":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, byte.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, byte.Parse(intermediate), null);
                                }
                                break;
                            case "Single":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, Single.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, Single.Parse(intermediate), null);
                                }
                                break;
                            case "Double":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, double.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, double.Parse(intermediate), null);
                                }
                                break;
                            case "Int16":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, short.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, short.Parse(intermediate), null);
                                }
                                break;
                            case "UInt16":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, ushort.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, ushort.Parse(intermediate), null);
                                }
                                break;
                            case "Int32":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, int.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else 
                                {
                                    prop.SetValue(item, int.Parse(intermediate), null);
                                }
                                break;
                            case "UInt32":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, uint.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, uint.Parse(intermediate), null);
                                }
                                break;
                            case "Int64":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, long.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, long.Parse(intermediate), null);
                                }
                                break;
                            case "UInt64":
                                intermediate = attrib.Value;
                                if (attrib.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    prop.SetValue(item, ulong.Parse(intermediate, System.Globalization.NumberStyles.HexNumber), null);
                                }
                                else
                                {
                                    prop.SetValue(item, ulong.Parse(intermediate), null);
                                }
                                break;
                            default:
                                throw new SerializationException("Unsupported property type: " + prop.PropertyType.Name);
                        }
                    }

                }
                else
                {
                    var child = element.Element(prop.Name);

                    if (child != null)
                    {
                        //var childType = Type.GetType();
                        var p = Deserialize(prop.PropertyType, child);
                        prop.SetValue(item, p, null);
                    }

                }
            }

            return item;
        }

        public override XElement Serialize(object source, bool includeNonPublicProperties)
        {
            return Serialize(source, null, includeNonPublicProperties);
        }

        public XElement Serialize(object source, string objectName, bool includeNonPublicProperties)
        {
            XElement element;
            var flags = BindingFlags.Instance | BindingFlags.Public;
            if(includeNonPublicProperties) 
            {
                flags |= BindingFlags.NonPublic;
            }

            var props = source.GetType().GetProperties(flags);

            var type = source.GetType();

            string nodeName;
            if(objectName == null)
            {
                if (type.IsGenericType)
                {
                    nodeName = type.Name.CropAtLast('`');
                }
                else
                {
                    nodeName = type.Name;
                }            
            }
            else
            {
                nodeName = objectName;
            }

            element = new XElement(nodeName);

            foreach (var prop in props)
            {
                string name = prop.Name;
                string value = null;
                bool valIsElement = false;

                if (!prop.CanRead) continue;
                if (prop.GetCustomAttributes(typeof(DoNotSerializeAttribute), true).Count() > 0) continue;

                if(prop.PropertyType.IsEnum)
                {
                    value = prop.GetValue(source, null).ToString();
                }
                else if (prop.PropertyType.IsArray)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    string typeName;

                    if (prop.PropertyType.IsNullable())
                    {
                        typeName = prop.PropertyType.GetGenericArguments()[0].Name;
                    }
                    else
                    {
                        typeName = prop.PropertyType.Name;
                    }

                    switch (typeName)
                    {
                        case "String":
                        case "Boolean":
                        case "Byte":
                        case "TimeSpan":
                        case "Single":
                        case "Double":
                        case "Int16":
                        case "UInt16":
                        case "Int32":
                        case "UInt32":
                        case "Int64":
                        case "UInt64":
                            value = (prop.GetValue(source, null) ?? string.Empty).ToString();
                            break;
                        case "DateTime":
                            try
                            {
                                var tempDT = Convert.ToDateTime(prop.GetValue(source, null));
                                if (tempDT == DateTime.MinValue) continue;
                                value = tempDT.ToString("MM/dd/yyyy HH:mm:ss.fffffff");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("BasicXmlSerializer: Error parsing DateTime: " + ex.Message);
                                continue;
                            }
                            break;
                        default:
                            var o = prop.GetValue(source, null);
                            XElement child;
                            if (o == null)
                            {
                                child = new XElement(prop.Name);
                            }
                            else
                            {
                                child = Serialize(o, prop.Name, includeNonPublicProperties);
                            }

                            element.Add(child);
                            valIsElement = true;
                            break;
                    }
                }

                if ((!valIsElement) && (value != null))
                {
                    element.AddAttribute(name, value);
                }
            }

            return element;
        }
    }
}
#endif