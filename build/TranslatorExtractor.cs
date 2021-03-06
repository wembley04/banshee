//
// Licensed under same license as Banshee
// Copyright (C) 2006 Novell, Inc.
// Written by Aaron Bockover <abock@gnome.org>
//
// This tool extracts translator information from .po files
// to generate information for display in the about dialog
//

using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TranslatorExtractor
{
    public static void Main(string [] args)
    {
        Console.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>"); 
        Console.WriteLine("<!-- AUTOGENERATED - DO NOT EDIT -->");
        Console.WriteLine("<translators>");
        foreach(string filename in Directory.GetFiles(args.Length == 0 ? "." : args[0], "*.po")) {
            using(FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                using(StreamReader reader = new StreamReader(stream)) {
                    ParseFile(filename, reader);
                }
            }
        }
        Console.WriteLine("</translators>");
    }

    private static void ParseFile(string file, TextReader reader)
    {
        Dictionary<string, string> names = new Dictionary<string, string>();
        string language_code = Path.GetFileNameWithoutExtension(file);
        string language_name = null;
    
        bool past_plural_forms = false;
    
        while(true) {
            string line = reader.ReadLine();
            if(line == null || (line.StartsWith("#:") && past_plural_forms)) {
                break;
            } else if(line.StartsWith("\"Plural-Forms:")) {
                past_plural_forms = true;
            }
            
            try {
                Match match = Regex.Match(line, ".*#(.*)<(.*)>(.*,.*|[ \n]*$)");
                if(!match.Success) {
                    match = Regex.Match(line, ".*\\\"Last-Translator:(.*)<(.*)>.*");
                    if(!match.Success) {
                        match = Regex.Match(line, ".*\\\"Language-Team:(.*)<.*>.*");
                        if(!match.Success) {
                            continue;
                        }

                        language_name = match.Groups[1].Value.Trim();
                        int pos = language_name.IndexOf('(');
                        if(pos > 0) {
                            language_name = language_name.Substring(0, pos).Trim();
                        }
                        
                        pos = language_name.IndexOf("GNOME");
                        if(pos > 0) {
                            language_name = language_name.Substring(0, pos).Trim();
                        }
                    }
                }
                
                string name = match.Groups[1].Value.Trim();
                string email = match.Groups[2].Value.Trim();
            
                if(name == String.Empty || email == String.Empty || name == "FIRST AUTHOR") {
                    continue;
                } else if(name.StartsWith("Maintainer:")) {
                    name = name.Substring(11).Trim();
                }

                if(!names.ContainsKey(email)) {
                    bool skip = false;
                    
                    foreach(string iter_name in names.Values) {
                        if(String.Compare(iter_name, name, true) == 0) {
                            skip = true;
                            break;
                        }
                    }
                    
                    if(!skip) {
                        names.Add(email, name);
                    }
                }
            } catch {
                continue;
            }
        }

        // hack for banshee, remove if used elsewhere
        if(language_code == "sr") {
            return;
        } else if(language_code == "sr@latin") {
            language_code = "sr";
        }
        // end special

        // override language names from extracted
        switch(language_code) {
            case "ast": language_name = "Asturian"; break;
            case "ca": language_name = "Catalan"; break;
            case "ca@valencia": language_name = "Catalan (Valencian)"; break;
            case "cs": language_name = "Czech"; break;
            case "de": language_name = "German"; break;
            case "fr": language_name = "French"; break;
            case "el": language_name = "Greek"; break;
            case "en_GB": language_name = "British English"; break;
		    case "gl": language_name = "Galician"; break;
            case "gu": language_name = "Gujarati"; break;
            case "he": language_name = "Hebrew"; break;
            case "id": language_name = "Indonesian"; break;
            case "ko": language_name = "Korean"; break;
            case "lt": language_name = "Lithuanian"; break;
            case "pt": language_name = "Portuguese"; break;
            case "ro": language_name = "Romanian"; break;
            case "zh_CN": language_name = "Simplified Chinese"; break;
            case "zh_HK": language_name = "Chinese (Hong Kong)"; break;
            case "zh_TW": language_name = "Chinese (Taiwan)"; break;
            default: break;
        }

        Console.WriteLine("  <language code=\"{0}\" name=\"{1}\">", language_code, language_name);
        List<string> sorted_names = new List<string> (names.Values);
        sorted_names.Sort ();
        foreach(string name in sorted_names) {
            Console.WriteLine("    <person>{0}</person>", name.Replace("\"", "&quot;"));
        }
        Console.WriteLine("  </language>");
    }
}

