using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using T4Editor.Common;
using T4Editor.Performance;

namespace T4Editor.Tests
{
    /// <summary>
    /// Simple test class to verify the token-based parser functionality
    /// This would typically be in a separate test project, but included here for demonstration
    /// </summary>
    internal static class T4TokenParserTests
    {
        /// <summary>
        /// Basic test to verify the parser can handle different T4 block types
        /// </summary>
        public static void RunBasicTests()
        {
            var testCases = new[]
            {
                // Basic control block
                "Hello <# code here #> World",
                
                // Expression block
                "Value: <#= expression #>",
                
                // Class feature block
                "<#+ class feature code #>",
                
                // Directive block
                "<#@ directive code #>",
                
                // Mixed content
                "Start <# control #> middle <#= expr #> end",
                
                // Nested-like content (should handle gracefully)
                "<# if (true) { <#= value #> } #>",
                
                // Unclosed block (error case)
                "Start <# unclosed block",
                
                // No T4 content
                "Just plain text content"
            };

            Console.WriteLine("Running T4 Token Parser Tests...");
            
            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\nTesting: {testCase}");
                
                try
                {
                    // Note: In actual use, this would use real ITextSnapshot and cache
                    // This is simplified for demonstration
                    var errors = T4ErrorDetector.DetectErrors(testCase);
                    
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"  Detected {errors.Count} error(s):");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"    {error.Type}: {error.Message} at position {error.Position}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  No errors detected - valid T4 syntax");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Exception: {ex.Message}");
                }
            }
            
            Console.WriteLine("\nTests completed.");
        }
    }
}