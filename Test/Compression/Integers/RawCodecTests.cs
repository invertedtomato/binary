﻿using System;
using System.IO;
using Xunit;

namespace InvertedTomato.Compression.Integers
{
    public class RawCodecTests
    {
        private Byte[] Encode(UInt64 value)
        {
            var codec = new RawCodec();
            using var stream = new MemoryStream();
            using (var writer = new StreamBitWriter(stream))
            {
                codec.EncodeUInt64(value, writer);
            }

            return stream.ToArray();
        }

        private UInt64 Decode(Byte[] encoded)
        {
            var codec = new RawCodec();
            using var stream = new MemoryStream(encoded);
            using var reader = new StreamBitReader(stream);

            return codec.DecodeUInt64(reader);
        }

        [Fact]
        public void Encode_0()
        {
            Assert.Equal(new Byte[] {0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, Encode(0));
        }

        [Fact]
        public void Encode_1()
        {
            Assert.Equal(new Byte[] {0b00000001, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, Encode(1));
        }

        [Fact]
        public void Encode_2()
        {
            Assert.Equal(new Byte[] {0b00000010, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, Encode(2));
        }

        [Fact]
        public void Encode_3()
        {
            Assert.Equal(new Byte[] {0b00000011, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, Encode(3));
        }

        [Fact]
        public void Encode_Max()
        {
            Assert.Equal(new Byte[] {0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111}, Encode(UInt64.MaxValue));
        }


        [Fact]
        public void Decode_0()
        {
            Assert.Equal((UInt64) 0, Decode(new Byte[] {0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}));
        }

        [Fact]
        public void Decode_1()
        {
            Assert.Equal((UInt64) 1, Decode(new Byte[] {0b00000001, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}));
        }

        [Fact]
        public void Decode_2()
        {
            Assert.Equal((UInt64) 2, Decode(new Byte[] {0b00000010, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}));
        }

        [Fact]
        public void Decode_3()
        {
            Assert.Equal((UInt64) 3, Decode(new Byte[] {0b00000011, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}));
        }

        [Fact]
        public void Decode_Max()
        {
            Assert.Equal(new RawCodec().MaxValue, Decode(new Byte[] {0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111}));
        }

        [Fact]
        public void EncodeDecode_1000()
        {
            var ta = new RawCodec();
            using var stream = new MemoryStream();

            // Encode
            using (var writer = new StreamBitWriter(stream))
            {
                for (UInt64 symbol = 0; symbol < 1000; symbol++)
                {
                    ta.EncodeUInt64(symbol, writer);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);

            // Decode
            using (var reader = new StreamBitReader(stream))
            {
                for (UInt64 symbol = 0; symbol < 1000; symbol++)
                {
                    Assert.Equal(symbol, ta.DecodeUInt64(reader));
                }
            }
        }
    }
}