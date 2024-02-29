using ArceusCore.Database.Attributes;
using ArceusCore.Utils.Parsers;

namespace ArceusCore.Tests.Console.Entities;


[Table("catalog_pages")]
public class CatalogPageData
{
    [Column("id")] public int Id { get; init; }
    [Column("parent_id")] public int ParentId { get; init; }
    [Column("caption_save")] public string CaptionSave { get; init; } = null!;
    [Column("caption")] public string Caption { get; init; } = null!;
    [Column("page_layout")] public string PageLayout { get; init; } = null!;
    [Column("icon_color")] public int IconColor { get; init; }
    [Column("icon_image")] public int IconImage { get; init; }
    [Column("min_rank")] public int MinRank { get; init; }
    [Column("order_num")] public int OrderNum { get; init; }
    [Column("visible")] [Converter(typeof(EnumToBoolConverter))] public bool Visible { get; init; }
    [Column("enabled")] [Converter(typeof(EnumToBoolConverter))] public bool Enabled { get; init; }
    [Column("club_only")] [Converter(typeof(EnumToBoolConverter))] public bool ClubOnly { get; init; }
    [Column("vip_only")] [Converter(typeof(EnumToBoolConverter))] public bool VipOnly { get; init; }
    [Column("page_headline")] public string PageHeadline { get; init; } = null!;
    [Column("page_teaser")] public string PageTeaser { get; init; } = null!;
    [Column("page_special")] public string? PageSpecial { get; init; }
    [Column("page_text1")] public string? PageText1 { get; init; }
    [Column("page_text2")] public string? PageText2 { get; init; }
    [Column("page_text_details")] public string? PageTextDetails { get; init; }
    [Column("page_text_teaser")] public string? PageTextTeaser { get; init; }
    [Column("room_id")] public int RoomId { get; init; }
    [Column("includes")] public string Includes { get; init; } = null!;
}