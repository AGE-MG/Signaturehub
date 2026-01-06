using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Domain.ValueObjects
{
    public class SignatureMetadata
    {
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }
        public string DeviceInfo { get; private set; }
        public string Location { get; private set; }
        public DateTime SignedAt { get; private set; }
        public string DocumentHash { get; private set; }

        private SignatureMetadata() { }

        public SignatureMetadata(
                string ipAddress,
                string userAgent,
                string deviceInfo,
                string location,
                DateTime signedAt,
                string documentHash
            )
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            UserAgent = userAgent;
            DeviceInfo = deviceInfo;
            Location = location;
            SignedAt = signedAt;
            DocumentHash = documentHash ?? throw new ArgumentNullException(nameof(documentHash));
        }
    }
}