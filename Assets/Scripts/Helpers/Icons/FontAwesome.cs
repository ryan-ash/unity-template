using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class FontAwesome : MonoBehaviour {
	public const string fontAssetName = "FontAwesome/FontAwesome";

	public int size = 128;
	public string name;
	public Color color = Color.white;

	private bool setupCompleted = false;
	private string defaultName = "fa-font-awesome";
	private string lastName;

	// Properties

	private Text IconHolder {
		get {
			if (iconHolder == null) {
				iconHolder = GetComponent<Text>();
				if (iconHolder == null) {
					iconHolder = gameObject.AddComponent<Text>() as Text;
				}
				iconHolder.font = FAFont;
				iconHolder.hideFlags = HideFlags.HideInInspector;
				iconHolder.alignment = TextAnchor.MiddleCenter;
				iconHolder.verticalOverflow = VerticalWrapMode.Overflow;
				iconHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
			}
			return iconHolder;
		}
	}
	private Text iconHolder;

	private Font FAFont {
		get {
			if (faFont == null) {
				faFont = (Font)Resources.Load(fontAssetName) as Font;
			}
			return faFont;
		}	
	}
	private static Font faFont;
	
	// MonoBehaviour Methods

	void Start() {
		UpdateTextParams();
		UpdateIcon();
	}

    void Update() {
		
		if(iconHolder == null) return;

		if (lastName != name) {
			UpdateIcon();
			lastName = name;
		}
		if (IconHolder.font != FAFont || IconHolder.fontSize != size || IconHolder.color != color)
			UpdateTextParams();
	}

	// Private Methods

	void UpdateIcon() {
		string iconHex = (!string.IsNullOrEmpty(name) && CSSParser.Icons.ContainsKey(name)) ? CSSParser.Icons[name] : CSSParser.Icons[defaultName];
		IconHolder.text = HexToChar(iconHex).ToString();
	}

	void UpdateTextParams() {		
		IconHolder.fontSize = size;
		IconHolder.color = color;
		setupCompleted = true;
	}

	// Public Methods

	public void ChangeIcon(string newIcon) {
		name = newIcon;
	} 
	public void ChangeColor(Color color) {
		this.color = color;
	}

	// Helpers

	public char HexToChar(string hex) {
    	return (char)ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }
}
