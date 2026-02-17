using System.Security.Cryptography;
using System.Text;

namespace Msh.Api.Domain.Core;


public static class StringExtensions
{
    private static readonly Dictionary<char, char> _map;

    static StringExtensions()
    {
        var withAccents = "_'!@#$%¨&+,*()?:;={}][/ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç /.";
        var withoutAccents = "-----------------------AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc---";

        _map = [];
        for (var i = 0; i < withAccents.Length; i++)
        {
            // Prevents error if the origin string has repeated characters
            _map.TryAdd(withAccents[i], withoutAccents[i]);
        }
    }

    public static string Sanitize(string input, bool toLower = true)
    {
        if (string.IsNullOrEmpty(input)) return input;

        if (toLower) input = input.ToLower();

        // We create a buffer on the stack (fast memory) to avoid allocations if the string is short
        // If it's long, we use a regular array
        var buffer = input.ToCharArray();

        for (var i = 0; i < buffer.Length; i++)
        {
            if (_map.TryGetValue(buffer[i], out char substitute))
            {
                buffer[i] = substitute;
            }
        }

        return new string(buffer);
    }

    public static string GenerateCacheKey(string key)
    {
        // Converte a string para bytes
        var inputBytes = Encoding.UTF8.GetBytes(key);

        // Gera o Hash de forma thread-safe sem instanciar objeto
        var hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}