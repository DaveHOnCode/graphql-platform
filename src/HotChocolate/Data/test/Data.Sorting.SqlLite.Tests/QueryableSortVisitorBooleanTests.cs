using System.Threading.Tasks;
using CookieCrumble;
using HotChocolate.Execution;

namespace HotChocolate.Data.Sorting;

public class QueryableSortVisitorBooleanTests : IClassFixture<SchemaCache>
{
    private static readonly Foo[] _fooEntities =
    {
        new() { Bar = true },
        new() { Bar = false }
    };

    private static readonly FooNullable[] _fooNullableEntities =
    {
        new() { Bar = true },
        new() { Bar = null },
        new() { Bar = false }
    };

    private readonly SchemaCache _cache;

    public QueryableSortVisitorBooleanTests(
        SchemaCache cache)
    {
        _cache = cache;
    }

    [Fact]
    public async Task Create_Boolean_OrderBy()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooSortType>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: { bar: ASC}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: { bar: DESC}){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .Add(res1, "ASC")
            .Add(res2, "DESC")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_Boolean_OrderBy_List()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooSortType>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: [{ bar: ASC}]){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: [{ bar: DESC}]){ bar}}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .Add(res1, "ASC")
            .Add(res2, "DESC")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_Boolean_OrderBy_Nullable()
    {
        // arrange
        var tester = _cache.CreateSchema<FooNullable, FooNullableSortType>(
            _fooNullableEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: { bar: ASC}){ bar}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root(order: { bar: DESC}){ bar}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    Snapshot
                        .Create(), res1, "ASC"), res2, "DESC")
            .MatchAsync();
    }

    public class Foo
    {
        public int Id { get; set; }

        public bool Bar { get; set; }
    }

    public class FooNullable
    {
        public int Id { get; set; }

        public bool? Bar { get; set; }
    }

    public class FooSortType
        : SortInputType<Foo>
    {
    }

    public class FooNullableSortType
        : SortInputType<FooNullable>
    {
    }
}
