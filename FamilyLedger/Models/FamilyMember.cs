namespace FamilyLedger.Models;

public sealed class FamilyMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#245b90";
}
