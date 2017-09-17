using Vala;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vala.Lang.CodeNodes;
using static GLibPorts.GLib;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;

namespace Vala.Lang.Parser
{
	public class SourceFile
	{
		/**
		 * The name of this source file.
		 */
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

		/**
		 * The installed package version or null
		 */
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
					Process pkgconfig = Process.Start(new ProcessStartInfo {
						UseShellExecute = false,
						FileName = context.path + "pkg-config" + GProcess.get_executable_suffix(),
						Arguments = "--silence - errors--modversion % s".printf(pkg_config_name),
						RedirectStandardError = true,
						RedirectStandardOutput = true
					});
					pkgconfig.WaitForExit();
					standard_output = pkgconfig.StandardOutput.ReadToEnd();
					exit_status = pkgconfig.ExitCode;
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


		/**
		 * Specifies whether this file is a VAPI package file.
		 */
		public SourceFileType file_type { get; set; }

		/**
		 * Specifies whether this file came from the command line directly.
		 */
		public bool from_commandline { get; set; }

		/**
		 *  GIR Namespace for this source file, if it's a VAPI package
		 */

		public string gir_namespace { get; set; }

		/**
		 *  GIR Namespace version for this source file, if it's a VAPI package
		 */

		public string gir_version { get; set; }

		/**
		 * The context this source file belongs to.
		 */
		public CodeContext context { get; set; }

		public string content {
			get { return this._content; }
			set {
				this._content = value;
				this.source_array = null;
			}
		}

		/**
		 * If the file has been used (ie: if anything in the file has
		 * been emitted into C code as a definition or declaration).
		 */
		public bool used { get; set; }

		/**
		 * Whether this source-file was explicitly passed on the commandline.
		 */
		public bool is_explicit { get; set; }

		private List<Comment> comments = new List<Comment>();

		public List<UsingDirective> current_using_directives { get; set; } = new List<UsingDirective>();

		private List<CodeNode> nodes = new List<CodeNode>();

		string _relative_filename;

		private string csource_filename = null;
		private string cinclude_filename = null;

		private List<string> source_array = null;

		private MemoryMappedFile mapped_file = null;

		private string _content = null;

		/**
		 * Creates a new source file.
		 *
		 * @param filename source file name
		 * @return         newly created source file
		 */
		public SourceFile(CodeContext context, SourceFileType type, string filename, string content = null, bool cmdline = false) {
			this.context = context;
			this.file_type = type;
			this.filename = filename;
			this.content = content;
			this.from_commandline = cmdline;
		}

		/**
		 * Adds a header comment to this source file.
		 */
		public void add_comment(Comment comment) {
			comments.Add(comment);
		}

		/**
		 * Returns a copy of the list of header comments.
		 *
		 * @return list of comments
		 */
		public List<Comment> get_comments() {
			return comments;
		}

		/**
		 * Adds a new using directive with the specified namespace.
		 *
		 * @param ns reference to namespace
		 */
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

		/**
		 * Adds the specified code node to this source file.
		 *
		 * @param node a code node
		 */
		public void add_node(CodeNode node) {
			nodes.Add(node);
		}

		public void remove_node(CodeNode node) {
			nodes.Remove(node);
		}

		/**
		 * Returns a copy of the list of code nodes.
		 *
		 * @return code node list
		 */
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
			if (filename.StartsWith(context.basedir + "/")) {
				var basename = Path.GetFileName(filename);
				var subdir = filename.Substring(context.basedir.Length, filename.Length - context.basedir.Length - basename.Length);
				while (subdir[0] == '/') {
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
			return GPath.build_path("/", context.directory, get_subdir());
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

		/**
		 * Returns the filename to use when generating C source files.
		 *
		 * @return generated C source filename
		 */
		public string get_csource_filename() {
			if (csource_filename == null) {
				if (context.run_output) {
					csource_filename = context.output + ".c";
				} else if (context.ccode_only || context.save_csources) {
					csource_filename = GPath.build_path("/", get_destination_directory(), get_basename() + ".c");
				} else {
					// temporary file
					csource_filename = GPath.build_path("/", get_destination_directory(), get_basename() + ".vala.c");
				}
			}
			return csource_filename;
		}

		/**
		 * Returns the filename to use when including the generated C header
		 * file.
		 *
		 * @return C header filename to include
		 */
		public string get_cinclude_filename() {
			if (cinclude_filename == null) {
				if (context.header_filename != null) {
					cinclude_filename = Path.GetFileName(context.header_filename);
					if (context.includedir != null) {
						cinclude_filename = GPath.build_path("/", context.includedir, cinclude_filename);
					}
				} else {
					cinclude_filename = GPath.build_path("/", get_subdir(), get_basename() + ".h");
				}
			}
			return cinclude_filename;
		}

		/**
		 * Returns the requested line from this file, loading it if needed.
		 *
		 * @param lineno 1-based line number
		 * @return       the specified source line
		 */
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

		/**
		 * Parses the input file into ::source_array.
		 */
		private void read_source_file() {
			string cont;
			try {
				FileUtils.get_contents(filename, out cont);
			} catch (Exception) {
				return;
			}
			read_source_lines(cont);
		}

		private void read_source_lines(string cont) {
			source_array = new List<string>();
			string[] lines = cont.Split('\n');
			int idx;
			for (idx = 0; lines[idx] != null; ++idx) {
				source_array.Add(lines[idx]);
			}
		}

		public byte[] get_mapped_contents(out long length) {
			/*if (content != null)
			{
				return Encoding.Default.GetBytes(content);
			}*/

			length = new FileInfo(filename).Length;

			if (mapped_file == null) {
				try {
					mapped_file = MemoryMappedFile.CreateFromFile(filename);
				} catch (Exception e)
				{
					length = 0;
					Report.error(null, "Unable to map file `%s': %s".printf(filename, e.Message));
					return null;
				}
			}

			var stream = mapped_file.CreateViewStream();
			BinaryReader binReader = new BinaryReader(stream);

			return binReader.ReadBytes((int)stream.Length);
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

	public enum SourceFileType
	{
		NONE,
		SOURCE,
		PACKAGE,
		FAST
	}
}
