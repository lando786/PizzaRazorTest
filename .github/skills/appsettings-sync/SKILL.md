---
name: appsettings-sync
description: Audit PizzaApp appsettings JSON leaf-key paths and report configuration drift without modifying files.
---

# Appsettings synchronization

Use this skill for changes to `PizzaApp/appsettings*.json` or when reviewing application configuration.

## Audit-only behavior

- Do not modify any files while using this skill.
- Report audit results only; do not reconcile, copy, add, remove, or rename settings.
- Ignore leaf values entirely. Values are expected to vary by environment and are outside this audit's scope.

## Scope and comparison contract

- Use `PizzaApp/appsettings.Development.json` as the reference and compare it against every `PizzaApp/appsettings.{Environment}.json` file.
- Ignore `PizzaApp/appsettings.json`; it is the base configuration whose values are copied, overridden, and appended by environment-specific files.
- Compare recursively by JSON leaf-key path, not by whole-object equality. For example, `Logging:LogLevel:Default` is a leaf path.
- Each leaf path in Development must be present in every compared file.
- Report missing or extra paths explicitly.

## Output format

- When every compared file contains every Development leaf path, report: `✅ All appsettings leaf-key paths are synchronized.`
- Otherwise, group findings by leaf path rather than by file. Use a clear line for each finding, for example: `❌ Logging:ConnectionStrings:Db1 is present in Development but missing in QA and Staging.`
- For paths that are not present in Development, identify the file that contains the extra path, for example: `❌ AllowedHosts is present in Production but not in Development.`
- Use environment names derived from the file names, such as `appsettings.Development.json` for Development and `appsettings.QA.json` for QA.

## Workflow

1. Identify all `PizzaApp/appsettings.{Environment}.json` files.
2. Inspect their JSON leaf-key paths, using Development as the reference and excluding `appsettings.json`.
3. Group and report each missing or extra path using the output format above.
4. State whether every Development leaf path is present in all compared files. Do not change any files.
