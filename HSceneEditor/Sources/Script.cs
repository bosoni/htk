/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Htk
{
    public class Script
    {
        public bool Optimize = false;
        Type loadedClass;
        object classInstance;

        public void Load(string fileName)
        {
            if (fileName.Contains(".dll")) LoadDLL(fileName);
            else LoadScript(fileName);
        }

        public void LoadDLL(string dllFile)
        {
            try
            {
                Assembly assemblyInfo = Assembly.LoadFrom(dllFile);
                loadedClass = assemblyInfo.GetType("CSScript");
                classInstance = Activator.CreateInstance(loadedClass);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
            }
        }

        public void LoadScript(string scriptFile)
        {
            try
            {
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CompilerParameters cp = new CompilerParameters();
                cp.ReferencedAssemblies.Add("System.dll");
                cp.ReferencedAssemblies.Add("OpenTK.dll");

                string reference;
                reference = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                cp.ReferencedAssemblies.Add(reference + "/HSceneEditor.exe");  // hack -todofix pitäis ottaa realtimenä exen nimi

                cp.GenerateExecutable = true; // jos false, tulee erroria konsoliin  (todo fix, ei tartteis kyl exejä)
                cp.GenerateInMemory = true;
                cp.TreatWarningsAsErrors = false;
                if (Optimize) cp.CompilerOptions = "/optimize";

                CompilerResults result = provider.CompileAssemblyFromSource(cp, System.IO.File.ReadAllText(scriptFile));

                if (result.Errors.HasErrors)
                {
                    string errstr = "Error:\n";
                    foreach (CompilerError err in result.Errors)
                        errstr += err.ErrorText + " at line " + err.Line + " column " + err.Column + "\n";
                    Log.WriteLine(errstr);
                    System.Windows.Forms.MessageBox.Show(errstr, "Error");
                }

                Assembly assemblyInfo = result.CompiledAssembly;
                loadedClass = assemblyInfo.GetType("CSScript");
                classInstance = Activator.CreateInstance(loadedClass);

                classInstance = assemblyInfo.CreateInstance("CSScript");
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
            }
        }

        public object Run(string methodName, object[] parameters)
        {
            try
            {
                MethodInfo method = loadedClass.GetMethod(methodName);
                return method.Invoke(classInstance, parameters);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error");
                return null;
            }
        }
    }
}
