# ConfigLocker

A powerful configuration file watcher and merger that supports multiple file formats including JSON, XML, INI, CFG, and PlainText.

## Features

- **Multi-format Support**: Watch and merge configuration files in JSON, XML, INI, CFG, and PlainText formats
- **Real-time Monitoring**: Automatically detect changes to input files and update output files
- **Smart Merging**: Intelligently merge configuration data while preserving structure
- **PlainText Overwriting**: For plaintext files, completely overwrite the output file with combined input content
- **Modular Configuration**: Organize watchers in individual JSON files within a `config-watchers` directory
- **Backup Support**: Automatic backup creation before making changes
- **Comprehensive Logging**: Detailed logging with NLog integration
- **Extensible Architecture**: Easy to add new file format processors

## Supported Formats

### JSON
- **Merging**: Deep recursive merging of JSON objects
- **Features**: Preserves nested structures, arrays, and data types
- **Extensions**: `.json`

### XML
- **Merging**: Intelligent XML element merging with attribute support
- **Features**: Handles attributes, nested elements, and text content
- **Extensions**: `.xml`

### INI
- **Merging**: Section-based merging with key-value pairs
- **Features**: Supports global keys and sectioned configuration
- **Extensions**: `.ini`

### CFG
- **Merging**: Section-based merging with custom syntax
- **Features**: Supports comments (`#`, `;`, `//`) and quoted values
- **Syntax**: `<any string> variable "value"`
- **Extensions**: `.cfg`

### PlainText
- **Behavior**: Overwrites entire output file (no merging)
- **Features**: Concatenates content from multiple input files
- **Extensions**: `.txt`, `.text`, `.log`

## Configuration

ConfigLocker supports two configuration approaches:

### 1. Modular Configuration (Recommended)

Create a `config-watchers` directory and place individual JSON files containing watcher configurations:

```
config-watchers/
├── json-watchers.json
├── xml-watchers.json
├── ini-watchers.json
├── cfg-watchers.json
├── plaintext-watchers.json
└── multi-watchers.json
```

Each JSON file should contain a `watchers` array:

```json
{
  "watchers": [
    {
      "name": "My JSON Config",
      "description": "Watches JSON configuration files",
      "type": "JSON",
      "enabled": true,
      "inputs": [
        "./config/base.json",
        "./config/override.json"
      ],
      "output": "./config/final.json",
      "checkonchange": true,
      "checkonstartup": true,
      "checkevery": "00:30:00"
    }
  ]
}
```

### 2. Single File Configuration (Legacy)

Alternatively, you can define all watchers in the main `ConfigLocker.json` file:

```json
{
  "watchers": [
    {
      "name": "My JSON Config",
      "description": "Watches JSON configuration files",
      "type": "JSON",
      "enabled": true,
      "inputs": [
        "./config/base.json",
        "./config/override.json"
      ],
      "output": "./config/final.json",
      "checkonchange": true,
      "checkonstartup": true,
      "checkevery": "00:30:00"
    }
  ]
}
```

### Watcher Configuration Options

- **name**: Human-readable name for the watcher
- **description**: Optional description of what the watcher does
- **type**: File format type (JSON, XML, INI, CFG, PlainText)
- **enabled**: Whether the watcher is active
- **inputs**: Array of input file paths to monitor
- **output**: Output file path to update
- **checkonchange**: Whether to check for changes when files are modified
- **checkonstartup**: Whether to process files when the application starts
- **checkevery**: How often to check for changes (TimeSpan format)

## CFG Format Specification

The CFG format uses a specific syntax:

```
<section> <variable> "value"
```

### Features:
- **Comments**: Lines starting with `#`, `;`, or `//` are ignored
- **Quoted Values**: Values must be enclosed in double quotes
- **Escaped Quotes**: Use `\"` to include quotes in values
- **Flexible Sections**: Section names can contain spaces

### Example CFG File:
```
# CFG Configuration File
# This is a comment

; Another comment style
database host "localhost"
database port "5432"
database name "mydb"

// Yet another comment style
logging level "info"
logging file "app.log"

features cache "true"
features debug "false"
```

## Usage

1. **Build the application**:
   ```bash
   dotnet build
   ```

2. **Configure your watchers**:
   - Create a `config-watchers` directory
   - Add individual JSON files for each watcher or group of watchers
   - Or use the legacy approach with a single `ConfigLocker.json` file

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Modify input files** and watch the output files update automatically

## Examples

### Individual Watcher Files

**config-watchers/json-watchers.json**:
```json
{
  "watchers": [
    {
      "name": "JSON Config Example",
      "type": "JSON",
      "enabled": true,
      "inputs": ["./examples/input1.json", "./examples/input2.json"],
      "output": "./examples/output.json"
    }
  ]
}
```

**config-watchers/multi-watchers.json**:
```json
{
  "watchers": [
    {
      "name": "Database Configs",
      "type": "JSON",
      "enabled": true,
      "inputs": ["./config/database/base.json", "./config/database/override.json"],
      "output": "./config/database/final.json"
    },
    {
      "name": "Logging Configs",
      "type": "XML",
      "enabled": true,
      "inputs": ["./config/logging/base.xml", "./config/logging/override.xml"],
      "output": "./config/logging/final.xml"
    }
  ]
}
```

### Sample Configuration Files

See the `examples/` directory for sample configuration files demonstrating each format:

- `examples/input1.json` + `examples/input2.json` → `examples/output.json`
- `examples/input1.xml` + `examples/input2.xml` → `examples/output.xml`
- `examples/input1.ini` + `examples/input2.ini` → `examples/output.ini`
- `examples/input1.cfg` + `examples/input2.cfg` → `examples/output.cfg`
- `examples/input1.txt` + `examples/input2.txt` → `examples/output.txt`

## Architecture

The application uses a modular, interface-based architecture:

- **IConfigProcessor**: Interface for file format processors
- **JsonConfigProcessor**: Handles JSON parsing and merging
- **XmlConfigProcessor**: Handles XML parsing and merging
- **IniConfigProcessor**: Handles INI parsing and merging
- **CfgConfigProcessor**: Handles CFG parsing and merging
- **PlainTextConfigProcessor**: Handles plaintext concatenation

### Adding New Formats

To add support for a new file format:

1. Implement the `IConfigProcessor` interface
2. Add the new format to the `ConfigType` enum
3. Update the `GetFileTypeFromExtension` method
4. Add the processor to the `InitializeConfigProcessor` method

## Logging

The application uses NLog for comprehensive logging. Logs are written to:
- Console output
- File: `%TEMP%\ConfigLocker.log`
- Internal log: `%TEMP%\ConfigLocker-internal.log`

## Dependencies

- .NET 8.0
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- NLog
- ini-parser-netcore

## License

See LICENSE.txt for license information.