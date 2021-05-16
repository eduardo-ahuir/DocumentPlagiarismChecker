﻿/*
    Copyright (C) 2018 Fernando Porrino Serrano.
    This software it's under the terms of the GNU Affero General Public License version 3.
    Please, refer to (https://github.com/FherStk/DocumentPlagiarismChecker/blob/master/LICENSE) for further licensing details.
 */
 
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DocumentPlagiarismChecker
{
    class App
    {
        static void Main(string[] args)
        {  
            Settings s = null;
            //Settings file must be loaded first.
            for(int i = 0; i < args.Length; i++){                
                if(args[i].StartsWith("--settings")){
                    s = new Settings(args[i].Split("=")[1]);
                    break;
                }
            }

            //The settings can be overwriten by input arguments.
            string[] kv = null;            
            if(s == null) s = new Settings("settings.yaml");

            for(int i = 0; i < args.Length; i++){   
                kv = args[i].Split("=");                             
                if(kv[0] == "--info"){
                    Help();
                    return;                    
                }
                else{
                    if(kv[0].StartsWith("--")) kv[0] = kv[0].Substring(2);
                    s.Set(kv[0], kv[1]);
                }              
            }

            if(string.IsNullOrEmpty(s.Folder)) throw new Exceptions.FolderNotSpecifiedException();
            if(string.IsNullOrEmpty(s.Extension)) throw new Exceptions.FileExtensionNotSpecifiedException();            

            //Multi-tasking in order to display progress
            using(Api api = new Api(s)){
                Task compare = Task.Run(() => 
                    api.CompareFiles()
                );

                //Polling for progress in order to display the output
                Task progress = Task.Run(() => {
                    do{
                        Console.Write("\rLoading... {0:P2}", api.Progress);
                        System.Threading.Thread.Sleep(1000);
                    }
                    while(api.Progress < 1);                

                    Console.Write("\rLoading... {0:P2}", 1);
                    Console.WriteLine();
                    Console.WriteLine("Done!");
                    Console.WriteLine();
                    Console.WriteLine("Printing results:");
                    Console.WriteLine();
                    api.WriteOutput();
                });

                progress.Wait();
            }           
        }

        private static void Help(){            
            WriteSeparator('#');

            Console.WriteLine(typeof(App).Assembly.GetCustomAttributesData().Where(x => x.AttributeType == typeof(AssemblyProductAttribute)).SingleOrDefault().ConstructorArguments[0].Value);
            Console.WriteLine();
            Console.WriteLine(typeof(App).Assembly.GetCustomAttributesData().Where(x => x.AttributeType == typeof(AssemblyDescriptionAttribute)).SingleOrDefault().ConstructorArguments[0].Value);
            Console.WriteLine();
            Console.WriteLine(string.Format("  Copyright: {0}", typeof(App).Assembly.GetCustomAttributesData().Where(x => x.AttributeType == typeof(AssemblyCompanyAttribute)).SingleOrDefault().ConstructorArguments[0].Value));
            Console.WriteLine(string.Format("  License: {0}", typeof(App).Assembly.GetCustomAttributesData().Where(x => x.AttributeType == typeof(AssemblyCopyrightAttribute)).SingleOrDefault().ConstructorArguments[0].Value));
            Console.WriteLine(string.Format("  Version: {0}", typeof(App).Assembly.GetCustomAttributesData().Where(x => x.AttributeType == typeof(AssemblyInformationalVersionAttribute)).SingleOrDefault().ConstructorArguments[0].Value));
            
            WriteSeparator('-');

            Console.WriteLine("Usage: Run the application with 'dotnet run' with the following arguments:");
            Console.WriteLine();
            Console.WriteLine("  --display: stablished how many details will be send to the output. Accepted values are:");
            Console.WriteLine("    - basic: displays only the compared file names and the global matching percentage.");
            Console.WriteLine("    - comparator: displays previous data plus the name of each comparator used with its individual matching percentage.");
            Console.WriteLine("    - detailed: displays previous data plus some details that produced a matching result over the specified threshold value.");
            Console.WriteLine("    - full: displays previous data plus all the details used by the comparator in order to calculate its marching value.");
            Console.WriteLine();
            Console.WriteLine("  --extension: files with other extensions inside the folder will be omited. Accepted values are:");
            Console.WriteLine("    - pdf: for PDF files.");
            Console.WriteLine();
            Console.WriteLine("  --folder: the absolute path to the folder containing the documents that must be compared.");
            Console.WriteLine();
            Console.WriteLine("  --sample: the absolute path to the sample file, results matching the content of this file will be ommited (like parts of homework statements).");            
            Console.WriteLine();
            Console.WriteLine("  --threshold-basic: matching values below the threshold will be ignored at basic results output.");
            Console.WriteLine();
            Console.WriteLine("  --threshold-comparator: matching values below the threshold will be ignored at comparator results output.");
            Console.WriteLine();
            Console.WriteLine("  --threshold-details: matching values below the threshold will be ignored at comparator's details output.");
            Console.WriteLine();
            Console.WriteLine("  --threshold-full: matching values below the threshold will be ignored at comparator's full details output.");
            
            WriteSeparator('-');

            Console.WriteLine("Examples (Windows):");
            Console.WriteLine("  dotnet run");
            Console.WriteLine("  dotnet run --threshold-basic=0.25");
            Console.WriteLine("  dotnet run --folder=\"C:\\test\" --sample=\"C:\\test\\sample.pdf\"");
            Console.WriteLine();
            Console.WriteLine("Examples (Linux):");
            Console.WriteLine("  dotnet run");
            Console.WriteLine("  dotnet run --threshold-basic=0.25");
            Console.WriteLine("  dotnet run --folder=\"/home/user/test\" --sample=\"/home/user/test/sample.pdf\"");
            
            WriteSeparator('#');
        }

        private static void WriteSeparator(char separator, bool spacing = true){
             if(spacing) Console.WriteLine();

             for(int i = 0; i < Console.WindowWidth; i++)
                Console.Write(separator);

            if(spacing) Console.WriteLine();
        }
    }
}
