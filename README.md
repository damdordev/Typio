# Typio

Serializable helper types for Unity. Typio lets you store `System.DateTime` and `System.TimeSpan` in components, ScriptableObjects, scenes, prefabs, and JSON, with custom Inspector drawers and implicit conversions to the standard .NET types.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
  - [Serializable Dates](#serializable-dates)
  - [Serializable Durations](#serializable-durations)
  - [Using Typio in Components](#using-typio-in-components)
  - [Working with Value Types](#working-with-value-types)
- [Inspector](#inspector)
- [Serialization](#serialization)
  - [JSON Serialization](#json-serialization)
  - [Compatibility and Time Zones](#compatibility-and-time-zones)
- [Supported Types](#supported-types)
- [Tests](#tests)

## Features

* **Serializable Dates**: Store `DateTime` ticks and `DateTimeKind` (`Unspecified`, `Utc`, or `Local`).
* **Serializable Durations**: Store positive and negative `TimeSpan` values across the full .NET range.
* **Tick Precision**: Runtime values retain their original ticks; one tick equals 100 nanoseconds.
* **Implicit Conversions**: Assign standard .NET values to wrappers and read them back without explicit casts.
* **Inspector Support**: Edit dates as calendar text or Unix timestamps, and durations in several units.
* **Unity Serialization**: Works with serialized fields and `JsonUtility` without a separate cached value to synchronize.
* **Runtime Assembly**: The `Typio` assembly has no `UnityEditor` dependency. Inspector drawers live in `Typio.Editor`.

## Installation

Requires **Unity 2022.3 or newer**.

**Option A: Install via Package Manager window**

1. In Unity, open **Window** > **Package Manager**.
2. Click the **+** button and select **Add package from git URL...**
3. Enter the following URL and click **Add**:
   `https://github.com/damdordev/Typio.git#1.0.0-preview`

**Option B: Install via `manifest.json`**

Add the package to the `dependencies` block in your project's `Packages/manifest.json`:

```json
"com.damdor.typio": "https://github.com/damdordev/Typio.git#1.0.0-preview"
```

For an embedded installation, place this package in `Packages/typio`. If your scripts use an assembly definition, add a reference to **Typio**.

## Basic Usage

### Serializable Dates

Use `SerializableDateTime` wherever Unity needs to serialize a date:

```csharp
using System;
using Damdor.Typio;

SerializableDateTime savedAt = DateTime.UtcNow;
DateTime restored = savedAt;

// The property and constructor are also available.
savedAt.DateTime = new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc);
var deadline = new SerializableDateTime(savedAt.DateTime.AddDays(7));
```

Both ticks and `Kind` are preserved. A default `SerializableDateTime` represents `DateTime.MinValue` with `DateTimeKind.Unspecified`.

### Serializable Durations

Use `SerializableTimeSpan` to store intervals, offsets, and timer settings:

```csharp
using System;
using Damdor.Typio;

SerializableTimeSpan cooldown = TimeSpan.FromSeconds(2.5);
TimeSpan duration = cooldown;

cooldown.TimeSpan = TimeSpan.FromMinutes(5);
var offset = new SerializableTimeSpan(TimeSpan.FromSeconds(-10));
```

Negative durations and the complete `TimeSpan.MinValue` to `TimeSpan.MaxValue` range are supported. A default `SerializableTimeSpan` represents `TimeSpan.Zero`.

### Using Typio in Components

```csharp
using System;
using Damdor.Typio;
using UnityEngine;

public class SessionTimer : MonoBehaviour
{
    [SerializeField] private SerializableDateTime startedAt;
    [SerializeField] private SerializableTimeSpan sessionLength = TimeSpan.FromMinutes(30);

    public DateTime EndsAt => startedAt.DateTime + sessionLength.TimeSpan;

    public void BeginSession()
    {
        startedAt = DateTime.UtcNow;
    }
}
```

The same fields can be used in a `ScriptableObject`. Unity serializes fields; use public fields or private fields marked with `[SerializeField]`.

### Working with Value Types

Both wrappers are structs. Assigning a wrapper copies its value, so changing a copy does not change the original. When a wrapper comes from a property or collection indexer, assign the modified value back:

```csharp
SerializableTimeSpan[] cooldowns = { TimeSpan.FromSeconds(1) };

var cooldown = cooldowns[0];
cooldown.TimeSpan += TimeSpan.FromSeconds(2);
cooldowns[0] = cooldown;
```

`DateTime` and `TimeSpan` are immutable too: methods such as `AddDays` return a new value. Assign the result to retain the change.

## Inspector

Typio provides custom property drawers with a display-mode selector next to each value.

**Date modes**

| Mode | Format / unit | Example |
| --- | --- | --- |
| FullTime | `yyyy-MM-dd HH:mm:ss` | `2026-10-24 12:00:00` |
| Milliseconds | Whole Unix milliseconds since 1970-01-01 UTC | `0` for the Unix epoch |
| Seconds | Whole Unix seconds since 1970-01-01 UTC | `0` for the Unix epoch |

Calendar input preserves the field's existing `Kind`. Unix modes interpret `Local` values in the system's local time zone and treat `Unspecified` values as UTC. Editing a Unix timestamp preserves the existing kind, converting the instant back to local time for a `Local` field. Dates before the epoch use the standard floor convention (`epoch - 1 tick` displays as `-1`).

Calendar input has second precision; Unix input has the precision of the selected unit. Existing sub-unit ticks remain stored until you commit a different value. Set values in code when you need finer date precision or want to change `Kind`.

**Duration modes**

| Mode | Format / unit | Example |
| --- | --- | --- |
| FullTime | Invariant .NET `g` format: `[-][d:]h:mm:ss[.fffffff]` | `1:02:03:04.5` |
| Milliseconds | Total milliseconds | `1250` |
| Seconds | Total seconds | `1.25` |
| Minutes | Total minutes | `1.5` |
| Hours | Total hours | `2.5` |
| Days | Total days | `0.5` |

Numeric input uses a dot as the decimal separator regardless of the operating system's locale. Duration input is rounded to the nearest tick, with midpoint values rounded away from zero. Invalid or out-of-range input leaves the stored value unchanged. Changing the display mode does not change the stored value.

## Serialization

`SerializableDateTime` stores a serialized `long ticks` and a `DateTimeKind kind`. `SerializableTimeSpan` stores a serialized `long ticks`. The public properties read these fields directly, so Inspector edits and deserialization do not rely on synchronizing a second copy of the value.

Both types implement `ISerializationCallbackReceiver` for API compatibility. Their callbacks require no work, and callers do not need to invoke them manually.

### JSON Serialization

Place the values in a serializable container when using Unity's `JsonUtility`:

```csharp
using System;
using Damdor.Typio;
using UnityEngine;

[Serializable]
public class SessionData
{
    public SerializableDateTime startedAt;
    public SerializableTimeSpan duration;
}

public static class SessionSerialization
{
    public static SessionData RoundTrip()
    {
        var data = new SessionData
        {
            startedAt = DateTime.UtcNow,
            duration = TimeSpan.FromMinutes(30)
        };

        string json = JsonUtility.ToJson(data);
        return JsonUtility.FromJson<SessionData>(json);
    }
}
```

This describes Unity serialization. Other JSON serializers may require their own converters or configuration.

### Compatibility and Time Zones

Existing data containing only `ticks` remains readable. Dates without a stored `kind` deserialize as `DateTimeKind.Unspecified`; the original kind cannot be inferred from ticks alone.

`Local` dates retain wall-clock ticks and kind, but do not store a time-zone identifier, UTC offset, or the internal daylight-saving ambiguity flag. Prefer UTC for timestamps shared between machines. Typio does not provide a `DateTimeOffset` wrapper.

Serialized date ticks and kind must be valid .NET values. Reading a manually corrupted date can throw `ArgumentOutOfRangeException`; values assigned through the public API are valid by construction.

## Supported Types

| Wrapper | .NET value | Serialized fields | Default value |
| --- | --- | --- | --- |
| `SerializableDateTime` | `System.DateTime` | `long ticks`, `DateTimeKind kind` | `DateTime.MinValue`, unspecified kind |
| `SerializableTimeSpan` | `System.TimeSpan` | `long ticks` | `TimeSpan.Zero` |
