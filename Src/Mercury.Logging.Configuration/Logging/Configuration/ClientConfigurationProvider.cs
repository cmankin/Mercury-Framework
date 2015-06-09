using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mercury.Logging.Configuration
{
    /// <summary>
    /// Provides a <see cref="System.Configuration.Configuration"/> object that can access configuration 
    /// files on a client application using the <see cref="System.Configuration.ConfigurationManager"/>.
    /// </summary>
    public class ClientConfigurationProvider : ConfigurationProvider
    {
        private ConfigurationUserLevel _userLevel = ConfigurationUserLevel.None;
        private readonly string _configExePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Configuration.ClientConfigurationProvider"/> class with the specified configuration user level.
        /// </summary>
        /// <param name="userLevel">The configuration user level.</param>
        public ClientConfigurationProvider(ConfigurationUserLevel userLevel)
            : this(userLevel, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Configuration.ClientConfigurationProvider"/> class with the specified executable path.
        /// </summary>
        /// <param name="exePath">The path of the executable (exe) file.</param>
        public ClientConfigurationProvider(string exePath)
            : this(ConfigurationUserLevel.None, exePath)
        {
        }

        private ClientConfigurationProvider(ConfigurationUserLevel userLevel, string exePath)
        {
            this._userLevel = userLevel;
            this._configExePath = exePath;
        }

        /// <summary>
        /// Gets the user configuration level.
        /// </summary>
        public ConfigurationUserLevel UserLevel
        {
            get { return this._userLevel; }
        }

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        public string ConfigExePath
        {
            get { return this._configExePath; }
        }

        /// <summary>
        /// Gets a value indicating whether the executable (exe) path will be used to open the configuration.  
        /// If an executable path is specified, it will be used.
        /// </summary>
        public bool UseExePath
        {
            get { return !string.IsNullOrEmpty(this._configExePath); }
        }

        /// <summary>
        /// Gets a <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.
        /// </summary>
        /// <returns>A <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.</returns>
        public override System.Configuration.Configuration LoadConfiguration()
        {
            if (this.UseExePath)
                return ConfigurationManager.OpenExeConfiguration(this._configExePath);
            return ConfigurationManager.OpenExeConfiguration(this._userLevel);
        }
    }
}
