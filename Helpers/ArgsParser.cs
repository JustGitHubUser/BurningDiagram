using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.Helpers {
    public class ArgOption {
        class StringEqualityComparer : IEqualityComparer<string> {
            public bool Equals(string x, string y) {
                return string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
            }
            public int GetHashCode(string obj) {
                return obj.ToLower().GetHashCode();
            }
        }
        Action<string> action;
        Dictionary<string, bool> keys = new Dictionary<string, bool>(new StringEqualityComparer());

        public ArgOption(string keys, bool allowMultiple, Action<string> action, string help) {
            SetKeys(keys);
            this.action = action;
            AllowMultiple = allowMultiple;
            Help = help;
        }
        public bool IsParameter { get; private set; }
        public bool AllowMultiple { get; private set; }
        public ReadOnlyDictionary<string, bool> Keys { get { return new ReadOnlyDictionary<string, bool>(keys); } }
        public string Help { get; private set; }
        public void DoAction(string v) {
            if(this.action != null)
                this.action(v);
        }
        void SetKeys(string keysString) {
            IsParameter = keysString[keysString.Length - 1] == '=';
            if(IsParameter)
                keysString = keysString.Remove(keysString.Length - 1);
            string[] parts = keysString.Split('|');
            foreach(string part in parts) {
                keys.Add(part, true);
            }
        }
    }
    public class ArgTarget {
        Func<string, bool> action;

        public ArgTarget(bool optional, Func<string, bool> action) {
            Optional = optional;
            this.action = action;
        }
        public bool Optional { get; private set; }
        public bool DoAction(string v) {
            return this.action == null ? true : this.action(v);
        }
    }
    public class ArgsParser {
        const char OptionChar = '/';
        string command;
        string targetsHelp;
        List<ArgOption> options = new List<ArgOption>();
        List<ArgTarget> targets = new List<ArgTarget>();

        public ArgsParser(string command, string targetsHelp) {
            this.command = command;
            this.targetsHelp = targetsHelp;
        }
        public void AddOption(ArgOption option) {
            this.options.Add(option);
        }
        public void AddTarget(ArgTarget target) {
            this.targets.Add(target);
        }
        public bool Parse(string[] args, bool includeFirst) {
            List<string> targets = new List<string>();
            bool onlyTargets = false;
            int targetIndex = 0;
            Dictionary<object, bool> usedOptions = new Dictionary<object, bool>();
            bool first = true;
            foreach(string arg in args) {
                if(first) {
                    first = false;
                    if(!includeFirst) continue;
                }
                if(!ParseArg(arg, ref onlyTargets, ref targetIndex, ref usedOptions)) return false;
            }
            foreach(ArgTarget target in this.targets) {
                if(!target.Optional && !usedOptions.ContainsKey(target))
                    return false;
            }
            return true;
        }
        bool ParseArg(string arg, ref bool onlyTargets, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            if(!PrepareArg(ref arg)) return false;
            if(string.IsNullOrEmpty(arg)) return false;
            return onlyTargets || arg[0] != OptionChar ? ParseTarget(arg, ref onlyTargets, ref targetIndex, ref usedOptions) : ParseKey(arg, ref onlyTargets, ref targetIndex, ref usedOptions);
        }
        bool ParseKeyValue(string arg, ref bool onlyTargets, ref ArgOption nextKey, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            nextKey.DoAction(arg);
            nextKey = null;
            return true;
        }
        bool ParseKey(string arg, ref bool onlyTargets, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            if(arg.Length == 1) return false;
            return arg[1] == OptionChar ? ParseOnlyTargets(arg, ref onlyTargets, ref targetIndex, ref usedOptions) : ParseKeyCore(arg, ref onlyTargets, ref targetIndex, ref usedOptions);
        }
        bool ParseOnlyTargets(string arg, ref bool onlyTargets, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            if(arg.Length != 2) return false;
            onlyTargets = true;
            return true;
        }
        bool ParseKeyCore(string arg, ref bool onlyTargets, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            string v;
            ArgOption option = FindOption(arg.Substring(1), out v, ref usedOptions);
            if(option == null) return false;
            if(!option.IsParameter && v != null) return false;
            if(option.IsParameter && v == null) return false;
            option.DoAction(v);
            return true;
        }
        bool ParseTarget(string arg, ref bool onlyTargets, ref int targetIndex, ref Dictionary<object, bool> usedOptions) {
            ArgTarget target = FindTarget(targetIndex, ref usedOptions);
            if(target == null) return false;
            bool useNextTarget = target.DoAction(arg);
            if(useNextTarget)
                ++targetIndex;
            return true;
        }
        ArgOption FindOption(string arg, out string v, ref Dictionary<object, bool> usedOptions) {
            int i = arg.IndexOfAny(new char[] { '=', ':' });
            v = i < 0 ? null : arg.SafeSubstring(i + 1);
            string keyName = i < 0 ? arg : arg.SafeRemove(i);
            ArgOption op = null;
            foreach(ArgOption option in this.options) {
                if(option.Keys.ContainsKey(keyName)) {
                    op = option;
                    break;
                }
            }
            if(op == null) return null;
            if(!op.AllowMultiple && usedOptions.ContainsKey(op)) return null;
            if(!usedOptions.ContainsKey(op))
                usedOptions.Add(op, true);
            return op;
        }
        ArgTarget FindTarget(int targetIndex, ref Dictionary<object, bool> usedOptions) {
            ArgTarget target = targetIndex >= this.targets.Count ? null : this.targets[targetIndex];
            if(target == null) return null;
            if(!usedOptions.ContainsKey(target))
                usedOptions.Add(target, true);
            return target;
        }
        bool PrepareArg(ref string arg) {
            StringBuilder ret = new StringBuilder();
            bool skip = false;
            bool insideQuotes = false;
            for(int i = 0; i < arg.Length; ++i) {
                if(skip || arg[i] != '\"') {
                    skip = false;
                    ret.Append(arg[i]);
                    continue;
                }
                if(insideQuotes) {
                    if(i + 1 < arg.Length && arg[i + 1] == '\"') {
                        skip = true;
                    } else {
                        insideQuotes = false;
                    }
                } else {
                    insideQuotes = true;
                }
            }
            if(insideQuotes) return false;
            arg = ret.ToString();
            return true;
        }
        public string GetUsage() {
            StringBuilder usage = new StringBuilder();
            usage.AppendLine(string.Format("Usage: {0} [options] {1}", command, targetsHelp));
            usage.AppendLine("Options:");
            foreach(ArgOption option in this.options) {
                usage.Append('\t');
                usage.AppendLine(GetOptionUsage(option));
            }
            bool first = true;
            foreach(ArgOption option in this.options) {
                if(!option.AllowMultiple) continue;
                if(first) {
                    usage.Append("Option(s) ");
                    first = false;
                } else {
                    usage.Append(" ");
                }
                usage.Append(GetOptionString(option));
            }
            if(!first)
                usage.AppendLine(" can be used multiple times.");
            return usage.ToString();
        }
        string GetOptionUsage(ArgOption option) {
            StringBuilder usage = new StringBuilder();
            bool first = true;
            foreach(string key in option.Keys.Keys) {
                if(!first)
                    usage.Append(" | ");
                first = false;
                usage.Append(OptionChar);
                usage.Append(key);
                if(option.IsParameter)
                    usage.Append("=<value>");
            }
            if(!string.IsNullOrEmpty(option.Help))
                usage.Append(" - " + option.Help);
            return usage.ToString();
        }
        string GetOptionString(ArgOption option) {
            return OptionChar.ToString() + option.Keys.Keys.First();
        }
    }
}
