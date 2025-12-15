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

#### 1. Interactive Container Dashboard
- Real-time container listing with visual table display
- Keyboard shortcuts for quick actions:
  - `L` - View container logs
  - `S` - Start/Stop container (toggles based on current state)
  - `D` - Delete container (with confirmation)
  - `C` - View container details
  - `ESC` - Return to main menu
- User can select from container list or choose "[Back to Main Menu]" option
- Automatic status detection for smart start/stop toggle

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/InteractiveContainerMenu.cs`
  - Added "[Back to Main Menu]" option to container selection (line 44)
  - Added prompt message for ESC key (line 41)
  - Implemented object-based selection to mix ContainerDto and string choices (lines 43-51)
  - Added check to return when back option is selected (lines 53-56)

#### 2. Volume Management Dashboard
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
- Removed "List all containers" option (replaced by Interactive Container Dashboard)
- Streamlined menu options:
  - Interactive Container Dashboard
  - Manage container (for advanced operations)
  - Manage volumes
  - Exit

**Files Modified:**
- `src/FancyContainerConsole/UI/Menus/MainMenu.cs`
  - Removed "List all containers" from menu choices (line 30 deleted)
  - Removed `ShowContainersAsync()` method entirely
  - Updated switch statement to remove corresponding case

### Key Design Decisions

1. **Navigation Consistency**: Both Interactive Container and Volume dashboards now provide:
   - Visual "[Back to Main Menu]" option in selection lists
   - ESC key support with clear messaging
   - Consistent user experience across all menus

2. **Single Dashboard Philosophy**: Removed redundant "List all containers" since the Interactive Dashboard provides superior functionality with both viewing and management capabilities.

3. **Object-Based Selections**: Used `List<object>` to mix DTO types with string options (like "[Back to Main Menu]"), allowing seamless navigation without breaking type safety.

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
- [ ] Interactive Container Dashboard displays all containers
- [ ] "← Back to Main Menu" appears as first option in container selection
- [ ] Can select "← Back to Main Menu" to return to main menu
- [ ] Can return to main menu after selecting container and pressing ESC
- [ ] All keyboard shortcuts work (L, S, D, C, ESC)
- [ ] Volume dashboard displays all volumes with usage status
- [ ] "← Back to Main Menu" appears as first option in volume selection
- [ ] Can select "← Back to Main Menu" to return to main menu
- [ ] Volume deletion prevented for in-use volumes
- [ ] Main menu no longer shows "List all containers"
- [ ] "← Back to Main Menu" appears as first option in "Manage container" selection
- [ ] "← Back to main menu" appears as first option in container actions menu
- [ ] Container logs display without color code errors
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

## Next Steps / Future Enhancements
- Add filtering/search functionality in dashboards
- Implement container stats display (CPU, memory usage)
- Add volume inspection details view
- Consider adding bulk operations (e.g., stop all containers)
- Add configuration file for customizing keyboard shortcuts
