// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetDiagrams
{

   internal class DiagramGenerator
   {
      private readonly Solution solution;

      private readonly ConcurrentDictionary<MethodDeclarationSyntax, ConcurrentBag<MethodDeclarationSyntax>> methodDeclCache
         = new ConcurrentDictionary<MethodDeclarationSyntax, ConcurrentBag<MethodDeclarationSyntax>>();

      private readonly ConcurrentDictionary<MethodDeclarationSyntax, ConcurrentDictionary<int, MethodDeclarationSyntax>> methodSequences
         = new ConcurrentDictionary<MethodDeclarationSyntax, ConcurrentDictionary<int, MethodDeclarationSyntax>>();

      public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace)
      {
         solution = workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
      }

      #region [process the tree]

      private async Task ProcessCompilation(Compilation compilation)
      {
         IEnumerable<SyntaxTree> trees = compilation.SyntaxTrees;

         foreach (SyntaxTree tree in trees)
         {
            SyntaxNode root = await tree.GetRootAsync();
            IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            SyntaxTree treeCopy = tree;

            foreach (ClassDeclarationSyntax @class in classes)
               await ProcessClass(@class, compilation, treeCopy);
         }
      }

      private async Task ProcessClass(ClassDeclarationSyntax classDecl
                                    , Compilation compilation
                                    , SyntaxTree syntaxTree)
      {
         IEnumerable<MethodDeclarationSyntax> methodDecls = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();

         foreach (MethodDeclarationSyntax methodDecl in methodDecls)
            await ProcessMethod(methodDecl, compilation, syntaxTree);
      }

      private async Task ProcessMethod(MethodDeclarationSyntax methodDecl
                                     , Compilation compilation
                                     , SyntaxTree syntaxTree)
      {
         if (SyntaxNodeHelper.TryGetParentSyntax(methodDecl, out ClassDeclarationSyntax _))
         {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);
            IMethodSymbol methodSymbol = model.GetDeclaredSymbol(methodDecl);

            Parallel.ForEach(await GetCallingMethodsAsync(methodSymbol, methodDecl)
                           , caller =>
                             {
                                if (!methodDeclCache.TryGetValue(caller, out ConcurrentBag<MethodDeclarationSyntax> value))
                                   methodDeclCache[caller] = new ConcurrentBag<MethodDeclarationSyntax>();

                                value.Add(methodDecl);
                             });
         }
      }

      /// <summary>
      ///    Gets a list of methods that call the method based on the method symbol
      ///    also builds a list of called methods by the calling method as the key and then the value is a dictionary
      ///    of UInt64,MethodDeclarationSyntax where the UInt64 is the start location of the span where the called method is
      ///    called
      ///    from inside the calling method - this will allow us to order our sequence diagrams, but this functionality should be
      ///    moved out into a separate method at some point
      ///    (in fact this whole method needs a ton of refactoring and is too complex)
      /// </summary>
      /// <param name="methodSymbol"></param>
      /// <param name="method"></param>
      /// <returns></returns>
      private async Task<List<MethodDeclarationSyntax>> GetCallingMethodsAsync(IMethodSymbol methodSymbol, MethodDeclarationSyntax method)
      {
         List<MethodDeclarationSyntax> references = new List<MethodDeclarationSyntax>();

         List<Location> locations = (await SymbolFinder.FindCallersAsync(methodSymbol, solution)).SelectMany(s => s.Locations).ToList();

         if (!locations.Any())
            return references;

         foreach (Location location in locations)
         {
            int position = location.SourceSpan.Start;
            SyntaxNode root = await location.SourceTree.GetRootAsync();

            MethodDeclarationSyntax[] methodCalls = root.FindToken(position)
                                                        .Parent
                                                        .AncestorsAndSelf()
                                                        .OfType<MethodDeclarationSyntax>()
                                                        .ToArray();
            references.AddRange(methodCalls);

            // we need to know what order methods are called in
            foreach (MethodDeclarationSyntax methodCall in methodCalls)
            {
               if (!methodSequences.ContainsKey(methodCall))
                  methodSequences[methodCall] = new ConcurrentDictionary<int, MethodDeclarationSyntax>();

               methodSequences[methodCall][location.SourceSpan.Start] = method;
            }
         }

         return references;
      }

      #endregion

      #region [build & output js-sequence-diagrams formatted text]

      /// <summary>
      ///    generates diagram by order of methods getting called based on the first method found that does not have anything
      ///    calling it
      /// </summary>
      public void GenerateDiagrams()
      {
         IEnumerable<MethodDeclarationSyntax> rootMethods =
            methodDeclCache.Keys.Where(key => !methodDeclCache.Values.Any(value => value.Contains(key)));

         foreach (MethodDeclarationSyntax root in rootMethods)
            PrintMethodInfo(root);
      }

      private void PrintMethodInfo(MethodDeclarationSyntax caller)
      {
         if (!methodDeclCache.ContainsKey(caller))
            return;

         foreach (MethodDeclarationSyntax calledMethod in methodSequences[caller].Keys
                                                                                 .OrderBy(k => k)
                                                                                 .Select(key => methodSequences[caller][key]))
         {
            if (!SyntaxNodeHelper.TryGetParentSyntax(caller, out ClassDeclarationSyntax callingClass)
             || !SyntaxNodeHelper.TryGetParentSyntax(calledMethod, out ClassDeclarationSyntax calledClass))
            {
               continue;
            }

            PrintOutgoingCallInfo(calledClass
                                , callingClass
                                , caller
                                , calledMethod);

            if (caller != calledMethod)
            {
               PrintMethodInfo(calledMethod);
            }

            PrintReturnCallInfo(calledClass
                              , callingClass
                              , caller
                              , calledMethod);
         }
      }

      private static void PrintOutgoingCallInfo(ClassDeclarationSyntax classBeingCalled
                                              , ClassDeclarationSyntax callingClass
                                              , MethodDeclarationSyntax callingMethod
                                              , MethodDeclarationSyntax calledMethod
                                              , bool includeCalledMethodArguments = false)
      {
         string callingMethodName = callingMethod.Identifier.ToFullString();
         string calledMethodReturnType = calledMethod.ReturnType.ToFullString();
         string calledMethodName = calledMethod.Identifier.ToFullString();
         string calledMethodArguments = calledMethod.ParameterList.ToFullString();
         string calledMethodModifiers = calledMethod.Modifiers.ToString();
         string calledMethodConstraints = calledMethod.ConstraintClauses.ToFullString();
         string actedUpon = classBeingCalled.Identifier.ValueText;
         string actor = callingClass.Identifier.ValueText;

         string calledMethodTypeParameters = calledMethod.TypeParameterList != null
                                                ? calledMethod.TypeParameterList.ToFullString()
                                                : string.Empty;

         string callingMethodTypeParameters = callingMethod.TypeParameterList != null
                                                 ? callingMethod.TypeParameterList.ToFullString()
                                                 : string.Empty;

         string callInfo = callingMethodName + callingMethodTypeParameters + " => " + calledMethodModifiers + " " + calledMethodReturnType + calledMethodName + calledMethodTypeParameters;

         if (includeCalledMethodArguments)
         {
            callInfo += calledMethodArguments;
         }

         callInfo += calledMethodConstraints;

         string info
            = BuildOutgoingCallInfo(actor
                                  , actedUpon
                                  , callInfo);

         Console.Write(info);
      }

      private static void PrintReturnCallInfo(ClassDeclarationSyntax classBeingCalled
                                            , ClassDeclarationSyntax callingClass
                                            , MethodDeclarationSyntax callingMethod
                                            , MethodDeclarationSyntax calledMethod)
      {
         string actedUpon = classBeingCalled.Identifier.ValueText;
         string actor = callingClass.Identifier.ValueText;
         string callerName = callingMethod.Identifier.ToFullString();

         string callingMethodTypeParameters = callingMethod.TypeParameterList != null
                                                 ? callingMethod.TypeParameterList.ToFullString()
                                                 : string.Empty;

         string calledMethodTypeParameters = calledMethod.TypeParameterList != null
                                                ? calledMethod.TypeParameterList.ToFullString()
                                                : string.Empty;

         string calledMethodInfo = calledMethod.Identifier.ToFullString() + calledMethodTypeParameters;

         callerName += callingMethodTypeParameters;

         string returnCallInfo = calledMethod.ReturnType.ToString();

         SeparatedSyntaxList<ParameterSyntax> returnMethodParameters = calledMethod.ParameterList.Parameters;

         foreach (ParameterSyntax parameter in returnMethodParameters)
         {
            if (parameter.Modifiers.Any(m => m.Text == "out"))
            {
               returnCallInfo += "," + parameter.ToFullString();
            }
         }

         string info = BuildReturnCallInfo(actor
                                         , actedUpon
                                         , calledMethodInfo
                                         , callerName
                                         , returnCallInfo);

         Console.Write(info);
      }

      private static string BuildOutgoingCallInfo(string actor, string actedUpon, string callInfo)
      {
         const string calls = "->";
         const string descriptionSeparator = ": ";

         string callingInfo = actor + calls + actedUpon + descriptionSeparator + callInfo;

         callingInfo = callingInfo.RemoveNewLines(true);

         string result = callingInfo + Environment.NewLine;

         return result;
      }

      private static string BuildReturnCallInfo(string actor
                                              , string actedUpon
                                              , string calledMethodInfo
                                              , string callerName
                                              , string returnInfo)
      {
         const string returns = "-->";
         const string descriptionSeparator = ": ";

         string returningInfo = actedUpon + returns + actor + descriptionSeparator + calledMethodInfo + " returns " + returnInfo + " to " + callerName;
         returningInfo = returningInfo.RemoveNewLines(true);

         string result = returningInfo + Environment.NewLine;

         return result;
      }

      public async Task ProcessSolution()
      {
         foreach (Project project in solution.Projects)
         {
            Compilation compilation = await project.GetCompilationAsync();
            await ProcessCompilation(compilation);
         }
      }

      #endregion
   }

}
