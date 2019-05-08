# T4Editor

VS2019 extension to highlight T4 Template code blocks and support code completion.
Currently for .tt and .ttinclude extensions.

Get it from the [Marketplace](https://marketplace.visualstudio.com/items?itemName=TimMaes.t4editor).

Currently, code completion only works with words. So typing 'import' and pressing 'tab' will add a import directive. Typing 'control' and pressing 'tab' will add a <# control block #>. Completion is not yet triggered by typing '<' for now.

![completion](https://i.ibb.co/LnkTzkf/importdirective.png)

![completion](https://i.ibb.co/kKGGf1D/featureblockcompletion.png)

Supported code completion :
- <#@ import|include directive #>
- <# Control Blocks #>
- <#+ Class Feature Blocks #>
- <#= Expression Blocks #>

You can now set custom colors via `Extensions > T4Editor > Adjust Colors`

![ColorPickerMenu](https://i.ibb.co/GkgNZs9/Color-Picker.png)


