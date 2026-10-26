using System.ComponentModel.DataAnnotations;

namespace AllocatrApi.Dtos;

// No review status or file properties are accepted from the owner.
public sealed class UpdateUserDocumentMetadataDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(200)]
    public string? IssuingInstitution { get; set; }
}
