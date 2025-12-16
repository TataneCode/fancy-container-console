# Fancy Container Console - Development Context

## Project Overview
A .NET console application for managing Docker containers and volumes with an interactive TUI (Terminal User Interface) built with Spectre.Console.

## Architecture
- **Domain Layer**: Core entities (Container, Volume) and value objects (VolumeId, PortMapping, NetworkInfo)
- **Application Layer**: Services, DTOs, Mappers, and Interfaces
- **Infrastructure Layer**: Docker adapters for interacting with Docker API
- **UI Layer**: Interactive menus and display helpers

## MVP 2 - Interactive Dashboards (2025-12-15)

### Features Implemented

#### 1. Container Management
- Real-time container listing with visual table display
- Keyboard shortcuts for quick actions:
  - `L` - View container logs
  - `S` - Start/Stop container (toggles based on current state)
  - `D` - Delete container (with confirmation)
  - `C` - View container details
  - `ESC` - Return to main menu
- User can select from container list or choose "← Back to Main Menu" option
- Automatic status detection for smart start/stop toggle
- Loops back to container list after each action

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Merged interactive dashboard functionality into ManageContainerAsync method
  - Added container table display using DisplayHelper.DisplayContainers
  - Replaced menu-based actions with keyboard shortcuts (L, S, D, C, ESC)
  - Added while loop to continuously show containers after actions
  - Added PromptForContainerActionAsync for keyboard shortcut handling
  - Added HandleContainerActionAsync to dispatch actions
  - Replaced separate Start/Stop methods with smart StartStopContainerAsync toggle
  - Updated DeleteContainerAsync to accept ContainerDto and include confirmation
  - Added ViewDetailsAsync method for viewing container details
  - Added ContainerActionType enum for action types
- `src/FancyContainerConsole/UI/Menus/InteractiveContainerMenu.cs`
  - File deleted (functionality merged into MainMenu)

#### 2. Volume Management
- Lists all Docker volumes with usage status
- Keyboard shortcuts:
  - `D` - Delete volume (only if not in use)
  - `ESC` - Return to main menu
- User can select from volume list or choose "[Back to Main Menu]" option
- Prevents deletion of volumes currently in use
- Confirmation prompt before deletion

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs`
  - Added "← Back to Main Menu" option to volume selection at top of list
  - Updated prompt message for consistency
  - Implemented object-based selection similar to container menu
  - Added check to return when back option is selected
  - Escaped volume names to prevent markup interpretation

#### 3. Image Management
- Lists all Docker images with usage status
- Keyboard shortcuts:
  - `D` - Delete image (only if not in use)
  - `C` - View image details
  - `ESC` - Return to main menu
- User can select from image list or choose "← Back to Main Menu" option
- Prevents deletion of images currently in use by containers
- Confirmation prompt before deletion
- Displays image details including repository, tag, ID, size, created date, and in-use status

**Files Created:**
- `src/FancyContainerConsole/UI/Menus/ImageMenu.cs`
  - Interactive image management menu with keyboard shortcuts
  - Object-based selection for images with back option at top
  - Delete functionality with in-use protection
  - View details functionality
  - Escaped image names to prevent markup interpretation
  - Fully localized with ILocalizationService integration

**Additional Support:**
- Image entity in Domain layer
- ImageDto in Application layer
- IImageService and ImageService for business logic
- IImageRepository and DockerImageAdapter for Docker API integration
- DisplayHelper methods: DisplayImages() and DisplayImageDetails()
- Image.Strings.resx and Image.Strings.fr.resx resource files for localization

#### 4. Main Menu Updates
- Simplified menu with unified container management
- Streamlined menu options:
  - Manage container (includes interactive dashboard with keyboard shortcuts)
  - Manage volumes
  - Manage images
  - Exit

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Removed "Interactive Container Dashboard" menu option
  - Removed ShowInteractiveContainerDashboardAsync method
  - Merged all interactive dashboard functionality into ManageContainerAsync
  - Added "Manage images" menu option

### Key Design Decisions

1. **Unified Management Approach**: Container, Volume, and Image management all follow the same pattern:
   - Visual table display of resources
   - Keyboard shortcuts for quick actions
   - "← Back to Main Menu" option in selection lists
   - ESC key support with clear messaging
   - Loop back to resource list after each action
   - Consistent user experience across all menus

2. **Simplified Menu Structure**: Merged "Interactive Container Dashboard" into "Manage container" to reduce redundancy and provide a single, powerful container management interface.

3. **Object-Based Selections**: Used `List<object>` to mix DTO types with string options (like "← Back to Main Menu"), allowing seamless navigation without breaking type safety.

4. **Keyboard Shortcuts for Efficiency**: Actions are triggered by single key presses (L, S, D, C, ESC) rather than navigating through menus, providing a faster workflow for common operations.

#### 5. Log Display Bug Fix
- Fixed error "color number must be less than or equal to 255" when viewing container logs
- Container logs often contain ANSI escape codes that Spectre.Console attempted to parse as markup
- Solution: Escape all markup characters using `Markup.Escape()` before displaying logs

**Files Modified:**
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs`
  - Added `Markup.Escape()` call in `DisplayLogs()` method
  - Logs are now displayed as literal text, preventing markup parsing errors

#### 6. Improved Back Navigation in Selection Prompts
- Improved "Back to Main Menu" option visibility and accessibility in all menus
- Moved "Back" option to the TOP of selection lists (first choice)
- Changed text from "[Back to Main Menu]" to "← Back to Main Menu" for better visibility
- Updated prompt titles to include "(use arrow keys)" instruction
- Applied consistently across all menu types

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs`
  - Moved back option to top of list
  - Updated prompt title with arrow key instruction
  - Simplified string check for back option
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Added back option to container selection list at top
  - Updated prompt title with arrow key instruction
  - Reordered logic to handle back option first

**Note:** Spectre.Console's SelectionPrompt doesn't natively support ESC key during selection. The workaround is to provide a prominent "Back" option as the first choice, making it immediately accessible without scrolling.

#### 7. Fixed Malformed Markup Tag Error in Menu Navigation
- Fixed "Encountered malformed markup tag at position X" error when navigating through containers and volumes
- Container/volume names containing special characters (`[`, `]`, etc.) were being interpreted as Spectre.Console markup
- Solution: Escape all container and volume names in selection converters using `Markup.Escape()`

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs`
  - Escaped volume name in UseConverter to prevent markup interpretation
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Escaped container name and state in UseConverter to prevent markup interpretation
- `src/FancyContainerConsole/UI/Menus/ImageMenu.cs`
  - Escaped image repository and tag in UseConverter to prevent markup interpretation

#### 8. Fixed Container and Volume Size Display
- Fixed containers and volumes always showing 0 MB for size
- Enabled size calculation by setting `Size = true` in Docker API list parameters
- Updated container size to use `SizeRootFs` (total filesystem size)
- Updated volume size to extract from `UsageData.Size` when available
- Changed display labels from "RAM (MB)" to "Size (MB)" for accuracy (shows disk size, not RAM)
- Changed container details label from "Memory Usage" to "Disk Size"

**Files Modified:**
- `src/FancyContainerConsole/Infrastructure/Docker/DockerClientAdapter.cs`
  - Added `Size = true` to ContainersListParameters to enable size calculation
- `src/FancyContainerConsole/Infrastructure/Mappers/DockerMapper.cs`
  - Changed to use `SizeRootFs` for container size
  - Changed to use `UsageData?.Size` or size override parameter for volume size
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs`
  - Changed column header from "RAM (MB)" to "Size (MB)"
  - Changed variable name from `ramMb` to `sizeMb`
  - Changed details label from "Memory Usage" to "Disk Size"

### Testing Checklist
- [x] Main menu shows four options: "Manage container", "Manage volumes", "Manage images", "Exit"
- [x] "Manage container" displays container table with all containers
- [x] "← Back to Main Menu" appears as first option in container selection
- [x] Can select "← Back to Main Menu" to return to main menu
- [x] All container keyboard shortcuts work (L, S, D, C, ESC)
- [x] Container logs display without color code errors (L key)
- [x] Start/Stop toggles correctly based on container state (S key)
- [x] Delete prompts for confirmation before deletion (D key)
- [x] Container details display correctly (C key)
- [x] ESC key returns to main menu
- [x] After each action, returns to container list (loops back)
- [x] Volume management displays all volumes with usage status
- [x] "← Back to Main Menu" appears as first option in volume selection
- [x] Can select "← Back to Main Menu" to return to main menu
- [x] Volume deletion prevented for in-use volumes
- [x] No malformed markup errors when navigating through containers/volumes with special characters in names
- [x] Container sizes display correctly (not 0 MB)
- [x] Volume sizes display correctly (not 0 MB)
- [x] Image management displays all images with usage status
- [x] "← Back to Main Menu" appears as first option in image selection
- [x] Can select "← Back to Main Menu" to return to main menu
- [x] Image keyboard shortcuts work (D, C, ESC)
- [x] Image deletion prevented for in-use images (D key)
- [x] Image details display correctly (C key)
- [x] No malformed markup errors when navigating through images with special characters in names

## Domain Entities

### Container
- Properties: Id, Name, Image, State, Created, Ports, Volumes, Networks
- Value Objects: PortMapping, NetworkInfo
- Location: `src/FancyContainerConsole/Domain/Entities/Container.cs`

### Volume
- Properties: Id (VolumeId value object), Name, Driver, Mountpoint, CreatedAt, InUse
- Location: `src/FancyContainerConsole/Domain/Entities/Volume.cs`

### Image
- Properties: Id (ImageId value object), Repository, Tag, Size, CreatedAt, InUse
- Location: `src/FancyContainerConsole/Domain/Entities/Image.cs`

## MVP 3 - Internationalization (2025-12-15)

### Features Implemented

#### 1. .resx-Based Localization System
- Implemented comprehensive localization infrastructure using .resx resource files
- Support for English (default) and French translations
- Culture-aware string management with strongly-typed resource access
- All UI text (~79+ strings) extracted to resource files

**Architecture:**
- `UI/Localization/ILocalizationService.cs` - Service interface for accessing localized strings
- `UI/Localization/LocalizationService.cs` - Implementation using ResourceManager
- `UI/Localization/LocalizationExtensions.cs` - Helper methods for culture resolution

**Resource Files Created:**
- `Resources/UI.Strings.resx` + `.fr.resx` - Common UI strings (menus, navigation, prompts)
- `Resources/Container.Strings.resx` + `.fr.resx` - Container-specific messages
- `Resources/Volume.Strings.resx` + `.fr.resx` - Volume-specific messages
- `Resources/Image.Strings.resx` + `.fr.resx` - Image-specific messages
- `Resources/Messages.Strings.resx` + `.fr.resx` - Status/error/success messages
- `Resources/Table.Strings.resx` + `.fr.resx` - Table headers and labels

#### 2. Culture Resolution
- Command-line argument support: `--culture fr` or `-c fr`
- Environment variable support: `FANCY_CONTAINER_CULTURE`
- Falls back to system culture if not specified
- Priority order: CLI arg → Environment variable → System culture → English

**Usage Examples:**
```bash
# Run in French
dotnet run --culture fr
# or
dotnet run -c fr

# Using environment variable
FANCY_CONTAINER_CULTURE=fr dotnet run

# Default (system culture)
dotnet run
```

#### 3. Updated Components
- **Program.cs**: Added culture resolution and DI registration for ILocalizationService
- **MainMenu.cs**: Injected ILocalizationService, replaced all hardcoded strings
- **VolumeMenu.cs**: Injected ILocalizationService, replaced all hardcoded strings
- **ImageMenu.cs**: Injected ILocalizationService, replaced all hardcoded strings
- **DisplayHelper.cs**: Added ILocalizationService parameter to all public methods

**Files Modified:**
- `src/FancyContainerConsole/Program.cs` - Culture parsing and service registration
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs` - Localization integration (~40 strings)
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs` - Localization integration (~12 strings)
- `src/FancyContainerConsole/UI/Menus/ImageMenu.cs` - Localization integration (~12 strings)
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs` - Method signatures updated (~35 strings)
- `src/FancyContainerConsole/FancyContainerConsole.csproj` - Embedded resource configuration

### Key Design Decisions

1. **Multiple Resource Files**: Organized by feature (UI, Container, Volume, Image, Messages, Table) for better maintainability and separation of concerns

2. **Service Abstraction**: Created ILocalizationService interface to:
   - Avoid tight coupling to resource files
   - Enable testing with mocked translations
   - Provide centralized culture management
   - Support runtime culture switching

3. **Markup Preservation**: Spectre.Console markup (e.g., `[blue]`, `[/]`) kept inside resource strings to preserve visual context for translators

4. **Key Naming Convention**: Used hierarchical pattern `{Context}_{Category}_{Identifier}` (e.g., `UI_Title_MainMenu`, `Container_Error_FailedToStart`)

5. **ResourceManager Approach**: Used direct ResourceManager instantiation with fully qualified resource names instead of generated Designer.cs files for better control

#### 4. Fixed Volume Size and Usage Data
- Fixed volumes always showing 0 MB for size
- Fixed "In Use" status always showing as false/none
- Updated `DockerVolumeAdapter.GetVolumesAsync()` to inspect each volume individually
- Inspection retrieves full volume data including UsageData (Size and RefCount)

**Issue:**
- Docker API's `ListAsync` returns volumes without UsageData populated
- UsageData is only available when inspecting individual volumes

**Solution:**
- Modified GetVolumesAsync to:
  1. List all volumes
  2. Inspect each volume individually to get usage data
  3. Map inspected volumes with full data including size and RefCount
  4. Fall back to basic data if inspection fails

**Files Modified:**
- `src/FancyContainerConsole/Infrastructure/Docker/DockerVolumeAdapter.cs`
  - Updated GetVolumesAsync to inspect each volume (lines 20-43)
  - Added try-catch for graceful degradation if inspection fails

#### 5. Fixed Volume Size Using Docker System DF (2025-12-15)
- Fixed volumes still showing 0 MB for size despite previous fix
- Root cause: Docker API's `UsageData` field is not populated even when inspecting volumes
- Solution: Use `docker system df -v` command to retrieve volume sizes

**Issue:**
- Previous fix attempted to use `VolumeResponse.UsageData.Size`, but this field is null on most systems
- Docker daemon doesn't always populate UsageData even during volume inspection
- Alternative approach using `du` command requires elevated privileges to access volume directories

**Solution:**
- Modified `DockerVolumeAdapter` to use `docker system df -v` command:
  1. Call `docker system df -v --format "{{json .Volumes}}"` to get all volume sizes at once
  2. Parse JSON output to extract size information for each volume
  3. Parse Docker size format (e.g., "1.014GB", "526.1kB", "48.41MB") and convert to bytes
  4. Use calculated sizes when `UsageData.Size` is not available
  5. Fall back to 0 if size calculation fails

**Benefits:**
- Doesn't require elevated privileges (unlike direct filesystem access with `du`)
- More efficient - fetches all volume sizes in a single command
- Uses Docker's own calculation, ensuring accuracy
- Works on all Docker installations

**Files Modified:**
- `src/FancyContainerConsole/Infrastructure/Docker/DockerVolumeAdapter.cs`
  - Added `GetAllVolumeSizesAsync()` method to fetch all volume sizes using docker system df
  - Added `ParseDockerSize()` method to parse Docker size format (e.g., "1.014GB") to bytes
  - Updated `GetVolumesAsync()` to use size dictionary from docker system df
  - Updated `GetVolumeByNameAsync()` to use size from docker system df
  - Check volume in-use status by inspecting container mounts
- `src/FancyContainerConsole/Infrastructure/Mappers/DockerMapper.cs`
  - Updated `ToDomain()` method signature to accept optional size override parameter
  - Use size override if provided, otherwise fallback to UsageData.Size

### Testing Checklist
- [x] Application builds successfully with no errors
- [x] All 44 tests pass (34 original + 10 localization)
- [x] English UI displays correctly (default)
- [x] French UI displays correctly with `--culture fr`
- [x] Error messages are localized (verified in French)
- [x] Culture switches via environment variable `FANCY_CONTAINER_CULTURE`
- [x] All menus display localized text (Container, Volume, Image, Main Menu)
- [x] Table headers and data labels are localized
- [x] Confirmation prompts are localized
- [x] Invalid culture falls back gracefully to system culture
- [x] Volume sizes display correctly (using docker system df)
- [x] Volume "In Use" status displays correctly
- [x] Image menu fully localized with Image.Strings resources

## Next Steps / Future Enhancements
- Add filtering/search functionality in dashboards
- Implement container stats display (CPU, memory usage)
- Add volume inspection details view
- Consider adding bulk operations (e.g., stop all containers)
- Add configuration file for customizing keyboard shortcuts
- Add more language translations (Spanish, German, etc.)
- Implement pluralization support for dynamic text
- Add translation validation tests
