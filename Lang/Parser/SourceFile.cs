using Vala;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vala.Lang.CodeNodes;
using static GLibPorts.GLib;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using Utils;

namespace Vala.Lang.Parser {
	public class SourceFile {
		/// <summary>
		/// The name of this source file.
		/// </summary>
		public string filename { get; set; }

		public string Relative_filename {
			set {
				this._relative_filename = value;
			}
		}

		private string _package_name;

		public string package_name {
			get {
				if (file_type != SourceFileType.PACKAGE) {
					return null;
				}

				if (_package_name == null) {
					_package_name = Path.GetFileName(filename.Substring(0, filename.LastIndexOf('.')));
				}

				return _package_name;
			}
			set {
				_package_name = value;
			}
		}

		private string _installed_version = null;
		private bool _version_requested = false;

		/// <summary>
		/// The installed package version or null
		/// </summary>
		public string installed_version {
			get {
				if (_version_requested) {
					return _installed_version;
				}

				_version_requested = true;

				string pkg_config_name = package_name;
				if (pkg_config_name == null) {
					return null;
				}

				string standard_output;
				int exit_status;

				try {
					using (Process pkgconfig = Process.Start(new ProcessStartInfo {
						UseShellExecute = false,
						FileName = context.path + "pkg-config" + GProcess.get_executable_suffix(),
						Arguments = "--silence-errors --modversion %s".printf(pkg_config_name),
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						WorkingDirectory = context.path
					})) {
						pkgconfig.WaitForExit();
						standard_output = pkgconfig.StandardOutput.ReadToEnd();
						exit_status = pkgconfig.ExitCode;
					}
					if (exit_status != 0) {
						return null;
					}
				} catch (Exception) {
					return null;
				}

				//standard_output = standard_output[0:-1];
				if (standard_output != "") {
					_installed_version = standard_output;
				}

				return _installed_version;
			}
			set {
				_version_requested = value != null;
				_installed_version = value;
			}
		}


		/// <summary>
		/// Specifies whether this file is a VAPI package file.
		/// </summary>
		public SourceFileType file_type { get; set; }

		/// <summary>
		/// Specifies whether this file came from the command line directly.
		/// </summary>
		public bool from_commandline { get; set; }

		/// <summary>
		/// GIR Namespace for this source file, if it's a VAPI package
		/// </summary>

		public string gir_namespace { get; set; }

		/// <summary>
		/// GIR Namespace version for this source file, if it's a VAPI package
		/// </summary>

		public string gir_version { get; set; }

		private WeakReference<CodeContext> context_weak = new WeakReference<CodeContext>(null);

		/// <summary>
		/// The context this source file belongs to.
		/// </summary>
		public CodeContext context {
			get {
				return context_weak.GetTarget();
			}
			set {
				context_weak.SetTarget(value);
			}
		}

		public string content {
			get { return this._content; }
			set {
				this._content = value;
				this.source_array = null;
			}
		}

		/// <summary>
		/// If the file has been used (ie: if anything in the file has
		/// been emitted into C code as a definition or declaration).
		/// </summary>
		public bool used { get; set; }

		/// <summary>
		/// Whether this source-file was explicitly passed on the commandline.
		/// </summary>
		public bool is_explicit { get; set; }

		private List<Comment> comments = new List<Comment>();

		public List<UsingDirective> current_using_directives { get; set; } = new List<UsingDirective>();

		private List<CodeNode> nodes = new List<CodeNode>();

		string _relative_filename;

		private string csource_filename = null;
		private string cinclude_filename = null;

		private List<string> source_array = null;

		private FastMemoryMappedFile mapped_file = null;

		private string _content = null;


		/// <summary>
		/// Creates a new source file.
		/// 
		/// <param name="filename">source file name</param>
		/// <returns>newly created source file</returns>
		/// </summary>
		public SourceFile(CodeContext context, SourceFileType type, string filename, string content = null, bool cmdline = false) {
			this.context = context;
			this.file_type = type;
			this.filename = filename;
			this.content = content;
			this.from_commandline = cmdline;
		}

		/// <summary>
		/// Adds a header comment to this source file.
		/// </summary>
		public void add_comment(Comment comment) {
			comments.Add(comment);
		}

		/// <summary>
		/// Returns a copy of the list of header comments.
		/// 
		/// <returns>list of comments</returns>
		/// </summary>
		public List<Comment> get_comments() {
			return comments;
		}

		/// <summary>
		/// Adds a new using directive with the specified namespace.
		/// 
		/// <param name="ns">reference to namespace</param>
		/// </summary>
		public void add_using_directive(UsingDirective ns) {
			// do not modify current_using_directives, it should be considered immutable
			// for correct symbol resolving
			var old_using_directives = current_using_directives;
			current_using_directives = new List<UsingDirective>();
			foreach (var using_directive in old_using_directives) {
				current_using_directives.Add(using_directive);
			}
			current_using_directives.Add(ns);
		}

		/// <summary>
		/// Adds the specified code node to this source file.
		/// 
		/// <param name="node">a code node</param>
		/// </summary>
		public void add_node(CodeNode node) {
			nodes.Add(node);
		}

		public void remove_node(CodeNode node) {
			nodes.Remove(node);
		}

		/// <summary>
		/// Returns a copy of the list of code nodes.
		/// 
		/// <returns>code node list</returns>
		/// </summary>
		public List<CodeNode> get_nodes() {
			return nodes;
		}

		public void accept(CodeVisitor visitor) {
			visitor.visit_source_file(this);
		}

		public void accept_children(CodeVisitor visitor) {
			foreach (CodeNode node in nodes) {
				node.accept(visitor);
			}
		}

		private string get_subdir() {
			if (context.basedir == null) {
				return "";
			}

			// filename and basedir are already canonicalized
			if (filename.StartsWith(context.basedir + Path.DirectorySeparatorChar)) {
				var basename = Path.GetFileName(filename);
				var subdir = filename.Substring(context.basedir.Length, filename.Length - context.basedir.Length - basename.Length);
				while (subdir.Length > 0 && subdir[0] == Path.DirectorySeparatorChar) {
					subdir = subdir.Substring(1);
				}
				return subdir;
			}
			return "";
		}

		private string get_destination_directory() {
			if (context.directory == null) {
				return get_subdir();
			}
			return GPath.build_path(Path.DirectorySeparatorChar.ToString(), context.directory, get_subdir());
		}

		private string get_basename() {
			int dot = filename.LastIndexOf('.');
			return Path.GetFileName(filename.Substring(0, dot));
		}

		public string get_relative_filename() {
			if (_relative_filename != null) {
				return _relative_filename;
			} else {
				return Path.GetFileName(filename);
			}
		}

		/// <summary>
		/// Returns the filename to use when generating C source files.
		/// 
		/// <returns>generated C source filename</returns>
		/// </summary>
		public string get_csource_filename() {
			if (csource_filename == null) {
				if (context.run_output) {
					csource_filename = context.output + ".c";
				} else if (context.ccode_only || context.save_csources) {
					csource_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), get_destination_directory(), get_basename() + ".c");
				} else {
					// temporary file
					csource_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), get_destination_directory(), get_basename() + ".vala.c");
				}
			}
			return csource_filename;
		}

		/// <summary>
		/// Returns the filename to use when including the generated C header
		/// file.
		/// 
		/// <returns>C header filename to include</returns>
		/// </summary>
		public string get_cinclude_filename() {
			if (cinclude_filename == null) {
				if (context.header_filename != null) {
					cinclude_filename = Path.GetFileName(context.header_filename);
					if (context.includedir != null) {
						cinclude_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), context.includedir, cinclude_filename);
					}
				} else {
					cinclude_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), get_subdir(), get_basename() + ".h");
				}
			}
			return cinclude_filename;
		}

		/// <summary>
		/// Returns the requested line from this file, loading it if needed.
		/// 
		/// <param name="lineno">1-based line number</param>
		/// <returns>the specified source line</returns>
		/// </summary>
		public string get_source_line(int lineno) {
			if (source_array == null) {
				if (content != null) {
					read_source_lines(content);
				} else {
					read_source_file();
				}
			}
			if (lineno < 1 || lineno > source_array.Count) {
				return null;
			}
			return source_array[lineno - 1];
		}

		/// <summary>
		/// Parses the input file into ::source_array.
		/// </summary>
		private void read_source_file() {
			string cont = mapped_file.GetContents();
			read_source_lines(cont);
		}

		private void remap_file() {
			if (mapped_file != null) {
				mapped_file.Dispose();
				mapped_file = null;
			}

			try {
				mapped_file = FastMemoryMappedFile.OpenExisting(filename);
			} catch (Exception e) {
				//length = 0;
				Report.error(null, "Unable to map file `%s': %s".printf(filename, e.Message));
			}
		}

		private void read_source_lines(string cont) {
			source_array = new List<string>();
			string[] lines = cont.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			foreach (string line in lines)
				source_array.Add(line);
		}

		public FastMView get_mapped_contents(out long length) {
			/*if (content != null)
			{
				return Encoding.Default.GetBytes(content);
			}*/

			length = new FileInfo(filename).Length;

			if (mapped_file == null) {
				remap_file();
			}

			return new FastMView(mapped_file);
		}

		public uint get_mapped_length() {
			if (content != null) {
				return (uint)content.Length;
			}

			return (uint)new FileInfo(filename).Length;
		}

		public bool check(CodeContext context) {
			foreach (CodeNode node in nodes) {
				node.check(context);
			}
			return true;
		}
	}

	public enum SourceFileType {
		NONE,
		SOURCE,
		PACKAGE,
		FAST
	}
}
