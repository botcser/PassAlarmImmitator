using Assets.Common;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Extensions
{
    public static class Extensions
    {
        public static bool IsAlive(this TcpClient socket)
        {
            return socket.Client != null && socket.Client.Connected && (!socket.Client.Poll(1000, SelectMode.SelectRead) || socket.Client.Available != 0);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static byte[] GetUTF8Bytes(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }

        public static string ToUTF8String(this byte[]? source)
        {
            return source == null ? "" : new UTF8Encoding(true, false).GetString(source, 0, source.Length);
        }
		
        public static T ToEnum<T>(this string value)
        {
            return (T) Enum.Parse(typeof(T), value);
        }

        public static void Shuffle<T>(this List<T> source)
        {
			var n = source.Count;

	        while (n > 1)
	        {
		        n--;

		        var k = CRandom.Next(n + 1);
		        var value = source[k];

		        source[k] = source[n];
		        source[n] = value;
	        }
		}

	    public static T Random<T>(this T[] source)
	    {
		    return source[CRandom.Next(source.Length)];
	    }

		public static T Random<T>(this List<T> source)
        {
            var x = CRandom.Next(source.Count);

			return source[x];
		}

        public static T Random<T>(this List<T> source, int seed)
	    {
		    return source[new Random(seed).Next(source.Count)];
	    }

		public static bool IsEmpty(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

	    public static List<T> Pop<T>(this List<T> source, T item)
	    {
			source.Add(item);

			return source;
	    }

        public static T JsonClone<T>(this T source)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((source)));
        }

	    public static string ToRoman(this int number)
	    {
		    if (number < 0 || number > 3999) throw new ArgumentOutOfRangeException("Insert value betwheen 1 and 3999.");
		    if (number < 1) return string.Empty;
		    if (number >= 1000) return "M" + ToRoman(number - 1000);
		    if (number >= 900) return "CM" + ToRoman(number - 900);
		    if (number >= 500) return "D" + ToRoman(number - 500);
		    if (number >= 400) return "CD" + ToRoman(number - 400);
		    if (number >= 100) return "C" + ToRoman(number - 100);
		    if (number >= 90) return "XC" + ToRoman(number - 90);
		    if (number >= 50) return "L" + ToRoman(number - 50);
		    if (number >= 40) return "XL" + ToRoman(number - 40);
		    if (number >= 10) return "X" + ToRoman(number - 10);
		    if (number >= 9) return "IX" + ToRoman(number - 9);
		    if (number >= 5) return "V" + ToRoman(number - 5);
		    if (number >= 4) return "IV" + ToRoman(number - 4);
		    if (number >= 1) return "I" + ToRoman(number - 1);

		    throw new ArgumentOutOfRangeException("Something bad happened.");
	    }
	}
}