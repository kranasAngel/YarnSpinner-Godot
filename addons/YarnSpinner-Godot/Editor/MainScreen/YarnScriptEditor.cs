using Godot;
using System;

namespace YarnSpinnerGodot.Editor {
	public partial class YarnScriptEditor : CodeEdit
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			YarnSyntaxHighlighter highlighter = ResourceLoader.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Editor/MainScreen/YarnSyntaxHighlighter.cs")
				.New().As<YarnSyntaxHighlighter>();
			SyntaxHighlighter = highlighter;
		}

	}	
}
