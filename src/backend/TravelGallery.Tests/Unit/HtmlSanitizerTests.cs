using Ganss.Xss;

namespace TravelGallery.Tests.Unit;

/// <summary>
/// Testy ověřující chování HtmlSanitizeru – stejná konfigurace jako v Program.cs
/// (výchozí konstruktor = bezpečné výchozí hodnoty).
/// </summary>
public class HtmlSanitizerTests
{
    private readonly HtmlSanitizer _sut = new HtmlSanitizer();

    // ── bezpečný obsah musí projít ───────────────────────────────────────────

    [Fact]
    public void Sanitize_SafeParagraph_PreservesContent()
    {
        const string input = "<p>Bezpečný text s <strong>tučným písmem</strong>.</p>";
        var result = _sut.Sanitize(input);

        result.Should().Contain("Bezpečný text");
        result.Should().Contain("<strong>");
        result.Should().Contain("<p>");
    }

    [Fact]
    public void Sanitize_EmptyString_ReturnsEmpty()
    {
        _sut.Sanitize("").Should().BeEmpty();
    }

    // ── script tagy musí být odstraněny ─────────────────────────────────────

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<script src='evil.js'></script>")]
    [InlineData("<SCRIPT>alert(1)</SCRIPT>")]
    [InlineData("<script type='text/javascript'>evil()</script>")]
    public void Sanitize_ScriptTag_IsRemoved(string payload)
    {
        var result = _sut.Sanitize(payload);

        result.Should().NotContain("<script",     because: "script tag musí být odstraněn");
        result.Should().NotContain("alert(",      because: "obsah script tagu musí být odstraněn");
        result.Should().NotContain("evil()",      because: "škodlivý kód musí být odstraněn");
    }

    // ── event handlery musí být odstraněny ──────────────────────────────────

    [Theory]
    [InlineData("<img src='x' onerror='alert(1)'>",       "onerror=")]
    [InlineData("<div onmouseover='evil()'>text</div>",   "onmouseover=")]
    [InlineData("<body onload='evil()'>text</body>",      "onload=")]
    [InlineData("<a onclick='evil()' href='#'>klik</a>",  "onclick=")]
    public void Sanitize_EventHandlers_AreRemoved(string payload, string forbidden)
    {
        var result = _sut.Sanitize(payload);
        result.Should().NotContain(forbidden, because: $"{forbidden} musí být odstraněn");
    }

    // ── javascript: protokol musí být odstraněn ──────────────────────────────

    [Fact]
    public void Sanitize_JavascriptProtocol_IsRemoved()
    {
        const string payload = "<a href='javascript:alert(1)'>klik</a>";
        var result = _sut.Sanitize(payload);

        result.Should().NotContain("javascript:",
            because: "javascript: URI musí být odstraněn");
    }

    // ── nebezpečné tagy musí být odstraněny ─────────────────────────────────
    // Pozn.: <form> je ve výchozím allowlistu Ganss.Xss povolen; testujeme jen
    //        skutečně zakázané tagy (<iframe>, <object>, <embed>).

    [Theory]
    [InlineData("<iframe src='https://evil.com'></iframe>", "<iframe")]
    [InlineData("<object data='evil.swf'></object>",        "<object")]
    [InlineData("<embed src='evil.swf'>",                   "<embed")]
    public void Sanitize_DangerousTags_AreRemoved(string payload, string forbidden)
    {
        var result = _sut.Sanitize(payload);
        result.Should().NotContain(forbidden, because: $"{forbidden} musí být odstraněn");
    }

    // ── smíšený payload: bezpečný obsah zůstane, nebezpečný odejde ──────────

    [Fact]
    public void Sanitize_MixedPayload_RemovesDangerousKeepsSafe()
    {
        const string input =
            "<p>Bezpečný text</p>" +
            "<script>alert('XSS_MARK')</script>" +
            "<img src='x' onerror='alert(2)'>" +
            "<iframe src='evil.com'></iframe>";

        var result = _sut.Sanitize(input);

        // Bezpečný obsah zůstane
        result.Should().Contain("Bezpečný text");

        // Škodlivý obsah odstraněn
        result.Should().NotContain("<script");
        result.Should().NotContain("XSS_MARK");
        result.Should().NotContain("onerror=");
        result.Should().NotContain("<iframe");
    }

    // ── atributy na obrázcích ────────────────────────────────────────────────

    [Fact]
    public void Sanitize_ImgWithSrc_PreservesSrcRemovesOnerror()
    {
        const string payload = "<img src='foto.jpg' onerror='evil()' alt='foto'>";
        var result = _sut.Sanitize(payload);

        result.Should().Contain("foto.jpg", because: "src atribut smí zůstat");
        result.Should().NotContain("onerror=", because: "onerror musí být odstraněn");
    }
}
