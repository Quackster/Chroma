# Chroma - Java Port

Chroma (Greek word χρώμα for paint) - A furniture renderer for Habbo SWF files, ported to Java with Spring Boot.

## Overview

This is a complete Java port of the original C#/.NET Chroma project. It maintains the same functionality while leveraging the Java ecosystem and Spring Boot framework.

## Project Structure

```
chroma-parent/
├── chroma-lib/          # Core library for furniture rendering
│   └── src/main/java/com/quackster/chroma/
│       ├── ChromaFurniture.java      # Main rendering class
│       ├── ChromaAsset.java          # Furniture asset representation
│       ├── ChromaAnimation.java      # Animation handling
│       ├── ChromaFrame.java          # Frame data
│       ├── util/                     # Utility classes
│       │   ├── FileUtil.java
│       │   └── ImageUtil.java
│       └── extractor/                # SWF extraction
│           └── FurniExtractor.java
│
└── chroma-webapp/       # Spring Boot web application
    └── src/main/java/com/quackster/chromawebapp/
        ├── ChromaWebApplication.java # Main application class
        └── controller/
            └── HomeController.java   # REST API endpoint
```

## Requirements

- **Java 17** or higher
- **Maven 3.6+** for building
- SWF files in the `swfs/hof_furni/` directory

## Building

Build the entire project using Maven:

```bash
mvn clean install
```

This will:
1. Compile the core library (`chroma-lib`)
2. Compile the web application (`chroma-webapp`)
3. Create an executable JAR file

## Running

### Using Maven

```bash
cd chroma-webapp
mvn spring-boot:run
```

### Using the JAR

After building, run the application:

```bash
java -jar chroma-webapp/target/chroma-webapp-1.0.0.jar
```

### Custom Port

To run on a different port:

```bash
java -jar chroma-webapp/target/chroma-webapp-1.0.0.jar --server.port=8090
```

## Setup

1. Create the SWF directory structure:
   ```
   your-working-directory/
   ├── swfs/
   │   └── hof_furni/
   │       ├── rare_dragonlamp.swf
   │       ├── rare_parasol.swf
   │       └── ...
   └── chroma-webapp-1.0.0.jar
   ```

2. Start the application (default port 5000):
   ```bash
   java -jar chroma-webapp-1.0.0.jar
   ```

3. Access furniture rendering via URL:
   ```
   http://localhost:5000/?sprite=rare_parasol&color=0&direction=4&small=false&state=1
   ```

## API Usage

### Endpoint

`GET /`

### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `sprite` | string | required | Furniture sprite name (without .swf extension) |
| `small` or `s` | boolean | false | Render small (32x32) version |
| `state` | integer | 0 | Furniture state/animation (0-100) |
| `direction` or `rotation` | integer | 0 | Viewing direction (0, 2, 4, 6) |
| `color` or `colour` | integer | 0 | Color variation (0-15) |
| `shadow` | boolean | false | Include shadow in render |
| `bg` | boolean | false | Use background image |
| `canvas` | string | transparent | Canvas background color (hex or "transparent") |
| `crop` | boolean | true | Auto-crop whitespace |
| `icon` | boolean | false | Render icon version |

### Example Requests

Basic furniture rendering:
```
http://localhost:5000/?sprite=rare_dragonlamp&color=2&direction=0&small=false
```

Small icon with shadow:
```
http://localhost:5000/?sprite=rare_parasol&small=true&shadow=true&icon=true
```

With custom background:
```
http://localhost:5000/?sprite=throne&canvas=FF0000&crop=false
```

## Integration with PHP

You can proxy requests through PHP for easier integration:

```php
<?php
header('Content-Type: image/png');
echo file_get_contents("http://127.0.0.1:5000/?" . $_SERVER['QUERY_STRING']);
?>
```

## Technology Stack

### Core Library
- **Java 17**: Modern Java features and performance
- **JPEXS FFDec**: SWF file parsing and extraction
- **Java AWT/ImageIO**: Image processing and manipulation
- **Gson**: JSON serialization
- **DOM Parser**: XML parsing for asset metadata

### Web Application
- **Spring Boot 3.2**: Web framework and dependency injection
- **Spring Web**: REST API implementation
- **SLF4J**: Logging facade

## Differences from C# Version

1. **SWF Parsing**: Uses JPEXS FFDec library instead of Flazzy
2. **Image Processing**: Uses Java AWT instead of ImageSharp/System.Drawing
3. **Web Framework**: Spring Boot instead of ASP.NET Core
4. **Dependency Management**: Maven instead of NuGet
5. **Project Structure**: Multi-module Maven project

## Performance Considerations

- First request for each furniture item will be slower (extraction + rendering)
- Subsequent requests use cached PNG files
- Cache directory: `furni_export/{sprite}/export/`
- Each unique combination of parameters creates a separate cached file

## Caching

Generated images are cached in the `furni_export/{sprite}/export/` directory with SHA-1 hashed filenames based on render parameters. This ensures:
- Fast subsequent renders of the same configuration
- No redundant processing
- Efficient disk usage

## Troubleshooting

### SWF File Not Found
- Ensure SWF files are in `swfs/hof_furni/` directory
- Check file permissions
- Verify sprite name matches filename (without .swf)

### Out of Memory Errors
Increase JVM heap size:
```bash
java -Xmx2G -jar chroma-webapp-1.0.0.jar
```

### Port Already in Use
Change the port:
```bash
java -jar chroma-webapp-1.0.0.jar --server.port=8090
```

## Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/Quackster/Chroma.git
cd Chroma

# Build
mvn clean install

# Run
java -jar chroma-webapp/target/chroma-webapp-1.0.0.jar
```

### Module Structure

The project uses Maven multi-module structure:
- `chroma-lib`: Core rendering library (can be used standalone)
- `chroma-webapp`: Spring Boot web application

You can use `chroma-lib` as a dependency in other Java projects:

```xml
<dependency>
    <groupId>com.quackster</groupId>
    <artifactId>chroma-lib</artifactId>
    <version>1.0.0</version>
</dependency>
```

## License

See LICENSE.txt

## Credits

### Original C# Version
- higoka
- Scottstamp
- Arachis
- Speaqer
- Parsnip
- Billsonnn

### Java Port
- Ported to Java with Spring Boot
- Maintains compatibility with original functionality
- Enhanced with modern Java features

## Contributing

Contributions are welcome! Please ensure:
- Code follows Java conventions
- Tests pass (when implemented)
- Documentation is updated
- Commits are clear and descriptive
