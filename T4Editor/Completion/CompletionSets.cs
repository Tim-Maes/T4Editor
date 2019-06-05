using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.Generic;

namespace T4Editor.Completion
{
    class CompletionSets
    {
        public List<Completion2> GetCompletionSets()
        {
            return new List<Completion2>
            {
                new Completion2("<#...#>", "<#   #>", "Control Block", null, null),
                new Completion2("<#+...#>", "<#+   #>", "Class Feature Block", null, null),
                new Completion2("<#=...#>", "<#=   #>", "Expression Block", null, null),
                new Completion2("<#@ import #>", "<#@ import namespace=\"\"#>", "Import Directive", null, null),
                new Completion2("<#@ include #>", "<#@ include file=\"\"#>", "Include Directive", null, null)
            };
        }
}
}
