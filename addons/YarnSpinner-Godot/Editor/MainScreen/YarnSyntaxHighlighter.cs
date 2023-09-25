using Godot;
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Godot.Collections;

namespace YarnSpinnerGodot.Editor {
    public partial class YarnSyntaxHighlighter : SyntaxHighlighter {
        private static readonly RegEx OptionCharacter = RegEx.CreateFromString("^\\s*"+
                                                                               "(?<option>->\\s*)?" +
                                                                               "(?<name>\\S+:)?" +
                                                                               "(?<main>([\\/<>]?[^#\n\\/<>]+)*)" +
                                                                               "(?<cmdstart><<" +
                                                                                    "(?<cmdname>[a-zA-Z]*)" +                                                                                            
                                                                                    "(?<cmdbody>[^<>#\\/]*)" +
                                                                                    "(?<cmdend>>{1,2}\\s*)?" +
                                                                               ")?" +
                                                                               "(?<tags>(#[^\\/]*\\/?)+)?" +
                                                                               "(?<comment>\\/{2,}.*)?" +
                                                                               "$");
        private static readonly RegEx NodeStart = RegEx.CreateFromString("^\\s*---\\s*$");
        private static readonly RegEx NodeEnd = RegEx.CreateFromString("^\\s*===\\s*$");
        private static readonly RegEx KeyValuePair = RegEx.CreateFromString("^(?<key>[a-zA-Z]*:)[a-zA-Z]*");
        private Dictionary _colors = null;
        private int _startIdx, _endIdx, _cmdStartIdx, _cmdEndIdx;
        private bool _cmdSet;
        private readonly List<int> _nodeList = new ArrayList<int>();
        public override Dictionary _GetLineSyntaxHighlighting(int line) {
            var textLine = YarnTextEditor.GetLine(line);
            _colors = new Dictionary();
            _startIdx = 0;
            _endIdx = textLine.Length;
            
            if (HandleNodeBounds(line, textLine)) {
                return _colors;
            }

            _cmdSet = false;

            if (Search(OptionCharacter, textLine, out var optCharMatch)) {
                SetColor(optCharMatch,"option","option_color");
                SetColor(optCharMatch,"name","character_color");
                _startIdx = optCharMatch.GetStart("main");
                _endIdx = optCharMatch.GetEnd("main");
                AddColor(optCharMatch.GetStart("cmdstart"),GetColor("command_color"));
                SetColor(optCharMatch,"cmdname","command_name_color");
                _cmdStartIdx = optCharMatch.GetStart("cmdbody");
                AddColor(_cmdStartIdx,GetColor("command_line_color"));
                _cmdEndIdx = optCharMatch.GetEnd("cmdbody");
                SetColor(optCharMatch,"cmdend","command_color");
                SetColor(optCharMatch,"tags","tag_color");
                SetColor(optCharMatch,"comment","comment_color");
            }
            return _colors;
        }

        private bool HandleNodeBounds(int line, string textLine) {
            var index = _nodeList.BinarySearch(line);
            bool contains = index >= 0;
            if (!contains) index = ~index;
            bool even = index % 2 == 0;
            if (Search(NodeStart, textLine, out _)) {
                if (even) {
                    AddColor(0, GetColor("node_color"));
                    if (!contains) _nodeList.Insert(index, line);
                }
                return true;
            }

            if (Search(NodeEnd, textLine, out _)) {
                if (!even) {
                    AddColor(0, GetColor("node_color"));
                    if (!contains) _nodeList.Insert(index, line);
                }

                return true;
            }
            if (contains) {
                _nodeList.RemoveAt(index);
            }
            if (even) {
                AddColor(0, GetColor("comment_color"));
                if (Search(KeyValuePair, textLine, out var match)) {
                    AddColor(match.GetStart("key"),GetColor("header_key_color"));
                    AddColor(match.GetEnd("key"),GetColor("header_value_color"));
                }
                return true;
            }
            return false;
        }


        private bool Search(RegEx regEx, string line, out RegExMatch match) {
            match = regEx.Search(line, _startIdx,_endIdx);
            return null != match;
        }
        
        
        private YarnScriptEditor YarnTextEditor => (YarnScriptEditor)GetTextEdit();

        private Color GetColor(string colorName) => YarnTextEditor.GetThemeColor(colorName);
        
        private void AddColor(int index, Color color) {
            if(index>=0) _colors[index] = new Dictionary { { "color", color } };
        }
        
        private void SetColor(RegExMatch match, string group, string color) =>  SetColor(match.GetStart(group), match.GetEnd(group), color);

        private void SetColor(RegExMatch match, string color) => SetColor(match.GetStart(), match.GetEnd(), color);

        private void SetColor(int startIndex, int endIndex, string color) {
            AddColor(startIndex,GetColor(color));
            AddColor(endIndex, _cmdStartIdx != -1 && startIndex >= _cmdStartIdx && endIndex <= _cmdEndIdx 
                ? GetColor("command_line_color") 
                : GetColor("font_color"));
        }
        
        public override void _UpdateCache() {
            base._UpdateCache();
        }
    }

}
