namespace DotNetDiagrams.ClassDiagrams.Tests.Classes.Walkers
{
    using System.Linq;

    using Microsoft.Build.Locator;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.MSBuild;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using DotNetDiagrams.ClassDiagrams.Classes.Walkers;
    using System.Text;

    [TestClass]
    public class PlantUMLClassDiagramCSharpSyntaxWalker
    {
        // TODO: Remove
        public CompilationUnitSyntax CreateCompilationUnit()
        {
            AdhocWorkspace workspace = new AdhocWorkspace();

            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(
                workspace: workspace,
                language: LanguageNames.CSharp);

            UsingDirectiveSyntax usingDirectives = (UsingDirectiveSyntax)generator.NamespaceImportDeclaration("System");
            
            FieldDeclarationSyntax lastNameField = (FieldDeclarationSyntax)generator.FieldDeclaration("_lastName",
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);

            FieldDeclarationSyntax firstNameField = (FieldDeclarationSyntax)generator.FieldDeclaration("_firstName",
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);

            // Generate two properties with explicit get/set
            PropertyDeclarationSyntax lastNameProperty = (PropertyDeclarationSyntax)generator.PropertyDeclaration(
                "LastName",
                generator.TypeExpression(
                    SpecialType.System_String),
                Accessibility.Public,
                getAccessorStatements: new SyntaxNode[]
                { 
                  generator.ReturnStatement(
                      generator.IdentifierName(
                          "_lastName"))
                },
                setAccessorStatements: new SyntaxNode[]
                {
                  generator.AssignmentStatement(
                      generator.IdentifierName(
                          "_lastName"),
                      generator.IdentifierName(
                          "value"))
                });

            PropertyDeclarationSyntax firstNameProperty = (PropertyDeclarationSyntax)generator.PropertyDeclaration(
                "FirstName",
                generator.TypeExpression(
                    SpecialType.System_String),
                Accessibility.Public,
                getAccessorStatements: new SyntaxNode[]
                { 
                    generator.ReturnStatement(
                        generator.IdentifierName("_firstName")) 
                },
                setAccessorStatements: new SyntaxNode[]
                { 
                    generator.AssignmentStatement(
                        generator.IdentifierName(
                            "_firstName"),
                        generator.IdentifierName("value"))
                });

            // Generate the method body for the Clone method
            ReturnStatementSyntax cloneMethodBody = (ReturnStatementSyntax)generator.ReturnStatement(
                generator.InvocationExpression(
                    generator.IdentifierName(
                        "MemberwiseClone")));

            // Generate a SyntaxNode for the interface's name you want to implement
            IdentifierNameSyntax ICloneableInterfaceType = (IdentifierNameSyntax)generator.IdentifierName(
                "ICloneable");

            // Generate the Clone method declaration
            MethodDeclarationSyntax cloneMethoDeclaration = (MethodDeclarationSyntax)generator.MethodDeclaration(
                name: "Clone",
                parameters: null,
                typeParameters: null,
                returnType: null,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Virtual,
                statements: new SyntaxNode[] 
                { 
                    cloneMethodBody 
                });

            // Explicit ICloneable.Clone implemenation
            MethodDeclarationSyntax cloneMethodWithInterfaceType = (MethodDeclarationSyntax)generator.AsPublicInterfaceImplementation(
                cloneMethoDeclaration,
                ICloneableInterfaceType);

            // Generate parameters for the class' constructor
            SyntaxNode[] constructorParameters = new SyntaxNode[] 
            {
                generator.ParameterDeclaration(
                    "LastName",
                    generator.TypeExpression(
                        SpecialType.System_String)),
                generator.ParameterDeclaration(
                    "FirstName",
                    generator.TypeExpression(
                        SpecialType.System_String))
            };

            // Generate the constructor's method body
            SyntaxNode[] constructorBody = new SyntaxNode[]
            {
                generator.AssignmentStatement(
                    generator.IdentifierName(
                        "_lastName"),
                    generator.IdentifierName(
                        "LastName")),
                generator.AssignmentStatement(
                    generator.IdentifierName(
                        "_firstName"),
                    generator.IdentifierName(
                        "FirstName"))};

            // Generate the class' constructor
            ConstructorDeclarationSyntax constructor = (ConstructorDeclarationSyntax)generator.ConstructorDeclaration(
                "Person",
                constructorParameters, 
                Accessibility.Public,
                statements: constructorBody);

            // An array of SyntaxNode as the class members
            SyntaxNode[] members = new SyntaxNode[] 
            { 
                lastNameField,
                firstNameField,
                lastNameProperty,
                firstNameProperty,
                cloneMethodWithInterfaceType, 
                constructor 
            };

            // Generate the class
            ClassDeclarationSyntax classDefinition = (ClassDeclarationSyntax)generator.ClassDeclaration(
              "Person", 
              typeParameters: null,
              accessibility: Accessibility.Public,
              modifiers: DeclarationModifiers.Abstract,
              baseType: null,
              interfaceTypes: new SyntaxNode[] { ICloneableInterfaceType },
              members: members);

            // Declare a namespace
            NamespaceDeclarationSyntax namespaceDeclaration = (NamespaceDeclarationSyntax)generator.NamespaceDeclaration(
                "MyTypes", 
                classDefinition);

            // Get a CompilationUnit (code file) for the generated code
            return (CompilationUnitSyntax)generator.CompilationUnit(
                usingDirectives,
                namespaceDeclaration).
                NormalizeWhitespace();
        }

        public void TestMethod0()
        {
            // Arrange
            MSBuildLocator.RegisterDefaults();

            MSBuildWorkspace MSBuildWorkspace = MSBuildWorkspace.Create();

            string solutionPath = "";

            //Solution solution = MSBuildWorkspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();

            //Project project = solution.Projects.Where(w => w.Language is LanguageNames.CSharp).First();

            //Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

            //SyntaxTree syntaxTree = compilation.SyntaxTrees.First();

            //DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
            //    compilation: compilation,
            //    syntaxTree: syntaxTree,
            //    solution: solution2,
            //    project: project);

            // Act

            // Assert
        }

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            ProjectId projectId = ProjectId.CreateNewId();

            DocumentId documentId = DocumentId.CreateNewId(
                projectId);

            AdhocWorkspace adhocWorkspace = new AdhocWorkspace();

            Solution solution = adhocWorkspace.CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddDocument(documentId, "MyFile.cs", this.CreateCompilationUnit().ToFullString());

            Project project = solution.Projects.Where(w => w.Language is LanguageNames.CSharp).First();

            Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

            SyntaxTree syntaxTree = compilation.SyntaxTrees.First();

            DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
                compilation: compilation,
                syntaxTree: syntaxTree,
                solution: solution,
                project: project);

            // Act
            ClassDeclarationSyntax classDeclaration = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().First();

            walker.Visit(
                classDeclaration);

            DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams.IPlantUMLClassDiagram diagram = walker.Diagrams.GetClassDiagramAtTitleOrDefault(
                "MyTypes.Person");

            diagram.EndDiagram();

            // Assert
            Assert.AreEqual(
                expected: "MyTypes.Person",
                actual: diagram.Title);
        }

        [TestMethod]
        public void TestMethod2()
        {
            // Arrange
            ProjectId projectId = ProjectId.CreateNewId();

            DocumentId documentId = DocumentId.CreateNewId(
                projectId);

            AdhocWorkspace adhocWorkspace = new AdhocWorkspace();

            Solution solution = adhocWorkspace.CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddDocument(documentId, "MyFile.cs", this.CreateCompilationUnit().ToFullString());

            Project project = solution.Projects.Where(w => w.Language is LanguageNames.CSharp).First();

            Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

            SyntaxTree syntaxTree = compilation.SyntaxTrees.First();

            DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
                compilation: compilation,
                syntaxTree: syntaxTree,
                solution: solution,
                project: project);

            // Act
            ClassDeclarationSyntax classDeclaration = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().First();

            walker.Visit(
                classDeclaration);

            DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams.IPlantUMLClassDiagram diagram = walker.Diagrams.GetClassDiagramAtTitleOrDefault(
                "MyTypes.Person");

            diagram.EndDiagram();

            // Assert
            Assert.AreEqual(
                expected: System.String.Concat(
                    "class MyTypes.Person <<public>> <<abstract>> implements ICloneable {",
                    "<<private>> string _lastName",
                    "<<private>> string _firstName",
                    "<<public>> string LastName : <<get>> <<set>>",
                    "<<public>> string FirstName : <<get>> <<set>>",
                    "<<public>> <<virtual>> void Clone()",
                    "<<public>> Person(string LastName, string FirstName)",
                    "}"),
                actual: System.String.Concat(
                    diagram.Body));
        }
    }
}