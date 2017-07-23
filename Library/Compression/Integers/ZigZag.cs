﻿using System;

namespace InvertedTomato.Compression.Integers {
    /// <summary>
    /// Encode signed values as unsigned using ProtoBuffer ZigZag bijection encoding algorithm. https://developers.google.com/protocol-buffers/docs/encoding
    /// </summary>
    public static class ZigZag {
        /// <summary>
        /// Encode a signed long into an ZigZag unsigned long.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong Encode(long value) {
            return (ulong)((value << 1) ^ (value >> 63));
        }

        /// <summary>
        /// Encode an array of signed longs into a ZigZag encoded array of unsigned longs.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ulong[] Encode(long[] values) {
            var output = new ulong[values.Length];
            for (var i = 0; i < values.Length; i++) {
                output[i] = ZigZag.Encode(values[i]);
            }
            return output;
        }

        /// <summary>
        /// Decode a ZigZag unsigned long back into a signed long. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Decode(ulong value) {
            var casted = (long)value;
            return (casted >> 1) ^ (-(casted & 1));
        }

        // <summary>
        /// Decode an array of unsigned longs into a ZigZag encoded array of signed longs.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static long[] Decode(ulong[] values) {
            var output = new long[values.Length];
            for (var i = 0; i < values.Length; i++) {
                output[i] = ZigZag.Decode(values[i]);
            }
            return output;
        }
    }
}
