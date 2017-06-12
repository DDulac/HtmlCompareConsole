using System;
using System.Security.Permissions;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace ContentCompare
{
	/// <summary>
	/// Content Compare Console Application
	/// Compare HTML changes between two file sets by parsing HTML files to generate text files containing the element and the internal content
	/// </summary>
	[FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
	class Program
	{
		#region ContentCompare Internals 

		#region ContentCompare ConsoleMessages
		private const string MessageContentCompare = "HTML Content Compare File Generator\n\n";
		private const string MessageContinue = "\nPress any key to continue...";
		private const string MessageSourceAPath = "\n\nPlace an uncompressed directory structure in C:\\temp\\ContentCompare\\SourceA then press enter...";
		private const string MessageSourceBPath = "\n\nPlace an uncompressed directory structure in C:\\temp\\ContentCompare\\SourceB then press enter...";
		private const string MessageElementChoice = "\n\nChoose HTML element to isolate\n\n<head> = \"h\" \n<title> = \"t\" \n<meta> = \"m\" \n<link> = \"l\" \n<body> = \"b\" \n<footer> = \"f\" \n<img> = \"i\" \n<h1> = \"1\" \n<h2> = \"2\" \n<h3> = \"3\" \n\nChoice: ";
		private const string MessageTargetAGenerated = "\nC:\\temp\\SourceA\\ processed to C:\\temp\\ContentCompare\\TargetA\\...";
		private const string MessageTargetBGenerated = "\nC:\\temp\\SourceB\\ processed to C:\\temp\\ContentCompare\\TargetB\\...";
		private const string MessageReportBGenerated = "\nBeyond Compare 4 Text Report processed to C:\\temp\\ContentCompare\\Reports\\TextReport.txt...";
		private const string MessageInvalidSelection = "Invalid selection, please retry...";
		private const string MessageProcessCompleted = "\nPress any key to exit...";
		private const string MessageContinueDeveloping = "\nShould Beyond Compare now compare the two directories?...";
		#endregion ContentCompare ConsoleMessages

		#region ContentCompare Variables
		public const string SourceA = @"C:\temp\ContentCompare\SourceA";
		public const string SourceB = @"C:\temp\ContentCompare\SourceB";

		public const string TargetA = @"C:\temp\ContentCompare\TargetA";
		public const string TargetB = @"C:\temp\ContentCompare\TargetB";

		public const string FileType = "*.htm*";

		public const string HeadXPath = "//head";
		public const string TitleXPath = "//title";
		public const string MetaXPath = "//meta";
		public const string LinkXPath = "//link";
		public const string BodyXPath = "//body";
		public const string FooterXPath = "//footer";
		public const string ScriptXPath = "//script";

		public const string ImageXPath = "//img";
		public const string H1XPath = "//h1";
		public const string H2XPath = "//h2";
		public const string H3XPath = "//h3";
		#endregion ContentCompare Variables

		#region ContentCompare Underdeveloped
		// For potential RegEx expression building. In this application or with any other command line application.
		//public const string HeadRegEx = "<head[^>]*>((.|[\\n\\r])*)<\\/head>";
		//public const string TitleRegEx = "<title[^>]*>((.|[\\n\\r])*)<\\/title>";
		//public const string MetaRegEx = "<meta[^>]*>((.|[\\n\\r])*)<\\/meta>";
		//public const string LinkRegEx = "<link[^>]*>((.|[\\n\\r])*)<\\/link>";
		//public const string BodyRegEx = "<body[^>]*>((.|[\\n\\r])*)<\\/body>";
		//public const string ScriptRegEx = "<script[^>]*>((.|[\\n\\r])*)<\\/script>";
		//public const string ImageRegEx = "<img[^>]*>((.|[\\n\\r])*)<\\/img>";
		//public const string ImageRegEx = "<footer[^>]*>((.|[\\n\\r])*)<\\/footer>";
		//public const string H1RegEx = "";
		//public const string H2RegEx = "";
		//public const string H3RegEx = "";
		#endregion ContentCompare Underdeveloped

		#endregion ContentCompare Internals 

		static void Main(string[] args)
		{
			Console.Write(MessageContentCompare);
			Console.WriteLine("{0:d} {0:T}", DateTime.Now);
			Console.Write(MessageContinue);
			while (Console.ReadKey().Key != ConsoleKey.Enter) { }

			try
			{
				Console.Write(MessageSourceAPath);
				while (Console.ReadKey().Key != ConsoleKey.Enter) { }
				Console.Write(MessageSourceBPath);
				while (Console.ReadKey().Key != ConsoleKey.Enter) { }
				Console.Write(MessageElementChoice);

				string xPath = "";
				string retry = "No";
				do
				{
					string userSelection = Console.ReadLine().ToUpper();
					switch (userSelection)
					{
						case "H":
							xPath = HeadXPath;
							retry = "No";
							break;
						case "T":
							xPath = TitleXPath;
							retry = "No";
							break;
						case "M":
							xPath = MetaXPath;
							retry = "No";
							break;
						case "L":
							xPath = LinkXPath;
							retry = "No";
							break;
						case "B":
							xPath = BodyXPath;
							retry = "No";
							break;
						case "F":
							xPath = FooterXPath;
							retry = "No";
							break;
						case "I":
							xPath = ImageXPath;
							retry = "No";
							break;
						case "1":
							xPath = H1XPath;
							retry = "No";
							break;
						case "2":
							xPath = H2XPath;
							retry = "No";
							break;
						case "3":
							xPath = H3XPath;
							retry = "No";
							break;
						default:
							Console.WriteLine(MessageInvalidSelection);
							retry = "Yes";
							break;
					}
				} while (retry != "No");

				ClearDirectory(TargetA);
				ClearDirectory(TargetB);

				List<String> sourceA = GetNameAndPathList(SourceA);
				List<String> sourceB = GetNameAndPathList(SourceB);

				ProcessSources(xPath, sourceA, sourceB);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				CreateBeyondCompareReport();
			}

			Console.WriteLine();
			Console.Write(MessageProcessCompleted);
			while (Console.ReadKey().Key != ConsoleKey.Enter) { }
		}

		/// <summary>
		/// Returns full path and file name as one string 
		/// </summary>
		/// <param name="directory">Source directory to search and get all files</param>
		/// <returns>Full path and file name as one string</returns>
		public static List<String> GetNameAndPathList(string directory)
		{
			var origianlPathAndName = new List<String>(Directory.GetFiles(directory, FileType, SearchOption.AllDirectories));
			return origianlPathAndName;
		}

		/// <summary>
		/// Generate text files containing the matched html xpath expression
		/// </summary>
		/// <param name="xPath">Xpath search expression</param>
		/// <param name="sourceA">List of paths and file names from source A</param>
		/// <param name="sourceB">List of paths and file names from source B</param>
		public static void ProcessSources(string xPath, List<String> sourceA, List<String> sourceB)
		{
			foreach (string file in sourceA)
			{
				// Update the path
				string targetAFileName = file.Replace("SourceA", "TargetA");
				string targetADirectoryName = Path.GetDirectoryName(targetAFileName);

				// Find the requested html element
				HtmlDocument targetAHtmlDoc = new HtmlDocument();
				targetAHtmlDoc.Load(file);
				HtmlNode targetASearchNode = targetAHtmlDoc.DocumentNode.SelectSingleNode(xPath);

				// Create a compare file
				CreateFiles(targetADirectoryName, targetAFileName, targetASearchNode);
			}

			// Target A files processed
			Console.WriteLine(MessageTargetAGenerated);

			foreach (string file in sourceB)
			{
				// Update the path
				string targetBFileName = file.Replace("SourceB", "TargetB");
				string targetBDirectoryName = Path.GetDirectoryName(targetBFileName);

				// Find the requested html element
				HtmlDocument targetBHtmlDoc = new HtmlDocument();
				targetBHtmlDoc.Load(file);
				HtmlNode targetBSearchNode = targetBHtmlDoc.DocumentNode.SelectSingleNode(xPath);

				CreateFiles(targetBDirectoryName, targetBFileName, targetBSearchNode);
			}

			Console.WriteLine(MessageTargetBGenerated);
		}

		/// <summary>
		/// Delete all files and directories
		/// </summary>
		/// <param name="dir">Target directory</param>
		public static void ClearDirectory(string dir)
		{
			Directory.SetCurrentDirectory(dir);

			DirectoryInfo di = new DirectoryInfo(dir);

			foreach (FileInfo existingFile in di.GetFiles())
			{
				existingFile.Delete();
			}

			foreach (DirectoryInfo existingDirectory in di.GetDirectories())
			{
				existingDirectory.Delete(true);
			}
		}

		/// <summary>
		/// Creates the text file based on given parameters
		/// </summary>
		/// <param name="path">The path to create</param>
		/// <param name="files">The file name to create</param>
		/// <param name="node">The HTML node (inner and outer HTML) to create</param>
		public static void CreateFiles(string path, string files, HtmlNode node)
		{
			if (node != null)
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				using (StreamWriter sw = File.CreateText(files))
				{
					sw.WriteLine("\nTimestamp {0:d} {0:T}", DateTime.Now);
					sw.WriteLine("\nTag:\n\n" + node.OuterHtml);
					sw.WriteLine("\nContent:\n\n" + node.InnerHtml);
				}
			}
		}

		public static void CreateBeyondCompareReport()
		{
			Directory.SetCurrentDirectory(@"c:\\temp\\");

			System.Diagnostics.Process.Start(@"C:\\Program Files\\Beyond Compare 4\\BCompare.exe", "@c:\\temp\\ContentCompare\\Scripts\\CommandLineScript.txt");

			Console.WriteLine(MessageReportBGenerated);
		}
	}
}