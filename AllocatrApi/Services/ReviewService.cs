using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Constants;

namespace AllocatrApi.Services;

public class ReviewService
{
    private readonly AllocatrDbContext _db;

    public ReviewService(
        AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<
        IReadOnlyList<ProjectAllocatRatingDto>
    > SubmitProjectRatingsAsync(
        Guid projectId,
        Guid reviewerId,
        SubmitProjectRatingsDto dto)
    {
        if (
            dto.Ratings == null ||
            dto.Ratings.Count == 0
        )
        {
            throw new ArgumentException(
                "At least one rating is required."
            );
        }

        /*
         * Prevent the same Allocat appearing twice
         * in one request.
         */
        var duplicateAllocatIds = dto.Ratings
            .GroupBy(r => r.AllocatId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateAllocatIds.Count > 0)
        {
            throw new ArgumentException(
                "An Allocat can only be rated once per request."
            );
        }

        /*
         * Validate individual rating input.
         */
        foreach (var rating in dto.Ratings)
        {
            if (rating.AllocatId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid Allocat ID is required."
                );
            }

            if (
                rating.Rating < 1 ||
                rating.Rating > 5
            )
            {
                throw new ArgumentException(
                    "Rating must be between 1 and 5."
                );
            }

            if (
                rating.Comment != null &&
                rating.Comment.Trim().Length > 1000
            )
            {
                throw new ArgumentException(
                    "Review comments cannot exceed 1,000 characters."
                );
            }
        }

        /*
         * Load project.
         *
         * We intentionally don't include all the
         * navigation collections here because the
         * queries below handle assignments separately.
         */
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == projectId
            );

        if (project == null)
        {
            throw new KeyNotFoundException(
                "Project not found."
            );
        }

        /*
         * Only the project owner/client can rate
         * Allocats on this project.
         */
        if (project.UserId != reviewerId)
        {
            throw new UnauthorizedAccessException(
                "Only the project owner can rate Allocats on this project."
            );
        }

        /*
         * Rating only becomes available after
         * project completion.
         */
        if (project.Status != ProjectStatuses.Completed)
        {
            throw new InvalidOperationException(
                "Ratings can only be submitted for completed projects."
            );
        }

        var requestedAllocatIds = dto.Ratings
            .Select(r => r.AllocatId)
            .ToHashSet();

        /*
         * Only accepted, currently active project
         * assignments are eligible for rating.
         */
        var eligibleAllocatIds =
            await _db.ProjectAllocats
                .AsNoTracking()
                .Where(pa =>
                    pa.ProjectId == projectId &&
                    pa.Status ==
                        ProjectAllocatStatus.Accepted &&
                    pa.RemovedAt == null &&
                    requestedAllocatIds.Contains(
                        pa.AllocatProfileId
                    )
                )
                .Select(pa =>
                    pa.AllocatProfileId
                )
                .ToListAsync();

        var eligibleSet =
            eligibleAllocatIds.ToHashSet();

        var invalidAllocatIds =
            requestedAllocatIds
                .Where(id =>
                    !eligibleSet.Contains(id)
                )
                .ToList();

        if (invalidAllocatIds.Count > 0)
        {
            throw new InvalidOperationException(
                "One or more Allocats were not active accepted members of this project."
            );
        }

        await using var transaction =
            await _db.Database
                .BeginTransactionAsync();

        try
        {
            var existingReviews =
                await _db.Reviews
                    .Where(r =>
                        r.ProjectId == projectId &&
                        requestedAllocatIds.Contains(
                            r.AllocatProfileId
                        )
                    )
                    .ToDictionaryAsync(
                        r => r.AllocatProfileId
                    );

            var now = DateTime.UtcNow;

            /*
             * Keeps track of both existing and newly
             * created reviews so we can construct the
             * response afterward.
             */
            var savedReviews =
                new Dictionary<Guid, Review>();

            foreach (var input in dto.Ratings)
            {
                var comment =
                    NormalizeComment(
                        input.Comment
                    );

                if (
                    existingReviews.TryGetValue(
                        input.AllocatId,
                        out var existingReview
                    )
                )
                {
                    /*
                     * Existing review:
                     * edit rather than create another.
                     */
                    existingReview.Rating =
                        input.Rating;

                    existingReview.Comment =
                        comment;

                    existingReview.UpdatedAt =
                        now;

                    savedReviews[input.AllocatId] =
                        existingReview;
                }
                else
                {
                    var review = new Review
                    {
                        Id = Guid.NewGuid(),

                        ProjectId =
                            projectId,

                        ReviewerId =
                            reviewerId,

                        AllocatProfileId =
                            input.AllocatId,

                        Rating =
                            input.Rating,

                        Comment =
                            comment,

                        CreatedAt =
                            now,

                        UpdatedAt =
                            now
                    };

                    _db.Reviews.Add(review);

                    savedReviews[input.AllocatId] =
                        review;
                }
            }

            /*
             * Save reviews first so aggregation queries
             * below see the new/updated values.
             *
             * Both SaveChanges calls remain inside the
             * same transaction.
             */
            await _db.SaveChangesAsync();

            /*
             * Calculate all affected Allocat summaries
             * in one query.
             */
            var summaries =
                await _db.Reviews
                    .AsNoTracking()
                    .Where(r =>
                        requestedAllocatIds.Contains(
                            r.AllocatProfileId
                        )
                    )
                    .GroupBy(r =>
                        r.AllocatProfileId
                    )
                    .Select(g => new
                    {
                        AllocatId = g.Key,

                        RatingCount =
                            g.Count(),

                        AverageRating =
                            g.Average(r =>
                                (decimal)r.Rating
                            )
                    })
                    .ToDictionaryAsync(
                        x => x.AllocatId
                    );

            var profiles =
                await _db.AllocatProfiles
                    .Where(a =>
                        requestedAllocatIds.Contains(
                            a.AllocatrUserId
                        )
                    )
                    .ToListAsync();

            foreach (var profile in profiles)
            {
                if (
                    !summaries.TryGetValue(
                        profile.AllocatrUserId,
                        out var summary
                    )
                )
                {
                    profile.AverageRating = 0m;
                    profile.RatingCount = 0;

                    continue;
                }

                profile.AverageRating =
                    Math.Round(
                        summary.AverageRating,
                        2
                    );

                profile.RatingCount =
                    summary.RatingCount;
            }

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            /*
             * Preserve the order sent by the frontend.
             */
            return dto.Ratings
                .Select(input =>
                {
                    var review =
                        savedReviews[
                            input.AllocatId
                        ];

                    var summary =
                        summaries[
                            input.AllocatId
                        ];

                    return new ProjectAllocatRatingDto(
                        review.Id,
                        review.ProjectId,
                        review.AllocatProfileId,
                        review.Rating,
                        review.Comment,
                        Math.Round(
                            summary.AverageRating,
                            2
                        ),
                        summary.RatingCount,
                        review.CreatedAt,
                        review.UpdatedAt
                    );
                })
                .ToList();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    private static string? NormalizeComment(
        string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        var normalized = comment.Trim();

        if (normalized.Length > 1000)
        {
            throw new ArgumentException(
                "Review comments cannot exceed 1,000 characters."
            );
        }

        return normalized;
    }
}