using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Workspace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vala.Lang;
using Vala.Lang.CodeNodes;
using Vala.Lang.Methods;
using Vala.Lang.Parser;
using Vala.Lang.Statements;
using Vala.Lang.Symbols;
using Vala.Lang.TypeSymbols;
using ValaCompilerLib;

namespace ValaLanguageServer {
	public class ValaDocumentValidator {
		public Compiler CurrentCompiler { get; private set; }

		private static readonly string[] HandledExtensions = new[]{
			".vala", ".vapi"
		};

		public static bool IsHandled(TextDocumentItem item) {
			if (!item.uri.IsFile)
				return false;
			string path = item.uri.LocalPath;
			string ext = Path.GetExtension(path).ToLowerInvariant();
			return Array.IndexOf(HandledExtensions, ext) > -1;
		}

		private Logger _logger {
			get {
				return Logger.Get;
			}
		}

		private DiagnosticSeverity SeverityFromReportType(ReportType type) {
			switch (type) {
			case ReportType.Error:
			default:
				return DiagnosticSeverity.Error;
			case ReportType.Warning:
				return DiagnosticSeverity.Warning;
			case ReportType.Notice:
			case ReportType.Experimental:
				return DiagnosticSeverity.Information;
			case ReportType.Deprecated:
				return DiagnosticSeverity.Hint;
			}
		}

		private Position PositionFromSourceLocation(SourceLocation loc) {
			return new Position {
				character = (loc?.column - 1) ?? 0,
				line = (loc?.line - 1) ?? 0
			};
		}

		private Range RangeFromSourceReference(SourceReference source) {
			return new Range {
				start = PositionFromSourceLocation(source?.begin),
				end = PositionFromSourceLocation(source?.end)
			};
		}

		public List<string> VapiDirs { get; private set; } = new List<string>();
		public List<string> Packages { get; private set; } = new List<string>();
		public string ToolchainPath { get; private set; } = null;
		public int MaxNumberOfProblems { get; private set; } = -1;

		private dynamic GetSettings(dynamic settings) {
			return settings?.valaServer;
		}

		private static string[] AsStringArray(dynamic value) {
			return value.ToObject<string[]>() ?? new string[] { };
		}

		public void UpdateConfig(dynamic config) {
			dynamic settings = GetSettings(config);

			ToolchainPath = settings.toolchainPath;
			_logger.Log($"Acquired toolchainPath '{settings.ToolchainPath}'");

			string[] _vapidirs = AsStringArray(settings.vapiDirs);
			_logger.Log($"Acquired vapiDirs '{string.Join(",", _vapidirs)}'");
			VapiDirs.AddRange(_vapidirs);

			string[] _packages = AsStringArray(settings.packages);
			_logger.Log($"Acquired packages '{string.Join(",", _packages)}'");
			Packages.AddRange(_packages);

			MaxNumberOfProblems = settings.maxNumberOfProblems ?? -1;
		}

		private Compiler MakeCompiler(string documentPath) {
			Compiler valac = new Compiler(new CompilerOptions {
				dry_run = true,
				sources = new List<string> { documentPath },
				vapi_directories = VapiDirs,
				packages = Packages,
				ccode_only = true,
				path = ToolchainPath,
				verbose_mode = false,
				quiet_mode = true
			});

			valac.init();
			return valac;
		}

		public List<Diagnostic> Validate(string docPath) {
			Compiler valac = MakeCompiler(docPath);

			List<Diagnostic> diagnostics = new List<Diagnostic>();
			int problems = 0;
			valac.context.report.OnReport += (object sender, ReportEventArgs ev) => {
				if (MaxNumberOfProblems > -1 && ++problems > MaxNumberOfProblems)
					return;

				diagnostics.Add(new Diagnostic {
					severity = SeverityFromReportType(ev.Type),
					range = RangeFromSourceReference(ev.Source),
					message = ev.Message,
					source = "valac"
				});
			};
			valac.run();

			CurrentCompiler = valac;
			return diagnostics;
		}

		public List<CompletionItem> GetCompletionItems(string docPath, Position position) {
			Compiler valac = CurrentCompiler;
			List<CompletionItem> results = new CodeSearchVisitor(docPath, position).Search(valac.context);
			return null;
		}
	}
}
