using Newtonsoft.Json;

namespace Todododo.Helpers
{
    public static class Extensions
    {
        public static T DeepClone<T>(this T data) =>
            JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
    }
}