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
    
    [TestClass]
    public class PlantUMLClassDiagramCSharpSyntaxWalker
    {
        // TODO: Remove
        public CompilationUnitSyntax CreateCompilationUnit()
        {
            AdhocWorkspace workspace = new AdhocWorkspace();

            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(
                workspace,
                LanguageNames.CSharp);

            var usingDirectives = generator.NamespaceImportDeclaration("System");
            
            var lastNameField = generator.FieldDeclaration("_lastName",
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);
            
            var firstNameField = generator.FieldDeclaration("_firstName",
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);

            // Generate two properties with explicit get/set
            var lastNameProperty = generator.PropertyDeclaration(
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

            var firstNameProperty = generator.PropertyDeclaration(
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
            var cloneMethodBody = generator.ReturnStatement(
                generator.InvocationExpression(
                    generator.IdentifierName(
                        "MemberwiseClone")));

            // Generate a SyntaxNode for the interface's name you want to implement
            var ICloneableInterfaceType = generator.IdentifierName(
                "ICloneable");

            // Generate the Clone method declaration
            var cloneMethoDeclaration = generator.MethodDeclaration(
                "Clone",
                null,
                null,
                null,
                Accessibility.Public,
                DeclarationModifiers.Virtual,
                new SyntaxNode[] 
                { 
                    cloneMethodBody 
                });

            // Explicit ICloneable.Clone implemenation
            var cloneMethodWithInterfaceType = generator.AsPublicInterfaceImplementation(
                cloneMethoDeclaration,
                ICloneableInterfaceType);

            // Generate parameters for the class' constructor
            var constructorParameters = new SyntaxNode[] 
            {
                generator.ParameterDeclaration(
                    "LastName",
                    generator.TypeExpression(
                        SpecialType.System_String)),
                generator.ParameterDeclaration("FirstName",
                generator.TypeExpression(
                    SpecialType.System_String))
            };

            // Generate the constructor's method body
            var constructorBody = new SyntaxNode[]
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
            var constructor = generator.ConstructorDeclaration(
                "Person",
                constructorParameters, 
                Accessibility.Public,
                statements: constructorBody);

            // An array of SyntaxNode as the class members
            var members = new SyntaxNode[] 
            { 
                lastNameField,
                firstNameField,
                lastNameProperty,
                firstNameProperty,
                cloneMethodWithInterfaceType, 
                constructor 
            };

            // Generate the class
            var classDefinition = generator.ClassDeclaration(
              "Person", 
              typeParameters:
              null,
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
                expected: "class MyTypes.Person <<public>> <<abstract>> implements ICloneable {",
                actual: diagram.Body[0]);

            Assert.AreEqual(
                expected: "<<private>> string _lastName",
                actual: diagram.Body[1]);

            Assert.AreEqual(
                expected: "<<private>> string _firstName",
                actual: diagram.Body[2]);

            Assert.AreEqual(
                expected: "<<public>> string LastName : <<get>> <<set>>",
                actual: diagram.Body[3]);

            Assert.AreEqual(
                expected: "<<public>> string FirstName : <<get>> <<set>>",
                actual: diagram.Body[4]);
        }
    }
}