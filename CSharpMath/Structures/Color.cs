﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
#warning Replace this with TColor
  public readonly struct Color {
    public Color(byte r, byte g, byte b, byte a = 0xFF) : this() {
      R = r;
      G = g;
      B = b;
      A = a;
    }
    public Color(float r, float g, float b, float a = 1f)
      : this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f)) { }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }
    public float Rf => R / 255f;
    public float Gf => G / 255f;
    public float Bf => B / 255f;
    public float Af => A / 255f;

    public void Deconstruct(out byte r, out byte g, out byte b) {
      r = R;
      g = G;
      b = B;
    }
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a) {
      Deconstruct(out r, out g, out b);
      a = A;
    }
    public void Deconstruct(out float r, out float g, out float b) {
      r = Rf;
      g = Gf;
      b = Bf;
    }
    public void Deconstruct(out float r, out float g, out float b, out float a) {
      Deconstruct(out r, out g, out b);
      a = Af;
    }

    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object obj) => obj is Color c ? Equals(c) : false;
    public override int GetHashCode() => unchecked(R * 13 + G * 37 + B * 113 + A * 239);

    public override string ToString() {
      string ToString(byte b) => b.ToString("X").PadLeft(2, '0');
      return $"#{ToString(A)}{ToString(R)}{ToString(G)}{ToString(B)}";
    }

    public static Color? Create(string hexOrName, bool extraSweet = true) {
      if (hexOrName == null) return null;
      if (extraSweet && (hexOrName.StartsWithInvariant("#") || hexOrName.StartsWithInvariant("0x"))) return FromHexString(hexOrName);
      else if (PredefinedColors.TryGetByFirst(hexOrName.ToLowerInvariant(), out var predefined)) return predefined;
      else return null;
    }
    private static byte _fromHex1(string hex, int index) //read one hex char -> byte
      => (byte)(Convert.ToInt32(hex.Substring(index, 1), 16) * 17);
    private static byte _fromHex2(string hex, int index) //read two hex chars -> byte
      => Convert.ToByte(hex.Substring(index, 2), 16);
    private static Color? FromHexString(string hex) {
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
#warning Do an actual validity check
      try {
        switch (hex.Length) {
          case 3:
            return new Color(_fromHex1(hex, 0), _fromHex1(hex, 1), _fromHex1(hex, 2));
          case 4:
            return new Color(_fromHex1(hex, 1), _fromHex1(hex, 2), _fromHex1(hex, 3), _fromHex1(hex, 0));
          case 6:
            return new Color(_fromHex2(hex, 0), _fromHex2(hex, 2), _fromHex2(hex, 4));
          case 8:
            return new Color(_fromHex2(hex, 2), _fromHex2(hex, 4), _fromHex2(hex, 6), _fromHex2(hex, 0));
          default:
            return null;
        }
      } catch (FormatException) {
        return null;
      }
    }
    //https://en.wikibooks.org/wiki/LaTeX/Colors#Predefined_colors
    public static BiDictionary<string, Color> PredefinedColors { get; } = new BiDictionary<string, Color> {
      { "black", new Color(0, 0, 0) },
      { "blue", new Color(0, 0, 255) },
      { "brown", new Color(150, 75, 0) },
      { "cyan", new Color(0, 255, 255) },
      { "darkgray", new Color(128, 128, 128) },
      { "gray", new Color(169, 169, 169) },
      { "green", new Color(0, 128, 0) },
      { "lightgray", new Color(211, 211, 211) },
      { "lime", new Color(0, 255, 0) },
      { "magenta", new Color(255, 0, 255) },
      { "olive", new Color(128, 128, 0) },
      { "orange", new Color(255, 128, 0) },
      { "pink", new Color(255, 192, 203) },
      { "purple", new Color(128, 0, 128) },
      { "red", new Color(255, 0,0) },
      { "teal", new Color(0, 128, 128) },
      { "violet", new Color(128, 0, 255) },
      { "white", new Color(255, 255, 255) },
      { "yellow", new Color(255, 255, 0) }
    };
  }
}