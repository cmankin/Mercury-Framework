using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Configuration
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class LineInfoAttribute
        : Attribute
    {
        private int _line;
        private int _column;

        public LineInfoAttribute(int line, int column)
        {
            this._line = line;
            this._column = column;
        }

        public int Line
        {
            get { return this._line; }
        }

        public int Column
        {
            get { return this._column; }
        }
    }
}
