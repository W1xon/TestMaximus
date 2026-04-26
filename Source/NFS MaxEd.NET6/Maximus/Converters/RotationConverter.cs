namespace Maximus.Converters;

/// <summary>
/// Provides conversion between Hexadecimal direction values and degrees.
/// </summary>
/// <remarks>
/// Core conversion logic is based on the 'HexadecimalDirectionValueConverter' 
/// by LEE-YAMADARYO. Many thanks for the clear implementation.
/// Reference: https://github.com/LEE-YAMADARYO/HexadecimalDirectionValueConverter
/// </remarks>
public static class RotationConverter
{
    private const double HexRange = 65536.0;
    private const double DegreeRange = 360.0;

    public static float HexToDegrees(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return 0f;

        string cleanHex = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
            ? hex[2..] 
            : hex;

        if (!uint.TryParse(cleanHex, System.Globalization.NumberStyles.HexNumber, null, out uint value))
            throw new FormatException($"Invalid hex rotation value: {hex}");

        double angle = (value / HexRange) * DegreeRange;

        return (float)Math.Round(angle, 4, MidpointRounding.AwayFromZero);
    }

    public static string DegreesToHex(float degrees)
    {
        double normalizedDegrees = degrees % DegreeRange;
        if (normalizedDegrees < 0) normalizedDegrees += DegreeRange;

        uint value = (uint)Math.Round((normalizedDegrees * HexRange) / DegreeRange);

        value %= 65536; 

        return $"0x{value:X4}";
    }
}