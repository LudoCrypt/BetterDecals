using System.Reflection;
using System.Runtime.CompilerServices;

namespace BetterDecals2 {
    internal abstract class ExtraDataClass<T, C> where T : class where C : class {
        private static readonly ConditionalWeakTable<T, C> weakData = new ConditionalWeakTable<T, C>();

        public static C GetData(T obj) {
            return weakData.GetOrCreateValue(obj);
        }

    }

    internal class FieldHelper {
        public static T GetField<T>(object obj, string name) {
            FieldInfo thefield = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (thefield != null) {
                if (thefield.GetValue(obj) is T t) {
                    return t;
                }
            }
            return default;
        }
        public static void SetField<T>(object obj, string name, T value) {
            FieldInfo theField = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (theField != null && theField.FieldType == typeof(T)) {
                theField.SetValue(obj, value);
            }
        }

    }
}
