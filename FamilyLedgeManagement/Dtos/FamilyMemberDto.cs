using System.ComponentModel.DataAnnotations;
using FamilyLedgeManagement.Models;

namespace FamilyLedgeManagement.Dtos;

public sealed class FamilyMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

