# T4Editor

<p align="center">
  <img src="T4Editor\T4Editor.jpg" alt="T4Editor Logo" width="128" height="128">
</p>

<p align="center">
  <strong>A powerful Visual Studio extension for enhanced T4 template editing</strong>
</p>

T4Editor brings professional-grade syntax highlighting, IntelliSense, and advanced editing features to T4 (Text Template Transformation Toolkit) files in Visual Studio. Whether you're working with code generation, automated documentation, or template-based development, T4Editor transforms your T4 editing experience.

## Key Features

### **Advanced Syntax Highlighting**

- **Smart T4 Block Recognition**: Distinct colors for all T4 block types
 
- **Control Blocks** (`<#...#>`) - For C# statements and control flow
- **Class Feature Blocks** (`<#+...#>`) - For methods, properties, and class-level features  
- **Expression Blocks** (`<#=...#>`) - For value output expressions
- **Directive Blocks** (`<#@...#>`) - For template directives and imports
- **Output Blocks** - Static text content outside T4 tags
- **T4 Tags** - The delimiters themselves (`<#`, `#>`, etc.)

### **Intelligent IntelliSense**

- **Context-Aware Code Completion**: Smart suggestions for T4 syntax elements
- **Predefined T4 Constructs**: Quick insertion of common patterns
  - Control block templates
  - Class feature block scaffolding
  - Expression block helpers
  - Import and include directive snippets

### **Advanced Editor Features**

- **Smart Brace Matching**: Highlights matching braces, brackets, parentheses, and quotes
- **Code Outlining**: Collapsible regions for T4 blocks to manage large templates
- **Block Navigation**: Easily navigate between related T4 constructs

### **Visual Studio Integration**

- **Theme Awareness**: Automatically adapts to Visual Studio's light and dark themes
- **Customizable Colors**: Full control over syntax highlighting colors
- **Settings Panel**: Easy-to-use configuration interface
- **File Support**: Works with `.tt`, `.t4`, and `.ttinclude` files

### **High Performance**

- **Optimized for Large Files**: Efficient parsing for templates over 50KB
- **Smart Caching**: Intelligent result caching for improved responsiveness
- **Regex Timeout Protection**: Prevents UI freezing on complex content
- **Incremental Processing**: Only processes visible areas in large files

## Installation

### Visual Studio Marketplace

1. Open Visual Studio
2. Go to **Extensions** -> **Manage Extensions**
3. Search for "T4Editor"
4. Click **Download** and restart Visual Studio

## Supported Versions

| Visual Studio Edition | Version Support |
|----------------------|----------------|
| **Community** | 2019 (16.0+), 2022 (17.0+) |
| **Professional** | 2019 (16.0+), 2022 (17.0+) |
| **Enterprise** | 2019 (16.0+), 2022 (17.0+) |

## Supported File Types

- **`.tt`** - T4 Text Templates
- **`.t4`** - T4 Templates (alternative extension)
- **`.ttinclude`** - T4 Include Files

## Configuration

### Accessing Settings

- **Method 1**: Go to **Tools** -> **T4Editor Settings**
- **Method 2**: Right-click in a T4 file -> **T4Editor Settings**

### Customization Options

- **Syntax Colors**: Customize colors for each T4 block type
- **Theme Integration**: Automatic theme detection and color adjustment
- **Performance Settings**: Adjust parsing thresholds for large files

### IntelliSense in Action

When you type `<#`, T4Editor provides intelligent suggestions:
- `<#...#>` - Control Block
- `<#+...#>` - Class Feature Block  
- `<#=...#>` - Expression Block
- `<#@ import #>` - Import Directive
- `<#@ include #>` - Include Directive

## Architecture & Performance

T4Editor is built with performance and reliability in mind:

### Smart Parsing Strategy

- **Small Files (< 50KB)**: Full document parsing with intelligent caching
- **Large Files (> 50KB)**: Incremental parsing of visible areas
- **Complex Content**: Fallback parser for regex-intensive scenarios

### Performance Optimizations

- **Classification Type Caching**: 90% reduction in registry lookups
- **Snapshot Result Caching**: Eliminates redundant parsing
- **Targeted Processing**: Only parses visible content in large files  
- **Timeout Protection**: Prevents UI freezing on malformed content
- **Graceful Fallbacks**: Maintains functionality even with complex templates

## Quick demo

![T4DEmo](https://github.com/user-attachments/assets/338bbce6-0727-45b8-8d1c-2e96ae5447e2)

## Contributing

We welcome contributions! Here's how you can help:

### Development Setup

1. **Clone the repository**git clone https://github.com/Tim-Maes/T4Editor.git
2. **Prerequisites**
   - Visual Studio 2019/2022 with VSIX development workload
   - .NET Framework 4.7.2

3. **Build and Test**
 
- Open T4Editor.sln in Visual Studio
- Build solution (Ctrl+Shift+B)
- Press F5 to run in experimental instance

## Troubleshooting

### Getting Help
- [Create an issue](https://github.com/Tim-Maes/T4Editor/issues) on GitHub
- Check existing issues for solutions
- Contact the maintainer for support

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

