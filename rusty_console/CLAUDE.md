# Rusty Console - Claude Context

## Project Overview
A Rust TUI application for managing Docker containers, volumes, and images. This is a port of FancyContainerConsole (C#/.NET) to Rust.

## Current Status: Complete (v0.1.0)
All implementation phases are complete and the application compiles and runs.

## Architecture
- **Domain-Driven Design (DDD)** with clean layer separation
- **MVP Pattern** for UI (Model-View-Presenter)
- **Feature-based organization** (screaming architecture)

### Layer Structure
```
src/
├── main.rs              # Entry point, wires all dependencies
├── lib.rs               # Module exports
├── domain/              # Business entities, value objects, no external deps
├── application/         # Services, DTOs, repository traits
├── infrastructure/      # Docker API adapters (bollard)
└── ui/                  # TUI with ratatui (presenters, views, actions)
```

## Key Dependencies
- `ratatui` 0.29 - Terminal UI framework
- `bollard` 0.18 - Docker API client
- `tokio` - Async runtime
- `crossterm` - Terminal backend

## Running the Application
```bash
cargo run
```

## Key Bindings
| Key | Action |
|-----|--------|
| j/k or arrows | Navigate |
| Enter | Select |
| Esc/q | Back/Quit |
| l | View logs (containers) |
| s | Start/Stop (containers) |
| d | Delete (with confirmation) |
| c | View details |
| r | Refresh |
| Ctrl+u/d | Scroll (in logs) |

## Implementation Notes

### Bollard 0.18 API Changes
- `df()` method takes no arguments (removed `DataUsageOptions`)
- `Port.typ` instead of `Port.r#type`
- `Port.private_port` is `u16` not `Option<u16>`

### Files Created/Modified in Last Session
1. `src/lib.rs` - Module exports
2. `src/ui/mod.rs` - UI module definition
3. `src/ui/app.rs` - Main app state machine and event loop
4. `src/main.rs` - Wired all dependencies together
5. Fixed API compatibility issues in infrastructure layer
6. Added `futures-util` to Cargo.toml
7. Created `.gitignore`

## Features
- Container list with start/stop, logs, details, delete
- Volume list with delete (prevents deleting in-use volumes)
- Image list with details, delete
- Confirmation dialogs for destructive actions
- Error popup display
- Main menu navigation

## GitHub Actions Workflows

### CI (`ci.yml`)
Runs on push/PR to main:
- `cargo check` - Compilation check
- `cargo fmt` - Format check
- `cargo clippy` - Linting
- `cargo test` - Tests
- Build on Linux, Windows, macOS

### Release (`release.yml`)
Triggered by version tags (e.g., `git tag v0.1.0 && git push --tags`):
- Builds binaries for:
  - Linux x86_64 (glibc)
  - Linux x86_64 (musl - static)
  - Windows x86_64
  - macOS x86_64
  - macOS ARM64 (Apple Silicon)
- Creates GitHub Release with all binaries

## Future Improvements
- Add loading indicators
- Add search/filter functionality
- Add container create/run
- Add image pull
- Add volume create
