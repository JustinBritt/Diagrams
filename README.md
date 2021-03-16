# DotNet.Diagrams

This is a .NET 6.0 project that uses the Roslyn code analysis APIs to generate UML class and sequence diagrams for C# projects.

## Use Case Diagrams

## Sequence Diagrams

## Class Diagrams

|C# Syntax Node Type                    | Method                                 |
|:--------------------------------------|---------------------------------------:|
| `BaseListSyntax`                      | `Visit(BaseListSyntax baseList)`       |

### Base Lists
BaseListSyntax

### Class Declarations
ClassDeclarationSyntax

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