using System;
using System.Collections.Generic;
using System.Text;
using T4Editor.Performance;

namespace T4Editor.Projection
{
    /// <summary>
    /// Parses T4 source text and generates a valid C# document that wraps
    /// all code blocks into a compilable class structure, while maintaining
    /// precise span mappings for bidirectional position translation.
    /// 
    /// Generated structure:
    /// <code>
    /// using [from import directives];
    /// namespace T4Generated {
    ///   class Template {
    ///     public string TransformText() {
    ///       [control blocks]
    ///       __output.Append([expression blocks]);
    ///     }
    ///     [class feature blocks]
    ///   }
    /// }
    /// </code>
    /// </summary>
    internal class T4CSharpDocumentGenerator
    {
        private const string Preamble =
            "namespace T4Generated {\n" +
            "class Template {\n" +
            "public string TransformText() {\n";

        private const string MethodClose =
            "return \"\";\n" +
            "}\n";

        private const string ClassClose =
            "}\n" +
            "}\n";

        /// <summary>
        /// Generates a C# document from T4 source text.
        /// </summary>
        public GeneratedDocument Generate(string t4Text)
        {
            if (string.IsNullOrEmpty(t4Text))
            {
                return new GeneratedDocument { CSharpCode = Preamble + MethodClose + ClassClose };
            }

            var blocks = ParseBlocks(t4Text);
            return BuildDocument(blocks, t4Text);
        }

        private List<T4Block> ParseBlocks(string text)
        {
            var blocks = new List<T4Block>();
            int position = 0;

            while (position < text.Length)
            {
                int blockStart = text.IndexOf("<#", position, StringComparison.Ordinal);
                if (blockStart == -1)
                    break;

                if (blockStart + 2 >= text.Length)
                    break;

                var blockType = DetermineBlockType(text, blockStart);
                int openTagLength = blockType == T4BlockType.Control ? 2 : 3;

                int contentStart = blockStart + openTagLength;
                int closeTag = text.IndexOf("#>", contentStart, StringComparison.Ordinal);

                if (closeTag == -1)
                {
                    // Unclosed block - take rest of file as content
                    blocks.Add(new T4Block
                    {
                        Type = blockType,
                        ContentStart = contentStart,
                        ContentLength = text.Length - contentStart,
                        Content = text.Substring(contentStart)
                    });
                    break;
                }

                int contentLength = closeTag - contentStart;
                blocks.Add(new T4Block
                {
                    Type = blockType,
                    ContentStart = contentStart,
                    ContentLength = contentLength,
                    Content = text.Substring(contentStart, contentLength)
                });

                position = closeTag + 2;
            }

            return blocks;
        }

        private GeneratedDocument BuildDocument(List<T4Block> blocks, string t4Text)
        {
            var sb = new StringBuilder();
            var mappings = new List<SpanMapping>();

            // Phase 1: using directives from import directives
            foreach (var block in blocks)
            {
                if (block.Type == T4BlockType.Directive)
                {
                    if (TryExtractImportNamespace(block.Content, out int nsOffset, out int nsLength))
                    {
                        sb.Append("using ");
                        int genStart = sb.Length;
                        sb.Append(block.Content, nsOffset, nsLength);
                        sb.Append(";\n");

                        // Map only the namespace text: it is identical on both sides,
                        // so offsets within the span translate one-to-one
                        mappings.Add(new SpanMapping
                        {
                            T4Start = block.ContentStart + nsOffset,
                            T4Length = nsLength,
                            GeneratedStart = genStart,
                            GeneratedLength = nsLength,
                            BlockType = T4MappingBlockType.UsingDirective
                        });
                    }
                }
            }

            // Phase 2: class/method preamble
            sb.Append(Preamble);

            // Phase 3: control blocks and expressions go into TransformText() body
            foreach (var block in blocks)
            {
                if (block.Type == T4BlockType.Control)
                {
                    int genStart = sb.Length;
                    sb.Append(block.Content);
                    sb.Append('\n');

                    mappings.Add(new SpanMapping
                    {
                        T4Start = block.ContentStart,
                        T4Length = block.ContentLength,
                        GeneratedStart = genStart,
                        GeneratedLength = block.ContentLength,
                        BlockType = T4MappingBlockType.ControlBlock
                    });
                }
                else if (block.Type == T4BlockType.Expression)
                {
                    // Wrap in a method call so it's a valid statement
                    string prefix = "Write(";
                    string suffix = ");\n";

                    sb.Append(prefix);
                    int genStart = sb.Length;
                    sb.Append(block.Content);
                    int genLength = block.ContentLength;
                    sb.Append(suffix);

                    mappings.Add(new SpanMapping
                    {
                        T4Start = block.ContentStart,
                        T4Length = block.ContentLength,
                        GeneratedStart = genStart,
                        GeneratedLength = genLength,
                        BlockType = T4MappingBlockType.ExpressionBlock
                    });
                }
            }

            // Phase 4: close method
            sb.Append(MethodClose);

            // Phase 5: class feature blocks go at class level
            foreach (var block in blocks)
            {
                if (block.Type == T4BlockType.ClassFeature)
                {
                    int genStart = sb.Length;
                    sb.Append(block.Content);
                    sb.Append('\n');

                    mappings.Add(new SpanMapping
                    {
                        T4Start = block.ContentStart,
                        T4Length = block.ContentLength,
                        GeneratedStart = genStart,
                        GeneratedLength = block.ContentLength,
                        BlockType = T4MappingBlockType.ClassFeatureBlock
                    });
                }
            }

            // Phase 6: close class and namespace
            sb.Append(ClassClose);

            // Add Write method stub so expressions don't error
            // Insert before final closing braces
            sb.Insert(sb.Length - ClassClose.Length, "void Write(object value) { }\n");

            return new GeneratedDocument
            {
                CSharpCode = sb.ToString(),
                Mappings = mappings
            };
        }

        /// <summary>
        /// Locates the namespace value of a T4 import directive within the directive content.
        /// E.g., ' import namespace="System.Linq" ' → offset and length of 'System.Linq'
        /// </summary>
        private bool TryExtractImportNamespace(string directiveContent, out int offset, out int length)
        {
            offset = 0;
            length = 0;

            // Look for: import namespace="..." (or single quotes)
            var trimmed = directiveContent.TrimStart();
            if (!trimmed.StartsWith("import", StringComparison.OrdinalIgnoreCase))
                return false;

            const string attribute = "namespace=";
            int attrStart = directiveContent.IndexOf(attribute, StringComparison.OrdinalIgnoreCase);
            if (attrStart == -1)
                return false;

            int quotePos = attrStart + attribute.Length;
            if (quotePos >= directiveContent.Length)
                return false;

            char quote = directiveContent[quotePos];
            if (quote != '"' && quote != '\'')
                return false;

            int nsStart = quotePos + 1;
            int nsEnd = directiveContent.IndexOf(quote, nsStart);
            if (nsEnd == -1)
                return false;

            offset = nsStart;
            length = nsEnd - nsStart;
            return true;
        }

        private T4BlockType DetermineBlockType(string text, int blockStart)
        {
            if (blockStart + 2 >= text.Length)
                return T4BlockType.Control;

            char thirdChar = text[blockStart + 2];
            switch (thirdChar)
            {
                case '@': return T4BlockType.Directive;
                case '+': return T4BlockType.ClassFeature;
                case '=': return T4BlockType.Expression;
                default: return T4BlockType.Control;
            }
        }

        private struct T4Block
        {
            public T4BlockType Type;
            public int ContentStart;   // Position in original T4 text (after open tag)
            public int ContentLength;
            public string Content;
        }
    }
}
