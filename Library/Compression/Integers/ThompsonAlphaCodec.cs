using System;

namespace InvertedTomato.Compression.Integers;

public class ThompsonAlphaCodec : ICodec
{
    private readonly Int32 LengthBits;

    public ThompsonAlphaCodec() : this(6)
    {
    }

    /// <summary>
    /// Instantiate with options
    /// </summary>
    /// <param name="lengthBits">Number of prefix bits used to store length.</param>
    public ThompsonAlphaCodec(Int32 lengthBits)
    {
        if (lengthBits is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException($"Must be between 1 and 6, not {lengthBits}.", nameof(lengthBits));
        }

        LengthBits = lengthBits;
    }

    private void Encode(UInt64 value, IBitWriter buffer)
    {
        // Offset value to allow zeros
        value++;

        // Count length
        var length = CountUsed(value);

        // Check not too large
        if (length > (LengthBits + 2) * 8)
        {
            throw new ArgumentOutOfRangeException($"Value is greater than maximum of {UInt64.MaxValue >> (64 - LengthBits - 1)}. Increase length bits to support larger numbers.");
        }

        // Clip MSB, it's redundant
        length--;

        // Write length
        buffer.WriteBits((UInt64) length, LengthBits);

        // Write number
        while (length > 0) // We may need to do this in multiple chunks if it exceeds the bit writers' capacity for a single operation
        {
            var load = Math.Min(length, buffer.MaxBits);
            length -= load;
            buffer.WriteBits(value >> (64 - load), load);
        }
    }

    private UInt64 Decode(IBitReader buffer)
    {
        // Read length
        var length = (Int32) buffer.ReadBits(LengthBits);

        // Read number
        UInt64 value = 0;
        while (length > 0) // We may need to do this in multiple chunks if it exceeds the bit writers' capacity for a single operation
        {
            var load = Math.Min(length, buffer.MaxBits);
            length -= load;
            value |= buffer.ReadBits(load) << (64 - load);
        }


        // Recover implied MSB
        value |= (UInt64) 1 << length;

        // Remove offset to allow zeros
        value--;

        return value;
    }

    public void EncodeBit(bool value, IBitWriter buffer) => Encode(1, buffer);
    public void EncodeUInt8(byte value, IBitWriter buffer) => Encode(value, buffer);
    public void EncodeUInt16(ushort value, IBitWriter buffer) => Encode(value, buffer);
    public void EncodeUInt32(uint value, IBitWriter buffer) => Encode(value, buffer);
    public void EncodeUInt64(ulong value, IBitWriter buffer) => Encode(value, buffer);
    public void EncodeInt8(sbyte value, IBitWriter buffer) => Encode(ZigZag.Encode(value), buffer);
    public void EncodeInt16(short value, IBitWriter buffer) => Encode(ZigZag.Encode(value), buffer);
    public void EncodeInt32(int value, IBitWriter buffer) => Encode(ZigZag.Encode(value), buffer);
    public void EncodeInt64(long value, IBitWriter buffer) => Encode(ZigZag.Encode(value), buffer);

    public Boolean DecodeBit(IBitReader buffer) => Decode(buffer) > 0;
    public Byte DecodeUInt8(IBitReader buffer) => (Byte) Decode(buffer);
    public UInt16 DecodeUInt16(IBitReader buffer) => (UInt16) Decode(buffer);
    public UInt32 DecodeUInt32(IBitReader buffer) => (UInt32) Decode(buffer);
    public UInt64 DecodeUInt64(IBitReader buffer) => Decode(buffer);
    public SByte DecodeInt8(IBitReader buffer) => (SByte) ZigZag.Decode(Decode(buffer));
    public Int16 DecodeInt16(IBitReader buffer) => (Int16) ZigZag.Decode(Decode(buffer));
    public Int32 DecodeInt32(IBitReader buffer) => (Int32) ZigZag.Decode(Decode(buffer));
    public Int64 DecodeInt64(IBitReader buffer) => ZigZag.Decode(Decode(buffer));

    /// <summary>
    ///     Count the number of bits used to express number
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static Int32 CountUsed(UInt64 value)
    {
        Byte bits = 0;

        do
        {
            bits++;
            value >>= 1;
        } while (value > 0);

        return bits;
    }
}