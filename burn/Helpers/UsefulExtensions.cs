using System;

#if DEMOLAUNCHER
namespace DemoLauncher.Helpers {
#else
namespace Utils.Helpers {
#endif
    public static class UsefulExtensions {
        const string Tail = "===================================================";

        public static string SafeSubstring(this string s, int startIndex) {
            return startIndex >= s.Length ? string.Empty : s.Substring(startIndex);
        }
        public static string SafeSubstring(this string s, int startIndex, int length) {
            return startIndex >= s.Length ? string.Empty : s.Substring(startIndex, length);
        }
        public static string SafeRemove(this string s, int startIndex) {
            return startIndex >= s.Length ? s : s.Remove(startIndex);
        }
        public static string SafeRemove(this string s, int startIndex, int length) {
            return startIndex >= s.Length ? s : s.Remove(startIndex, length);
        }
        public static string ToStringEx(this Exception e) {
            string inner = e.InnerException == null ? string.Empty : e.InnerException.ToStringEx();
            string message = string.Empty;
            message += string.Format("Message:\n{0}\n{1}\n", e.Message, Tail);
            message += string.Format("Inner Exception:\n{0}\n{1}\n", inner, Tail);
            message += string.Format("Stack Trace:\n{0}{1}\n", e.StackTrace, Tail);
            return message;
        }
    }
}
