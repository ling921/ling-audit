using FluentAssertions.Equivalency;
using System.Linq.Expressions;

namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

internal static class EquivalencyOptionsExtensions
{
    public static EquivalencyOptions<TExpectation> ExcludingAuditProperties<TExpectation>(
        this EquivalencyOptions<TExpectation> options)
    {
        if (typeof(TExpectation).GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHasCreator<>)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasCreator<int?>.CreatedBy)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        if (typeof(TExpectation).GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHasModifier<>)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasModifier<int?>.LastModifiedBy)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        if (typeof(TExpectation).GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHasDeleter<>)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasDeleter<int?>.DeletedBy)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        return options.ExcludingAuditTimes();
    }

    public static EquivalencyOptions<TExpectation> ExcludingAuditTimes<TExpectation>(
        this EquivalencyOptions<TExpectation> options)
    {
        if (typeof(TExpectation).GetInterfaces().Contains(typeof(IHasCreationTime)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasCreationTime.CreatedAt)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        if (typeof(TExpectation).GetInterfaces().Contains(typeof(IHasModificationTime)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasModificationTime.LastModifiedAt)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        if (typeof(TExpectation).GetInterfaces().Contains(typeof(IHasDeletionTime)))
        {
            var exp = Expression.Lambda<Func<TExpectation, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.Parameter(typeof(TExpectation), "x"),
                        nameof(IHasDeletionTime.DeletedAt)),
                    typeof(object)),
                Expression.Parameter(typeof(TExpectation), "x"));
            options = options.Excluding(exp);
        }

        return options;
    }
}
