using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrashMonkeyContentPipelineExtension {
    static class Utils {
        /// Dictionary extension method
        /// Taken from: http://stackoverflow.com/questions/16192906/net-dictionary-get-or-create-new
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new() {
            TValue val;

            if (!dict.TryGetValue(key, out val)) {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }
    }
}
