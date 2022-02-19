using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

[System.Serializable]
public class LuaHighlighter
{
    private static readonly string[] KEYWORDS = 
    {
        "and", "break", "do", "else", "elseif", "end",
        "for", "function", "global", "if", "in", "local", "nil",
        "not", "or", "repeat", "return", "then", "until", "while", "arg", "self",
    };

    private static readonly string[] INTERNAL_FUNCTIONS =
    {
        "assert", "call", "collectgarbage", "copytagmethods",
        "dofile", "dostring", "error", "foreach", "foreachi",
        "getglobal", "getn", "gettagmethod", "globals", "newtag", "next",
        "print", "rawget", "rawset", "setglobal", "settagmethod",
        "sort", "tag", "tonumber", "tostring", "tinsert", "tremove", "type",
        "strbyte", "strchar", "strfind", "strlen", "strrep",
        "strsub", "strupper", "format", "gsub",
        "abs", "acos", "asin", "atan", "atan2",
        "ceil", "cos", "deg", "exp", "floor",
        "log", "log10", "max", "min", "mod",
        "rad", "sin", "sqrt", "tan", "frexp",
        "ldexp", "random", "randomseed",
        "openfile", "closefile", "readfrom", "writeto", "appendto",
        "remove", "rename", "flush", "seek", "tmpname",  "read", "write",
        "clock", "date", "execute", "exit", "getenv", "setlocale",
    };

    private readonly List<TokenPattern> patterns = new List<TokenPattern>();

    private Regex regex;

    public LuaHighlighter()
    {
        InitPatterns();
    }

    public string Highlight(string input)
    {
        var output = regex.Replace(input, MatchHandler);
        return output;
    }

    private string MatchHandler(Match match)
    {
        var pattern = patterns.Find(x => match.Groups[x.name].Success);
        if (pattern != null)
        {
            return $"<color={pattern.color}>{match.Value}</color>";
        }
        return match.Value;
    }


    private void InitPatterns()
    {
        AddPattern(@"\-\-\[\[[\w\W\s\S]*?\]\]", TokenType.MultiLineComment, "#5c6370");
        AddPattern(@"\-\-(?!(\[=*\[|\]=*\])).*\n?", TokenType.LineComment, "#5c6370");
        AddPattern(@"\[\[(?>\\.|[^\]\]]|.)*?\]\]", TokenType.MultiLineString, "#98c379");
        AddPattern(@"""(?>\\.|[^""]|.)*?""", TokenType.DoubleQuoteString, "#98c379");
        AddPattern(@"'(?>\\.|[^']|.)*?'", TokenType.SingleQuoteString, "#98c379");
        int blockCount = patterns.Count;
        AddPattern(GetKeywordsRegex(), TokenType.Keyword, "#c678dd");
        AddPattern(GetInternalFunctionsRegex(), TokenType.InternalFunction, "#56b6c2");
        AddPattern("(?<![\\d.])\\b0x[a-fA-F\\d]+|\\b\\d+(\\.\\d+)?([eE]-?\\d+)?|\\.\\d+([eE]-?\\d+)?", TokenType.Number, "#d19a66");

        var allPatterns = new StringBuilder();
        var blockPatterns = new StringBuilder();
        var wordPatterns = new StringBuilder();

        for (int i = 0; i < blockCount; i++)
        {
            if (i != 0)
            {
                blockPatterns.Append("|");
            }
            blockPatterns.AppendFormat("(?'{0}'{1})", patterns[i].name, patterns[i].regex);
        }
        for (int i = blockCount; i < patterns.Count; i++)
        {
            if (i != blockCount)
            {
                wordPatterns.Append("|");
            }
            wordPatterns.AppendFormat("(?'{0}'{1})", patterns[i].name, patterns[i].regex);
        }
        bool hasPattern = false;
        if (blockPatterns.Length > 0)
        {
            hasPattern = true;
            allPatterns.AppendFormat("(?'blocks'{0})+?", blockPatterns);
        }
        if (wordPatterns.Length > 0)
        {
            if (hasPattern)
            {
                allPatterns.Append("|");
            }
            allPatterns.AppendFormat("(?'words'{0})+?", wordPatterns);
        }

        var regexOptions = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
        regex = new Regex(allPatterns.ToString(), regexOptions, System.TimeSpan.FromSeconds(0.2));
    }

    private void AddPattern(string regex, TokenType type, string color)
    {
        var pattern = new TokenPattern()
        {
            regex = regex,
            name = type.ToString(),
            color = color,
        };
        patterns.Add(pattern);
    }

    private static string GetKeywordsRegex()
    {
        var sb = new StringBuilder();
        sb.Append(@"\b(");
        for (int i = 0; i < KEYWORDS.Length; i++)
        {
            if (i != 0)
            {
                sb.Append("|");
            }
            sb.Append(KEYWORDS[i]);
        }
        sb.Append(@")\b");
        return sb.ToString();
    }

    private static string GetInternalFunctionsRegex()
    {
        var sb = new StringBuilder();
        sb.Append(@"\b(");
        for (int i = 0; i < INTERNAL_FUNCTIONS.Length; i++)
        {
            if (i != 0)
            {
                sb.Append("|");
            }
            sb.Append(INTERNAL_FUNCTIONS[i]);
        }
        sb.Append(@")\b");
        return sb.ToString();
    }
}

public class TokenPattern
{
    public string regex;
    public string name;
    public string color;
}

public enum TokenType
{
    None,
    LineComment,
    MultiLineComment,
    DoubleQuoteString,
    SingleQuoteString,
    MultiLineString,
    Keyword,
    InternalFunction,
    Number,
}