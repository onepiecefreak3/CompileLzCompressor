using Kompression.Contract.Decoder;
using Kompression.IO;

namespace CompileLzCompressor;

internal class CompileLzDecoder : IDecoder
{
    public void Decode(Stream input, Stream output)
    {
        var circularBuffer = new CircularBuffer(0x100)
        {
            Position = 0xFF
        };

        var buffer = new byte[0x82];

        EnsureContent(input);
        int flag = input.ReadByte();

        while (flag > 0)
        {
            if (flag < 0x80)
            {
                _ = input.Read(buffer, 0, flag);

                circularBuffer.Write(buffer, 0, flag);
                output.Write(buffer, 0, flag);
            }
            else
            {
                EnsureContent(input);

                int flag2 = input.ReadByte();

                int length = (flag ^ 0x80) + 3;
                int displacement = flag2 + 1;

                circularBuffer.Copy(output, displacement, length);
            }

            EnsureContent(input);
            flag = input.ReadByte();
        }
    }

    public void Dispose() { }

    private static void EnsureContent(Stream input)
    {
        if (input.Position >= input.Length)
            throw new InvalidOperationException("Not enough data in file.");
    }
}