using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

#if !XAMARIN
namespace System.Runtime.Serialization
{
    // | ONCF | [version] | [hash] | [type] | data |
    // 
    // string:      | [length] | [UTF8 encoded data]  |
    // value types: | [BitConverter.ToBytes() result] |

    public class BasicBinarySerializer : BasicSerializer<byte[]>
    {
        private const byte Version = 0x01;
        private byte[] Header = new byte[] { 0x4f, 0x4e, 0x43, 0x46 }; // ONCF
        private PropertyHashDictionary m_propertyDictionary;

        public override TOuptutType Deserialize<TOuptutType>(byte[] data)
        {
            return (TOuptutType)Deserialize(typeof(TOuptutType), data);
        }
       
        public object Deserialize(Type type, byte[] data)
        {
            if (m_propertyDictionary == null)
            {
                m_propertyDictionary = new PropertyHashDictionary();
            }

            // validate header
            for (int i = 0; i < Header.Length; i++)
            {
                if (data[i] != Header[i])
                {
                    throw new ArgumentException("Data does not appear to be properly serialized");
                }

                if (data[4] > Version)
                {
                    throw new ArgumentException("Data serializer version is greater than this serializer supports");
                }
            }

            // create the instance
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
                throw new SerializationException(string.Format("Unable to create instance of type '{0}'.  Is there a parameterless constuctor?", type.Name), ex);
            }

            var pos = 5;
            while (pos < data.Length)
            {
                // get the hash
                var hash = BitConverter.ToInt32(data, pos);
                pos += 4;
                // get the type
                var propertyType = (DataType)data[pos];
                pos ++;

                object value = null;
                switch (propertyType)
                {
                    case DataType.Null:
                        value = null;
                        break;
                    case DataType.String:
                        var length = BitConverter.ToInt32(data, pos);
                        pos += 4;
                        value = Encoding.UTF8.GetString(data, pos, length);
                        pos += length;
                        break;
                    case DataType.Byte:
                        value = data[pos];
                        pos++;
                        break;
                    case DataType.Int16:
                        value = BitConverter.ToInt16(data, pos);
                        pos += 2;
                        break;
                    case DataType.Int32:
                        value = BitConverter.ToInt32(data, pos);
                        pos += 4;
                        break;
                    case DataType.Int64:
                        value = BitConverter.ToInt64(data, pos);
                        pos += 8;
                        break;
                    case DataType.UInt16:
                        value = BitConverter.ToUInt16(data, pos);
                        pos += 2;
                        break;
                    case DataType.UInt32:
                        value = BitConverter.ToUInt32(data, pos);
                        pos += 4;
                        break;
                    case DataType.UInt64:
                        value = BitConverter.ToUInt64(data, pos);
                        pos += 8;
                        break;
                    case DataType.Single:
                        value = BitConverter.ToSingle(data, pos);
                        pos += 4;
                        break;
                    case DataType.Double:
                        value = BitConverter.ToDouble(data, pos);
                        pos += 8;
                        break;
                    case DataType.Boolean:
                        value = data[pos] == 0 ? false : true;
                        pos++;
                        break;
                    case DataType.DateTime:
                        value = new DateTime(BitConverter.ToInt64(data, pos));
                        pos += 8;
                        break;
                    case DataType.TimeSpan:
                        value = new TimeSpan(BitConverter.ToInt64(data, pos));
                        pos += 8;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // get the property
                var prop = m_propertyDictionary.GetPropertyForNameHash(type, hash);
                if (prop != null)
                {
                    // and set its value
                    prop.SetValue(item, value, null);
                }
            }
            return item;
        }

        public override byte[] Serialize(object source, bool includeNonPublicProperties)
        {
            var data = new List<byte>();

            // header and version
            data.AddRange(Header);
            data.Add(Version);

            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (includeNonPublicProperties)
            {
                flags |= BindingFlags.NonPublic;
            }

            var props = source.GetType().GetProperties(flags);

            foreach(var prop in props)
            {
                if (!prop.CanRead) continue;
                if (prop.GetCustomAttributes(typeof(DoNotSerializeAttribute), true).Count() > 0) continue;
                if (!ShouldSerialize(prop)) continue;

                var f = EncodeProperty(prop, prop.GetValue(source, null));
                data.AddRange(f);
            }

            return data.ToArray();
        }

        // this is member level to help reduce garbage generation
        private List<byte> m_propertyBuffer = new List<byte>();

        private byte[] EncodeProperty(PropertyInfo pi, object value)
        {
            m_propertyBuffer.Clear();
            m_propertyBuffer.AddRange(BitConverter.GetBytes( pi.Name.GetHashCode()));

            var type = GetDataType(pi.PropertyType);

            // special case for nulls
            if (value == null)
            {
                m_propertyBuffer.Add((byte)DataType.Null);
            }
            else
            {
                m_propertyBuffer.Add((byte)type);

                switch (type)
                {
                    case DataType.String:
                        var s = value as string;
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(s.Length));
                        m_propertyBuffer.AddRange(Encoding.UTF8.GetBytes(s));
                        break;
                    case DataType.Byte:
                        m_propertyBuffer.Add(Convert.ToByte(value));
                        break;
                    case DataType.Int16:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToInt16(value)));
                        break;
                    case DataType.UInt16:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToUInt16(value)));
                        break;
                    case DataType.Int32:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToInt32(value)));
                        break;
                    case DataType.UInt32:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToUInt32(value)));
                        break;
                    case DataType.Int64:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToInt64(value)));
                        break;
                    case DataType.UInt64:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToUInt64(value)));
                        break;
                    case DataType.Single:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToSingle(value)));
                        break;
                    case DataType.Double:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToDouble(value)));
                        break;
                    case DataType.Boolean:
                        m_propertyBuffer.Add((byte)(Convert.ToBoolean(value) ? 0x01 : 0x00));
                        break;
                    case DataType.DateTime:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(Convert.ToDateTime(value).Ticks));
                        break;
                    case DataType.TimeSpan:
                        m_propertyBuffer.AddRange(BitConverter.GetBytes(((TimeSpan)value).Ticks));
                        break;
                    default:
                        // call any custom serializer in a child class
                        var propData = OnSerialize(pi, value);
                        if (propData == null)
                        {
                            throw new NotSupportedException();
                        }
                        m_propertyBuffer.AddRange(propData);
                        break;
                }
            }
            return m_propertyBuffer.ToArray();
        }

        private enum DataType : byte
        {
            Unknown = 0,
            String = 1,
            Byte = 2,
            Int16 = 3,
            Int32 = 4,
            Int64 = 5,
            Single = 6,
            Double = 7,
            Boolean = 8,
            UInt16 = 9,
            UInt32 = 10,
            UInt64 = 11,
            DateTime = 20,
            TimeSpan = 21,

            Nullable = 0xFB, // not yet supported, but a placeholder
            Generic = 0xFC, // not yet supported, but a placeholder
            Object = 0xFD, // not yet supported, but a placeholder
            Array = 0xFE, // not yet supported, but a placeholder
            Null = 0xFF
        }

        private static DataType GetDataType(Type t)
        {
            var type = DataType.Unknown;

            Switch
                .On<Type>(t)
                .Case(typeof(string), () => { type = DataType.String; })
                .Case(typeof(byte), () => { type = DataType.Byte; })
                .Case(typeof(short), () => { type = DataType.Int16; })
                .Case(typeof(ushort), () => { type = DataType.UInt16; })
                .Case(typeof(int), () => { type = DataType.Int32; })
                .Case(typeof(uint), () => { type = DataType.UInt32; })
                .Case(typeof(long), () => { type = DataType.Int64; })
                .Case(typeof(ulong), () => { type = DataType.UInt64; })
                .Case(typeof(float), () => { type = DataType.Single; })
                .Case(typeof(double), () => { type = DataType.Double; })
                .Case(typeof(bool), () => { type = DataType.Boolean; })
                .Case(typeof(DateTime), () => { type = DataType.DateTime; })
                .Case(typeof(TimeSpan), () => { type = DataType.TimeSpan; })
                ;

            return type;
        }

        private class PropertyHashDictionary
        {
            public Dictionary<Type, Dictionary<int, PropertyInfo>> m_cache = new Dictionary<Type, Dictionary<int, PropertyInfo>>();

            public PropertyInfo GetPropertyForNameHash(Type type, int nameHash)
            {
                if (!m_cache.ContainsKey(type))
                {
                    PropertyInfo pi = null;
                    var typeDictionary = new Dictionary<int, PropertyInfo>();

                    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    var props = type.GetProperties(flags); 
                    foreach (var prop in props)
                    {
                        var propHash = prop.Name.GetHashCode();
                        typeDictionary.Add(propHash, prop);
                        if((pi == null) && (propHash == nameHash))
                        {
                            pi = prop;
                        }
                    }
                    m_cache.Add(type, typeDictionary);
                    return pi;
                }

                if (!m_cache[type].ContainsKey(nameHash)) return null;

                return m_cache[type][nameHash];
            }
        }
    }
}
#endif