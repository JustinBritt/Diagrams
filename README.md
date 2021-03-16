# DotNet.Diagrams

This is a .NET 6.0 project that uses the Roslyn code analysis APIs to generate UML class and sequence diagrams for C# projects.

## Use Case Diagrams

## Sequence Diagrams

## Class Diagrams
The `DotNetDiagrams.ClassDiagrams` project uses a visitor pattern to generate class diagrams.
### Base Lists
BaseListSyntax

### Class Declarations
ClassDeclarationSyntax

#### Base List

#### Constraint Clauses

#### Modifiers
In `DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker`, `private string GetJoinedModifiers(ClassDeclarationSyntax classDeclaration)` 
uses pattern matching to map entries in the `Modifiers` property of the `classDeclaration` instance to PlantUML stereotypes as follows:

|C#                    | PlantUML           |
|:---------------------|-------------------:|
| `abstract`           | `<<abstract>>`     |
| `internal`           | `<<internal>>`     |
| `partial`            | `<<partial>>`      |	
| `private`            | `<<private>>`      |
| `protected`          | `<<protected>>`    |
| `public`             | `<<public>>`       |
| `sealed`             | `<<sealed>>`       |
| `static`             | `<<static>>`       |
| `unsafe`             | `<<unsafe>>`       |

These stereotypes are then joined using `String.Join` with separator `private const string stringJoinSeparator_modifiers = " "`.

#### Relationships

#### Type Parameter List

### Constructor Declarations
ConstructorDeclarationSyntax

### Enum Declarations
EnumDeclarationSyntax

### Enum Member Declarations
EnumMemberDeclarationSyntax

### Event Declarations
EventDeclarationSyntax

### Event Field Declarations
EventFieldDeclarationSyntax

### Field Declarations
FieldDeclarationSyntax

### Interface Declarations
InterfaceDeclarationSyntax

### Method Declarations

MethodDeclarationSyntax

#### Constraint Clauses

#### Explicit Interface Specifier

#### Identifier

#### Modifiers

|C#                    | PlantUML           |
|:---------------------|-------------------:|
| `abstract`           | `{abstract}`       |
| `async`              | `<<async>>`        |
| `extern`             | `<<extern>>`       |
| `internal`           | `<<internal>>`     |
| `new`                | `<<new>>`          |
| `override`           | `<<override>>`     |
| `partial`            | `<<partial>>`      |	
| `private`            | `<<private>>`      |
| `protected`          | `<<protected>>`    |
| `public`             | `<<public>>`       |
| `static`             | `{static}`         |
| `unsafe`             | `<<unsafe>>`       |
| `virtual`            | `<<virtual>>`      |

#### Parameter List

#### Return Type

#### Type Parameter List

### Property Declarations
PropertyDeclarationSyntax

### Struct Declarations
StructDeclarationSyntax

## Based On
* https://github.com/SoundLogic/Diagrams/wiki 
* https://github.com/msawczyn/Diagrams
* https://github.com/pierre3/PlantUmlClassDiagramGenerator