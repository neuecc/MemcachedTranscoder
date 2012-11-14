using System;
using System.Text.RegularExpressions;

namespace MemcachedTranscoder
{
    internal static class TypeHelper
    {
        static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);

        // see:http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx
        // subtract Version, Culture and PublicKeyToken from AssemblyQualifiedName 
        internal static string BuildTypeName(Type type)
        {
            return SubtractFullNameRegex.Replace(type.AssemblyQualifiedName, "");
        }
    }
}