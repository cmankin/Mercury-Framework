using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mercury.Logging
{
    /// <summary>
    /// Represents a collection of System.Diagnostics.TraceSource objects.
    /// </summary>
    public class TraceCollection 
        : IEnumerable<TraceSource>
    {
        /// <summary>
        /// Initializes a default instance of the TraceCollection class.
        /// </summary>
        public TraceCollection()
        {
            this._traces = new Dictionary<string, TraceSource>();
        }

        private Dictionary<string, TraceSource> _traces;

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return this._traces.Count; }
        }

        /// <summary>
        /// Gets the trace source with the specified name.
        /// </summary>
        /// <param name="name">The name of the trace source to find.</param>
        /// <returns>The trace source with the specified name or null.</returns>
        public TraceSource this[string name]
        {
            get { return this.GetTrace(name); }
        }

        /// <summary>
        /// Gets an enumerable collection of names for the System.Diagnostics.TraceSource objects in this collection.
        /// </summary>
        public IEnumerable<string> Names
        {
            get { return this._traces.Keys.AsEnumerable<string>(); }
        }
        
        /// <summary>
        /// Returns the TraceSource with the specified name.
        /// </summary>
        /// <param name="name">The name of the TraceSource to find.</param>
        /// <returns>The TraceSource with the specified name or null.</returns>
        public TraceSource GetTrace(string name)
        {
            if (this._traces.ContainsKey(name))
                return this._traces[name];
            return null;
        }

        /// <summary>
        /// Removes the specified TraceSource.
        /// </summary>
        /// <param name="name">The name of the TraceSource to remove.</param>
        public void RemoveTrace(string name)
        {
            if (this._traces.ContainsKey(name))
                this._traces.Remove(name);
        }

        /// <summary>
        /// Attempts to add the specified TraceSource to this collection.
        /// </summary>
        /// <param name="ts">The TraceSource to add.</param>
        /// <returns>True if the TraceSource was added to the collection; otherwise, false.</returns>
        public bool TryAddTrace(TraceSource ts)
        {
            if (!this._traces.ContainsKey(ts.Name))
            {
                this._traces.Add(ts.Name, ts);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the TraceSource with the specified name.
        /// </summary>
        /// <param name="name">The name of the TraceSource to find.</param>
        /// <param name="ts">Out. The found TraceSource or null.</param>
        /// <returns>True if the TraceSource was found; otherwise, false.</returns>
        public bool TryGetTrace(string name, out TraceSource ts)
        {
            ts = null;
            if (this._traces.ContainsKey(name))
            {
                ts = this._traces[name];
                return true;
            }
            return false;
        }

        #region IEnumerable
        /// <summary>
        /// Returns a TraceSource enumerator for this collection.
        /// </summary>
        /// <returns>A TraceSource enumerator.</returns>
        public IEnumerator<TraceSource> GetEnumerator()
        {
            return new TraceCollectionEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region Enumerator
        /// <summary>
        /// A private enumerator class for a TraceCollection.
        /// </summary>
        private class TraceCollectionEnumerator
            : IEnumerator<TraceSource>
        {
            private int iPos = -1;
            private TraceSource[] values;

            public TraceCollectionEnumerator(TraceCollection collection)
            {
                this.values = collection._traces.Values.ToArray<TraceSource>();
            }

            public TraceSource Current
            {
                get { return this.values[iPos]; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (this.iPos < this.values.Length - 1)
                {
                    this.iPos++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                this.iPos = -1;
            }

            private bool _disposedValue;
            public void Dispose()
            {
                Dispose(true);
            }

            protected void Dispose(bool disposing)
            {
                if (!this._disposedValue)
                {
                    if (disposing)
                    {
                    }
                }
                this._disposedValue = true;
            }
        }
        #endregion
    }
}
