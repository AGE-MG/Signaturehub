using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Domain.ValueObjects
{
    public class CertificateInfo
    {
        public string SerialNumber { get; private set; }
        public string SubjectName { get; private set; }
        public string IssuerName { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }
        public string Thumbprint { get; private set; }
        public bool isValid { get; private set; }

        private CertificateInfo() { }

        public CertificateInfo(
                string serialNumber, 
                string subjectName, 
                string issuerName, 
                DateTime validFrom, 
                DateTime validTo, 
                string thumbprint
            )
        {
            SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
            SubjectName = subjectName ?? throw new ArgumentNullException(nameof(subjectName));
            IssuerName = issuerName ?? throw new ArgumentNullException(nameof(issuerName));
            ValidFrom = validFrom;
            ValidTo = validTo;
            Thumbprint = thumbprint ?? throw new ArgumentNullException(nameof(thumbprint));
            isValid = DateTime.UtcNow >= validFrom && DateTime.UtcNow <= validTo;
        }
        public bool IsExpired()
        {
            return DateTime.UtcNow > ValidTo;
        }
    }
}