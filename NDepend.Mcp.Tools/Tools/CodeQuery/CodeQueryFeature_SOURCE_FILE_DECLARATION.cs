namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string SOURCE_FILE_DECLARATION_PROMPT =
          """
          # Code Element Source File Declaration
          
          Code elements expose source location via `ICodeElement`. A element can have zero, one, or multiple declarations (e.g. partial classes, namespaces span files; external types have none).

          **IMPORTANT**: Always check `SourceFileDeclAvailable` before accessing `SourceDecls`.
          
          ## Key Concepts
          
          ### Multiple Declarations
          A code element can have **zero, one, or multiple** source declarations:
          - **Partial classes**: Multiple declarations across different files
          - **Namespaces**: Can span multiple files
          - **Regular classes**: Typically one declaration
          - **External/compiled types**: May have zero declarations
          
          ## API

          ### ICodeElement
          ```csharp
          IEnumerable<ISourceDecl> SourceDecls { get; }
          bool SourceFileDeclAvailable { get; }
          ```

          ### ISourceDecl
          ```csharp
          ISourceFile SourceFile { get; }
          uint Line { get; }    // 1-based
          uint Column { get; }  // 1-based
          ```

          ### ISourceFile
          - `IAbsoluteFilePath FilePath` / `string FilePathString` / `string FileName` / `string FileNameWithoutExtension`
          - `SourceFileLanguage Language` – CSharp, VBNet, FSharp, Other
          - `IEnumerable<ICodeElement> CodeElements`, `uint NbLines`, `uint NbCharacters`
          - `uint? NbLinesOfCode`, `uint NbLinesOfComment`, `uint? NbILInstructions`
          - `bool CoverageDataAvailable`, `float? PercentageCoverage`, `uint? NbLinesOfCodeCovered`, `uint? NbLinesOfCodeNotCovered`

          ## Patterns
          
          **Never** start a `select` tuple with a source file (its path or its name).
          A `select` tuple must always begin with code elements only.
          Source file information may be included, but only as additional fields—not as the leading element.

          There is no `SourceFiles` domain. Always access the source file via `elem.SourceDecls.First().SourceFile`.
          
          ```csharp
          // Safe single access
          from m in Application.Methods
          where m.SourceFileDeclAvailable
          let decl = m.SourceDecls.FirstOrDefault()
          where decl != null
          select new { m, decl.SourceFile.FilePath }
          
          // Multiple declarations (e.g. partial types)
          from t in Application.Types.Where(t => t.SourceFileDeclAvailable)
          from decl in t.SourceDecls
          select new { Type = t, File = decl.SourceFile.FileName, decl.Line }
          
          // Filter by directory
          from m in Application.Methods
          where m.SourceFileDeclAvailable
          let parentDir = m.SourceDecls.Single().SourceFile.FilePath.ParentDirectoryPath.ToString()
          where parentDir.ContainsAny(@"\Customer\", @"/Customer/", StringComparison.OrdinalIgnoreCase)
          select m
          
          // Files with multiple type declarations
          let lookup = Application.Types
              .Where(t => t.SourceFileDeclAvailable && t.SourceDecls.Count() == 1)
              .ToLookup(t => t.SourceDecls.Single().SourceFile.FilePathString, StringComparer.OrdinalIgnoreCase)
          from t in lookup.SelectMany(g => g)
          let filePath = t.SourceDecls.Single().SourceFile.FilePathString
          orderby filePath
          select new { t, filePath }
          ```
          """;

}
