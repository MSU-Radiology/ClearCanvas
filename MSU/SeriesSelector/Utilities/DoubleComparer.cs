#region License

// Modifications: Copyright (c) 2013, Michigan State University
// Author: Matt Latourette
// All rights reserved.
//
// These modifications are released under the terms of the GNU General Public 
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
//
// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;

// Code taken from:
// ClearCanvas.ImageViewer.Mathematics.FloatComparer, which is itself taken from
// http://www.windojitsu.com/code/floatcomparer.html
//
// Modified to facilitate comparison of double precision floating point numbers
// 
// This modification may not be necessary anymore, due to the addition of features
// for dealing with double precision numbers in the FloatComparer class which were
// not available at the time this modified class was created.  It may be possible 
// to replace usage of DoubleComparer with FloatComparer now, but I haven't had the 
// time to investigate this yet, so DoubleComparer remains in use for now.
namespace ClearCanvas.MSU.SeriesSelector.Utilities
{
    /// <summary>
    /// A utility class to facilitate comparison of doubles.
    /// </summary>
    public static class DoubleComparer
    {
        public const long DefaultRelativeTolerance = 100;

        /// <summary>
        /// Performs an equality comparison for double precision floating point numbers that takes numerical error
        /// into account.  Values within the specified tolerance are considered to be equal for the comparison test.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <param name="tolerance">Relative tolerance. Required. Long.</param>
        /// <returns><list>
        /// <item> 0 if left = right &#177; tolerance ULPs</item>
        /// <item>+1 if left &gt; right + tolerance ULPs</item>
        /// <item>-1 if left &lt; right - tolerance ULPs</item>
        /// </list></returns>
        /// <remarks>Look <a href="http://www.windojitsu.com/code/floatcomparer.html">here</a> for details on floating 
        /// point comparison. The relative tolerance specifies the maximum number of representable floating point values 
        /// that may exist between two operands considered equivalent within numerical tolerance.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tolerance"/> is negative or in excess of
        /// the limit of the significand in a double-precision floating point value.</exception>
        public static long Compare(double left, double right, long tolerance)
        {
            long dummy;
            return Compare(left, right, tolerance, out dummy);
        }

        /// <summary>
        /// Compares two doubles with a specified tolerance.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <param name="tolerance">Relative tolerance. Required. Long. </param>
        /// <param name="difference">Number of representable values between <paramref name="left"/> and <paramref name="right"/>. 
        /// Required. Long.</param>
        /// <returns><list>
        /// <item> 0 if left = right &#177; tolerance ULPs</item>
        /// <item>+1 if left &gt; right + tolerance ULPs</item>
        /// <item>-1 if left &lt; right - tolerance ULPs</item>
        /// </list></returns>
        /// <remarks>Look <a href="http://www.windojitsu.com/code/floatcomparer.html">here</a> for details on floating 
        /// point comparison. The relative tolerance specifies the maximum number of representable floating point values 
        /// that may exist between two operands considered equivalent within numerical tolerance.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tolerance"/> is negative or in excess of
        /// the limit of the significand, which happens to be 53 bits for IEEE 754 double-precision floating point 
        /// values.</exception>
        public static long Compare(double left, double right, long tolerance, out long difference)
        {
            // Ensures that maxUlps (maximum units in the last place) is non-negative and small enought that the 
            // default NaN will never compare as equal to anything.
            if (tolerance < 0 || tolerance >= 0x0020000000000000L)
            {
                throw new ArgumentOutOfRangeException("tolerance", "Tolerance must be in the range [0x0, 0x001FFFFFFFFFFFFF]");
            }

            // Reinterpret float bits as sign-magnitude integers.
            long xi = BitReinterpreter.Convert(left);
            long yi = BitReinterpreter.Convert(right);

            // Transform xi and yi so they are lexicographically ordered as twos-complement integers
            // Note:  Int64.MinValue is 0x8000000000000000
            if (xi < 0)
            {
                xi = Int64.MinValue - xi;
            }
            if (yi < 0)
            {
                yi = Int64.MinValue - yi;
            }

            // How many epsilons apart?
            difference = xi - yi;

            // Is the difference outside our tolerance?
            if (xi > yi + tolerance)
            {
                return +1;
            }
            if (xi < yi - tolerance)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Returns a value indicating whether two values are equal within a specified absolute tolerance.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <param name="tolerance">Absolute numerical tolerance. Required. Double.</param>
        /// <returns>True if left and right are equivalent within the specified numerical tolerance, false otherwise.</returns>
        public static bool AreEqual(double left, double right, double tolerance)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Returns a value indicating whether two values are equal within a specified tolerance in units of the number of
        /// representable floating point values by which the two values may differ while still being treated as equal.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <param name="tolerance">Relative tolerance. Required. Long.</param>
        /// <returns>True if left and right are equivalent within the specified tolerance, false otherwise.</returns>
        /// <remarks>
        /// Uses <see cref="Compare(double,double,long)"/> to perform the comparison.
        /// </remarks>
        public static bool AreEqual(double left, double right, long tolerance)
        {
            long dummy;
            long result = Compare(left, right, tolerance, out dummy);

            if (result == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether two values are equal within a specified tolerance in units of the number of 
        /// representable floating point values by which the two values may differ while still being treated as equal.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <returns>True if left and right are equivalent within the default relative tolerance, false otherwise.</returns>
        /// <remarks>
        /// Uses <see cref="Compare(double,double,long)"/> to perform the comparison.  Assumes the default relative tolerance.
        /// </remarks>
        public static bool AreEqual(double left, double right)
        {
            long dummy;
            long result = Compare(left, right, DefaultRelativeTolerance, out dummy);

            if (result == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether left is greater than right.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <returns>True if <paramref name="left"/> &gt; <paramref name="right"/> by more than the default relative tolerance, 
        /// false otherwise.</returns>
        /// <remarks>
        /// Uses <see cref="Compare(double,double,long)"/> to perform the comparison.  Assumes the default relative tolerance.
        /// </remarks>
        public static bool IsGreaterThan(double left, double right)
        {
            long dummy;
            long result = Compare(left, right, DefaultRelativeTolerance, out dummy);

            if (result == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether left is less than right.
        /// </summary>
        /// <param name="left">Left operand. Required. Double.</param>
        /// <param name="right">Right operand. Required. Double.</param>
        /// <returns>True if <paramref name="left"/> &lt; <paramref name="right"/> by more than the default relative tolerance,
        /// false otherwise.</returns>
        /// <remarks>
        /// Uses <see cref="Compare(double,double,long)"/> to perform the comparison.  Assumes a tolerance of 100.
        /// </remarks>
        public static bool IsLessThan(double left, double right)
        {
            long dummy;
            long result = Compare(left, right, DefaultRelativeTolerance, out dummy);

            if (result == -1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Struct that represents a 64-bit value and permits interpretation of the value as either a double precision floating
        /// point number or as a long integer
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct BitReinterpreter
        {
            public static long Convert(double doubleValue)
            {
                BitReinterpreter br = new BitReinterpreter(doubleValue);
                return br.longValue;
            }

            public static double Convert(long longValue)
            {
                BitReinterpreter br = new BitReinterpreter(longValue);
                return br.doubleValue;
            }

            [FieldOffset(0)] readonly double doubleValue;

            [FieldOffset(0)] readonly long longValue;

            private BitReinterpreter(double doubleValue)
            {
                this.longValue = 0; 
                this.doubleValue = doubleValue;
            }

            private BitReinterpreter(long longValue)
            {
                this.doubleValue = 0; 
                this.longValue = longValue;
            }
        }
    }
}