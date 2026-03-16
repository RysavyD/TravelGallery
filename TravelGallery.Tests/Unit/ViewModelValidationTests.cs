using System.ComponentModel.DataAnnotations;
using TravelGallery.Areas.Admin.ViewModels;

namespace TravelGallery.Tests.Unit;

public class TripFormViewModelValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx     = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    // ── platný model nemá chyby ──────────────────────────────────────────────

    [Fact]
    public void ValidModel_ProducesNoValidationErrors()
    {
        var model = new TripFormViewModel
        {
            Title = "Výlet na hory",
            Date  = DateTime.Today
        };

        Validate(model).Should().BeEmpty();
    }

    // ── prázdný název ────────────────────────────────────────────────────────

    [Fact]
    public void EmptyTitle_HasValidationError()
    {
        var model = new TripFormViewModel { Title = "", Date = DateTime.Today };

        Validate(model).Should().Contain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.Title)),
            because: "prázdný název musí způsobit validační chybu");
    }

    // ── příliš dlouhý název ──────────────────────────────────────────────────

    [Fact]
    public void TitleOver200Chars_HasValidationError()
    {
        var model = new TripFormViewModel
        {
            Title = new string('A', 201),
            Date  = DateTime.Today
        };

        Validate(model).Should().Contain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.Title)),
            because: "název nad 200 znaků musí způsobit validační chybu");
    }

    [Fact]
    public void TitleExactly200Chars_IsValid()
    {
        var model = new TripFormViewModel
        {
            Title = new string('A', 200),
            Date  = DateTime.Today
        };

        Validate(model).Should().NotContain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.Title)));
    }

    // ── nullable pole nemají validační chyby ─────────────────────────────────

    [Fact]
    public void NullTagNames_ProducesNoValidationError()
    {
        // TagNames je nullable – prázdná/null hodnota (trip bez tagů) musí projít
        var model = new TripFormViewModel
        {
            Title    = "Test",
            Date     = DateTime.Today,
            TagNames = null
        };

        Validate(model).Should().NotContain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.TagNames)));
    }

    [Fact]
    public void EmptyTagNames_ProducesNoValidationError()
    {
        var model = new TripFormViewModel
        {
            Title    = "Test",
            Date     = DateTime.Today,
            TagNames = ""
        };

        Validate(model).Should().NotContain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.TagNames)));
    }

    [Fact]
    public void NullDescription_ProducesNoValidationError()
    {
        var model = new TripFormViewModel
        {
            Title       = "Test",
            Date        = DateTime.Today,
            Description = null
        };

        Validate(model).Should().NotContain(e =>
            e.MemberNames.Contains(nameof(TripFormViewModel.Description)));
    }
}
