using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

public sealed class UserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "First name cannot be blank.")]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Last name cannot be blank.")]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [StringLength(80)]
    public string? Department { get; init; }
}