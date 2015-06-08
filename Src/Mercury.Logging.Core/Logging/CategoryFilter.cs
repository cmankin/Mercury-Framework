using System;
using System.Collections.Generic;
using Mercury.Logging.Configuration;

namespace Mercury.Logging
{
    /// <summary>
    /// Provides filtering support by specified categories.
    /// </summary>
    public class CategoryFilter
        : LogFilter, IAddChild
    {
        /// <summary>
        /// Initializes a default instance of the <see cref="Mercury.Logging.CategoryFilter"/> class.
        /// </summary>
        public CategoryFilter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.CategoryFilter"/> class.
        /// </summary>
        /// <param name="isInclude">A value indicating whether the categories 
        /// on this filter comprise an inclusive list.</param>
        public CategoryFilter(bool isInclude)
        {
            this.IsInclusive = isInclude;
        }

        private HashSet<string> _categories = new HashSet<string>();

        /// <summary>
        /// Gets a value indicating whether the categories on this filter comprise an inclusive list.
        /// </summary>
        /// <returns>True if the categories on this filter comprise an inclusive list; otherwise, false.</returns>
        public bool IsInclusive { get; set; }

        void IAddChild.AddChild(object child)
        {
            this.AddCategory((string)child);
        }

        /// <summary>
        /// Adds the specified category to this filter.
        /// </summary>
        /// <param name="category">The category to add.</param>
        public void AddCategory(string category)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            this._categories.Add(category);
        }

        /// <summary>
        /// Removes all categories from this filter.
        /// </summary>
        public void Clear()
        {
            this._categories.Clear();
        }

        /// <summary>
        /// Removes the specified category from this filter.
        /// </summary>
        /// <param name="category">The category to remove.</param>
        public void RemoveCategory(string category)
        {
            this._categories.Remove(category);
        }

        /// <summary>
        /// Returns a value indicating whether the specified log entry can pass the filter.
        /// </summary>
        /// <param name="entry">The log entry to test.</param>
        /// <returns>True if the specified log entry can pass the filter; otherwise, false.</returns>
        public override bool Allow(LogEntry entry)
        {
            var data = entry.FilterData != null ? entry.FilterData.ToString() : null;
            if (this.IsInclusive)
            {
                if (data == null)
                    return false;
                return this._categories.Contains(data);
            }
            else
            {
                if (data == null)
                    return true;
                return (!this._categories.Contains(data));
            }
        }
    }
}
