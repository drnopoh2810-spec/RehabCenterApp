---
name: DB migration strategy
description: How new columns are safely added to existing SQLite DB in this EnsureCreated() app
---

App uses `Database.EnsureCreated()` (no EF migrations). New columns added via `RunMigrations()` called in DatabaseService constructor.

**Rule:** Any new column on an existing model must be added in `RunMigrations()` with a raw SQL `ALTER TABLE ... ADD COLUMN` inside a try/catch (SQLite throws if column already exists — catch ignores it silently).

```csharp
void TryAdd(string table, string column, string type) {
    try { cmd.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {type}"; cmd.ExecuteNonQuery(); }
    catch { }
}
```

**Why:** `EnsureCreated()` only creates tables that don't exist — it never modifies existing tables. Without `RunMigrations()`, new model properties cause runtime `SQLite Error 1: 'no such column'` on first query that touches the new field.

**How to apply:** Every time a new nullable or defaulted property is added to an existing model, add a `TryAdd(...)` call in `RunMigrations()` in `Services/DatabaseService.cs`.
