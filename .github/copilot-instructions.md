# Copilot instructions

- Keep this project focused on the TechHive user-management API.
- Preserve the `api/users` CRUD routes, validation, duplicate-email protection, and HTTP status behavior documented in `README.md`.
- Keep persistence in the in-memory `UserStore` unless a later activity explicitly requests a database.
- When changing endpoints, update `UserManagementAPI.http` and the README, then run `dotnet build` and the available API checks.