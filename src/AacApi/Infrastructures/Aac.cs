using RepoDb.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AacApi.Infrastructures;

[Map("aac")]
public class Aac
{
    public Aac() { }
    internal Aac(string state, string ndc, decimal price, DateOnly effectiveDate)
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
    public string State { get; set; } = string.Empty;

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
