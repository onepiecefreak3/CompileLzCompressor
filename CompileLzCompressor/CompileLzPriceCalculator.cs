using Kompression.Contract.Encoder.LempelZiv.PriceCalculator;

namespace CompileLzCompressor;

internal class CompileLzPriceCalculator : ILempelZivPriceCalculator
{
    public int CalculateLiteralPrice(int value, int literalRunLength, bool firstLiteralRun)
    {
        int runs = literalRunLength / 0x7F;
        if (literalRunLength % 0x7F != 0)
            runs += 1;

        return runs * 8 + literalRunLength * 8;
    }

    public int CalculateMatchPrice(int displacement, int length, int matchRunLength, int firstValue)
    {
        return 16;
    }
}