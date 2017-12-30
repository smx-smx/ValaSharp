using GLibPorts;
using LanguageServer;
using LanguageServer.Json;
using LanguageServer.Parameters;
using LanguageServer.Parameters.General;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Vala.Lang;
using Vala.Lang.Parser;
using ValaCompilerLib;

namespace ValaLanguageServer {
	public class App : ServiceConnection {
		private Uri _workerSpaceRoot;
		private int _maxNumberOfProblems;
		private TextDocumentManager _documents;

		private ValaDocumentValidator _validator;

		public App(Stream input, Stream output) : base(input, output) {
			GLib.GLibInitialize();

			_documents = new TextDocumentManager();
			_documents.Changed += Documents_Changed;

			_validator = new ValaDocumentValidator();

			Logger.Get.Attach(this);
		}

		~App() {
			GLib.GLibDispose();
		}

		private void Documents_Changed(object sender, TextDocumentChangedEventArgs e) {
			ValidateTextDocument(e.Document);
		}

		protected override Result<InitializeResult, ResponseError<InitializeErrorData>> Initialize(InitializeParams @params) {
			_workerSpaceRoot = @params.rootUri;
			var result = new InitializeResult {
				capabilities = new ServerCapabilities {
					textDocumentSync = TextDocumentSyncKind.Incremental,
					completionProvider = new CompletionOptions {
						resolveProvider = true
					}
				}
			};
			return Result<InitializeResult, ResponseError<InitializeErrorData>>.Success(result);
		}

		protected override void DidOpenTextDocument(DidOpenTextDocumentParams @params) {
			_documents.Add(@params.textDocument);
			Logger.Get.Log($"{@params.textDocument.uri} opened.");

			ValidateTextDocument(@params.textDocument);
		}

		protected override void DidChangeTextDocument(DidChangeTextDocumentParams @params) {
			_documents.Change(@params.textDocument.uri, @params.textDocument.version, @params.contentChanges);
			Logger.Get.Log($"{@params.textDocument.uri} changed.");

			var doc = @params.textDocument;
			foreach(var change in @params.contentChanges) {
				Logger.Get.Log($"{change.text}");
			}
			// Recompile source
			ValidateTextDocument(new TextDocumentItem {
				uri = doc.uri,
				version = doc.version
			});
		}

		protected override void DidCloseTextDocument(DidCloseTextDocumentParams @params) {
			_documents.Remove(@params.textDocument.uri);
			Logger.Get.Log($"{@params.textDocument.uri} closed.");
		}

		protected override void DidChangeConfiguration(DidChangeConfigurationParams @params) {
			_validator.UpdateConfig(@params.settings);

			foreach (var document in _documents.All) {
				ValidateTextDocument(document);
			}
		}

		private void ValidateTextDocument(TextDocumentItem document) {
			if (!ValaDocumentValidator.IsHandled(document))
				return;

			List<Diagnostic> diagnostics = new List<Diagnostic>();
			try {
				List<Diagnostic> messages = _validator.Validate(document.GetDocumentPath());
				diagnostics.AddRange(messages);
			} catch (ParseException ex) {
				if(ex.error_type != ParseError.SYNTAX) {
					throw ex;
				}

				diagnostics.Add(new Diagnostic {
					severity = DiagnosticSeverity.Error,
					message = ex.Message,
					source = "syntax"
				});
			} finally {
				Proxy.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams {
					uri = document.uri,
					diagnostics = diagnostics.ToArray()
				});
			}
		}

		protected override void DidChangeWatchedFiles(DidChangeWatchedFilesParams @params) {
			Logger.Get.Log("We received a file change event");
		}

		protected override Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError> Completion(TextDocumentPositionParams @params) {
			List<CompletionItem> items = _validator.GetCompletionItems(
				@params.textDocument.GetDocumentPath(),
				@params.position
			);

			if (items == null) {
				return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Error(new ResponseError {
					code = ErrorCodes.InternalError,
					message = $"Failed to get completion items for {@params.position.line}:{@params.position.character}"
				});
			}

			return Result<ArrayOrObject<CompletionItem, CompletionList>, ResponseError>.Success(items.ToArray());
		}
	}
}