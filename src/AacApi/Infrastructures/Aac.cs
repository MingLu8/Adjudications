using RepoDb.Attributes;
using SharedContracts;
using System.ComponentModel.DataAnnotations;

namespace AacApi.Infrastructures;

[Map("aac")]
public class Aac
{
    public Aac() { }
    internal Aac(AacState state, string ndc, decimal price, DateOnly effectiveDate)
    {
        State = state;
        Ndc = ndc;
        Price = price;
        EffectiveDate = effectiveDate;
        IsActive = true;
    }

    [Primary]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(2)]
    public AacState State { get; set; }

    [Required]
    [StringLength(20)]
    public string Ndc { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [Map("effective_date")]
    [PropertyHandler(typeof(DateOnlyPropertyHandler))]
    public DateOnly EffectiveDate { get; set; }

    [Map("created_date")]
    public DateTime CreatedDate { get; set; }

    [Map("updated_date")]
    public DateTime UpdatedDate { get; set; }

    [Map("is_active")]
    public bool IsActive { get; set; }
}
