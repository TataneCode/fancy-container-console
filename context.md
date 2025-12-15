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
  - Added "[Back to Main Menu]" option to volume selection (line 44)
  - Updated prompt message for consistency (line 41)
  - Implemented object-based selection similar to container menu (lines 43-51)
  - Added check to return when back option is selected (lines 53-56)

#### 3. Main Menu Updates
- Simplified menu with unified container management
- Streamlined menu options:
  - Manage container (includes interactive dashboard with keyboard shortcuts)
  - Manage volumes
  - Exit

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Removed "Interactive Container Dashboard" menu option
  - Removed ShowInteractiveContainerDashboardAsync method
  - Merged all interactive dashboard functionality into ManageContainerAsync

### Key Design Decisions

1. **Unified Management Approach**: Both Container and Volume management now follow the same pattern:
   - Visual table display of resources
   - Keyboard shortcuts for quick actions
   - "← Back to Main Menu" option in selection lists
   - ESC key support with clear messaging
   - Loop back to resource list after each action
   - Consistent user experience across all menus

2. **Simplified Menu Structure**: Merged "Interactive Container Dashboard" into "Manage container" to reduce redundancy and provide a single, powerful container management interface.

3. **Object-Based Selections**: Used `List<object>` to mix DTO types with string options (like "← Back to Main Menu"), allowing seamless navigation without breaking type safety.

4. **Keyboard Shortcuts for Efficiency**: Actions are triggered by single key presses (L, S, D, C, ESC) rather than navigating through menus, providing a faster workflow for common operations.

#### 4. Log Display Bug Fix
- Fixed error "color number must be less than or equal to 255" when viewing container logs
- Container logs often contain ANSI escape codes that Spectre.Console attempted to parse as markup
- Solution: Escape all markup characters using `Markup.Escape()` before displaying logs

**Files Modified:**
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs`
  - Added `Markup.Escape()` call in `DisplayLogs()` method (line 62)
  - Logs are now displayed as literal text, preventing markup parsing errors

#### 5. Improved Back Navigation in Selection Prompts
- Improved "Back to Main Menu" option visibility and accessibility in all menus
- Moved "Back" option to the TOP of selection lists (first choice)
- Changed text from "[Back to Main Menu]" to "← Back to Main Menu" for better visibility
- Updated prompt titles to include "(use arrow keys)" instruction
- Applied consistently across all three menu types

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/InteractiveContainerMenu.cs`
  - Moved back option to top of list (line 42)
  - Updated prompt title with arrow key instruction (line 47)
  - Simplified string check (line 52)
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs`
  - Moved back option to top of list (line 42)
  - Updated prompt title with arrow key instruction (line 47)
  - Simplified string check (line 52)
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Added back option to container selection list at top (line 84)
  - Updated prompt title with arrow key instruction (line 89)
  - Moved back option to top in container actions menu (line 118)
  - Reordered switch statement to handle back first (line 127)

**Note:** Spectre.Console's SelectionPrompt doesn't natively support ESC key during selection. The workaround is to provide a prominent "Back" option as the first choice, making it immediately accessible without scrolling.

#### 6. Fixed Malformed Markup Tag Error in Menu Navigation
- Fixed "Encountered malformed markup tag at position X" error when navigating through containers and volumes
- Container/volume names containing special characters (`[`, `]`, etc.) were being interpreted as Spectre.Console markup
- Solution: Escape all container and volume names in selection converters using `Markup.Escape()`

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/InteractiveContainerMenu.cs`
  - Escaped container name and state in UseConverter (lines 49-50)
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs`
  - Escaped volume name in UseConverter (line 50)
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Escaped container name and state in UseConverter (lines 91-92)

#### 7. Fixed Container and Volume Size Display
- Fixed containers and volumes always showing 0 MB for size
- Enabled size calculation by setting `Size = true` in Docker API list parameters
- Updated container size to use `SizeRootFs` (total filesystem size)
- Updated volume size to extract from `UsageData.Size` when available
- Changed display labels from "RAM (MB)" to "Size (MB)" for accuracy (shows disk size, not RAM)
- Changed container details label from "Memory Usage" to "Disk Size"

**Files Modified:**
- `src/FancyContainerConsole/Infrastructure/Docker/DockerClientAdapter.cs`
  - Added `Size = true` to ContainersListParameters (line 23)
- `src/FancyContainerConsole/Infrastructure/Mappers/DockerMapper.cs`
  - Changed to use `SizeRootFs` for container size (line 31)
  - Changed to use `UsageData?.Size` for volume size (line 68)
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs`
  - Changed column header from "RAM (MB)" to "Size (MB)" (line 28)
  - Changed variable name from `ramMb` to `sizeMb` (line 43)
  - Changed details label from "Memory Usage" to "Disk Size" (line 137)

### Testing Checklist
- [ ] Main menu shows three options: "Manage container", "Manage volumes", "Exit"
- [ ] "Manage container" displays container table with all containers
- [ ] "← Back to Main Menu" appears as first option in container selection
- [ ] Can select "← Back to Main Menu" to return to main menu
- [ ] All container keyboard shortcuts work (L, S, D, C, ESC)
- [ ] Container logs display without color code errors (L key)
- [ ] Start/Stop toggles correctly based on container state (S key)
- [ ] Delete prompts for confirmation before deletion (D key)
- [ ] Container details display correctly (C key)
- [ ] ESC key returns to main menu
- [ ] After each action, returns to container list (loops back)
- [ ] Volume management displays all volumes with usage status
- [ ] "← Back to Main Menu" appears as first option in volume selection
- [ ] Can select "← Back to Main Menu" to return to main menu
- [ ] Volume deletion prevented for in-use volumes
- [ ] No malformed markup errors when navigating through containers/volumes with special characters in names
- [ ] Container sizes display correctly (not 0 MB)
- [ ] Volume sizes display correctly (not 0 MB)

## Domain Entities

### Container
- Properties: Id, Name, Image, State, Created, Ports, Volumes, Networks
- Value Objects: PortMapping, NetworkInfo
- Location: `src/FancyContainerConsole/Domain/Entities/Container.cs`

### Volume
- Properties: Id (VolumeId value object), Name, Driver, Mountpoint, CreatedAt, InUse
- Location: `src/FancyContainerConsole/Domain/Entities/Volume.cs`

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
- **DisplayHelper.cs**: Added ILocalizationService parameter to all public methods

**Files Modified:**
- `src/FancyContainerConsole/Program.cs` - Culture parsing and service registration
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs` - Localization integration (~40 strings)
- `src/FancyContainerConsole/UI/Menus/VolumeMenu.cs` - Localization integration (~12 strings)
- `src/FancyContainerConsole/UI/Helpers/DisplayHelper.cs` - Method signatures updated (~25 strings)
- `src/FancyContainerConsole/FancyContainerConsole.csproj` - Embedded resource configuration

### Key Design Decisions

1. **Multiple Resource Files**: Organized by feature (UI, Container, Volume, Messages, Table) for better maintainability and separation of concerns

2. **Service Abstraction**: Created ILocalizationService interface to:
   - Avoid tight coupling to resource files
   - Enable testing with mocked translations
   - Provide centralized culture management
   - Support runtime culture switching

3. **Markup Preservation**: Spectre.Console markup (e.g., `[blue]`, `[/]`) kept inside resource strings to preserve visual context for translators

4. **Key Naming Convention**: Used hierarchical pattern `{Context}_{Category}_{Identifier}` (e.g., `UI_Title_MainMenu`, `Container_Error_FailedToStart`)

5. **ResourceManager Approach**: Used direct ResourceManager instantiation with fully qualified resource names instead of generated Designer.cs files for better control

### Testing Checklist
- [x] Application builds successfully with no errors
- [x] All 34 existing tests pass
- [ ] English UI displays correctly (default)
- [ ] French UI displays correctly with `--culture fr`
- [ ] Culture switches via environment variable `FANCY_CONTAINER_CULTURE`
- [ ] All menus display localized text
- [ ] All error/success messages are localized
- [ ] Table headers and data labels are localized
- [ ] Confirmation prompts are localized
- [ ] Invalid culture falls back gracefully to system culture

## Next Steps / Future Enhancements
- Add filtering/search functionality in dashboards
- Implement container stats display (CPU, memory usage)
- Add volume inspection details view
- Consider adding bulk operations (e.g., stop all containers)
- Add configuration file for customizing keyboard shortcuts
- Add more language translations (Spanish, German, etc.)
- Implement pluralization support for dynamic text
- Add translation validation tests
