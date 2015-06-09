using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Provides a <see cref="System.Configuration.Configuration"/> object that can be used to access the configuration file. 
    /// </summary>
    public abstract class ConfigurationProvider
    {
        /// <summary>
        /// Gets a <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.
        /// </summary>
        /// <returns>A <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.</returns>
        public abstract System.Configuration.Configuration LoadConfiguration();
    }
}
