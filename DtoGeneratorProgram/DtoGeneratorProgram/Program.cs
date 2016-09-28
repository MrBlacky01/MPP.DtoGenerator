﻿using System;
using System.IO;
using System.Web.Script.Serialization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using DtoGenerator;
using DtoGenerator.Descriptors;
using TypeDescription;

namespace DtoGeneratorProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "samples.json";
            string taskCountPath = "ConfigurationFile.xml";
            string directory = "./MadeClasses/";
            string plugins = "./TypePlugins/";

            int threadCount;
            string Namespace;
         
            if ( (File.Exists(path)) && (File.Exists(taskCountPath)) )
            {
                FinderTypes dog = new FinderTypes(plugins);
                List<TypeDescriptor> pluginTypes = dog.FindPlugins();

                var document = XDocument.Load(taskCountPath);
                threadCount = Int32.Parse(document.Root.Element("thread_count").Value);
                Namespace = document.Root.Element("namespace").Value;
                string json = File.ReadAllText(path);

                JavaScriptSerializer ser = new JavaScriptSerializer();
                DescriptionsOfClass classes = ser.Deserialize<DescriptionsOfClass>(json);
                classes.Namespace = Namespace;

                Console.WriteLine("Find classes = " + classes.classDescriptions.Count);

                DtoGenerator.DtoGenerator tempDto = new DtoGenerator.DtoGenerator(classes, pluginTypes, threadCount);
                Dictionary<string, CodeCompileUnit> temp = tempDto.GetUnitsOfDtoClasses();

                foreach(var unit in temp)
                {
                    SaveCode(directory + unit.Key + ".cs", unit.Value);
                }
            }

            Console.WriteLine("Work done");
            Console.ReadLine();
        }

        public static void SaveCode(string path, CodeCompileUnit compileunit)
        {

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            IndentedTextWriter codeWriter = new IndentedTextWriter(new StreamWriter(path, false), "    ");
            provider.GenerateCodeFromCompileUnit(compileunit, codeWriter, new CodeGeneratorOptions());

            codeWriter.Close();
        }
    }
}
