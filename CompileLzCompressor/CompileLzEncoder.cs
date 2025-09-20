using Kompression.Contract.Configuration;
using Kompression.Contract.DataClasses.Encoder.LempelZiv;
using Kompression.Contract.Encoder;

namespace CompileLzCompressor;

internal class CompileLzEncoder : ILempelZivEncoder
{
    private static readonly byte[] Buffer = new byte[0x7F];

    public void Configure(ILempelZivEncoderOptionsBuilder lempelZivOptions)
    {
        lempelZivOptions
            .AdjustInput(builder => builder.Prepend(0x100))
            .CalculatePricesWith(() => new CompileLzPriceCalculator())
            .FindPatternMatches().WithinLimitations(0x3, 0x82, 1, 0x100);
    }

    public void Encode(Stream input, Stream output, IEnumerable<LempelZivMatch> matches)
    {
        foreach (LempelZivMatch match in matches)
        {
            if (input.Position < match.Position)
            {
                long rawLength = match.Position - input.Position;
                WriteRaw(input, output, rawLength);
            }

            output.WriteByte((byte)(match.Length - 3 + 0x80));
            output.WriteByte((byte)(match.Displacement - 1));

            input.Position += match.Length;
        }

        if (input.Position < input.Length)
        {
            long rawLength = input.Length - input.Position;
            WriteRaw(input, output, rawLength);
        }

        output.WriteByte(0);
    }

    private static void WriteRaw(Stream input, Stream output, long rawLength)
    {
        while (rawLength > 0)
        {
            var rawRunLength = (int)Math.Min(0x7F, rawLength);
            _ = input.Read(Buffer, 0, rawRunLength);

            output.WriteByte((byte)(rawRunLength));
            output.Write(Buffer, 0, rawRunLength);

            rawLength -= rawRunLength;
        }
    }
}