using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Infrastructure.Configuration
{
    public class StorageSettings
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string LocalPath { get; set; }

        public StorageSettings(string provider, string connectionString, string containerName, string localPath)
        {
            Provider = provider;
            ConnectionString = connectionString;
            ContainerName = containerName;
            LocalPath = localPath;
        }
    }
}