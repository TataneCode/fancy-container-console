# Fancy Container Console

A modern, interactive .NET console application for managing Docker containers, volumes, and images with an elegant Terminal User Interface (TUI).

## Features

### Container Management
- **Interactive Table Navigation**: Navigate through containers with arrow keys
- **Visual Feedback**: Yellow highlighting and ">" cursor indicator for selected items
- **Real-time Operations**:
  - 🔍 **View logs** (L key)
  - ▶️ **Start/Stop** containers with auto-refresh (S key)
  - 🗑️ **Delete** with confirmation (D key)
  - ℹ️ **View details** - networks, ports, volumes, size (C key)
- **Smart Status Detection**: Automatically toggles between start/stop based on container state
- **Live Updates**: Table refreshes automatically after state changes

### Volume Management
- **Dashboard View**: List all volumes with name, size, and usage status
- **Safe Deletion**: Prevents deletion of volumes in use
- **Size Calculation**: Accurate volume sizes using Docker system df
- **Usage Tracking**: Real-time in-use status based on container mounts

### Image Management
- **Image Catalog**: View all images with repository, tag, ID, size, and creation date
- **Usage Protection**: Prevents deletion of images used by containers
- **Details View**: Inspect image metadata
- **Smart Actions**: Context-aware operations (C for details, D for delete)

### Internationalization
- **Multi-language Support**: English (default) and French
- **Culture Resolution**: Via command-line args, environment variables, or system culture
- **Comprehensive Translation**: All UI strings, messages, and prompts localized

### User Experience
- **Direct Action Keys**: No need to press Enter before action keys - press D to delete directly
- **Unified Navigation**: Consistent keyboard shortcuts across all views
- **Visual Hierarchy**: Clear separation between navigation options and data tables
- **Error Handling**: Graceful error messages with localization support

## Architecture

### Project Structure

```
FancyContainerConsole/
├── src/FancyContainerConsole/
│   ├── Domain/                      # Core business logic
│   │   ├── Entities/                # Domain entities
│   │   │   ├── Container.cs         # Container entity with state, ports, volumes
│   │   │   ├── Volume.cs            # Volume entity with size and usage
│   │   │   └── Image.cs             # Image entity with repository and tags
│   │   └── ValueObjects/            # Value objects
│   │       ├── VolumeId.cs          # Volume identifier
│   │       ├── ImageId.cs           # Image identifier
│   │       ├── PortMapping.cs       # Port mapping (public:private)
│   │       └── NetworkInfo.cs       # Network configuration
│   ├── Application/                 # Application layer
│   │   ├── DTOs/                    # Data transfer objects
│   │   │   ├── ContainerDto.cs      # Container data for UI
│   │   │   ├── VolumeDto.cs         # Volume data for UI
│   │   │   └── ImageDto.cs          # Image data for UI
│   │   ├── Interfaces/              # Service interfaces
│   │   │   ├── IContainerService.cs # Container operations
│   │   │   ├── IVolumeService.cs    # Volume operations
│   │   │   └── IImageService.cs     # Image operations
│   │   ├── Services/                # Business logic implementation
│   │   │   ├── ContainerService.cs  # Container management
│   │   │   ├── VolumeService.cs     # Volume management
│   │   │   └── ImageService.cs      # Image management
│   │   └── Mappers/                 # Object mapping
│   │       └── DtoMapper.cs         # Entity ↔ DTO conversion
│   ├── Infrastructure/              # External systems
│   │   ├── Docker/                  # Docker API integration
│   │   │   ├── DockerClientAdapter.cs  # Container operations
│   │   │   ├── DockerVolumeAdapter.cs  # Volume operations with system df
│   │   │   └── DockerImageAdapter.cs   # Image operations
│   │   ├── Interfaces/              # Repository interfaces
│   │   │   ├── IContainerRepository.cs
│   │   │   ├── IVolumeRepository.cs
│   │   │   └── IImageRepository.cs
│   │   └── Mappers/                 # Infrastructure mapping
│   │       └── DockerMapper.cs      # Docker API ↔ Domain conversion
│   ├── UI/                          # Presentation layer
│   │   ├── Menus/                   # Interactive menus
│   │   │   ├── MainMenu.cs          # Main menu with Spectre.Console
│   │   │   ├── ContainerMenu.cs     # Container management UI
│   │   │   ├── VolumeMenu.cs        # Volume management UI
│   │   │   └── ImageMenu.cs         # Image management UI
│   │   ├── Helpers/                 # UI utilities
│   │   │   ├── DisplayHelper.cs     # Table rendering with highlighting
│   │   │   └── TableSelectionHelper.cs  # Interactive table navigation
│   │   └── Localization/            # Internationalization
│   │       ├── ILocalizationService.cs
│   │       ├── LocalizationService.cs
│   │       └── LocalizationExtensions.cs
│   ├── Resources/                   # Localization resources
│   │   ├── UI.Strings.resx/.fr.resx       # Common UI strings
│   │   ├── Container.Strings.resx/.fr.resx # Container messages
│   │   ├── Volume.Strings.resx/.fr.resx    # Volume messages
│   │   ├── Image.Strings.resx/.fr.resx     # Image messages
│   │   ├── Messages.Strings.resx/.fr.resx  # Status/error messages
│   │   └── Table.Strings.resx/.fr.resx     # Table headers
│   └── Program.cs                   # Entry point with DI setup
└── tests/FancyContainerConsole.Tests/  # Unit tests (xUnit, FluentAssertions, Moq)
```

### Layer Responsibilities

#### Domain Layer
- **Pure business logic** - no external dependencies
- **Entities**: Core business objects (Container, Volume, Image)
- **Value Objects**: Immutable, side-effect-free objects (VolumeId, PortMapping)
- **Domain rules**: Container state transitions, volume usage validation

#### Application Layer
- **Use cases** and **business workflows**
- **Services**: Orchestrate domain operations (ContainerService, VolumeService)
- **DTOs**: Decouple domain from presentation
- **Interfaces**: Define contracts for infrastructure

#### Infrastructure Layer
- **External integrations**: Docker API via Docker.DotNet library
- **Repositories**: Implement data access patterns
- **Adapters**: Translate Docker API responses to domain entities
- **Special features**:
  - Volume size calculation using `docker system df`
  - Volume usage detection via container mount inspection
  - Size parsing for Docker format (e.g., "1.014GB" → bytes)

#### UI Layer
- **Presentation logic** using Spectre.Console
- **Interactive tables** with keyboard navigation (↑↓ arrows)
- **Visual feedback**: Yellow highlighting, cursor indicators
- **Action handling**: Direct key press actions (L, S, D, C)
- **Localization**: Multi-language support via ILocalizationService

### Design Patterns

- **Dependency Injection**: Built-in .NET DI container for loose coupling
- **Repository Pattern**: Abstract data access behind interfaces
- **Adapter Pattern**: DockerClientAdapter wraps Docker.DotNet client
- **Service Layer**: Separate business logic from presentation
- **DTO Pattern**: Decouple layers with data transfer objects
- **Generic Table Selection**: Reusable `TableSelectionHelper<T>` for all resource types

## Prerequisites

- **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Docker** installed and running
- **Permissions**:
  - Linux: May require `sudo` or user in `docker` group
  - Windows: Docker Desktop running

### Adding User to Docker Group (Linux)

```bash
# Add current user to docker group
sudo usermod -aG docker $USER

# Log out and back in for changes to take effect
# Or use: newgrp docker

# Verify
docker ps
```

## Building the Application

### Development Build

```bash
# Clone the repository
git clone <repository-url>
cd fancy-container-console

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Build output location
# Debug: ./src/FancyContainerConsole/bin/Debug/net10.0/
# Release: ./src/FancyContainerConsole/bin/Release/net10.0/
```

### Release Build

```bash
# Build with Release configuration
dotnet build --configuration Release

# Output: ./src/FancyContainerConsole/bin/Release/net10.0/FancyContainerConsole.dll
```

### Run Without Building

```bash
# Run directly from source
dotnet run --project src/FancyContainerConsole/FancyContainerConsole.csproj

# Run with culture
dotnet run --project src/FancyContainerConsole/FancyContainerConsole.csproj -- --culture fr
```

## Running the Application

### Quick Start

```bash
# Default (system culture)
dotnet run --project src/FancyContainerConsole

# Run in French
dotnet run --project src/FancyContainerConsole -- --culture fr
dotnet run --project src/FancyContainerConsole -- -c fr

# Using environment variable
FANCY_CONTAINER_CULTURE=fr dotnet run --project src/FancyContainerConsole
```

### From Build Output

```bash
# After building
cd src/FancyContainerConsole/bin/Debug/net10.0
dotnet FancyContainerConsole.dll

# With culture argument
dotnet FancyContainerConsole.dll --culture fr
```

### Navigation Reference

#### Main Menu
- **Arrow keys (↑↓)**: Navigate options
- **Enter**: Select option
- **ESC**: Exit application (with confirmation)

#### Container Management
- **Arrow keys (↑↓)**: Navigate containers
- **L**: View container logs
- **S**: Start/Stop container (auto-refresh)
- **D**: Delete container (with confirmation)
- **C**: View container details
- **ESC** or **Enter on "Back"**: Return to main menu

#### Volume Management
- **Arrow keys (↑↓)**: Navigate volumes
- **D**: Delete volume (only if not in use)
- **ESC** or **Enter on "Back"**: Return to main menu

#### Image Management
- **Arrow keys (↑↓)**: Navigate images
- **D**: Delete image (only if not in use)
- **C**: View image details
- **ESC** or **Enter on "Back"**: Return to main menu

## Publishing

### Self-Contained Executable (Recommended for Distribution)

Creates a single executable with .NET runtime included - no .NET installation required on target machine.

#### Linux (x64)

```bash
# Publish for Linux x64
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/linux-x64

# Output: ./publish/linux-x64/FancyContainerConsole
# Size: ~70-90 MB (includes .NET runtime)

# Run the executable
./publish/linux-x64/FancyContainerConsole

# Make it executable (if needed)
chmod +x ./publish/linux-x64/FancyContainerConsole

# Optional: Move to PATH
sudo mv ./publish/linux-x64/FancyContainerConsole /usr/local/bin/
```

#### Linux (ARM64, e.g., Raspberry Pi)

```bash
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime linux-arm64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/linux-arm64
```

#### Windows (x64)

```bash
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  --output ./publish/win-x64

# Output: ./publish/win-x64/FancyContainerConsole.exe
# Run: ./publish/win-x64/FancyContainerConsole.exe
```

#### macOS (x64)

```bash
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime osx-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/osx-x64
```

#### macOS (ARM64, Apple Silicon)

```bash
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime osx-arm64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  --output ./publish/osx-arm64
```

### Framework-Dependent Deployment (Smaller Size)

Requires .NET 10 runtime on target machine. Produces smaller executables (~200 KB).

```bash
# Linux x64
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained false \
  --output ./publish/linux-x64-fd

# Windows x64
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained false ^
  --output ./publish/win-x64-fd
```

### Size-Optimized Build (Trimmed)

Removes unused code for smaller executables. Use with caution - test thoroughly.

```bash
dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:TrimMode=partial \
  --output ./publish/linux-x64-trimmed

# Size: ~40-50 MB (smaller than default self-contained)
```

### Publish Options Explained

| Option | Description |
|--------|-------------|
| `--configuration Release` | Optimized build with no debug symbols |
| `--runtime <RID>` | Target runtime identifier (linux-x64, win-x64, etc.) |
| `--self-contained true` | Include .NET runtime (no installation needed) |
| `--self-contained false` | Require .NET runtime on target machine |
| `-p:PublishSingleFile=true` | Bundle everything into one executable |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | Include native deps in single file |
| `-p:PublishTrimmed=true` | Remove unused code (advanced) |
| `--output <path>` | Output directory |

### Runtime Identifiers (RID)

| Platform | RID | Notes |
|----------|-----|-------|
| Linux x64 | `linux-x64` | Most Linux distributions |
| Linux ARM64 | `linux-arm64` | Raspberry Pi, ARM servers |
| Windows x64 | `win-x64` | Windows 10/11 64-bit |
| macOS x64 | `osx-x64` | Intel Macs |
| macOS ARM64 | `osx-arm64` | Apple Silicon (M1/M2/M3) |

[Full RID catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

### Docker Image (Alternative Deployment)

Create a Docker image to run Fancy Container Console in a container:

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/FancyContainerConsole/FancyContainerConsole.csproj \
    -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .

# Mount Docker socket to manage host containers
VOLUME /var/run/docker.sock

ENTRYPOINT ["dotnet", "FancyContainerConsole.dll"]
```

```bash
# Build image
docker build -t fancy-container-console .

# Run with Docker socket mounted
docker run -it --rm \
  -v /var/run/docker.sock:/var/run/docker.sock \
  fancy-container-console
```

## Testing

### Run All Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

### Test Structure

```
tests/FancyContainerConsole.Tests/
├── Application/           # Service tests (70%+ coverage)
│   ├── ContainerServiceTests.cs
│   ├── VolumeServiceTests.cs
│   └── ImageServiceTests.cs
├── Domain/                # Entity and value object tests
├── Infrastructure/        # Repository and adapter tests
└── UI/                    # UI logic tests
```

### Test Technologies

- **xUnit**: Test framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework for dependencies
- **NetArchTest**: Architecture rules enforcement

## Configuration

### Culture Settings

Priority order:
1. Command-line argument: `--culture fr` or `-c fr`
2. Environment variable: `FANCY_CONTAINER_CULTURE=fr`
3. System culture (detected automatically)
4. Default: English (en)

### Docker Socket Location

Default: `/var/run/docker.sock` (Linux/macOS) or `npipe://./pipe/docker_engine` (Windows)

Override via environment variable:
```bash
DOCKER_HOST=tcp://192.168.1.100:2375 dotnet run
```

## Troubleshooting

### Permission Denied (Linux)

```bash
# Error: permission denied while trying to connect to Docker daemon
# Solution: Add user to docker group
sudo usermod -aG docker $USER
newgrp docker
```

### Docker Not Running

```bash
# Error: Cannot connect to the Docker daemon
# Solution: Start Docker service

# Linux (systemd)
sudo systemctl start docker

# macOS/Windows
# Start Docker Desktop application
```

### Volume Sizes Show 0 MB

```bash
# Verify docker system df works
docker system df -v

# Check Docker version (requires 1.25+)
docker --version
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Run tests (`dotnet test`)
5. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
6. Push to the branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- **Spectre.Console** - Beautiful console UI framework
- **Docker.DotNet** - Docker API client for .NET
- **.NET 10** - Modern, cross-platform runtime
- **xUnit, FluentAssertions, Moq** - Excellent testing tools

## Roadmap

- [ ] Container stats display (CPU, memory usage)
- [ ] Filtering and search in tables
- [ ] Volume inspection details view
- [ ] Bulk operations (stop all containers)
- [ ] Network management
- [ ] Compose file support
- [ ] Customizable keyboard shortcuts
- [ ] Additional language translations (Spanish, German, Italian)
- [ ] Pagination for large lists
- [ ] Color theme customization
- [ ] Export logs to file
- [ ] Container resource limits editing
