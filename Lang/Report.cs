using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using static GLibPorts.GLib;
using Vala.Lang.Parser;
using System.Runtime.InteropServices;
using Vala.Lang.CodeNodes;
using Vala;

namespace Vala.Lang {
	public class Report {
		public enum Colored {
			AUTO,
			NEVER,
			ALWAYS
		}

		public event EventHandler<ReportEventArgs> OnReport;


		/// <summary>
		/// SGR (Select Graphic Rendition) end tag
		/// </summary>
		private const string ANSI_COLOR_END = "\x1b[0m";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for source location
		/// </summary>
		private string locus_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for source location
		/// </summary>
		private string locus_color_end = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for warning titles
		/// </summary>
		private string warning_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for warning titles
		/// </summary>
		private string warning_color_end = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for error titles
		/// </summary>
		private string error_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for error titles
		/// </summary>
		private string error_color_end = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for note titles
		/// </summary>
		private string note_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for note titles
		/// </summary>
		private string note_color_end = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for caret line (^^^)
		/// </summary>
		private string caret_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for caret line (^^^)
		/// </summary>
		private string caret_color_end = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) start tag for quotes line ('...', `...`, `...')
		/// </summary>
		private string quote_color_start = "";

		/// <summary>
		/// SGR (Select Graphic Rendition) end tag for quotes line ('...', `...`, `...')
		/// </summary>
		private string quote_color_end = "";


		protected int warnings;
		protected int errors;

		private bool verbose_errors;

		public bool enable_warnings { get; set; } = true;

		static Regex val_regex;

		/// <summary>
		/// Set all colors by string
		/// 
		/// {{{
		/// "error=01;31:warning=01;35:note=01;36:caret=01;32:locus=01:quote=01"
		/// }}}
		/// </summary>
		public bool set_colors(string str, Report.Colored colored_output = Report.Colored.AUTO) {
			try {
				if (val_regex == null)
					val_regex = new Regex("^\\s*[0-9]+(;[0-9]*)*\\s*$");
			} catch (Exception) {
				assert_not_reached();
			}

			string error_color = null;
			string warning_color = null;
			string note_color = null;
			string caret_color = null;
			string locus_color = null;
			string quote_color = null;

			string[] fragments = str.Split(':');
			foreach (string fragment in fragments) {
				string[] eq = fragment.Split(new char[] { '=' }, 2);
				if (eq.Length != 2) {
					return false;
				}

				if (!val_regex.IsMatch(eq[1])) {
					return false;
				}


				string checked_value = eq[1].Trim();
				switch (eq[0].Trim()) {
				case "error":
					error_color = checked_value;
					break;

				case "warning":
					warning_color = checked_value;
					break;

				case "note":
					note_color = checked_value;
					break;

				case "caret":
					caret_color = checked_value;
					break;

				case "locus":
					locus_color = checked_value;
					break;

				case "quote":
					quote_color = checked_value;
					break;

				default:
					return false;
				}
			}

			if (colored_output == Report.Colored.ALWAYS || (
				colored_output == Report.Colored.AUTO && is_atty(stderr.fileno())
			)) {
				if (error_color != null) {
					this.error_color_start = "\x1b[0" + error_color + "m";
					this.error_color_end = ANSI_COLOR_END;
				}

				if (warning_color != null) {
					this.warning_color_start = "\x1b[0" + warning_color + "m";
					this.warning_color_end = ANSI_COLOR_END;
				}

				if (note_color != null) {
					this.note_color_start = "\x1b[0" + note_color + "m";
					this.note_color_end = ANSI_COLOR_END;
				}

				if (caret_color != null) {
					this.caret_color_start = "\x1b[0" + caret_color + "m";
					this.caret_color_end = ANSI_COLOR_END;
				}

				if (locus_color != null) {
					this.locus_color_start = "\x1b[0" + locus_color + "m";
					this.locus_color_end = ANSI_COLOR_END;
				}

				if (quote_color != null) {
					this.quote_color_start = "\x1b[0" + quote_color + "m";
					this.quote_color_end = ANSI_COLOR_END;
				}
			}
			return true;
		}

		/// <summary>
		/// Set the error verbosity.
		/// </summary>
		public void set_verbose_errors(bool verbose) {
			verbose_errors = verbose;
		}

		/// <summary>
		/// Returns the total number of warnings reported.
		/// </summary>
		public int get_warnings() {
			return warnings;
		}

		/// <summary>
		/// Returns the total number of errors reported.
		/// </summary>
		public int get_errors() {
			return errors;
		}

		/// <summary>
		/// Pretty-print the actual line of offending code if possible.
		/// </summary>
		private void report_source(SourceReference source) {
			if (source.begin.line != source.end.line) {
				// FIXME Cannot report multi-line issues currently
				return;
			}

			string offending_line = source.file.get_source_line(source.begin.line);

			if (offending_line != null) {
				stderr.printf("%s\n", offending_line);
				int idx;

				/* We loop in this manner so that we don't fall over on differing
				 * tab widths. This means we get the ^s in the right places.
				 */
				for (idx = 1; idx < source.begin.column; ++idx) {
					if (offending_line[idx - 1] == '\t') {
						stderr.printf("\t");
					} else {
						stderr.printf(" ");
					}
				}

				stderr.puts(caret_color_start);
				for (idx = source.begin.column; idx <= source.end.column; ++idx) {
					if (offending_line[idx - 1] == '\t') {
						stderr.printf("\t");
					} else {
						stderr.printf("^");
					}
				}
				stderr.puts(caret_color_end);

				stderr.printf("\n");
			}
		}

		private void print_highlighted_message(string message) {
			int start = 0;
			int cur = 0;

			int length = message.Length;
			while (cur < length) {
				if (message[cur] == '\'' || message[cur] == '`') {
					string end_chars = (message[cur] == '`') ? "`'" : "'";
					stderr.puts(message.Substring(start, cur - start));
					start = cur;
					cur++;

					while (message[cur] != '\0' && end_chars.IndexOf(message[cur]) < 0) {
						cur++;
					}
					if (message[cur] == '\0') {
						stderr.puts(message.Substring(start, cur - start));
						start = cur;
					} else {
						cur++;
						stderr.printf("%s%s%s", quote_color_start, message.Substring(start, cur - start), quote_color_end);
						start = cur;
					}
				} else {
					cur++;
				}
			}

			stderr.puts(message.Substring(start));
		}

		private void print_message(SourceReference source, string type, string type_color_start, string type_color_end, string message, bool do_report_source) {
			if (source != null) {
				stderr.printf("%s%s:%s ", locus_color_start, source.ToString(), locus_color_end);
			}

			stderr.printf("%s%s:%s ", type_color_start, type, type_color_end);

			// highlight '', `', ``
			print_highlighted_message(message);
			stderr.putc('\n');

			if (do_report_source && source != null) {
				report_source(source);
			}

			if (Debugger.IsAttached)
				Debug.WriteLine($"[{type_color_start}] {message}");
		}

		/// <summary>
		/// Reports the specified message as note.
		/// 
		/// <param name="source">reference to source code</param>
		/// <param name="message">note message</param>
		/// </summary>
		public virtual void note(SourceReference source, string message) {
			if (!enable_warnings) {
				return;
			}

			print_message(source, "note", note_color_start, note_color_end, message, verbose_errors);
		}

		/// <summary>
		/// Reports the specified message as deprecation warning.
		/// 
		/// <param name="source">reference to source code</param>
		/// <param name="message">warning message</param>
		/// </summary>
		public virtual void depr(SourceReference source, string message) {
			if (!enable_warnings) {
				return;
			}

			warnings++;

			print_message(source, "warning", warning_color_start, warning_color_end, message, false);
		}

		/// <summary>
		/// Reports the specified message as warning.
		/// 
		/// <param name="source">reference to source code</param>
		/// <param name="message">warning message</param>
		/// </summary>
		public virtual void warn(SourceReference source, string message) {
			if (!enable_warnings) {
				return;
			}

			warnings++;

			print_message(source, "warning", warning_color_start, warning_color_end, message, verbose_errors);
		}

		/// <summary>
		/// Reports the specified message as error.
		/// 
		/// <param name="source">reference to source code</param>
		/// <param name="message">error message</param>
		/// </summary>
		public virtual void err(SourceReference source, string message) {
			errors++;

			print_message(source, "error", error_color_start, error_color_end, message, verbose_errors);
		}

		/* Convenience methods calling warn and err on correct instance */
		public static void notice(SourceReference source, string message) {
			var report = CodeContext.get().report;
			report.note(source, message);
			report.OnReport?.Invoke(report, new ReportEventArgs(ReportType.Notice, message, source));
		}
		public static void deprecated(SourceReference source, string message) {
			var report = CodeContext.get().report;
			report.depr(source, message);
			report.OnReport?.Invoke(report, new ReportEventArgs(ReportType.Deprecated, message, source));
		}
		public static void experimental(SourceReference source, string message) {
			var report = CodeContext.get().report;
			report.depr(source, message);
			report.OnReport?.Invoke(report, new ReportEventArgs(ReportType.Experimental, message, source));
		}
		public static void warning(SourceReference source, string message) {
			var report = CodeContext.get().report;
			report.warn(source, message);
			report.OnReport?.Invoke(report, new ReportEventArgs(ReportType.Warning, message, source));
		}
		public static void error(SourceReference source, string message) {
			var report = CodeContext.get().report;
			report.err(source, message);
			report.OnReport?.Invoke(report, new ReportEventArgs(ReportType.Error, message, source));
		}


		private delegate int AttyFunc(int fd);

		private bool is_atty(int fd) {
			Module module = Module.open(null, ModuleFlags.BIND_LAZY);
			if (module == null) {
				return false;
			}

			IntPtr _func;
			module.symbol("isatty", out _func);
			if (_func == IntPtr.Zero) {
				module.Dispose();
				return false;
			}

			AttyFunc func = Marshal.GetDelegateForFunctionPointer<AttyFunc>(_func);
			bool res = func(fd) == 1;

			module.Dispose();
			return res;
		}
	}
}
