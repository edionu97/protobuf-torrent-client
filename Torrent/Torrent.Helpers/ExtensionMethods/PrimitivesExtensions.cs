using System;
using System.Text.RegularExpressions;

namespace Torrent.Helpers.ExtensionMethods
{
    public static class PrimitivesExtensions
    {
        /// <summary>
        /// Check if the regex is valid
        /// </summary>
        /// <param name="regex">the regex that we check against</param>
        /// <returns>true if the regex is valid or false otherwise</returns>
        public static bool IsRegexValid(this string regex)
        {
            try
            {
                var _ = new Regex(regex);
                return true;
            }
            catch (Exception _)
            {
                return false;
            }
        }
    }
}
