﻿using InvertedTomato.IO.Buffers;
using InvertedTomato.IO.Bits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedTomato.Compression.Integers {
    [TestClass]
    public class VLQCodecTests {
        public string CompressMany(ulong[] set, int outputBufferSize = 8) {
            var input = new Buffer<ulong>(set);
            var output = new Buffer<byte>(outputBufferSize);
            var codec = new VLQCodec();
            Assert.IsTrue( codec.Compress(input, output));

            return output.ToArray().ToBinaryString();
        }
        public string CompressOne(ulong value, int outputBufferSize = 8) {
            var output = new Buffer<byte>(outputBufferSize);
            var codec = new VLQCodec();
            codec.Compress(value, output);

            return output.ToArray().ToBinaryString();
        }

        [TestMethod]
        public void Compress_Empty() {
            Assert.AreEqual("", CompressMany(new ulong[] { }));
        }
        [TestMethod]
        public void Compress_0() {
            Assert.AreEqual("10000000", CompressOne(0));
        }
        [TestMethod]
        public void Compress_1() {
            Assert.AreEqual("10000001", CompressOne(1));
        }
        [TestMethod]
        public void Compress_2() {
            Assert.AreEqual("10000010", CompressOne(2));
        }
        [TestMethod]
        public void Compress_3() {
            Assert.AreEqual("10000011", CompressOne(3));
        }
        [TestMethod]
        public void Compress_127() {
            Assert.AreEqual("11111111", CompressOne(127));
        }
        [TestMethod]
        public void Compress_128() {
            Assert.AreEqual("00000000 10000000", CompressOne(128));
        }
        [TestMethod]
        public void Compress_129() {
            Assert.AreEqual("00000001 10000000", CompressOne(129));
        }
        [TestMethod]
        public void Compress_16511() {
            Assert.AreEqual("01111111 11111111", CompressOne(16511));
        }
        [TestMethod]
        public void Compress_16512() {
            Assert.AreEqual("00000000 00000000 10000000", CompressOne(16512));
        }
        [TestMethod]
        public void Compress_2113663() {
            Assert.AreEqual("01111111 01111111 11111111", CompressOne(2113663));
        }
        [TestMethod]
        public void Compress_2113664() {
            Assert.AreEqual("00000000 00000000 00000000 10000000", CompressOne(2113664));
        }
        [TestMethod]
        public void Compress_Max() {
            Assert.AreEqual("01111111 01111110 01111110 01111110 01111110 01111110 01111110 01111110 01111110 10000000", CompressOne(ulong.MaxValue, 32));
        }
        [TestMethod]
        public void Compress_1_1_1() {
            Assert.AreEqual("10000001 10000001 10000001", CompressMany(new ulong[] { 1, 1, 1 }));
        }
        [TestMethod]
        public void Compress_128_128_128() {
            Assert.AreEqual("00000000 10000000 00000000 10000000 00000000 10000000", CompressMany(new ulong[] { 128, 128, 128 }));
        }
        /// <summary>
        /// When the output buffer gets full before all of the input is consumed.
        /// </summary>
        [TestMethod]
        public void Compress_InsufficentOutput() {
            var input = new Buffer<ulong>(new ulong[] { 128 });
            var output = new Buffer<byte>(1);
            var codec = new VLQCodec();
            Assert.IsFalse(codec.Compress(input, output));
            Assert.AreEqual("", output.ToArray().ToBinaryString());
            Assert.AreEqual(0, input.Start);
            Assert.AreEqual(1, input.End);
        }
        /// <summary>
        /// When there is exactly enough space in the output buffer for all the input.
        /// </summary>
        [TestMethod]
        public void Compress_PerfectOutput() {
            var input = new Buffer<ulong>(new ulong[] { 128 });
            var output = new Buffer<byte>(2);
            var codec = new VLQCodec();
            Assert.IsTrue(codec.Compress(input, output));
            Assert.AreEqual("00000000 10000000", output.ToArray().ToBinaryString());
            Assert.AreEqual(1, input.Start);
            Assert.AreEqual(1, input.End);
        }



        private ulong[] DecompressMany(string value, int count) {
            var input = new Buffer<byte>(BitOperation.ParseToBytes(value));
            var output = new Buffer<ulong>(count);
            var codec = new VLQCodec();
            Assert.IsTrue(codec.Decompress(input, output));

            return output.ToArray();
        }
        private ulong DecompressOne(string value) {
            var input = new Buffer<byte>(BitOperation.ParseToBytes(value));
            var codec = new VLQCodec();
            return codec.Decompress(input);
        }

        [TestMethod]
        public void Decompress_Empty() {
            Assert.AreEqual(0, DecompressMany("", 0).Length);
        }
        [TestMethod]
        public void Decompress_0() {
            Assert.AreEqual((ulong)0, DecompressOne("10000000"));
        }
        [TestMethod]
        public void Decompress_1() {
            Assert.AreEqual((ulong)1, DecompressOne("10000001"));
        }
        [TestMethod]
        public void Decompress_2() {
            Assert.AreEqual((ulong)2, DecompressOne("10000010"));
        }
        [TestMethod]
        public void Decompress_3() {
            Assert.AreEqual((ulong)3, DecompressOne("10000011"));
        }
        [TestMethod]
        public void Decompress_127() {
            Assert.AreEqual((ulong)127, DecompressOne("11111111"));
        }
        [TestMethod]
        public void Decompress_128() {
            Assert.AreEqual((ulong)128, DecompressOne("00000000 10000000"));
        }
        [TestMethod]
        public void Decompress_129() {
            Assert.AreEqual((ulong)129, DecompressOne("00000001 10000000"));
        }
        [TestMethod]
        public void Decompress_16511() {
            Assert.AreEqual((ulong)16511, DecompressOne("01111111 11111111"));
        }
        [TestMethod]
        public void Decompress_16512() {
            Assert.AreEqual((ulong)16512, DecompressOne("00000000 00000000 10000000"));
        }
        [TestMethod]
        public void Decompress_16513() {
            Assert.AreEqual((ulong)16513, DecompressOne("00000001 00000000 10000000"));
        }
        [TestMethod]
        public void Decompress_2113663() {
            Assert.AreEqual((ulong)2113663, DecompressOne("01111111 01111111 11111111"));
        }
        [TestMethod]
        public void Decompress_2113664() {
            Assert.AreEqual((ulong)2113664, DecompressOne("00000000 00000000 00000000 10000000"));
        }
        [TestMethod]
        public void Decompress_Max() {
            Assert.AreEqual(ulong.MaxValue, DecompressOne("01111111 01111110 01111110 01111110 01111110 01111110 01111110 01111110 01111110 10000000"));
        }
        [TestMethod]
        public void Decompress_1_1_1() {
            var set = DecompressMany("10000001 10000001 10000001", 3);
            Assert.AreEqual(3, set.Length);
            Assert.AreEqual((ulong)1, set[0]);
            Assert.AreEqual((ulong)1, set[1]);
            Assert.AreEqual((ulong)1, set[2]);
        }
        /// <summary>
        /// When the output buffer gets full before all of the input is consumed.
        /// </summary>
        [TestMethod]
        public void Decompress_InsufficentInput() {
            var input = new Buffer<byte>(BitOperation.ParseToBytes("00000000 10000000"));
            var output = new Buffer<ulong>(2);
            var codec = new VLQCodec();
            Assert.IsFalse(codec.Decompress(input, output));
            Assert.AreEqual(1, output.Used);
            Assert.AreEqual((ulong)128, output.Dequeue());
        }
        /// <summary>
        /// When there is exactly enough space in the output buffer for all the input.
        /// </summary>
        [TestMethod]
        public void Decompress_OutputPerfect() {
            var input = new Buffer<byte>(BitOperation.ParseToBytes("00000000 10000000"));
            var output = new Buffer<ulong>(1);
            var codec = new VLQCodec();
            Assert.IsTrue(codec.Decompress(input, output));
            Assert.AreEqual(1, output.Used);
            Assert.AreEqual((ulong)128, output.Dequeue());
        }
        [TestMethod]
        public void Decompress_InsufficentBytes() {
            var input = new Buffer<byte>(BitOperation.ParseToBytes("00000000"));
            var output = new Buffer<ulong>(1);
            var codec = new VLQCodec();
            Assert.IsFalse( codec.Decompress(input, output));
            Assert.AreEqual(0, output.Used);
        }
        [TestMethod]
        public void Decompress_UnneededBytes() {
            Assert.AreEqual((ulong)1, DecompressOne("10000001 10000001 10000000")); // Ignore trailing bytes
        }




        [TestMethod]
        public void CompressDecompress_First1000_Series() {
            for (ulong input = 0; input < 1000; input++) {
                var encoded = CompressOne(input);
                var output = DecompressOne(encoded);

                Assert.AreEqual(input, output);
            }
        }
        [TestMethod]
        public void CompressDecompress_First1000_Parallel() {
            var max = 1000;

            var codec = new VLQCodec();

            // Create input
            var input = new Buffer<ulong>(max);
            for (ulong i = 0; i < (ulong)input.MaxCapacity; i++) {
                input.Enqueue(i);
            }

            // Compress in chunks
            var compressed = new Buffer<byte>(128);
            var cycles = 0;
            while (input.Used > 0 && cycles++ < 10) {
                var count = codec.Compress(input, compressed);
                compressed = compressed.Resize(compressed.Used * 2);
            }
            Assert.IsTrue(cycles < 10, "Aborted - was going to loop forever");

            // Decompress in chunks
            var decompressed = new Buffer<ulong>(100);
            cycles = 0;
            ulong index = 0;
            while (index < (ulong)max &&
                cycles++ < 10) {
                codec.Decompress(compressed, decompressed);

                while (decompressed.Used > 0) {
                    var a = decompressed.Dequeue();
                    Assert.AreEqual(index++, a);
                }

                decompressed = decompressed.Resize(decompressed.MaxCapacity * 2);
            }

            Assert.IsTrue(cycles < 10, "Aborted - was going to loop forever");
        }
    }
}
