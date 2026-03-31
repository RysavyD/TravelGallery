using TravelGallery.Models;

namespace TravelGallery.Tests.Unit;

public class TagSlugTests
{
    // ── základní transformace ────────────────────────────────────────────────

    [Theory]
    [InlineData("Hory",              "hory")]
    [InlineData("Hrubý Jeseník",     "hruby-jesenik")]
    [InlineData("Česká republika",   "ceska-republika")]
    [InlineData("šířka výška",       "sirka-vyska")]
    [InlineData("already-slug",      "already-slug")]
    [InlineData("VELKÁ PÍSMENA",     "velka-pismena")]
    [InlineData("Zimní turistika",   "zimni-turistika")]
    public void ToSlug_VariousInputs_ReturnsExpectedSlug(string input, string expected)
    {
        Tag.ToSlug(input).Should().Be(expected);
    }

    // ── prázdný vstup ────────────────────────────────────────────────────────

    [Fact]
    public void ToSlug_EmptyString_ReturnsEmpty()
    {
        Tag.ToSlug("").Should().BeEmpty();
    }

    // ── mezery ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToSlug_LeadingTrailingSpaces_AreTrimmed()
    {
        Tag.ToSlug("  Hory  ").Should().Be("hory");
    }

    // ── povolené znaky ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("Hory")]
    [InlineData("Zimní turistika")]
    [InlineData("Hrubý Jeseník")]
    public void ToSlug_ResultContainsOnlyAllowedChars(string input)
    {
        Tag.ToSlug(input).Should().MatchRegex(@"^[a-z0-9-]+$",
            "slug smí obsahovat jen malá písmena, číslice a pomlčky");
    }

    // ── nezačíná/nekončí pomlčkou ────────────────────────────────────────────

    [Theory]
    [InlineData("!special!")]
    [InlineData("@tag@")]
    public void ToSlug_SpecialCharsOnly_DoesNotStartOrEndWithHyphen(string input)
    {
        var slug = Tag.ToSlug(input);
        if (!string.IsNullOrEmpty(slug))
        {
            slug.Should().NotStartWith("-").And.NotEndWith("-");
        }
    }

    // ── deterministika ───────────────────────────────────────────────────────

    [Fact]
    public void ToSlug_SameInputTwice_ReturnsSameSlug()
    {
        const string input = "Krkonoše";
        Tag.ToSlug(input).Should().Be(Tag.ToSlug(input));
    }

    // ── case-insensitivita pro deduplikaci ───────────────────────────────────

    [Fact]
    public void ToSlug_DifferentCaseSameWord_ReturnsSameSlug()
    {
        Tag.ToSlug("HORY").Should().Be(Tag.ToSlug("hory"));
        Tag.ToSlug("Hory").Should().Be(Tag.ToSlug("hory"));
    }

    // ── víceslovné výrazy ────────────────────────────────────────────────────

    [Fact]
    public void ToSlug_MultipleWords_JoinedWithHyphens()
    {
        // Diakritika je normalizována (é → e), mezery → pomlčky
        Tag.ToSlug("Vysoké Tatry").Should().Be("vysoke-tatry");
        Tag.ToSlug("Zimní turistika").Should().Be("zimni-turistika");
    }
}
