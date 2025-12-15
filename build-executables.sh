#!/bin/bash

echo "Building FancyContainerConsole executables..."
echo ""

PROJECT_PATH="src/FancyContainerConsole/FancyContainerConsole.csproj"
OUTPUT_DIR="dist"

# Clean previous builds
if [ -d "$OUTPUT_DIR" ]; then
    echo "Cleaning previous builds..."
    rm -rf "$OUTPUT_DIR"
fi

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Build for Windows x64
echo "Building for Windows (x64)..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$OUTPUT_DIR/win-x64"

if [ $? -eq 0 ]; then
    echo "✓ Windows build completed"
else
    echo "✗ Windows build failed"
    exit 1
fi

echo ""

# Build for Linux x64
echo "Building for Linux (x64)..."
dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$OUTPUT_DIR/linux-x64"

if [ $? -eq 0 ]; then
    echo "✓ Linux build completed"
else
    echo "✗ Linux build failed"
    exit 1
fi

echo ""
echo "Build completed successfully!"
echo ""
echo "Executables location:"
echo "  Windows: $OUTPUT_DIR/win-x64/FancyContainerConsole.exe"
echo "  Linux:   $OUTPUT_DIR/linux-x64/FancyContainerConsole"
echo ""
