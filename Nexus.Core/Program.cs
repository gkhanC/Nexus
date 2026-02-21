using System;
using System.IO;

namespace Nexus.Core;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        string command = args[0].ToLower();
        switch (command)
        {
            case "new":
                CreateNewProject(args.Length > 1 ? args[1] : "MyNexusProject");
                break;
            case "add":
                if (args.Length > 2 && args[1].ToLower() == "component")
                {
                    AddComponent(args[2]);
                }
                else
                {
                    ShowHelp();
                }
                break;
            default:
                ShowHelp();
                break;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Nexus CLI v1.0");
        Console.WriteLine("Usage:");
        Console.WriteLine("  nexus new <ProjectName>      - Scaffolds a new Nexus project");
        Console.WriteLine("  nexus add component <Name>   - Adds a new unmanaged component struct");
    }

    static void CreateNewProject(string name)
    {
        Console.WriteLine($"Scaffolding new Nexus project: {name}...");
        Directory.CreateDirectory(name);
        Directory.CreateDirectory(Path.Combine(name, "Components"));
        Directory.CreateDirectory(Path.Combine(name, "Systems"));
        
        File.WriteAllText(Path.Combine(name, $"{name}.csproj"), GetProjectFileContent());
        Console.WriteLine("Done.");
    }

    static void AddComponent(string name)
    {
        string fileName = $"{name}.cs";
        string content = $@"namespace MyNexusProject.Components;

public struct {name}
{{
    public float Value;
}}
";
        File.WriteAllText(fileName, content);
        Console.WriteLine($"Added component {name} to {fileName}");
    }

    static string GetProjectFileContent()
    {
        return @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
</Project>";
    }
}
