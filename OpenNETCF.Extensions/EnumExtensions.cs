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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

#if !PCL
namespace System.Text
{
    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : value.ToString();
        }

    }

    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; protected set; }

        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
#endif

#if(ANDROID)

namespace System.Runtime.Serialization
{
    // Summary:
    //     Specifies that the field is an enumeration member and should be serialized.
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        // Summary:
        //     Initializes a new instance of the System.Runtime.Serialization.EnumMemberAttribute
        //     class.
        public EnumMemberAttribute()
        {

        }

        // Summary:
        //     Gets or sets the value associated with the enumeration member the attribute
        //     is applied to.
        //
        // Returns:
        //     The value associated with the enumeration member.
        public string Value { get; set; }
    }
}

#endif
