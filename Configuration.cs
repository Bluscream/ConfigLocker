﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

// <auto-generated />
//
// To parse this JSON data, add NuGet 'System.Text.Json' then do:
//
//    using ConfigLocker;
//
//    var sourceList = SourceList.FromJson(jsonString);

public partial class Configuration {
    public static Configuration FromJson(string json) => JsonSerializer.Deserialize<Configuration>(json, Converter.Settings);
}

public static class Serialize {
    public static string ToJson(this Configuration self) => JsonSerializer.Serialize(self, Converter.Settings);
}

internal static class Converter {
    public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General) {
        Converters =
        {
            LayoutUnionConverter.Singleton,
            new DateOnlyConverter(),
            new TimeOnlyConverter(),
            IsoDateTimeOffsetConverter.Singleton
        },
    };
}

internal class LayoutUnionConverter : JsonConverter<LayoutUnion> {
    public override bool CanConvert(Type t) => t == typeof(LayoutUnion);

    public override LayoutUnion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                return new LayoutUnion { String = stringValue };
            case JsonTokenType.StartObject:
                var objectValue = JsonSerializer.Deserialize<LayoutLayout>(ref reader, options);
                return new LayoutUnion { LayoutLayout = objectValue };
        }
        throw new Exception("Cannot unmarshal type LayoutUnion");
    }

    public override void Write(Utf8JsonWriter writer, LayoutUnion value, JsonSerializerOptions options) {
        if (value.String != null) {
            JsonSerializer.Serialize(writer, value.String, options);
            return;
        }
        if (value.LayoutLayout != null) {
            JsonSerializer.Serialize(writer, value.LayoutLayout, options);
            return;
        }
        throw new Exception("Cannot marshal type LayoutUnion");
    }

    public static readonly LayoutUnionConverter Singleton = new LayoutUnionConverter();
}

internal class ParseStringConverter : JsonConverter<bool> {
    public override bool CanConvert(Type t) => t == typeof(bool);

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        bool b;
        if (Boolean.TryParse(value, out b)) {
            return b;
        }
        throw new Exception("Cannot unmarshal type bool");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) {
        var boolString = value ? "true" : "false";
        JsonSerializer.Serialize(writer, boolString, options);
        return;
    }

    public static readonly ParseStringConverter Singleton = new ParseStringConverter();
}

public class DateOnlyConverter : JsonConverter<DateOnly> {
    private readonly string serializationFormat;
    public DateOnlyConverter() : this(null) { }

    public DateOnlyConverter(string? serializationFormat) {
        this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
    }

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        return DateOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
}

public class TimeOnlyConverter : JsonConverter<TimeOnly> {
    private readonly string serializationFormat;

    public TimeOnlyConverter() : this(null) { }

    public TimeOnlyConverter(string? serializationFormat) {
        this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
    }

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        return TimeOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
}

internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
    public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

    private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
    private string? _dateTimeFormat;
    private CultureInfo? _culture;

    public DateTimeStyles DateTimeStyles {
        get => _dateTimeStyles;
        set => _dateTimeStyles = value;
    }

    public string? DateTimeFormat {
        get => _dateTimeFormat ?? string.Empty;
        set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
    }

    public CultureInfo Culture {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
        string text;


        if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal) {
            value = value.ToUniversalTime();
        }

        text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

        writer.WriteStringValue(text);
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        string? dateText = reader.GetString();

        if (string.IsNullOrEmpty(dateText) == false) {
            if (!string.IsNullOrEmpty(_dateTimeFormat)) {
                return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
            } else {
                return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
            }
        } else {
            return default(DateTimeOffset);
        }
    }


    public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603