using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace T4Editor.Projection
{
    /// <summary>
    /// Creates and manages a Roslyn AdhocWorkspace containing a C# document
    /// that represents the generated code from a T4 template. This enables
    /// IntelliSense, diagnostics, and other language services.
    /// </summary>
    internal class T4CSharpWorkspaceManager : IDisposable
    {
        private AdhocWorkspace _workspace;
        private ProjectId _projectId;
        private DocumentId _documentId;
        private bool _disposed;

        /// <summary>The Roslyn document representing the generated C#</summary>
        public Document Document
        {
            get
            {
                if (_workspace == null || _documentId == null) return null;
                return _workspace.CurrentSolution.GetDocument(_documentId);
            }
        }

        /// <summary>The workspace containing the generated project</summary>
        public Workspace Workspace => _workspace;

        public void Initialize()
        {
            _workspace = new AdhocWorkspace();
            _projectId = ProjectId.CreateNewId("T4Generated");

            var references = GetDefaultReferences();

            var projectInfo = ProjectInfo.Create(
                _projectId,
                VersionStamp.Default,
                "T4Generated",
                "T4Generated",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                metadataReferences: references);

            _workspace.AddProject(projectInfo);

            _documentId = DocumentId.CreateNewId(_projectId, "Generated.cs");
            var documentInfo = DocumentInfo.Create(
                _documentId,
                "Generated.cs",
                loader: TextLoader.From(TextAndVersion.Create(
                    SourceText.From(string.Empty), VersionStamp.Default)));

            _workspace.AddDocument(documentInfo);
        }

        /// <summary>
        /// Updates the generated C# document content and returns the document for exactly that text.
        /// </summary>
        public Document UpdateDocument(string csharpCode)
        {
            if (_workspace == null || _documentId == null) return null;

            var sourceText = SourceText.From(csharpCode);
            var solution = _workspace.CurrentSolution.WithDocumentText(_documentId, sourceText);
            _workspace.TryApplyChanges(solution);
            return solution.GetDocument(_documentId);
        }

        /// <summary>
        /// Adds assembly references parsed from T4 assembly directives.
        /// </summary>
        public void AddReferences(IEnumerable<string> assemblyPaths)
        {
            if (_workspace == null || _projectId == null) return;

            var refs = new List<MetadataReference>();
            foreach (var path in assemblyPaths)
            {
                if (File.Exists(path))
                {
                    refs.Add(MetadataReference.CreateFromFile(path));
                }
            }

            if (refs.Count > 0)
            {
                var solution = _workspace.CurrentSolution.AddMetadataReferences(_projectId, refs);
                _workspace.TryApplyChanges(solution);
            }
        }

        private List<MetadataReference> GetDefaultReferences()
        {
            var references = new List<MetadataReference>();

            // Add core framework references
            var trustedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => a.Location)
                .Distinct();

            // Fallback: use known framework paths
            var frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var coreAssemblies = new[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
                "System.Runtime.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Text.RegularExpressions.dll",
            };

            foreach (var asm in coreAssemblies)
            {
                var path = Path.Combine(frameworkDir, asm);
                if (File.Exists(path))
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }

            // Also add the running mscorlib directly
            var mscorlibPath = typeof(object).Assembly.Location;
            if (!references.Any(r => (r as PortableExecutableReference)?.FilePath == mscorlibPath))
            {
                references.Add(MetadataReference.CreateFromFile(mscorlibPath));
            }

            return references;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _workspace?.Dispose();
                _workspace = null;
            }
        }
    }
}
