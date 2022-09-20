using NetScript.Compiler.Tokens;
using NetScript.Compiler.Rules;
using NetScript.Compiler.AST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Dynamic;

namespace NetScript.Compiler
{
    public class CompilerArgs
    {
        public List<object> Constants { get; }
        public List<string> Names { get; }
        public List<string> Dlls { get; }
        public List<string> Variables { get; }
        public Dictionary<string, Type> Imports { get; }
        public Dictionary<string, Type> AvariableImports { get; }
        public List<Assembly> Assemblies { get; }
        public ushort CurrentCodeSector { get; private set; }
        public int UnnamedID => GetNameID("__unnamed");

        public CompilerArgs()
        {
            Constants = new();
            Names = new();
            Imports = new();
            Dlls = new();
            Assemblies = new();
            Variables = new();
            AvariableImports = new()
            {
                ["object"] = typeof(object),
                ["expando"] = typeof(ExpandoObject),
                ["bool"] = typeof(bool),
                ["byte"] = typeof(byte),
                ["sbyte"] = typeof(sbyte),
                ["short"] = typeof(short),
                ["ushort"] = typeof(ushort),
                ["int"] = typeof(int),
                ["uint"] = typeof(uint),
                ["long"] = typeof(long),
                ["ulong"] = typeof(ulong),
                ["float"] = typeof(float),
                ["decimal"] = typeof(decimal),
                ["double"] = typeof(double),
                ["string"] = typeof(string),
                ["char"] = typeof(char),

                [nameof(List<object>)] = typeof(List<>),
                [nameof(Console)] = typeof(Console),
                [nameof(Math)] = typeof(Math),
                [nameof(Interpreter.Range)] = typeof(Interpreter.Range),
                [nameof(Interpreter.Function)] = typeof(Interpreter.Function),
            };

            LoadAssemblies();
        }

        private void LoadAssemblies()
        {
            List<string> list = new ();
            Stack<Assembly> stack = new();

            stack.Push(Assembly.GetEntryAssembly());

            do
            {
                var asm = stack.Pop();

                Assemblies.Add(asm);

                foreach (var reference in asm.GetReferencedAssemblies())
                {
                    if (!list.Contains(reference.FullName))
                    {
                        try
                        {
                            stack.Push(Assembly.Load(reference));
                            list.Add(reference.FullName);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            while (stack.Count > 0);
        }

        public void LoadDLL(string path)
        {
            if (!path.Contains(':') && Assemblies.Count > 0)
            {
                string fullpath = Path.Combine(Path.GetDirectoryName(Assemblies.First().Location), path);
                if (File.Exists(fullpath))
                {
                    Assemblies.Add(Assembly.LoadFrom(fullpath));
                    Dlls.Add(fullpath);
                    return;
                }
            }
            Assemblies.Add(Assembly.LoadFrom(path));
            Dlls.Add(path);
        }

        public void AddVariable(string name)
        {
            if (AvariableImports.ContainsKey(name) || Imports.ContainsKey(name))
            {
                throw new CompilerException($"Can not use {name} as name for variable because it is reserved for type");
            }
            else if (!Variables.Contains(name))
            {
                Variables.Add(name);
            }
        }

        public int GetConstant(object obj)
        {
            if (Constants.Contains(obj))
            {
                return Constants.IndexOf(obj);
            }
            else
            {
                Constants.Add(obj);
                return Constants.Count - 1;
            }
        }

        public int GetNameID(string name, bool isGet = false)
        {
            if (isGet && !Imports.ContainsKey(name) && AvariableImports.ContainsKey(name))
            {
                Imports.Add(name, AvariableImports[name]);
            }
            if (Names.Contains(name))
            {
                return Names.IndexOf(name);
            }
            else
            {
                Names.Add(name);
                return Names.Count - 1;
            }
        }

        public void AddImport(string name, Type t)
        {
            if (name.Contains('`'))
            {
                name = name[..name.IndexOf('`')];
            }
            if (Variables.Contains(name))
            {
                throw new CompilerException($"Can not use {name} as name for variable because it is reserved for type");
            }
            else if (!AvariableImports.ContainsKey(name))
            {
                AvariableImports.Add(name, t);
            }
        }

        public ushort NextCodeSector() =>
            CurrentCodeSector++;
    }
}