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

namespace OpenNETCF
{
    public class MovingAverage<T>  : IEnumerable<T>
        where T : struct
    {
        private int m_maxSamples;
        private List<T> m_samples = new List<T>();
        private T m_default;
        private object m_syncRoot = new object();

        public MovingAverage(int samplesToAverage)
            : this(samplesToAverage, default(T))
        {
        }

        public MovingAverage(int samplesToAverage, T defaultWhenEmpty)
        {
            m_maxSamples = samplesToAverage;
            m_samples = new List<T>(m_maxSamples);
            m_default = defaultWhenEmpty;
        }

        public object SyncRoot
        {
            get { return m_syncRoot; }
        }

        public void Reset()
        {
            lock (m_syncRoot)
            {
                m_samples.Clear();
            }
        }

        public int MaxSamples
        {
            get { return m_maxSamples; }
            set
            {
                m_maxSamples = value;
                lock (m_syncRoot)
                {
                    while (m_samples.Count > m_maxSamples)
                    {
                        m_samples.RemoveAt(0);
                    }
                }
            }
        }

        public int SampleCount
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_samples.Count;
                }
            }
        }

        public T MostRecent
        {
            get
            {
                lock (m_syncRoot)
                {
                    if (m_samples.Count == 0) return m_default;

                    return m_samples[m_samples.Count - 1];
                }
            }
        }

        public double GetBiasedAverage(double bias)
        {
            lock (m_syncRoot)
            {
                var sum = m_samples.Sum(t => Convert.ToDouble(t)) + (m_samples.Count * bias);
                return sum / m_samples.Count;
            }
        }

        public double Average
        {
            get
            {
                lock (m_syncRoot)
                {
                    if (m_samples.Count == 0) return Convert.ToDouble(m_default);

                    return m_samples.Sum(t => Convert.ToDouble(t)) / m_samples.Count;
                }
            }
        }

        public void NewValue(T value)
        {
            lock (m_samples)
            {
                m_samples.Add(value);
                while (m_samples.Count > m_maxSamples)
                {
                    m_samples.RemoveAt(0);
                }
            }
        }

        public T this[int index]
        {
            get 
            {
                lock (m_syncRoot)
                {
                    return m_samples[index];
                }
            }
            set
            {
                lock (m_syncRoot)
                {
                    m_samples[index] = value;
                }
            }

        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (m_syncRoot)
            {
                return m_samples.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
