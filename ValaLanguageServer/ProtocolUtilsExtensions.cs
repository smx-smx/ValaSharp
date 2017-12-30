using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValaLanguageServer {
	public static class ProtocolUtilsExtensions {
		public static string GetDocumentPath(Uri uri) {
			string path = uri.LocalPath;
			if (path.Length > 0 && (
				path[0] == Path.DirectorySeparatorChar ||
				path[0] == Path.AltDirectorySeparatorChar
			)) {
				return Path.GetFullPath(path.Substring(1));
			}
			return path;
		}

		public static string GetDocumentPath(this VersionedTextDocumentIdentifier doc) {
			return GetDocumentPath(doc.uri);
		}

		public static string GetDocumentPath(this TextDocumentItem doc) {
			return GetDocumentPath(doc.uri);
		}

		public static string GetDocumentPath(this TextDocumentIdentifier doc) {
			return GetDocumentPath(doc.uri);
		}

	}
}
