using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(UserStore userStore) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<User>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<User>> GetAll()
    {
        return Ok(userStore.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetById(Guid id)
    {
        var user = userStore.GetById(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [ProducesResponseType<User>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<User> Create(UserRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        var user = new User(
            Guid.NewGuid(),
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email.Trim(),
            NormalizeDepartment(request.Department),
            now,
            now);

        if (!userStore.TryAdd(user))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email address already in use",
                Detail = "A user with this email address already exists."
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<User> Update(Guid id, UserRequest request)
    {
        var existingUser = userStore.GetById(id);
        if (existingUser is null)
        {
            return NotFound();
        }

        var updatedUser = existingUser with
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Department = NormalizeDepartment(request.Department),
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        return userStore.TryUpdate(updatedUser) switch
        {
            UpdateUserResult.Updated => Ok(updatedUser),
            UpdateUserResult.NotFound => NotFound(),
            UpdateUserResult.EmailInUse => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email address already in use",
                Detail = "A user with this email address already exists."
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return userStore.TryDelete(id) ? NoContent() : NotFound();
    }

    private static string? NormalizeDepartment(string? department)
    {
        var normalized = department?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}