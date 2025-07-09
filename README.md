# ConfigLocker

A powerful configuration file watcher and merger that supports multiple file formats including JSON, XML, INI, and PlainText.

## Features

- **Multi-format Support**: Watch and merge configuration files in JSON, XML, INI, and PlainText formats
- **Real-time Monitoring**: Automatically detect changes to input files and update output files
- **Smart Merging**: Intelligently merge configuration data while preserving structure
- **PlainText Overwriting**: For plaintext files, completely overwrite the output file with combined input content
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
- **Extensions**: `.ini`, `.cfg`

### PlainText
- **Behavior**: Overwrites entire output file (no merging)
- **Features**: Concatenates content from multiple input files
- **Extensions**: `.txt`, `.text`, `.log`

## Configuration

Create a `ConfigLocker.json` file in your application directory:

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
    },
    {
      "name": "My XML Config",
      "description": "Watches XML configuration files",
      "type": "XML",
      "enabled": true,
      "inputs": [
        "./config/base.xml",
        "./config/override.xml"
      ],
      "output": "./config/final.xml",
      "checkonchange": true,
      "checkonstartup": true,
      "checkevery": "00:30:00"
    },
    {
      "name": "My PlainText Config",
      "description": "Watches plaintext files",
      "type": "PlainText",
      "enabled": true,
      "inputs": [
        "./config/base.txt",
        "./config/override.txt"
      ],
      "output": "./config/final.txt",
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
- **type**: File format type (JSON, XML, INI, PlainText)
- **enabled**: Whether the watcher is active
- **inputs**: Array of input file paths to monitor
- **output**: Output file path to update
- **checkonchange**: Whether to check for changes when files are modified
- **checkonstartup**: Whether to process files when the application starts
- **checkevery**: How often to check for changes (TimeSpan format)

## Usage

1. **Build the application**:
   ```bash
   dotnet build
   ```

2. **Configure your watchers** in `ConfigLocker.json`

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Modify input files** and watch the output files update automatically

## Examples

See the `examples/` directory for sample configuration files demonstrating each format:

- `examples/input1.json` + `examples/input2.json` → `examples/output.json`
- `examples/input1.xml` + `examples/input2.xml` → `examples/output.xml`
- `examples/input1.ini` + `examples/input2.ini` → `examples/output.ini`
- `examples/input1.txt` + `examples/input2.txt` → `examples/output.txt`

## Architecture

The application uses a modular, interface-based architecture:

- **IConfigProcessor**: Interface for file format processors
- **JsonConfigProcessor**: Handles JSON parsing and merging
- **XmlConfigProcessor**: Handles XML parsing and merging
- **IniConfigProcessor**: Handles INI parsing and merging
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
- System.Xml.Linq

## License

See LICENSE.txt for license information.