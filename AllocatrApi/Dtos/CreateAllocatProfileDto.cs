using AllocatrApi.Models;
using Microsoft.AspNetCore.Http;

namespace AllocatrApi.Dtos;

public class CreateAllocatProfileDto
{
    public string? IdNumber { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Bio { get; set; }
    public int? YearsExperience { get; set; }

    public List<AllocatProfileSkill> Skills { get; set; } = [];

    public IFormFile? IdDocument { get; set; }

    public List<IFormFile> CredentialFiles { get; set; } = [];
}