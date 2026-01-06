namespace AGE.SignatureHub.Domain.Enums
{
    public enum DocumentStatus
    {
        Draft = 1,
        PendingSignatures = 2,
        PartiallyCompleted = 3,
        Completed = 4,
        Rejected = 5,
        Expired = 6,
        Cancelled = 7
    }
}