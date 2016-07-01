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
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace OpenNETCF
{
    public static class ValidationExtensions
    {
        public const string UnspecifiedParameterName = "{unspecified}";

        private static Validation AddExceptionInternal(this Validation validation, Exception exception)
        {
            return (validation ?? new Validation()).AddException(exception);
        }

        public static Validation Check(this Validation validation)
        {
            if (validation == null)
            {
                return validation;
            }
            else
            {
                if (validation.Exceptions.Count() == 1)
                {
                    var ex = validation.Exceptions.First();
                    if (Debugger.IsAttached) Debugger.Break();
                    throw new ValidationException(ex.Message, ex);
                }
                else
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    throw new ValidationException("Multiple validation failures", new MultiException(validation.Exceptions));
                }
            }
        }

        public static Validation HasValue<T>(this Validation validation, T? t)
            where T : struct
        {
            return validation.HasValue(t, null);
        }

        public static Validation HasValue<T>(this Validation validation, T? t, string paramName)
            where T : struct
        {
            if (t.HasValue) return validation;

            return validation.AddExceptionInternal(new ArgumentException(paramName ?? UnspecifiedParameterName));
        }

        public static Validation HasNoValue<T>(this Validation validation, T? t)
            where T : struct
        {
            return validation.HasNoValue(t, null);
        }

        public static Validation HasNoValue<T>(this Validation validation, T? t, string paramName)
            where T : struct
        {
            if (!t.HasValue) return validation;

            return validation.AddExceptionInternal(new ArgumentException(paramName ?? UnspecifiedParameterName));
        }

        public static Validation IsNull<T>(this Validation validation, T t)
            where T : class
        {
            return validation.IsNull(t, null);
        }

        public static Validation IsNull<T>(this Validation validation, T t, string paramName)
            where T : class
        {
            if (t == null) return validation;

            return validation.AddExceptionInternal(new ArgumentException(paramName ?? UnspecifiedParameterName));
        }

        public static Validation IsNotNull<T>(this Validation validation, T t)
            where T : class
        {
            return validation.IsNotNull(t, null);
        }

        public static Validation IsNotNull<T>(this Validation validation, T theObject, string paramName)
            where T : class
        {
            if (theObject == null)
            {
                return validation.AddExceptionInternal(new ArgumentNullException(paramName ?? UnspecifiedParameterName));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsNotNull<T>(this Validation validation, T? t)
            where T : struct
        {
            return validation.IsNotNull(t, null);
        }

        public static Validation IsNotNull<T>(this Validation validation, T? theObject, string paramName)
            where T : struct
        {
            if (theObject == null)
            {
                return validation.AddExceptionInternal(new ArgumentNullException(paramName ?? UnspecifiedParameterName));
            }
            else
            {
                return validation;
            }
        }


        public static Validation IsTrue(this Validation validation, bool condition)
        {
            if (!condition)
            {
                return validation.AddExceptionInternal(new ArgumentException());
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsFalse(this Validation validation, bool condition)
        {
            if (condition)
            {
                return validation.AddExceptionInternal(new ArgumentException());
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsNotNullOrEmpty(this Validation validation, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return validation.AddExceptionInternal(new ArgumentNullException());
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsNotNullOrEmpty(this Validation validation, string item, string paramName)
        {
            if (string.IsNullOrEmpty(item))
            {
                return validation.AddExceptionInternal(new ArgumentNullException());
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsPositive(this Validation validation, long value)
        {
            return validation.IsPositive(value, null);
        }

        public static Validation IsPositive(this Validation validation, long value, string paramName)
        {
            if (value <= 0)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(paramName ?? UnspecifiedParameterName, "must be positive, but was " + value.ToString()));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsPositive(this Validation validation, decimal value)
        {
            return validation.IsPositive(value, null);
        }

        public static Validation IsPositive(this Validation validation, decimal value, string paramName)
        {
            if (value <= 0)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(paramName ?? UnspecifiedParameterName, "must be positive, but was " + value.ToString()));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsPositive(this Validation validation, double value)
        {
            return validation.IsPositive(value, null);
        }

        public static Validation IsPositive(this Validation validation, double value, string paramName)
        {
            if (value <= 0)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(paramName ?? UnspecifiedParameterName, "must be positive, but was " + value.ToString()));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsPositiveOrZero(this Validation validation, long value)
        {
            return validation.IsPositiveOrZero(value, null);
        }

        public static Validation IsPositiveOrZero(this Validation validation, long value, string paramName)
        {
            if (value < 0)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(paramName, "must be >= 0, but was " + value.ToString()));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsLessThanOrEqualTo(this Validation validation, long value, long upperLimit)
        {
            return validation.IsLessThanOrEqualTo(value, upperLimit, null);
        }

        public static Validation IsLessThanOrEqualTo(this Validation validation, long value, long upperLimit, string paramName)
        {
            if (value > upperLimit)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be <= {1}, but was {2}", paramName, upperLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsLessThan(this Validation validation, long value, long upperLimit)
        {
            return validation.IsLessThan(value, upperLimit, null);
        }

        public static Validation IsLessThan(this Validation validation, long value, long upperLimit, string paramName)
        {
            if (value >= upperLimit)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be < {1}, but was {2}", paramName, upperLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsGreaterThanOrEqualTo(this Validation validation, long value, long lowerLimit)
        {
            return validation.IsGreaterThanOrEqualTo(value, lowerLimit, null);
        }

        public static Validation IsGreaterThanOrEqualTo(this Validation validation, long value, long lowerLimit, string paramName)
        {
            if (value < lowerLimit)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be >= {1}, but was {2}", paramName, lowerLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsGreaterThan(this Validation validation, long value, long lowerLimit)
        {
            return validation.IsGreaterThan(value, lowerLimit, null);
        }

        public static Validation IsGreaterThan(this Validation validation, long value, long lowerLimit, string paramName)
        {
            if (value <= lowerLimit)
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be > {1}, but was {2}", paramName, lowerLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsWithinBoundsInclusive(this Validation validation, long value, long lowerLimit, long upperLimit)
        {
            return validation.IsWithinBoundsInclusive(value, lowerLimit, upperLimit, null);
        }

        public static Validation IsWithinBoundsInclusive(this Validation validation, long value, long lowerLimit, long upperLimit, string paramName)
        {
            if ((value < lowerLimit) || (value > upperLimit))
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be < {1} and > {2}, but was {3}", paramName ?? UnspecifiedParameterName, upperLimit, lowerLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsWithinBoundsInclusive(this Validation validation, double value, double lowerLimit, double upperLimit)
        {
            return validation.IsWithinBoundsInclusive(value, lowerLimit, upperLimit, null);
        }

        public static Validation IsWithinBoundsInclusive(this Validation validation, double value, double lowerLimit, double upperLimit, string paramName)
        {
            if ((value < lowerLimit) || (value > upperLimit))
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("{0} must be < {1} and > {2}, but was {3}", paramName ?? UnspecifiedParameterName, upperLimit, lowerLimit, value)));
            }
            else
            {
                return validation;
            }
        }

        public static Validation IsPositiveIfNotNull(this Validation validation, decimal? t)
        {
            return validation.IsPositiveIfNotNull(t, null);
        }

        public static Validation IsPositiveIfNotNull(this Validation validation, decimal? t, string paramName)
        {
            if (t.HasValue)
            {
                return validation.IsPositive((long)(t.Value), paramName);
            }

            return validation;
        }

        public static Validation AreNotEqual<T>(this Validation validation, T actual, T expected)
        {
            return validation.AreNotEqual(actual, expected, null);
        }

        public static Validation AreNotEqual<T>(this Validation validation, T actual, T expected, string paramName)
        {
            if (actual.Equals(expected))
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("parameter {0} is {1}, not the expected {2}", paramName ?? UnspecifiedParameterName, actual, expected)));
            }

            return validation;
        }

        public static Validation AreEqual<T>(this Validation validation, T actual, T expected)
        {
            return validation.AreEqual(actual, expected, null);
        }

        public static Validation AreEqual<T>(this Validation validation, T actual, T expected, string paramName)
        {
            if (!actual.Equals(expected))
            {
                return validation.AddExceptionInternal(new ArgumentOutOfRangeException(string.Format("parameter {0} is {1}, not the expected {2}", paramName ?? UnspecifiedParameterName, actual, expected)));
            }

            return validation;
        }

#if !XAMARIN
        public static Validation FileExists(this Validation validation, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return validation.AddExceptionInternal(new FileNotFoundException(string.Format("File '{0}' not found", filePath)));
            }
            return  validation;
        }
#endif
    }

    public sealed class MultiException
        : Exception
    {
        private Exception[] innerExceptions;

        public IEnumerable<Exception> InnerExceptions
        {
            get
            {
                if (this.innerExceptions != null)
                {
                    for (int i = 0; i < this.innerExceptions.Length; ++i)
                    {
                        yield return this.innerExceptions[i];
                    }
                }
            }
        }

        public MultiException()
            : base()
        {
        }

        public MultiException(string message)
            : base()
        {
        }

        public MultiException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.innerExceptions = new Exception[1] { innerException };
        }

        public MultiException(IEnumerable<Exception> innerExceptions)
            : this(null, innerExceptions)
        {
        }

        public MultiException(Exception[] innerExceptions)
            : this(null, (IEnumerable<Exception>)innerExceptions)
        {
        }

        public MultiException(string message, Exception[] innerExceptions)
            : this(message, (IEnumerable<Exception>)innerExceptions)
        {
        }

        public MultiException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions.FirstOrDefault())
        {
            foreach (var item in innerExceptions)
            {
                if (item == null)
                {
                    throw new ArgumentNullException();
                }
            }

            this.innerExceptions = innerExceptions.ToArray();
        }

    }
}
