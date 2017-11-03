using NUnit.Framework;
namespace ValaTests
{
	[TestFixture]
	public class ValaCompilerTests {

		[Test]
		public void Annotations_deprecated() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Annotations/deprecated.vala") == 0);
		}
		[Test]
		public void Annotations_description() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Annotations/description.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug595735() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug595735.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug595755() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug595755.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug596177() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug596177.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug596861() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug596861.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug597294() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug597294.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug598677() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug598677.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug598697() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug598697.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug598698() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug598698.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug599568() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug599568.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug600827() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug600827.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug601558() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug601558.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug613484() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug613484.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug620740() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug620740.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug639591() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug639591.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug640721() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug640721.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug641182() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug641182.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug646945() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug646945.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug652252() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug652252.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug653861() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug653861.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug654336() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug654336.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug654337() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug654337.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug659886() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug659886.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug661961() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug661961.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug710103() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug710103.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug741929() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug741929.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug742621() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug742621.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug762819() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug762819.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug777242() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug777242.vala") == 0);
		}
		[Test]
		public void Asyncronous_bug783543() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/bug783543.vala") == 0);
		}
		[Test]
		public void Asyncronous_closures() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/closures.vala") == 0);
		}
		[Test]
		public void Asyncronous_generator() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/generator.vala") == 0);
		}
		[Test]
		public void Asyncronous_yield() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Asyncronous/yield.vala") == 0);
		}
		[Test]
		public void BasicTypes_arrays() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/arrays.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug571486() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug571486.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug591552() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug591552.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug595751() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug595751.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug596637() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug596637.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug596785() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug596785.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug632322() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug632322.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug643612() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug643612.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug644046() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug644046.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug647222() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug647222.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug648364() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug648364.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug650993() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug650993.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug652380() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug652380.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug655908() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug655908.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug659975() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug659975.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug678791() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug678791.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug686336() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug686336.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug729907() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug729907.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug731017() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug731017.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug756376() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug756376.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug761307() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug761307.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug761736() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug761736.vala") == 0);
		}
		[Test]
		public void BasicTypes_bug772426() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/bug772426.vala") == 0);
		}
		[Test]
		public void BasicTypes_escape_chars() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/escape-chars.vala") == 0);
		}
		[Test]
		public void BasicTypes_floats() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/floats.vala") == 0);
		}
		[Test]
		public void BasicTypes_glists() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/glists.vala") == 0);
		}
		[Test]
		public void BasicTypes_integers() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/integers.vala") == 0);
		}
		[Test]
		public void BasicTypes_pointers() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/pointers.vala") == 0);
		}
		[Test]
		public void BasicTypes_sizeof() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/sizeof.vala") == 0);
		}
		[Test]
		public void BasicTypes_strings() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("BasicTypes/strings.vala") == 0);
		}
		[Test]
		public void Chainup_class_base_foo() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/class-base-foo.vala") == 0);
		}
		[Test]
		public void Chainup_class_base() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/class-base.vala") == 0);
		}
		[Test]
		public void Chainup_class_object() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/class-object.vala") == 0);
		}
		[Test]
		public void Chainup_class_this_foo() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/class-this-foo.vala") == 0);
		}
		[Test]
		public void Chainup_class_this() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/class-this.vala") == 0);
		}
		[Test]
		public void Chainup_no_chainup() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/no-chainup.vala") == 0);
		}
		[Test]
		public void Chainup_struct_base_foo() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/struct-base-foo.vala") == 0);
		}
		[Test]
		public void Chainup_struct_base() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/struct-base.vala") == 0);
		}
		[Test]
		public void Chainup_struct_this_foo() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/struct-this-foo.vala") == 0);
		}
		[Test]
		public void Chainup_struct_this() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Chainup/struct-this.vala") == 0);
		}
		[Test]
		public void ControlFlow_break() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/break.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug628336() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug628336.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug639482() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug639482.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug652549() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug652549.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug661985() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug661985.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug665904() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug665904.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug691514() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug691514.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug736774_1() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug736774-1.vala") == 0);
		}
		[Test]
		public void ControlFlow_bug736774_2() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/bug736774-2.vala") == 0);
		}
		[Test]
		public void ControlFlow_expressions_conditional() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/expressions-conditional.vala") == 0);
		}
		[Test]
		public void ControlFlow_for() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/for.vala") == 0);
		}
		[Test]
		public void ControlFlow_foreach() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/foreach.vala") == 0);
		}
		[Test]
		public void ControlFlow_nested_conditional() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/nested-conditional.vala") == 0);
		}
		[Test]
		public void ControlFlow_sideeffects() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/sideeffects.vala") == 0);
		}
		[Test]
		public void ControlFlow_switch() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ControlFlow/switch.vala") == 0);
		}
		[Test]
		public void DBus_bug596862() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("DBus/bug596862.vala") == 0);
		}
		[Test]
		public void DBus_enum_string_marshalling() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("DBus/enum-string-marshalling.vala") == 0);
		}
		[Test]
		public void Delegates_bug539166() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug539166.vala") == 0);
		}
		[Test]
		public void Delegates_bug595610() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug595610.vala") == 0);
		}
		[Test]
		public void Delegates_bug595639() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug595639.vala") == 0);
		}
		[Test]
		public void Delegates_bug638415() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug638415.vala") == 0);
		}
		[Test]
		public void Delegates_bug639751() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug639751.vala") == 0);
		}
		[Test]
		public void Delegates_bug659778() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug659778.vala") == 0);
		}
		[Test]
		public void Delegates_bug683925() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug683925.vala") == 0);
		}
		[Test]
		public void Delegates_bug703804() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug703804.vala") == 0);
		}
		[Test]
		public void Delegates_bug761360() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/bug761360.vala") == 0);
		}
		[Test]
		public void Delegates_casting() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/casting.vala") == 0);
		}
		[Test]
		public void Delegates_delegates() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/delegates.vala") == 0);
		}
		[Test]
		public void Delegates_reference_transfer() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Delegates/reference_transfer.vala") == 0);
		}
		[Test]
		public void Enums_bug673879() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/bug673879.vala") == 0);
		}
		[Test]
		public void Enums_bug763831() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/bug763831.vala") == 0);
		}
		[Test]
		public void Enums_bug780050() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/bug780050.vala") == 0);
		}
		[Test]
		public void Enums_enums() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/enums.vala") == 0);
		}
		[Test]
		public void Enums_enum_only() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/enum_only.vala") == 0);
		}
		[Test]
		public void Enums_flags() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Enums/flags.vala") == 0);
		}
		[Test]
		public void Errors_bug567181() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug567181.vala") == 0);
		}
		[Test]
		public void Errors_bug579101() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug579101.vala") == 0);
		}
		[Test]
		public void Errors_bug596228() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug596228.vala") == 0);
		}
		[Test]
		public void Errors_bug623049() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug623049.vala") == 0);
		}
		[Test]
		public void Errors_bug639589() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug639589.vala") == 0);
		}
		[Test]
		public void Errors_bug651145() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug651145.vala") == 0);
		}
		[Test]
		public void Errors_bug778224() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/bug778224.vala") == 0);
		}
		[Test]
		public void Errors_errors() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Errors/errors.vala") == 0);
		}
		[Test]
		public void Methods_bug595538() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug595538.vala") == 0);
		}
		[Test]
		public void Methods_bug596726() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug596726.vala") == 0);
		}
		[Test]
		public void Methods_bug597426() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug597426.vala") == 0);
		}
		[Test]
		public void Methods_bug598738() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug598738.vala") == 0);
		}
		[Test]
		public void Methods_bug599892() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug599892.vala") == 0);
		}
		[Test]
		public void Methods_bug613483() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug613483.vala") == 0);
		}
		[Test]
		public void Methods_bug620673() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug620673.vala") == 0);
		}
		[Test]
		public void Methods_bug622570() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug622570.vala") == 0);
		}
		[Test]
		public void Methods_bug626783() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug626783.vala") == 0);
		}
		[Test]
		public void Methods_bug639054() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug639054.vala") == 0);
		}
		[Test]
		public void Methods_bug642350() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug642350.vala") == 0);
		}
		[Test]
		public void Methods_bug642885() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug642885.vala") == 0);
		}
		[Test]
		public void Methods_bug642899() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug642899.vala") == 0);
		}
		[Test]
		public void Methods_bug646345() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug646345.vala") == 0);
		}
		[Test]
		public void Methods_bug648320() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug648320.vala") == 0);
		}
		[Test]
		public void Methods_bug649562() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug649562.vala") == 0);
		}
		[Test]
		public void Methods_bug652098() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug652098.vala") == 0);
		}
		[Test]
		public void Methods_bug653391() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug653391.vala") == 0);
		}
		[Test]
		public void Methods_bug653908() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug653908.vala") == 0);
		}
		[Test]
		public void Methods_bug663210() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug663210.vala") == 0);
		}
		[Test]
		public void Methods_bug710862() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug710862.vala") == 0);
		}
		[Test]
		public void Methods_bug723009() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug723009.vala") == 0);
		}
		[Test]
		public void Methods_bug723195() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug723195.vala") == 0);
		}
		[Test]
		public void Methods_bug726347() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug726347.vala") == 0);
		}
		[Test]
		public void Methods_bug736235() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug736235.vala") == 0);
		}
		[Test]
		public void Methods_bug737222() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug737222.vala") == 0);
		}
		[Test]
		public void Methods_bug743877() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug743877.vala") == 0);
		}
		[Test]
		public void Methods_bug771964() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug771964.vala") == 0);
		}
		[Test]
		public void Methods_bug774060() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug774060.vala") == 0);
		}
		[Test]
		public void Methods_bug781061() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/bug781061.vala") == 0);
		}
		[Test]
		public void Methods_closures() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/closures.vala") == 0);
		}
		[Test]
		public void Methods_generics() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/generics.vala") == 0);
		}
		[Test]
		public void Methods_lambda() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/lambda.vala") == 0);
		}
		[Test]
		public void Methods_prepostconditions() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/prepostconditions.vala") == 0);
		}
		[Test]
		public void Methods_printf_constructor() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/printf-constructor.vala") == 0);
		}
		[Test]
		public void Methods_symbolresolution() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Methods/symbolresolution.vala") == 0);
		}
		[Test]
		public void Nullability_bug611223() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Nullability/bug611223.vala") == 0);
		}
		[Test]
		public void Objects_bug566909() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug566909.vala") == 0);
		}
		[Test]
		public void Objects_bug588203() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug588203.vala") == 0);
		}
		[Test]
		public void Objects_bug589928() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug589928.vala") == 0);
		}
		[Test]
		public void Objects_bug593260() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug593260.vala") == 0);
		}
		[Test]
		public void Objects_bug596621() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug596621.vala") == 0);
		}
		[Test]
		public void Objects_bug597155() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug597155.vala") == 0);
		}
		[Test]
		public void Objects_bug597161() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug597161.vala") == 0);
		}
		[Test]
		public void Objects_bug613486() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug613486.vala") == 0);
		}
		[Test]
		public void Objects_bug613840() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug613840.vala") == 0);
		}
		[Test]
		public void Objects_bug620675() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug620675.vala") == 0);
		}
		[Test]
		public void Objects_bug620706() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug620706.vala") == 0);
		}
		[Test]
		public void Objects_bug624594() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug624594.vala") == 0);
		}
		[Test]
		public void Objects_bug626038() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug626038.vala") == 0);
		}
		[Test]
		public void Objects_bug628639() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug628639.vala") == 0);
		}
		[Test]
		public void Objects_bug631267() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug631267.vala") == 0);
		}
		[Test]
		public void Objects_bug634782() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug634782.vala") == 0);
		}
		[Test]
		public void Objects_bug641828() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug641828.vala") == 0);
		}
		[Test]
		public void Objects_bug642809() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug642809.vala") == 0);
		}
		[Test]
		public void Objects_bug643711() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug643711.vala") == 0);
		}
		[Test]
		public void Objects_bug644938() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug644938.vala") == 0);
		}
		[Test]
		public void Objects_bug646362() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug646362.vala") == 0);
		}
		[Test]
		public void Objects_bug646792() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug646792.vala") == 0);
		}
		[Test]
		public void Objects_bug647018() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug647018.vala") == 0);
		}
		[Test]
		public void Objects_bug653138() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug653138.vala") == 0);
		}
		[Test]
		public void Objects_bug654702() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug654702.vala") == 0);
		}
		[Test]
		public void Objects_bug663134() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug663134.vala") == 0);
		}
		[Test]
		public void Objects_bug664529() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug664529.vala") == 0);
		}
		[Test]
		public void Objects_bug667668() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug667668.vala") == 0);
		}
		[Test]
		public void Objects_bug681356() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug681356.vala") == 0);
		}
		[Test]
		public void Objects_bug683646() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug683646.vala") == 0);
		}
		[Test]
		public void Objects_bug695671() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug695671.vala") == 0);
		}
		[Test]
		public void Objects_bug701978() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug701978.vala") == 0);
		}
		[Test]
		public void Objects_bug702736() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug702736.vala") == 0);
		}
		[Test]
		public void Objects_bug702846() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug702846.vala") == 0);
		}
		[Test]
		public void Objects_bug731547() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug731547.vala") == 0);
		}
		[Test]
		public void Objects_bug751338() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug751338.vala") == 0);
		}
		[Test]
		public void Objects_bug758816() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug758816.vala") == 0);
		}
		[Test]
		public void Objects_bug764481() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug764481.vala") == 0);
		}
		[Test]
		public void Objects_bug766739() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug766739.vala") == 0);
		}
		[Test]
		public void Objects_bug778632() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug778632.vala") == 0);
		}
		[Test]
		public void Objects_bug779219() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug779219.vala") == 0);
		}
		[Test]
		public void Objects_bug779955() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug779955.vala") == 0);
		}
		[Test]
		public void Objects_bug783897() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/bug783897.vala") == 0);
		}
		[Test]
		public void Objects_chainup() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/chainup.vala") == 0);
		}
		[Test]
		public void Objects_classes() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/classes.vala") == 0);
		}
		[Test]
		public void Objects_constructors() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/constructors.vala") == 0);
		}
		[Test]
		public void Objects_fields() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/fields.vala") == 0);
		}
		[Test]
		public void Objects_generics() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/generics.vala") == 0);
		}
		[Test]
		public void Objects_interfaces() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/interfaces.vala") == 0);
		}
		[Test]
		public void Objects_methods() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/methods.vala") == 0);
		}
		[Test]
		public void Objects_properties() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/properties.vala") == 0);
		}
		[Test]
		public void Objects_regex() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/regex.vala") == 0);
		}
		[Test]
		public void Objects_signals() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/signals.vala") == 0);
		}
		[Test]
		public void Objects_test_025() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/test-025.vala") == 0);
		}
		[Test]
		public void Objects_test_026() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/test-026.vala") == 0);
		}
		[Test]
		public void Objects_test_029() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/test-029.vala") == 0);
		}
		[Test]
		public void Objects_test_034() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Objects/test-034.vala") == 0);
		}
		[Test]
		public void Pointers_bug590641() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Pointers/bug590641.vala") == 0);
		}
		[Test]
		public void Structs_bug530605() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug530605.vala") == 0);
		}
		[Test]
		public void Structs_bug572091() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug572091.vala") == 0);
		}
		[Test]
		public void Structs_bug583603() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug583603.vala") == 0);
		}
		[Test]
		public void Structs_bug595587() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug595587.vala") == 0);
		}
		[Test]
		public void Structs_bug596144() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug596144.vala") == 0);
		}
		[Test]
		public void Structs_bug603056() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug603056.vala") == 0);
		}
		[Test]
		public void Structs_bug606202() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug606202.vala") == 0);
		}
		[Test]
		public void Structs_bug609642() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug609642.vala") == 0);
		}
		[Test]
		public void Structs_bug613513() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug613513.vala") == 0);
		}
		[Test]
		public void Structs_bug613825() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug613825.vala") == 0);
		}
		[Test]
		public void Structs_bug621176() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug621176.vala") == 0);
		}
		[Test]
		public void Structs_bug622422() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug622422.vala") == 0);
		}
		[Test]
		public void Structs_bug623092() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug623092.vala") == 0);
		}
		[Test]
		public void Structs_bug651441() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug651441.vala") == 0);
		}
		[Test]
		public void Structs_bug654646() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug654646.vala") == 0);
		}
		[Test]
		public void Structs_bug654753() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug654753.vala") == 0);
		}
		[Test]
		public void Structs_bug656693() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug656693.vala") == 0);
		}
		[Test]
		public void Structs_bug657378() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug657378.vala") == 0);
		}
		[Test]
		public void Structs_bug658048() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug658048.vala") == 0);
		}
		[Test]
		public void Structs_bug660426() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug660426.vala") == 0);
		}
		[Test]
		public void Structs_bug661945() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug661945.vala") == 0);
		}
		[Test]
		public void Structs_bug667890() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug667890.vala") == 0);
		}
		[Test]
		public void Structs_bug669580() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug669580.vala") == 0);
		}
		[Test]
		public void Structs_bug685177() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug685177.vala") == 0);
		}
		[Test]
		public void Structs_bug686190() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug686190.vala") == 0);
		}
		[Test]
		public void Structs_bug690380() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug690380.vala") == 0);
		}
		[Test]
		public void Structs_bug694140() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug694140.vala") == 0);
		}
		[Test]
		public void Structs_bug749952() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug749952.vala") == 0);
		}
		[Test]
		public void Structs_bug775761() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug775761.vala") == 0);
		}
		[Test]
		public void Structs_bug777194() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/bug777194.vala") == 0);
		}
		[Test]
		public void Structs_gvalue() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/gvalue.vala") == 0);
		}
		[Test]
		public void Structs_structs() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/structs.vala") == 0);
		}
		[Test]
		public void Structs_struct_only() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("Structs/struct_only.vala") == 0);
		}
		[Test]
		public void ValaTests_test() {
			ValaTestRunner runner = new ValaTestRunner();
			Assert.IsTrue(runner.RunValaTest("ValaTests/test.vala") == 0);
		}
	}
}