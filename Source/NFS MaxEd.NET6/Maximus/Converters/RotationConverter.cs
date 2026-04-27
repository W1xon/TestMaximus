using System.Globalization;

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
    private const float HexRange = 65536f;
    private const float DegreeRange = 360f;

    public static float HexToDegrees(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return 0f;

        string cleanHex = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? hex[2..]
            : hex;

        if (!uint.TryParse(cleanHex,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out uint value))
        {
            throw new FormatException($"Invalid hex rotation value: {hex}");
        }

        double angle = (value / HexRange) * DegreeRange;

        return (float)Math.Round(angle, 4);
    }
    public static string DegreesToHex(float degrees)
    {
        double normalized = degrees % DegreeRange;
        if (normalized < 0)
            normalized += DegreeRange;

        uint value = (uint)Math.Round((normalized / DegreeRange) * HexRange);
        return $"0x{value:X4}";
    }
}