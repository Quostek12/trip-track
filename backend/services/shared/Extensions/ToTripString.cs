namespace Shared.Extensions;

public static class StringExtensions
{
    public static string ToTripString(this string input, string number)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        number = number.Length switch
        {
            <3 => number.PadLeft(3, '0'),
            _  => number
        };

        return "TRP/"+input.Replace('-','/')+"/"+number;
    }
}