# T4Editor

<p align="center">
  <img src="T4Editor\T4Editor.jpg" alt="T4Editor Logo" width="128" height="128">
</p>

<p align="center">
  <strong>A powerful Visual Studio extension for enhanced T4 template editing</strong>
</p>

<div align="center">

|Build Status| Downloads |
|------------|------------|
|![Build Status](https://dev.azure.com/Epsicode/T4Editor/_apis/build/status/Tim-Maes.T4Editor%20(1)?branchName=master)|![Downloads](https://img.shields.io/visual-studio-marketplace/d/TimMaes.t4editor)|

</div>

T4Editor brings professional-grade syntax highlighting, IntelliSense, C# language features to T4 (Text Template Transformation Toolkit) files in Visual Studio. Whether you're working with code generation, automated documentation, or template-based development, T4Editor transforms your T4 editing experience.


<div align="center">

<img width="1825" height="744" alt="image" src="https://github.com/user-attachments/assets/f8ad260a-3af4-46dc-9027-db696b078f75" />

</div>

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

- **C# Language Services**: Full Roslyn-powered IntelliSense inside code blocks
  - Type and member completion
  - Context-aware suggestions based on available variables and types
  - Powered by a hidden Roslyn workspace with framework references
- **Context-Aware Completion**: C# completions inside code blocks, T4 snippets outside
- **Predefined T4 Constructs**: Quick insertion of common patterns
  - Control block templates
  - Class feature block scaffolding
  - Expression block helpers
  - Import and include directive snippets

### **Advanced Editor Features**

- **Smart Brace Matching**: Highlights matching braces, brackets, parentheses, and quotes
- **Code Outlining**: Collapsible regions for T4 blocks to manage large templates
- **Block Navigation**: Easily navigate between related T4 constructs
- **Error Detection**: Identifies unclosed T4 blocks and syntax issues
- **C# Error Squiggles**: Real-time Roslyn diagnostics shown as squiggles inside code blocks
- **C# Syntax Highlighting**: Full C# colorization (keywords, types, strings, etc.) within code blocks

### **Visual Studio Integration**

- **Theme Awareness**: Automatically adapts to Visual Studio's light and dark themes
- **Customizable Colors**: Full control over syntax highlighting colors
- **Settings Panel**: Easy-to-use configuration interface
- **File Support**: Works with `.tt`, `.t4`, and `.ttinclude` files

### **High Performance**

- **Token-Based Parsing**: Modern linear parsing approach replaces complex RegEx patterns
- **Optimized for Large Files**: Efficient parsing for templates over 50KB with minimal performance impact
- **Smart Caching**: Intelligent result caching for improved responsiveness
- **Incremental Processing**: Only processes visible areas in large files
- **Error-Tolerant**: Gracefully handles malformed content without UI freezing

### IntelliSense in Action

When editing outside code blocks, T4Editor provides T4 snippet suggestions:
- `<#...#>` - Control Block
- `<#+...#>` - Class Feature Block  
- `<#=...#>` - Expression Block
- `<#@ import #>` - Import Directive
- `<#@ include #>` - Include Directive

When editing inside code blocks (`<# #>`, `<#+ #>`, `<#= #>`), you get full C# IntelliSense powered by Roslyn, including types, members, and framework APIs.

<img width="1712" height="357" alt="image" src="https://github.com/user-attachments/assets/0880fb8d-8341-4d0a-bfd8-250f2b10e35e" />


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
- **Parser Selection**: Choose between token-based and legacy parsing (advanced users)

## Contributing

### Development Setup

1. **Clone the repository**git clone https://github.com/Tim-Maes/T4Editor.git
2. **Prerequisites**
   - Visual Studio 2019/2022 with VSIX development workload
   - .NET Framework 4.7.2
   - NuGet packages: Microsoft.CodeAnalysis.CSharp.Workspaces, Microsoft.CodeAnalysis.CSharp.Features

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
