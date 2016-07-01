using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace System
{
    public static class Metrics
    {
        private static Stopwatch m_sw = new Stopwatch();

        [Conditional("DEBUG")]
        public static void Start(string message)
        {
            Debug.WriteLine(message);
            Start();
        }

        [Conditional("DEBUG")]
        public static void Start()
        {
            m_sw.Reset();
        }

        [Conditional("DEBUG")]
        public static void Checkpoint(string message)
        {
            Debug.WriteLine(message + " " + m_sw.ElapsedMilliseconds + "ms");
            m_sw.Reset();
            m_sw.Start();
        }

        [Conditional("DEBUG")]
        public static void Checkpoint(int threshold, string message)
        {
            if (m_sw.ElapsedMilliseconds >= threshold)
            {
                Debug.WriteLine(message + " " + m_sw.ElapsedMilliseconds + "ms");
            }
            m_sw.Reset();
            m_sw.Start();
        }
    }
}
