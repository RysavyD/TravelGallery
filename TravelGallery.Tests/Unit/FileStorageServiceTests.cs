using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using TravelGallery.Services;

namespace TravelGallery.Tests.Unit;

public class FileStorageServiceTests
{
    private static FileStorageService CreateService()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return new FileStorageService(env.Object);
    }

    private static IFormFile MockFile(string fileName)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        return mock.Object;
    }

    // ── povolené přípony – obrázky ────────────────────────────────────────────

    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("foto.jpeg")]
    [InlineData("foto.png")]
    [InlineData("foto.gif")]
    [InlineData("foto.webp")]
    public void IsAllowedExtension_ImageExtensions_ReturnsTrue(string fileName)
    {
        CreateService().IsAllowedExtension(MockFile(fileName)).Should().BeTrue();
    }

    // ── povolené přípony – videa ──────────────────────────────────────────────

    [Theory]
    [InlineData("video.mp4")]
    [InlineData("video.mov")]
    [InlineData("video.avi")]
    [InlineData("video.webm")]
    public void IsAllowedExtension_VideoExtensions_ReturnsTrue(string fileName)
    {
        CreateService().IsAllowedExtension(MockFile(fileName)).Should().BeTrue();
    }

    // ── zakázané přípony ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("program.exe")]
    [InlineData("data.csv")]
    [InlineData("script.php")]
    [InlineData("archive.zip")]
    [InlineData("code.js")]
    [InlineData("noextension")]
    public void IsAllowedExtension_ForbiddenExtensions_ReturnsFalse(string fileName)
    {
        CreateService().IsAllowedExtension(MockFile(fileName)).Should().BeFalse();
    }

    // ── case insensitivita ────────────────────────────────────────────────────

    [Theory]
    [InlineData("FOTO.JPG")]
    [InlineData("Foto.PNG")]
    [InlineData("VIDEO.MP4")]
    [InlineData("video.MOV")]
    public void IsAllowedExtension_UppercaseExtension_ReturnsTrue(string fileName)
    {
        CreateService().IsAllowedExtension(MockFile(fileName)).Should().BeTrue(
            because: "porovnání přípon musí být case-insensitive");
    }

    [Theory]
    [InlineData("script.PHP")]
    [InlineData("program.EXE")]
    public void IsAllowedExtension_UppercaseForbiddenExtension_ReturnsFalse(string fileName)
    {
        CreateService().IsAllowedExtension(MockFile(fileName)).Should().BeFalse(
            because: "porovnání přípon musí být case-insensitive i pro zakázané typy");
    }
}
