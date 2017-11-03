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

namespace Vala.Lang.CodeNodes
{
	public class CodeContext
	{
		/**
 * Enable run-time checks for programming errors.
 */
		public bool assert { get; set; }

		/**
		 * Enable additional run-time checks such as type checks.
		 */
		public bool checking { get; set; }

		/**
		 * Do not warn when using deprecated features.
		 */
		public bool deprecated { get; set; }

		/**
		 * Hide the symbols marked as internal
		 */
		public bool hide_internal { get; set; }

		/**
		 * Do not check whether used symbols exist in local packages.
		 */
		public bool since_check { get; set; }

		/**
		 * Do not warn when using experimental features.
		 */
		public bool experimental { get; set; }

		/**
		 * Enable experimental enhancements for non-null types.
		 */
		public bool experimental_non_null { get; set; }

		/**
		 * Enable GObject creation tracing.
		 */
		public bool gobject_tracing { get; set; }

		/**
		 * Output C code, don't compile to object code.
		 */
		public bool ccode_only { get; set; }

		/**
		 * Output C header file.
		 */
		public string header_filename { get; set; }

		/**
		 * Output internal C header file.
		 */
		public string internal_header_filename { get; set; }

		public bool use_header { get; set; }

		/**
		 * Base directory used for header_filename in the VAPIs.
		 */
		public string includedir { get; set; }

		/**
		 * Output symbols file.
		 */
		public string symbols_filename { get; set; }

		/**
		 * Compile but do not link.
		 */
		public bool compile_only { get; set; }

		/**
		 * Output filename.
		 */
		public string output { get; set; }

		/**
		 * Base source directory.
		 */
		public string basedir { get; set; }

		/**
		 * Code output directory.
		 */
		public string directory { get; set; }

		/**
		 * List of directories where to find .vapi files.
		 */
		public string[] vapi_directories;

		/**
		 * List of directories where to find .gir files.
		 */
		public string[] gir_directories;

		/**
		 * List of directories where to find .metadata files for .gir files.
		 */
		public string[] metadata_directories;

		/**
		 * Produce debug information.
		 */
		public bool debug { get; set; }

		/**
		 * Optimization level.
		 */
		public int optlevel { get; set; }

		/**
		 * Enable memory profiler.
		 */
		public bool mem_profiler { get; set; }

		/**
		 * Specifies the optional module initialization method.
		 */
		public Method module_init_method { get; set; }

		/**
		 * Keep temporary files produced by the compiler.
		 */
		public bool save_temps { get; set; }

		public Profile profile { get; set; }

		/**
		 * Target major version number of glib for code generation.
		 */
		public int target_glib_major { get; set; }

		/**
		 * Target minor version number of glib for code generation.
		 */
		public int target_glib_minor { get; set; }

		public bool verbose_mode { get; set; }

		public bool version_header { get; set; }

		public bool nostdpkg { get; set; }

		public bool use_fast_vapi { get; set; }

		/**
		 * Include comments in generated vapi.
		 */
		public bool vapi_comments { get; set; }

		/**
		 * Returns true if the target version of glib is greater than or 
		 * equal to the specified version.
		 */
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
			Debug.WriteLine(string.Format("CodeContext: disposing {0} leftovers", context_stack_key.Count));
			context_stack_key = new List<CodeContext>();
		}

		/**
		 * The root namespace of the symbol tree.
		 */
		public Namespace root {
			get { return _root; }
		}

		public SymbolResolver resolver { get; private set; }

		public SemanticAnalyzer analyzer { get; private set; }

		public FlowAnalyzer flow_analyzer { get; private set; }

		/**
		 * The selected code generator.
		 */
		public CodeGenerator codegen { get; set; }

		/**
		 * Mark attributes used by the compiler and report unused at the end.
		 */
		public UsedAttr used_attr { get; set; }

		public CodeContext() {
			resolver = new SymbolResolver();
			analyzer = new SemanticAnalyzer();
			flow_analyzer = new FlowAnalyzer();
			used_attr = new UsedAttr();
		}

		/**
		 * Return the topmost context from the context stack.
		 */
		public static CodeContext get() {
			List<CodeContext> context_stack = context_stack_key;

			return context_stack[context_stack.Count - 1];
		}

		/**
		 * Push the specified context to the context stack.
		 */
		public static void push(CodeContext context) {
			List<CodeContext> context_stack = context_stack_key;
			if (context_stack == null) {
				context_stack = new List<CodeContext>();
				context_stack_key = context_stack;
			}

			context_stack.Add(context);
		}

		/**
		 * Remove the topmost context from the context stack.
		 */
		public static void pop() {
			List<CodeContext> context_stack = context_stack_key;

			context_stack.RemoveAt(context_stack.Count - 1);
		}

		/**
		 * Returns a copy of the list of source files.
		 *
		 * @return list of source files
		 */
		public List<SourceFile> get_source_files() {
			return source_files;
		}

		/**
		 * Returns a copy of the list of C source files.
		 *
		 * @return list of C source files
		 */
		public List<string> get_c_source_files() {
			return c_source_files;
		}

		/**
		 * Adds the specified file to the list of source files.
		 *
		 * @param file a source file
		 */
		public void add_source_file(SourceFile file) {
			source_files.Add(file);
		}

		/**
		 * Adds the specified file to the list of C source files.
		 *
		 * @param file a C source file
		 */
		public void add_c_source_file(string file) {
			c_source_files.Add(file);
		}

		/**
		 * Returns a copy of the list of used packages.
		 *
		 * @return list of used packages
		 */
		public List<string> get_packages() {
			return packages;
		}

		/**
		 * Returns whether the specified package is being used.
		 *
		 * @param pkg a package name
		 * @return    true if the specified package is being used
		 */
		public bool has_package(string pkg) {
			return packages.Contains(pkg);
		}

		/**
		 * Adds the specified package to the list of used packages.
		 *
		 * @param pkg a package name
		 */
		public void add_package(string pkg) {
			packages.Add(pkg);
		}

		/**
		 * Pull the specified package into the context.
		 * The method is tolerant if the package has been already loaded.
		 *
		 * @param pkg a package name
		 * @return false if the package could not be loaded
		 *
		 */
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

		/**
		 * Read the given filename and pull in packages.
		 * The method is tolerant if the file does not exist.
		 *
		 * @param filename a filename
		 * @return false if an error occurs while reading the file or if a package could not be added
		 */
		public bool add_packages_from_file(string filename) {
			if (!File.Exists(filename)) {
				return true;
			}

			try {
				string contents;
				FileUtils.get_contents(filename, out contents);
				foreach (string package in contents.Split('\n')) {
					var _package = package.Trim();
					if (package != "") {
						add_external_package(package);
					}
				}
			} catch (Exception e) {
				Report.error(null, "Unable to read dependency file: %s".printf(e.Message));
				return false;
			}

			return true;
		}

		/**
		 * Add the specified source file to the context. Only .vala, .vapi, .gs,
		 * and .c extensions are supported.
		 *
		 * @param filename a filename
		 * @param is_source true to force adding the file as .vala or .gs
		 * @param cmdline true if the file came from the command line.
		 * @return false if the file is not recognized or the file does not exist
		 */
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

		/**
		 * Visits the complete code tree file by file.
		 * It is possible to add new source files while visiting the tree.
		 *
		 * @param visitor the visitor to be called when traversing
		 */
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

		/**
		 * Resolve and analyze.
		 */
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
			var filename = get_file_path(resource, null, null, new string[]{ Path.GetDirectoryName(gresource) });
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
