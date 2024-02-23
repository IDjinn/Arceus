using System.Collections.Concurrent;
using System.Diagnostics;
using ArceusCore.Database.Attributtes;
using ArceusCore.Tests.Console.Entities;
using ArceusCore.Utils;
using ArceusCore.Utils.Interfaces;
using ArceusCore.Utils.Parsers;
using Microsoft.Extensions.Logging;

namespace ArceusCore.Tests.Console;

public class CmdTest
{
    private readonly Arceus _arceus;
    private readonly ILogger<CmdTest> _logger;

    public CmdTest(Arceus arceus, ILogger<CmdTest> logger)
    {
        _arceus = arceus;
        _logger = logger;

        Try();
    }


    public readonly record struct CatalogItemId(int Id)
    {
        public static explicit operator int(CatalogItemId value) => value.Id;
        public static explicit operator CatalogItemId(int id) => new(id);
    }

    public class CatalogItemConverter : IConvertible<int, CatalogItemId>
    {
        public CatalogItemId Parse(int source)
        {
            return new CatalogItemId(source);
        }

        public int Convert(CatalogItemId value)
        {
            return value.Id;
        }
    }

    public record ComplexObjectTest(string Name);

    [Table("catalog_items")]
    public interface ICatalogItemData
    {
        [Column("id")]
        [Converter(typeof(CatalogItemConverter))]
        public CatalogItemId Id { get; init; }

        [Column("item_ids")] public string ItemIds { get; init; }
        [Column("page_id")] public int PageId { get; init; }
        [Column("catalog_name")] public string Name { get; init; }
        [Column("cost_credits")] public int Credits { get; init; }
        [Column("cost_points")] public int Points { get; init; }
        [Column("points_type")] public int PointsType { get; init; }
        [Column("amount")] public int Amount { get; init; }
        [Column("limited_stack")] public int LimitedStack { get; init; }
        [Column("limited_sells")] public int LimitedSells { get; init; }
        [Column("order_number")] public int OrderNumber { get; init; }
        [Column("offer_id")] public int OfferId { get; init; }
        [Column("song_id")] public uint SongId { get; init; }
        [Column("extradata")] public string ExtraData { get; init; }

        [Column("have_offer")]
        [Converter(typeof(EnumToBoolConverter))]
        public bool HaveOffer { get; init; }

        [Column("club_only")]
        [Converter(typeof(EnumToBoolConverter))]
        public bool ClubOnly { get; init; }
    }

    [Table("catalog_items")]
    public class CatalogItemData : ICatalogItemData
    {
        public CatalogItemData()
        {
        }

        private string _something;
        private ComplexObjectTest _complexObject;

        public CatalogItemData(string something, ComplexObjectTest complexObject)
        {
            _something = something;
            _complexObject = complexObject;
        }

        [Column("id")]
        [Converter(typeof(CatalogItemConverter))]
        public CatalogItemId Id { get; init; }

        [Column("item_ids")] public string ItemIds { get; init; } = null!;
        [Column("page_id")] public int PageId { get; init; }
        [Column("catalog_name")] public string Name { get; init; } = null!;
        [Column("cost_credits")] public int Credits { get; init; }
        [Column("cost_points")] public int Points { get; init; }
        [Column("points_type")] public int PointsType { get; init; }
        [Column("amount")] public int Amount { get; init; }
        [Column("limited_stack")] public int LimitedStack { get; init; }
        [Column("limited_sells")] public int LimitedSells { get; init; }
        [Column("order_number")] public int OrderNumber { get; init; }
        [Column("offer_id")] public int OfferId { get; init; }
        [Column("song_id")] public uint SongId { get; init; }
        [Column("extradata")] public string ExtraData { get; init; }

        [Column("have_offer")]
        [Converter(typeof(EnumToBoolConverter))]
        public bool HaveOffer { get; init; }

        [Column("club_only")]
        [Converter(typeof(EnumToBoolConverter))]
        public bool ClubOnly { get; init; }
    }

    private async Task Try()
    {
        using var perfMonitor = new PerformanceMonitor(_logger);
        var sqlReader = await _arceus.Query(
            $@"SELECT * from `catalog_items`",
            () => new CatalogItemData("something_test", new ComplexObjectTest("Luke"))
            );
        var items = new ConcurrentBag<ICatalogItemData>();
        foreach (var catalogItem in sqlReader)
        {
            items.Add(catalogItem);
        }

        var elapsed = perfMonitor.Elapsed();
        perfMonitor.Lap("end");
    }
}