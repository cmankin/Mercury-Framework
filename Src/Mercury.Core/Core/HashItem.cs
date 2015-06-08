using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury
{
    /// <summary>
    /// An object representing an item that has been hashed by a hashing function.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in this item.</typeparam>
    /// <typeparam name="TCode">The type of the hash code for the value.</typeparam>
    public class HashItem<TValue, TCode>
    {
        /// <summary>
        /// Initializes a new instance of the Mercury.HashItem class with the specified values.
        /// </summary>
        /// <param name="value">The raw value of the item that was hashed.</param>
        /// <param name="hashCode">The hash code generated from the item.</param>
        /// <param name="comparer">The IEqualityComparer(of T) comparer to use for value comparisons.</param>
        public HashItem(TValue value, TCode hashCode, IEqualityComparer<TValue> comparer)
        {
            this.Value = value;
            this.HashCode = hashCode;
            this.Comparer = comparer;
        }

        /// <summary>
        /// Gets the equality comparer to use for value comparisons.
        /// </summary>
        public IEqualityComparer<TValue> Comparer { get; private set; }

        /// <summary>
        /// Gets the value of the hash item.
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// Gets the hash code generated for this value.
        /// </summary>
        public TCode HashCode { get; private set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the specified object is equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return HashItem<TValue, TCode>.IsEqual(this, obj as HashItem<TValue, TCode>, false);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>The hashcode generated for this instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Compares the two Mercury.HashItem objects for equality.
        /// </summary>
        /// <param name="x">The first Mercury.HashItem object to compare.</param>
        /// <param name="y">The second Mercury.HashItem object to compare.</param>
        /// <param name="forceValueComparison">A value indicating whether to compare on the 
        /// item value.  The default is a HashItem(Of TValue,TCode).HashCode comparison.</param>
        /// <returns></returns>
        public static bool IsEqual(HashItem<TValue, TCode> x, HashItem<TValue, TCode> y, bool forceValueComparison)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            if (forceValueComparison)
                return IsEqualValue(x.Value, y.Value, x.Comparer);
            else
                return IsEqualHash(x.HashCode, y.HashCode);
        }

        private static bool IsEqualHash(TCode x, TCode y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Equals(y);
        }

        private static bool IsEqualValue(TValue x, TValue y, IEqualityComparer<TValue> comparer)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return comparer.Equals(x, y);
        }
    }
}
