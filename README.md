# PiShockApiLibrary

A small C# library for controlling [PiShock](https://pishock.com) shockers, targeting .NET 10 and .NET Standard 2.0 (for compatibility with .NET Framework 4.7.2+ and older runtimes, e.g. Unity/BepInEx mods). It talks to both the current V3 API and the legacy API, and picks whichever one a given shocker actually supports.

## Installation

Available on [NuGet](https://www.nuget.org/packages/PiShockApiLibrary/):

```
dotnet add package PiShockApiLibrary
```

## Usage

```csharp
using PiShockApiLibrary;

// Tries the V3 API first, transparently falls back to legacy if the shocker doesn't support it.
IShocker shocker = await ShockerFactory.CreateShocker(apiKey, username, shockerId);

await shocker.Vibrate(duration: 1.0, intensity: 30);
await shocker.Beep(duration: 0.5);
await shocker.Shock(duration: 1.0, intensity: 50);
```

If you know which API a shocker uses, you can skip the fallback attempt:

```csharp
var v3 = await ShockerFactory.CreateV3Shocker(apiKey, shockerId);
var legacy = await ShockerFactory.CreateLegacyShocker(apiKey, username, shockerId);
```

All three factory methods return an `IShocker`, so calling code doesn't need to know which API is actually backing a given instance.

### Exceptions

All library-specific errors derive from `PishockException`:

| Exception | Meaning |
|---|---|
| `PishockAuthenticationException` | API key/username/share code is invalid or missing. |
| `PishockPermissionException` | Credentials are valid but not permitted to do this (paused shocker, locked share, mode not allowed). |
| `PishockDataException` | A value was rejected as out of range, or a response couldn't be parsed. |
| `PishockShockerException` | The shocker/share couldn't be resolved. |
| `PishockNotSupportedException` | The client refused the request client-side because the target API can't honor it (only in `strict` mode). |

Out-of-range `duration`/`intensity` arguments throw `ArgumentOutOfRangeException` instead, since those are caught before any request is made.

## Limitations

- **No `CancellationToken` support.** Calls can't currently be cancelled mid-flight.
- **Legacy API can't randomize duration server-side.** Passing `minimumDuration` to `Shock`/`Vibrate`/`Beep` on a `LegacyShocker` is silently ignored unless `strict` mode is set, in which case it throws instead.
- **Legacy API's `Operate` endpoint doesn't validate duration/intensity server-side**, so a `LegacyShocker`'s cached limits can drift; call `Refresh()` periodically if a share's limits might have changed. `V3Shocker.Refresh()` is a no-op — every V3 call is validated server-side anyway.
- **Ambiguous legacy share codes** (multiple shares matching the same shocker ID) resolve to the first match by default. Pass an explicit `shareCode`, or set `strict: true` to throw instead of guessing.
- Each `IShocker` implementation uses a single static, shared `HttpClient` — it's not injectable, which makes this harder to unit test or route through a custom handler.

## License

[MIT](LICENSE)
