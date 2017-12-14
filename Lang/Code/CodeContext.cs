using Vala;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vala.Lang.Parser;
using static GLibPorts.GLib;
using Vala.Lang.Methods;
using Vala.Lang.Code;
using Vala.Lang.Symbols;
using System.IO;
using GLibPorts;
using ValaConfig;

namespace Vala.Lang.CodeNodes {
	public class CodeContext {
		/// <summary>
		/// Enable run-time checks for programming errors.
		/// </summary>
		public bool assert { get; set; }

		/// <summary>
		/// Enable additional run-time checks such as type checks.
		/// </summary>
		public bool checking { get; set; }

		/// <summary>
		/// Do not warn when using deprecated features.
		/// </summary>
		public bool deprecated { get; set; }

		/// <summary>
		/// Hide the symbols marked as internal
		/// </summary>
		public bool hide_internal { get; set; }

		/// <summary>
		/// Do not check whether used symbols exist in local packages.
		/// </summary>
		public bool since_check { get; set; }

		/// <summary>
		/// Do not warn when using experimental features.
		/// </summary>
		public bool experimental { get; set; }

		/// <summary>
		/// Enable experimental enhancements for non-null types.
		/// </summary>
		public bool experimental_non_null { get; set; }

		/// <summary>
		/// Enable GObject creation tracing.
		/// </summary>
		public bool gobject_tracing { get; set; }

		/// <summary>
		/// Output C code, don't compile to object code.
		/// </summary>
		public bool ccode_only { get; set; }

		/// <summary>
		/// Only check source for validity
		/// </summary>
		public bool dry_run { get; set; }

		/// <summary>
		/// Output C header file.
		/// </summary>
		public string header_filename { get; set; }

		/// <summary>
		/// Output internal C header file.
		/// </summary>
		public string internal_header_filename { get; set; }

		public bool use_header { get; set; }

		/// <summary>
		/// Base directory used for header_filename in the VAPIs.
		/// </summary>
		public string includedir { get; set; }

		/// <summary>
		/// Output symbols file.
		/// </summary>
		public string symbols_filename { get; set; }

		/// <summary>
		/// Compile but do not link.
		/// </summary>
		public bool compile_only { get; set; }

		/// <summary>
		/// Output filename.
		/// </summary>
		public string output { get; set; }

		/// <summary>
		/// Base source directory.
		/// </summary>
		public string basedir { get; set; }

		/// <summary>
		/// Code output directory.
		/// </summary>
		public string directory { get; set; }

		/// <summary>
		/// List of directories where to find .vapi files.
		/// </summary>
		public string[] vapi_directories;

		/// <summary>
		/// List of directories where to find .gir files.
		/// </summary>
		public string[] gir_directories;

		/// <summary>
		/// List of directories where to find .metadata files for .gir files.
		/// </summary>
		public string[] metadata_directories;

		/// <summary>
		/// Produce debug information.
		/// </summary>
		public bool debug { get; set; }

		/// <summary>
		/// Optimization level.
		/// </summary>
		public int optlevel { get; set; }

		/// <summary>
		/// Enable memory profiler.
		/// </summary>
		public bool mem_profiler { get; set; }

		/// <summary>
		/// Specifies the optional module initialization method.
		/// </summary>
		public Method module_init_method { get; set; }

		/// <summary>
		/// Keep temporary files produced by the compiler.
		/// </summary>
		public bool save_temps { get; set; }

		public Profile profile { get; set; }

		/// <summary>
		/// Target major version number of glib for code generation.
		/// </summary>
		public int target_glib_major { get; set; }

		/// <summary>
		/// Target minor version number of glib for code generation.
		/// </summary>
		public int target_glib_minor { get; set; }

		public bool verbose_mode { get; set; }

		public bool version_header { get; set; }

		public bool nostdpkg { get; set; }

		public bool use_fast_vapi { get; set; }

		/// <summary>
		/// Include comments in generated vapi.
		/// </summary>
		public bool vapi_comments { get; set; }

		/// <summary>
		/// Returns true if the target version of glib is greater than or
		/// equal to the specified version.
		/// </summary>
		public bool require_glib_version(int major, int minor) {
			return (target_glib_major > major) || (target_glib_major == major && target_glib_minor >= minor);
		}

		public bool save_csources {
			get { return save_temps; }
		}

		public string path { get; set; }

		public Report report { get; set; } = new Report();

		public Method entry_point { get; set; }

		public string entry_point_name { get; set; }

		public bool run_output { get; set; }

		public string[] gresources;

		public string[] gresources_directories;

		private List<SourceFile> source_files = new List<SourceFile>();
		private List<string> c_source_files = new List<string>();
		private Namespace _root = new Namespace(null);

		private List<string> packages = new List<string>();

		private HashSet<string> defines = new HashSet<string>();

		public static List<CodeContext> context_stack_key = null;

		public static void DisposeStatic() {
			context_stack_key = new List<CodeContext>();
		}

		/// <summary>
		/// The root namespace of the symbol tree.
		/// </summary>
		public Namespace root {
			get { return _root; }
		}

		public SymbolResolver resolver { get; private set; }

		public SemanticAnalyzer analyzer { get; private set; }

		public FlowAnalyzer flow_analyzer { get; private set; }

		/// <summary>
		/// The selected code generator.
		/// </summary>
		public CodeGenerator codegen { get; set; }

		/// <summary>
		/// Mark attributes used by the compiler and report unused at the end.
		/// </summary>
		public UsedAttr used_attr { get; set; }

		public CodeContext() {
			resolver = new SymbolResolver();
			analyzer = new SemanticAnalyzer();
			flow_analyzer = new FlowAnalyzer();
			used_attr = new UsedAttr();
		}

		/// <summary>
		/// Return the topmost context from the context stack.
		/// </summary>
		public static CodeContext get() {
			List<CodeContext> context_stack = context_stack_key;

			return context_stack[context_stack.Count - 1];
		}

		/// <summary>
		/// Push the specified context to the context stack.
		/// </summary>
		public static void push(CodeContext context) {
			List<CodeContext> context_stack = context_stack_key;
			if (context_stack == null) {
				context_stack = new List<CodeContext>();
				context_stack_key = context_stack;
			}

			context_stack.Add(context);
		}

		/// <summary>
		/// Remove the topmost context from the context stack.
		/// </summary>
		public static void pop() {
			List<CodeContext> context_stack = context_stack_key;

			context_stack.RemoveAt(context_stack.Count - 1);
		}

		/// <summary>
		/// Returns a copy of the list of source files.
		/// 
		/// <returns>list of source files</returns>
		/// </summary>
		public List<SourceFile> get_source_files() {
			return source_files;
		}

		/// <summary>
		/// Returns a copy of the list of C source files.
		/// 
		/// <returns>list of C source files</returns>
		/// </summary>
		public List<string> get_c_source_files() {
			return c_source_files;
		}

		/// <summary>
		/// Adds the specified file to the list of source files.
		/// 
		/// <param name="file">a source file</param>
		/// </summary>
		public void add_source_file(SourceFile file) {
			source_files.Add(file);
		}

		/// <summary>
		/// Adds the specified file to the list of C source files.
		/// 
		/// <param name="file">a C source file</param>
		/// </summary>
		public void add_c_source_file(string file) {
			c_source_files.Add(file);
		}

		/// <summary>
		/// Returns a copy of the list of used packages.
		/// 
		/// <returns>list of used packages</returns>
		/// </summary>
		public List<string> get_packages() {
			return packages;
		}

		/// <summary>
		/// Returns whether the specified package is being used.
		/// 
		/// <param name="pkg">a package name</param>
		/// <returns>true if the specified package is being used</returns>
		/// </summary>
		public bool has_package(string pkg) {
			return packages.Contains(pkg);
		}

		/// <summary>
		/// Adds the specified package to the list of used packages.
		/// 
		/// <param name="pkg">a package name</param>
		/// </summary>
		public void add_package(string pkg) {
			packages.Add(pkg);
		}

		/// <summary>
		/// Pull the specified package into the context.
		/// The method is tolerant if the package has been already loaded.
		/// 
		/// <param name="pkg">a package name</param>
		/// <returns>false if the package could not be loaded</returns>
		/// 
		/// </summary>
		public bool add_external_package(string pkg) {
			if (has_package(pkg)) {
				// ignore multiple occurrences of the same package
				return true;
			}

			// first try .vapi
			var path = get_vapi_path(pkg);
			if (path == null) {
				// try with .gir
				path = get_gir_path(pkg);
			}
			if (path == null) {
				Report.error(null, "Package `%s' not found in specified Vala API directories or GObject-Introspection GIR directories".printf(pkg));
				return false;
			}

			add_package(pkg);

			add_source_file(new SourceFile(this, SourceFileType.PACKAGE, path));

			if (verbose_mode) {
				stdout.printf("Loaded package `%s'\n", path);
			}

			var deps_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), Path.GetDirectoryName(path), pkg + ".deps");
			if (!add_packages_from_file(deps_filename)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Read the given filename and pull in packages.
		/// The method is tolerant if the file does not exist.
		/// 
		/// <param name="filename">a filename</param>
		/// <returns>false if an error occurs while reading the file or if a package could not be added</returns>
		/// </summary>
		public bool add_packages_from_file(string filename) {
			if (!File.Exists(filename)) {
				return true;
			}

			try {
				string contents;
				FileUtils.get_contents(filename, out contents);
				foreach (string package in contents.Split('\n')) {
					var _package = package.Trim();
					if (_package != "") {
						add_external_package(_package);
					}
				}
			} catch (Exception e) {
				Report.error(null, "Unable to read dependency file: %s".printf(e.Message));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Add the specified source file to the context. Only .vala, .vapi, .gs,
		/// and .c extensions are supported.
		/// 
		/// <param name="filename">a filename</param>
		/// <param name="is_source">true to force adding the file as .vala or .gs</param>
		/// <param name="cmdline">true if the file came from the command line.</param>
		/// <returns>false if the file is not recognized or the file does not exist</returns>
		/// </summary>
		public bool add_source_filename(string filename, bool is_source = false, bool cmdline = false) {
			if (!File.Exists(filename)) {
				Report.error(null, "%s not found".printf(filename));
				return false;
			}

			var rpath = Path.GetFullPath(filename);
			if (is_source || filename.EndsWith(".vala") || filename.EndsWith(".gs")) {
				var source_file = new SourceFile(this, SourceFileType.SOURCE, rpath, null, cmdline);
				source_file.Relative_filename = filename;

				// import the GLib namespace by default (namespace of backend-specific standard library)
				var ns_ref = new UsingDirective(new UnresolvedSymbol(null, "GLib", null));
				source_file.add_using_directive(ns_ref);
				root.add_using_directive(ns_ref);

				add_source_file(source_file);
			} else if (filename.EndsWith(".vapi") || filename.EndsWith(".gir")) {
				var source_file = new SourceFile(this, SourceFileType.PACKAGE, rpath, null, cmdline);
				source_file.Relative_filename = filename;

				add_source_file(source_file);
			} else if (filename.EndsWith(".c")) {
				add_c_source_file(rpath);
			} else if (filename.EndsWith(".h")) {
				/* Ignore */
			} else {
				Report.error(null, "%s is not a supported source file type. Only .vala, .vapi, .gs, and .c files are supported.".printf(filename));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Visits the complete code tree file by file.
		/// It is possible to add new source files while visiting the tree.
		/// 
		/// <param name="visitor">the visitor to be called when traversing</param>
		/// </summary>
		public void accept(CodeVisitor visitor) {
			root.accept(visitor);

			// support queueing new source files
			int index = 0;
			while (index < source_files.Count) {
				var source_file = source_files[index];
				source_file.accept(visitor);
				index++;
			}
		}

		/// <summary>
		/// Resolve and analyze.
		/// </summary>
		public void check() {
			resolver.resolve(this);

			if (report.get_errors() > 0) {
				return;
			}

			analyzer.analyze(this);

			if (report.get_errors() > 0) {
				return;
			}

			flow_analyzer.analyze(this);

			if (report.get_errors() > 0) {
				return;
			}

			used_attr.check_unused(this);
		}

		public void add_define(string define) {
			defines.Add(define);
		}

		public bool is_defined(string define) {
			return defines.Contains(define);
		}

		public string get_vapi_path(string pkg) {
			var path = get_file_path(
				pkg + ".vapi",
				"vala" + Config.PACKAGE_SUFFIX + "/vapi",
				"vala/vapi",
				vapi_directories
			);

			if (path == null) {
				/* last chance: try the package compiled-in vapi dir */
				var filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), Config.PACKAGE_DATADIR, "vapi", pkg + ".vapi");
				if (File.Exists(filename)) {
					path = filename;
				}
			}

			return path;
		}

		public string get_gir_path(string gir) {
			return get_file_path(gir + ".gir", "gir-1.0", null, gir_directories);
		}

		public string get_gresource_path(string gresource, string resource) {
			var filename = get_file_path(resource, null, null, new string[] { Path.GetDirectoryName(gresource) });
			if (filename == null) {
				filename = get_file_path(resource, null, null, gresources_directories);
			}
			return filename;
		}

		/*
		 * Returns the .metadata file associated with the given .gir file.
		 */
		public string get_metadata_path(string gir_filename) {
			var basename = Path.GetFileName(gir_filename);
			var metadata_basename = "%s.metadata".printf(basename.Substring(0, basename.Length - ".gir".Length));

			// look into metadata directories
			var metadata_filename = get_file_path(metadata_basename, null, null, metadata_directories);
			if (metadata_filename != null) {
				return metadata_filename;
			}

			// look into the same directory of .gir
			metadata_filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), Path.GetDirectoryName(gir_filename), metadata_basename);
			if (File.Exists(metadata_filename)) {
				return metadata_filename;
			}

			return null;
		}

		string get_file_path(string basename, string versioned_data_dir, string data_dir, string[] directories) {
			string filename = null;

			if (directories != null) {
				foreach (string dir in directories) {
					filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), dir, basename);
					if (File.Exists(filename)) {
						return filename;
					}
				}
			}

			if (data_dir != null) {
				foreach (string dir in GEnvironment.get_system_data_dirs()) {
					filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), dir, data_dir, basename);
					if (File.Exists(filename)) {
						return filename;
					}
				}
			}

			if (versioned_data_dir != null) {
				foreach (string dir in GEnvironment.get_system_data_dirs()) {
					filename = GPath.build_path(Path.DirectorySeparatorChar.ToString(), dir, versioned_data_dir, basename);
					if (File.Exists(filename)) {
						return filename;
					}
				}
			}

			return null;
		}

		public void write_dependencies(string filename) {
			var stream = GLib.FileStream.open(filename, "w");

			if (stream == null) {
				Report.error(null, "unable to open `%s' for writing".printf(filename));
				return;
			}

			stream.printf("%s:", filename);
			foreach (var src in source_files) {
				if (src.file_type == SourceFileType.FAST && src.used) {
					stream.printf(" %s", src.filename);
				}
			}
			stream.printf("\n\n");
		}

		private static bool ends_with_dir_separator(string s) {
			return GPath.is_dir_separator(s[s.Length - 1]);
		}

		public static string realpath(string vapi) {
			return Path.GetFullPath(vapi);
		}
	}
}
