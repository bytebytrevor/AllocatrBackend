namespace AllocatrApi.Dtos;

// Public claims and their verification status only. Never add file metadata here.
// VerificationStatus is "UnderReview" or "Verified"; it is always set by the server.
public record PublicQualificationDto(
    string Title,
    string IssuingInstitution,
    string VerificationStatus
)
{
    // Stable public category, independent of the application's enum serializer.
    // Keep the existing constructor and qualifications response field compatible.
    public string DocumentType { get; init; } = "Qualification";
}
