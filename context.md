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

### Testing Checklist
- [ ] Interactive Container Dashboard displays all containers
- [ ] Can return to main menu from container selection
- [ ] Can return to main menu after selecting container and pressing ESC
- [ ] All keyboard shortcuts work (L, S, D, C, ESC)
- [ ] Volume dashboard displays all volumes with usage status
- [ ] Can return to main menu from volume selection
- [ ] Can return to main menu after selecting volume and pressing ESC
- [ ] Volume deletion prevented for in-use volumes
- [ ] Main menu no longer shows "List all containers"

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
