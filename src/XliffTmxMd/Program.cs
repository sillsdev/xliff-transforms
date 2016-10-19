// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XliffTmxMd.cs
// Responsibility: Greg Trihus
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Mono.Options;
using Saxon.Api; // http://www.saxonica.com/documentation/index.html#!dotnetdoc/Saxon.Api


namespace XliffTmxMd
{
    /// <summary>
    /// Converts TMX and MD using XSLT 2.0 style sheets
    /// </summary>
    public class Program
    {
        static int _verbosity;
        private static XsltTransformer _tmxXslt;
        private static XsltTransformer _mdXslt;
        private static readonly List<XsltTransformer> Transformers = new List<XsltTransformer>();
        private static XPathSelector _getOutputName;

        static void Main(string[] args)
        {
            var processor = new Processor();
            var compiler = processor.NewXsltCompiler();
            _tmxXslt = compiler.Compile(Assembly.GetExecutingAssembly().GetManifestResourceStream("XliffTmxMd.xlf-tmx.xsl")).Load();
            _mdXslt = compiler.Compile(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "XliffTmxMd.xlf-md.xsl")).Load();
            var xpathCompiler = processor.NewXPathCompiler();
            _getOutputName = xpathCompiler.Compile(@"//*[local-name()='file'][1]/@original").Load();
            bool showHelp = false;
            bool makeBackup = false;
            var transforms = new List<string>();
            var output = new List<string>();
            var myArgs = new List<string>();
            string persist = string.Empty;

            if (args.Length == 1 && args[0][0] != '-')
            {
                // If just the file name is given on the command line and the arguments for processing this 
                // file type were persisted in the registry previously, then update command line arguments
                try
                {
                    var ext = Path.GetExtension(args[0]);
                    string cmdLine = RegistryAccess.GetRegistryValue(ext, "") as string;
                    cmdLine += args[0];
                    args = cmdLine.Split(' ');
                }
                catch // If there is no command line in the registry, just go with what we have.
                {
                    // ignored
                }
            }

            // see: http://stackoverflow.com/questions/491595/best-way-to-parse-command-line-arguments-in-c
            var p = new OptionSet {
                { "t|transform=", "the {TRANSFORM} to apply to the item.\n" +
                        "Defaults to the internal transform.",
                   v => transforms.Add (v) },
                { "o|output=", "the output file for saving the result.\n" +
                        "Defaults to replacing the file in the source package.",
                   v => output.Add (v) },
                { "d|define=", "define argument:value for transformation.",
                   v => myArgs.Add (v) },
                { "p|persist=", "Persist this command so XliffTmxMd will process it on any file with this extension.",
                   v => persist = v },
                { "r|reset=", "Remove persistted command.",
                   v => { RegistryAccess.DeleteRegistryValue(v);
                            Environment.Exit(0);
                   } },
                { "v", "increase debug message verbosity",
                   v => { if (v != null) ++_verbosity; } },
                { "b|backup",  "controls making backup files", 
                   v => makeBackup = v != null },
                { "h|help",  "show this message and exit", 
                   v => showHelp = v != null },
            };

            List<string> extra = null;
            try
            {
                extra = p.Parse(args);
                if (showHelp || extra.Count > 1)
                {
                    ShowHelp(p);
                    Environment.Exit(0);
                }

                if (!string.IsNullOrEmpty(persist))
                {
                    var cmdLine = new StringBuilder();
                    for (int i=0; i < args.Length - 1; i++)
                    {
                        cmdLine.Append(args[i]);
                        cmdLine.Append(' ');
                    }
                    RegistryAccess.SetStringRegistryValue(persist, cmdLine.ToString());
                    Environment.Exit(0);
                }

                if (extra.Count == 0)
                {
                    Console.WriteLine("Enter full file name to process");
                    extra.Add(Console.ReadLine());
                }
            }
            catch (OptionException e)
            {
                Console.Write("XliffTmxMd: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `XliffTmxMd --help' for more information.");
                Environment.Exit(-1);
            }

            foreach (var transform in transforms)
            {
                Transformers.Add(compiler.Compile(File.Open(transform, FileMode.Open)).Load());
            }
            var builder = processor.NewDocumentBuilder();
            var nextOutput = 0;
            string firstOutput = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            foreach (string  fileName in extra)
            {
                Debug("Processing: {0}", fileName);
                var fullName = Path.GetFullPath(fileName);
                var folder = Path.GetDirectoryName(fullName);
                var fileUri = new Uri(fullName);
                System.Diagnostics.Debug.Assert(folder != null, "folder != null");
                var folderUri = new Uri(folder);
                if (makeBackup)
                {
                    MakeBackupFile(fileName);
                }
                string outName;
                if (output.Count != 0)
                {
                    nextOutput = nextOutput + 1 < output.Count ? nextOutput + 1 : nextOutput;
                    firstOutput = outName = output[nextOutput];
                }
                else
                {
                    _getOutputName.ContextItem = builder.Build(XmlReader.Create(fileName));
                    var outNameNode = _getOutputName.EvaluateSingle() as XdmNode;
                    System.Diagnostics.Debug.Assert(outNameNode != null, "outNameNode != null");
                    outName = outNameNode.StringValue;
                    AddBaseParam(outName, myArgs, folderUri);
                }
                System.Diagnostics.Debug.Assert(outName != null, "outName != null");
                Debug("Writing to: {0}", outName);
                if (Transformers.Count > 0)
                {
                    Transformers[0].SetInputStream(File.Open(fileName, FileMode.Open), fileUri);
                    AddArgumentList(myArgs, Transformers[0]);
                    var writer = XmlWriter.Create(firstOutput);
                    Transformers[0].Run(new TextWriterDestination(writer));
                    writer.Close();
                }
                else if (Path.GetExtension(outName) == ".tmx")
                {
                    _tmxXslt.SetInputStream(File.Open(fileName, FileMode.Open), fileUri);
                    AddArgumentList(myArgs, _tmxXslt);
                    var writer = XmlWriter.Create(firstOutput);
                    _tmxXslt.Run(new TextWriterDestination(writer));
                    writer.Close();
                }
                else
                {
                    _mdXslt.SetInputStream(File.Open(fileName, FileMode.Open), fileUri);
                    AddArgumentList(myArgs, _mdXslt);
                    var writer = XmlWriter.Create(firstOutput);
                    _mdXslt.Run(new TextWriterDestination(writer));
                    writer.Close();
                }
            }
            if (output.Count == 0)
            {
                File.Delete(firstOutput);
            }
        }

        private static void AddBaseParam(string outName, List<string> myArgs, Uri folderUri)
        {
            if (!Path.IsPathRooted(outName))
            {
                var found = false;
                for (var i = 0; i < myArgs.Count; i += 1)
                {
                    var myArg = myArgs[i];
                    if (myArg.Contains(":"))
                    {
                        var pos = myArg.IndexOf(":", StringComparison.Ordinal);
                        if (myArg.Substring(0, pos) == "outputBase")
                        {
                            myArgs[i] = "outputBase:" + folderUri.AbsoluteUri + "/";
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    myArgs.Add("outputBase:" + folderUri.AbsoluteUri + "/");
                }
                Debug("outputBase={0}/", folderUri.AbsoluteUri);
            }
        }

        private static void AddArgumentList(IEnumerable<string> myArgs, XsltTransformer transformer)
        {
            foreach (string definition in myArgs)
            {
                if (definition.Contains(":"))
                {
                    var pos = definition.IndexOf(":", StringComparison.Ordinal);
                    transformer.SetParameter(new QName(definition.Substring(0, pos)), new XdmAtomicValue(definition.Substring(pos + 1)));
                }
                else
                {
                    transformer.SetParameter(new QName(definition), new XdmAtomicValue(true));
                }
            }
        }

        private static void MakeBackupFile(string fullName)
        {
            int n = 0;
            string bakName;
            var fullNameWOext = Path.GetFileNameWithoutExtension(fullName);
            var folder = Path.GetDirectoryName(fullName);
            if (!string.IsNullOrEmpty(folder))
            {
                fullNameWOext = Path.Combine(folder, fullName);
            }
            var ext = Path.GetExtension(fullName);
            do
            {
                n += 1;
                bakName = string.Format("{0}-{1}{2}", fullNameWOext, n, ext);
            } while (File.Exists(bakName));
            try
            {
                Debug("Making backup file: {0}", bakName);
                File.Copy(fullName, bakName);
            }
            catch (UnauthorizedAccessException)   // Don't make backup if the folder is not writeable
            {
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: XliffTmxMd [OPTIONS]+ message");
            Console.WriteLine("Apply an XSLT to the some ITEM in the ODT (or DOCx) file.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Debug(string format, params object[] args)
        {
            if (_verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }
    }
}
