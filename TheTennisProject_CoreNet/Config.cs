using Microsoft.Extensions.Configuration;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Configuration management.
    /// </summary>
    public static class Config
    {
        private static IConfiguration _inner;

        private const string MAIn_APP_KEY = "application";

        /// <summary>
        /// Initialize the configuration.
        /// </summary>
        /// <param name="inner">The inner <see cref="IConfiguration"/>.</param>
        public static void Initialize(IConfiguration inner)
        {
            _inner = inner;
        }

        /// <summary>
        /// Gets a <seealso cref="string"/> config value.
        /// </summary>
        /// <param name="key">The config key.</param>
        /// <returns>The string value.</returns>
        public static string GetString(AppKey key)
        {
            return _inner[string.Format("{0}:{1}", MAIn_APP_KEY, key)];
        }

        /// <summary>
        /// Gets a <seealso cref="bool"/> config value.
        /// </summary>
        /// <param name="key">The config key.</param>
        /// <returns>The boolean value.</returns>
        public static bool GetBool(AppKey key)
        {
            return _inner[string.Format("{0}:{1}", MAIn_APP_KEY, key)].ToLowerInvariant().Equals(bool.TrueString.ToLowerInvariant());
        }

        /// <summary>
        /// Gets a <seealso cref="int"/> config value.
        /// </summary>
        /// <param name="key">The config key.</param>
        /// <returns>The int value.</returns>
        public static int GetInt32(AppKey key)
        {
            int intValue = 0;
            int.TryParse(_inner[string.Format("{0}:{1}", MAIn_APP_KEY, key)], out intValue);
            return intValue;
        }
    }
}
