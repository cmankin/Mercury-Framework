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
        private readonly string _configurationFilePath;
        private ConfigurationFileMap _configMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Configuration.ClientConfigurationProvider"/> class with the specified configuration user level.
        /// </summary>
        /// <param name="userLevel">The configuration user level.</param>
        public ClientConfigurationProvider(ConfigurationUserLevel userLevel)
            : this(userLevel, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mercury.Logging.Configuration.ClientConfigurationProvider"/> class with the specified congiguration file path.
        /// </summary>
        /// <param name="configPath">The path to the configuration file.</param>
        public ClientConfigurationProvider(string configPath)
            : this(ConfigurationUserLevel.None, configPath)
        {
        }

        private ClientConfigurationProvider(ConfigurationUserLevel userLevel, string configPath)
        {
            this._userLevel = userLevel;
            this._configurationFilePath = configPath;
            if (!string.IsNullOrEmpty(configPath))
            {
                this._configMap = new ConfigurationFileMap();
                this._configMap.MachineConfigFilename = configPath;
            }
        }

        /// <summary>
        /// Gets the user configuration level.
        /// </summary>
        public ConfigurationUserLevel UserLevel
        {
            get { return this._userLevel; }
        }

        /// <summary>
        /// Gets the path to the configuration file.
        /// </summary>
        public string ConfigurationFilePath
        {
            get { return this._configurationFilePath; }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration file path will be used to open the configuration.  
        /// If a configuration file path is specified, it will be used instead of the user level.
        /// </summary>
        public bool UseFilePath
        {
            get { return !string.IsNullOrEmpty(this._configurationFilePath); }
        }

        /// <summary>
        /// Gets a <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.
        /// </summary>
        /// <returns>A <see cref="System.Configuration.Configuration"/> object used to access a specific configuration file.</returns>
        public override System.Configuration.Configuration LoadConfiguration()
        {
            if (this.UseFilePath)
            {
                if (this._configMap == null)
                    throw new InvalidOperationException(string.Format(Strings.Config_file_map_not_set_1, this._configurationFilePath));
                return ConfigurationManager.OpenMappedMachineConfiguration(this._configMap);
            }
            return ConfigurationManager.OpenExeConfiguration(this._userLevel);
        }
    }
}
