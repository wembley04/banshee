//
// StringUtil.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2006-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hyena
{    
    public static class StringUtil
    {
        private static CompareOptions compare_options = 
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace |
            CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

        public static int RelaxedIndexOf (string haystack, string needle)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf (haystack, needle, compare_options);
        }
        
        public static int RelaxedCompare (string a, string b)
        {
            if (a == null && b == null) {
                return 0;
            } else if (a != null && b == null) {
                return 1;
            } else if (a == null && b != null) {
                return -1;
            }
            
            int a_offset = a.StartsWith ("the ") ? 4 : 0;
            int b_offset = b.StartsWith ("the ") ? 4 : 0;

            return CultureInfo.CurrentCulture.CompareInfo.Compare (a, a_offset, a.Length - a_offset, 
                b, b_offset, b.Length - b_offset, compare_options);
        }
        
        public static string CamelCaseToUnderCase (string s)
        {
            return CamelCaseToUnderCase (s, '_');
        }
        
        private static Regex camelcase = new Regex ("([A-Z]{1}[a-z]+)", RegexOptions.Compiled);
        public static string CamelCaseToUnderCase (string s, char underscore)
        {
            if (String.IsNullOrEmpty (s)) {
                return null;
            }
        
            StringBuilder undercase = new StringBuilder ();
            string [] tokens = camelcase.Split (s);
            
            for (int i = 0; i < tokens.Length; i++) {
                if (tokens[i] == String.Empty) {
                    continue;
                }

                undercase.Append (tokens[i].ToLower (System.Globalization.CultureInfo.InvariantCulture));
                if (i < tokens.Length - 2) {
                    undercase.Append (underscore);
                }
            }
            
            return undercase.ToString ();
        }

        public static string UnderCaseToCamelCase (string s)
        {
            if (String.IsNullOrEmpty (s)) {
                return null;
            }

            StringBuilder builder = new StringBuilder ();

            for (int i = 0, n = s.Length, b = -1; i < n; i++) {
                if (b < 0 && s[i] != '_') {
                    builder.Append (Char.ToUpper (s[i]));
                    b = i;
                } else if (s[i] == '_' && i + 1 < n && s[i + 1] != '_') {
                    if (builder.Length > 0 && Char.IsUpper (builder[builder.Length - 1])) {
                        builder.Append (Char.ToLower (s[i + 1]));
                    } else {
                        builder.Append (Char.ToUpper (s[i + 1]));
                    }
                    i++;
                    b = i;
                } else if (s[i] != '_') {
                    builder.Append (Char.ToLower (s[i]));
                    b = i;
                }
            }

            return builder.ToString ();
        }

        public static string RemoveNewlines (string input)
        {
            if (input != null) {
                return input.Replace ("\r\n", String.Empty).Replace ("\n", String.Empty);
            }
            return null;
        }

        private static Regex tags = new Regex ("<[^>]+>", RegexOptions.Compiled | RegexOptions.Multiline);
        public static string RemoveHtml (string input)
        {
            if (input == null) {
                return input;
            }

            return tags.Replace (input, String.Empty);
        }

        public static string DoubleToTenthsPrecision (double num)
        {
            return DoubleToTenthsPrecision (num, false);
        }
        
        public static string DoubleToTenthsPrecision (double num, bool always_decimal)
        {
            num = Math.Round (num, 1, MidpointRounding.ToEven);
            return String.Format (
                !always_decimal && num == (int)num 
                    ? "{0:N0}" : "{0:N1}",
                num
            );
        }
        
        // This method helps us pluralize doubles. Probably a horrible i18n idea.
        public static int DoubleToPluralInt (double num)
        {
            if (num == (int)num)
                return (int)num;
            else
                return (int)num + 1;
        }
        
        // A mapping of non-Latin characters to be considered the same as
        // a Latin equivalent.
        private static Dictionary<char, char> BuildSpecialCases ()
        {
            Dictionary<char, char> dict = new Dictionary<char, char> ();
            dict['\u00f8'] = 'o';
            dict['\u0142'] = 'l';
            return dict;
        }
        private static Dictionary<char, char> ignored_special_cases = BuildSpecialCases ();
        
        //  Removes accents from Latin characters, and some kinds of punctuation.
        public static string SearchKey (string val)
        {
            if (String.IsNullOrEmpty (val)) {
                return val;
            }
            
            val = val.ToLower ();
            StringBuilder sb = new StringBuilder ();
            UnicodeCategory category;
            bool previous_was_latin = false;
            
            // Normalizing to KD splits into (base, combining) so we can check for Latin
            // characters and then strip off any NonSpacingMarks following them
            foreach (char c in val.Normalize (NormalizationForm.FormKD)) {
                category = Char.GetUnicodeCategory (c);

                if (ignored_special_cases.ContainsKey (c)) {
                    sb.Append (ignored_special_cases[c]);
                } else if (category == UnicodeCategory.OtherPunctuation) {
                    // Skip punctuation
                } else if (!(previous_was_latin && category == UnicodeCategory.NonSpacingMark)) {
                    sb.Append (c);
                }

                // Can ignore A-Z because we've already lowercased the char
                previous_was_latin = (c >= 'a' && c <= 'z');
            }
            
            return sb.ToString ().Normalize (NormalizationForm.FormKC);
        }
        
        private static Regex invalid_path_regex = BuildInvalidPathRegex ();

        private static Regex BuildInvalidPathRegex ()
        {
            char [] invalid_path_characters = new char [] {
                // Control characters: there's no reason to ever have one of these in a track name anyway,
                // and they're invalid in all Windows filesystems.
                '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
                '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
                '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
                '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F',
                
                // Invalid in FAT32 / NTFS: " \ / : * | ? < >
                // Invalid in HFS   :
                // Invalid in ext3  /
                '"', '\\', '/', ':', '*', '|', '?', '<', '>'
            };

            string regex_str = "[";
            for (int i = 0; i < invalid_path_characters.Length; i++) {
                regex_str += "\\" + invalid_path_characters[i];
            }
            regex_str += "]+";
            
            return new Regex (regex_str, RegexOptions.Compiled);
        }
        
        public static string EscapeFilename (string input)
        {
            if (input == null)
                return "";
            
            return invalid_path_regex.Replace (input, "_");
        }
    }
}
