using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public enum UpdateUserResult
{
    Updated,
    NotFound,
    EmailInUse
}

public sealed class UserStore
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, User> _users = [];

    public IReadOnlyList<User> GetAll()
    {
        lock (_syncRoot)
        {
            // Return a snapshot without sorting: list order is not part of the API contract.
            return _users.Values.ToArray();
        }
    }

    public User? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _users.GetValueOrDefault(id);
        }
    }

    public bool TryAdd(User user)
    {
        lock (_syncRoot)
        {
            if (EmailIsInUse(user.Email))
            {
                return false;
            }

            _users.Add(user.Id, user);
            return true;
        }
    }

    public UpdateUserResult TryUpdate(User user)
    {
        lock (_syncRoot)
        {
            if (!_users.ContainsKey(user.Id))
            {
                return UpdateUserResult.NotFound;
            }

            if (EmailIsInUse(user.Email, user.Id))
            {
                return UpdateUserResult.EmailInUse;
            }

            _users[user.Id] = user;
            return UpdateUserResult.Updated;
        }
    }

    public bool TryDelete(Guid id)
    {
        lock (_syncRoot)
        {
            return _users.Remove(id);
        }
    }

    private bool EmailIsInUse(string email, Guid? exceptId = null)
    {
        return _users.Values.Any(user =>
            user.Id != exceptId &&
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}