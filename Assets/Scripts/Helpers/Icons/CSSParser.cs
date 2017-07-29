using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class CSSParser : MonoBehaviour {
    
    public const string statementPattern = @"([.a-z0-9-:,]+){content:""\\([a-z0-9A-Z]+)";
    public const string classSplitPattern = @".([a-z0-9-]+):before";
    public const string cssAssetName = "FontAwesome/font-awesome.min";

    public bool rebuild = false;

    public static Dictionary<string, string> Icons {
        get {
            if (icons == null) {
                icons = new Dictionary<string, string>();
                foreach (Match match in Regex.Matches(CSS, statementPattern)){
                    foreach(Match classMatch in Regex.Matches(match.Groups[1].Value, classSplitPattern)){
                        icons[classMatch.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
            }
            return icons;
        }
    }
    private static Dictionary<string, string> icons;

    private static string CSS {
        get {
            if (css == null){
                TextAsset cssAsset = (TextAsset)Resources.Load(cssAssetName) as TextAsset;
                css = cssAsset.text;
            } 
            return css;
        }
    }
    private static string css;

    private static string[] matches;

    void Update () {
        if (rebuild) {
            icons = null;
            rebuild = false;
        }
    }        
}