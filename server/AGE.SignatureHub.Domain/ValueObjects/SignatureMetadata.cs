using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Domain.ValueObjects
{
    public class SignatureMetadata
    {
        public string IpAddress { get; private set; } = string.Empty;
        public string UserAgent { get; private set; } = string.Empty;
        public string DeviceInfo { get; private set; } = string.Empty;
        public string Location { get; private set; } = string.Empty;
        public string DocumentHash { get; private set; } = string.Empty;

        private SignatureMetadata() { }

        public SignatureMetadata(
                string ipAddress,
                string userAgent,
                string deviceInfo,
                string location,
                string documentHash
            )
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            UserAgent = userAgent;
            DeviceInfo = deviceInfo;
            Location = location;
            DocumentHash = documentHash ?? throw new ArgumentNullException(nameof(documentHash));
        }
    }
}