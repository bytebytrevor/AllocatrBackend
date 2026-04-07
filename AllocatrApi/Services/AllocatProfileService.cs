using System;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class AllocatProfileService
{
    private readonly AllocatrDbContext _db;

    public AllocatProfileService(AllocatrDbContext db)
    {
        _db = db;        
    }

    public async Task<AllocatProfile> CreateAllocatProfileAsync(AllocatProfile allocatProfile)
    {
        allocatProfile.CreatedAt = DateTime.UtcNow;
        allocatProfile.UpdatedAt = DateTime.UtcNow;

        _db.AllocatProfiles.Add(allocatProfile);
        await _db.SaveChangesAsync();

        return allocatProfile;        
    }

    // Get allocat profile by user id
    public async Task<AllocatProfileDto?> GetAllocatProfileByUserIdAsync(string allocatId)
    {
        return await _db.AllocatProfiles
            .Where(a => a.AllocatrUserId == allocatId)
            .Select(a => new AllocatProfileDto (
                a.AllocatrUserId,
                a.IdNumber,
                a.HourlyRate,
                a.Bio,
                a.Availability,
                a.YearsExperience,
                a.IsVisible,
                a.CreatedAt,
                a.UpdatedAt
        ))
        .FirstOrDefaultAsync();   
    }


}
