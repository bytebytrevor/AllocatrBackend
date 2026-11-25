using System.Linq.Expressions;
using System.Text.RegularExpressions;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

// Builds SQL-translatable expressions. Nothing is compiled or filtered after paging.
public static class AllocatSearch
{
    public sealed record Plan(
        Expression<Func<AllocatProfile, bool>> Matches,
        Expression<Func<AllocatProfile, int>> Rank);

    public static Plan? Create(string? value)
    {
        var query = value?.Trim().ToLowerInvariant() ?? "";
        if (query.Length == 0) return null;
        var aliasKey = Regex.Replace(query, @"\s+", " ");
        var exactGroups = AllocatSearchAliasGroups.Groups
            .Where(terms => terms.Contains(aliasKey))
            .ToArray();
        // Keep complete terms precise; expand prefixes of three or more characters
        // while typing. This vocabulary lookup happens before building SQL.
        string[][] groups = exactGroups.Length > 0
            ? exactGroups
            : aliasKey.Length >= 3
                ? AllocatSearchAliasGroups.Groups
                    .Where(terms => terms.Any(term => term.StartsWith(aliasKey, StringComparison.Ordinal)))
                    .ToArray()
                : [];
        // Expand original-query groups once, without recursively merging services
        // or changing canonical required skill IDs.
        var aliases = groups
            .SelectMany(terms => terms)
            .Where(term => term != query)
            .Distinct()
            .ToArray();
        var rules = new List<(Expression<Func<AllocatProfile, bool>> Test, int Rank)>();
        var exact = EscapeLike(query);
        var contains = "%" + exact + "%";

        rules.Add((Professional(exact), 1000));
        rules.Add((a => a.AllocatrUser.FullName != null && EF.Functions.ILike(a.AllocatrUser.FullName.Trim(), exact, "\\"), 950));
        rules.Add((Professional(contains), 900));
        rules.Add((a => a.Headline != null && EF.Functions.ILike(a.Headline, contains, "\\"), 800));
        rules.Add((a => a.AllocatrUser.FullName != null && EF.Functions.ILike(a.AllocatrUser.FullName.Trim(), contains, "\\"), 700));
        rules.Add((a => a.Bio != null && EF.Functions.ILike(a.Bio, contains, "\\"), 400));
        rules.Add((Category(contains), 300));
        foreach (var alias in aliases)
        {
            var aliasExact = EscapeLike(alias);
            var aliasContains = "%" + aliasExact + "%";
            rules.Add((Professional(aliasExact), 775));
            rules.Add((Professional(aliasContains), 750));
            rules.Add((a => a.Headline != null && EF.Functions.ILike(a.Headline, aliasContains, "\\"), 600));
            rules.Add((a => a.Bio != null && EF.Functions.ILike(a.Bio, aliasContains, "\\"), 200));
            rules.Add((Category(aliasContains), 100));
        }

        var parameter = Expression.Parameter(typeof(AllocatProfile), "profile");
        Expression matches = Expression.Constant(false);
        Expression rank = Expression.Constant(0);
        foreach (var rule in rules.OrderBy(rule => rule.Rank))
        {
            var test = new ReplaceParameter(rule.Test.Parameters[0], parameter).Visit(rule.Test.Body)!;
            // Exact hits are already covered by the corresponding contains rule.
            // Avoid repeating these tests in the count/filter SQL; keep ranking intact.
            if (rule.Rank is not (1000 or 950 or 775))
                matches = Expression.OrElse(matches, test);
            // Later/higher-ranked conditions wrap lower-ranked ones. Rank is the best hit,
            // not a sum that rewards repeated keywords or long biographies.
            rank = Expression.Condition(test, Expression.Constant(rule.Rank), rank);
        }
        return new Plan(Expression.Lambda<Func<AllocatProfile, bool>>(matches, parameter),
            Expression.Lambda<Func<AllocatProfile, int>>(rank, parameter));
    }

    private static Expression<Func<AllocatProfile, bool>> Professional(string pattern) =>
        a => (a.Title != null && EF.Functions.ILike(a.Title.Trim(), pattern, "\\"))
            || a.Skills.Any(link => EF.Functions.ILike(link.Skill.Name.Trim(), pattern, "\\"));

    private static Expression<Func<AllocatProfile, bool>> Category(string pattern) =>
        a => a.Skills.Any(link => EF.Functions.ILike(link.Skill.SkillCategory.Name, pattern, "\\"));

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private sealed class ReplaceParameter(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}
