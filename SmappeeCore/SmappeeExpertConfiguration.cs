using System;
using System.Collections.Generic;
using System.Text;

namespace SmappeeCore
{
    /// <summary>
    /// Smappee Expert Configuration
    /// </summary>
    public class SmappeeExpertConfiguration
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SmappeeExpertConfiguration()
        {
            Port = 80;
            LoginPassword = "admin";
        }

        /// <summary>
        /// The address of your Energy Monitor on the home LAN
        /// </summary>
        public string SmappeLocalAddress { get; set; }
        
        /// <summary>
        /// Port exposed
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The Login password (default is admin)
        /// </summary>
        public string LoginPassword { get; set; }
    }
}
