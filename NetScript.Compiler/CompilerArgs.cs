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
    /// <summary>
    /// Class that stores additional info of compilation
    /// </summary>
    public class CompilerArgs
    {
        /// <summary>
        /// List of constant values
        /// </summary>
        public List<object> Constants { get; }

        /// <summary>
        /// List of names
        /// </summary>
        public List<string> Names { get; }

        /// <summary>
        /// List of imported dlls that were imported by "loaddll" expression
        /// </summary>
        public List<string> Dlls { get; }

        /// <summary>
        /// List of variables
        /// </summary>
        public List<string> Variables { get; }

        /// <summary>
        /// List of used imports
        /// </summary>
        public Dictionary<string, Type> Imports { get; }

        /// <summary>
        /// List of usable imports
        /// </summary>
        public Dictionary<string, Type> AvariableImports { get; }

        /// <summary>
        /// List of all assemblies that were collected in begin of compilation
        /// or with "loaddll" expression
        /// </summary>
        public List<Assembly> Assemblies { get; }

        /// <summary>
        /// ID of current sector
        /// </summary>
        public ushort CurrentCodeSector { get; private set; }

        /// <summary>
        /// ID of "__unnamed". Required for constructs that can but don't have a name
        /// </summary>
        public int UnnamedID => GetNameID("__unnamed");

        public CompilerArgs()
        {
            Constants = new();
            Names = new();
            Imports = new();
            Dlls = new();
            Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            Variables = new();
            AvariableImports = new()
            {
                // Preimported types
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
                [nameof(Dictionary<object, object>)] = typeof(Dictionary<,>),
                [nameof(Console)] = typeof(Console),
                [nameof(Math)] = typeof(Math),
                [nameof(Interpreter.Range)] = typeof(Interpreter.Range),
                [nameof(Interpreter.Function)] = typeof(Interpreter.Function),
            };
        }

        private void LoadReferences(Assembly assembly)
        {
            Assemblies.Clear();
            Stack<Assembly> asms = new();
            asms.Push(assembly);
            while (asms.Count > 0)
            {
                Assembly curr = asms.Pop();

                Assemblies.Add(curr);

                foreach (AssemblyName re in curr.GetReferencedAssemblies())
                {
                    try
                    {
                        Assembly asm = Assembly.Load(re);

                        if (!Assemblies.Contains(asm))
                        {
                            asms.Push(asm);
                        }
                    }
                    catch { }
                }
            }
        }

        public void LoadDLL(string path)
        {
            if (!path.Contains(':') && Assemblies.Count > 0)
            {
                HashSet<string> dirs = new(
                    from asm in Assemblies
                    select Path.GetDirectoryName(asm.Location)
                );
                dirs.Add(Directory.GetCurrentDirectory());
                foreach (string dir in dirs)
                {
                    string fullpath = Path.Combine(dir, path);
                    if (File.Exists(fullpath))
                    {
                        LoadReferences(Assembly.LoadFrom(fullpath));
                        Dlls.Add(path);
                        return;
                    }
                }
            }
            throw new FileNotFoundException($"Can not find {path}");
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