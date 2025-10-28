using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Casualbunker.Server.Common
{
	public static class JsonGeneric
	{
		#if UNITY_EDITOR || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS

		public static string ToJson(object obj)
		{
			return UnityEngine.JsonUtility.ToJson(obj);
		}

		public static T FromJson<T>(string json)
		{
			return UnityEngine.JsonUtility.FromJson<T>(json);
		}

		#else

		public static string ToJson(object obj, bool minimize = true)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

			return minimize ? Minimize(json) : json;
		}

        public static string ToJsonNullIgnore(object obj, bool minimize = true)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return minimize ? Minimize(json) : json;
        }

        public static T FromJson<T>(string json)
        {
            return json == null ? default : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
		}

		#endif

		public static List<T> FromJsonList<T>(string json)
		{
			var wrapper = FromJson<Wrapper<T>>("{\"Items\":" + json + "}");

			return wrapper.Items;
		}

		public static string ToJson<T>(List<T> array, bool minimize = true)
		{
			if (array == null) throw new ArgumentException(nameof(array));

			var wrapper = new Wrapper<T> { Items = array };
			var json = ToJson(wrapper);

			json = json.Substring(9, json.Length - 9 - 1);

			return minimize ? Minimize(json) : json;
		}

		[Serializable]
		private class Wrapper<T>
		{
			public List<T> Items;
		}

		public static string Minimize(string json)
		{
			if (json == null) return json;

			json = Regex.Replace(json, "\"\\w+\":\\[\\],", "");
			json = Regex.Replace(json, ",\"\\w+\":\\[\\]", "");

			return json;
		}
	}
}