using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using tgAttribute = Terminal.Gui.Attribute;

namespace UICatalog.Scenarios
{
    [ScenarioMetadata(Name: "SynthColors", Description: "Demo for SynthColors")]
    [ScenarioCategory("Colors")]
    public class SynthColorsExample : Scenario
    {
        public override void Setup()
        {
            var canvas = new DrawingArea(this, Win)
            {
                X = Pos.Center(),
                Y = 4
            };
            Win.Add(canvas);

            paletteView = new PaletteArea()
            {
                X = Pos.Center(),
                Y = 24
            };
            Win.Add(paletteView);
        }

        PaletteArea paletteView;

        class DrawingArea : View
        {
            SynthColorsExample _owner;
            View _view;

            public DrawingArea(SynthColorsExample owner, View view)
            {
                _owner = owner;
                _view = view;

                Width = 16 * 5;
                Height = 16;
            }

            public override void Redraw(Rect bounds)
            {
                base.Redraw(bounds);

                int colorSum = 0;
                List<MyColor> colors = new List<MyColor>();

                for (int bg = 0; bg < PaletteBuilder.Win16Palette.Length; bg++)
                {
                    var bgc = (Color)bg;
                    TrueColor bgEntry = PaletteBuilder.Win16Palette[bg];

                    int y = bg;
                    int x = 0;

                    for (int fg = 0; fg < PaletteBuilder.Win16Palette.Length; fg++)
                    {
                        var fgc = (Color)fg;
                        TrueColor fgEntry = PaletteBuilder.Win16Palette[fg];

                        for (int i = 0; i < PaletteBuilder.Shades.Length; i++)
                        {
                            if (i == 0 && fg != 0)
                                continue;

                            var shade = PaletteBuilder.Shades[i];

                            /*if (fg == bg) {
                                fgc = Color.White;
                                bgc = Color.White;
                            }*/

                            float r = Math.Min(1.0f, bgEntry.Red * (1 - shade.weight) + fgEntry.Red * shade.weight);
                            float g = Math.Min(1.0f, bgEntry.Green * (1 - shade.weight) + fgEntry.Green * shade.weight);
                            float b = Math.Min(1.0f, bgEntry.Blue * (1 - shade.weight) + fgEntry.Blue * shade.weight);
                            colors.Add(new MyColor((byte)r, (byte)g, (byte)b, bgc, fgc, shade.chr));

                            Driver.SetAttribute(new tgAttribute(fgc, bgc));
                            AddRune(x, y, shade.chr);

                            x++;
                            colorSum++;
                        }
                    }
                }

                _view.Text = string.Format("Colors total = {0}", colorSum);
                var palette = PaletteBuilder.CreateSnakePalette(colors);
                _owner.paletteView.Colors = palette;
            }
        }

        class PaletteArea : View
        {
            private MyColor[,] colors;

            public MyColor[,] Colors
            {
                get => colors;
                set
                {
                    if (colors != value)
                    {
                        colors = value;
                        if (colors != null)
                        {
                            this.Width = this.Height = colors.GetLength(0);
                            this.SetNeedsDisplay();
                        }
                    }
                }
            }

            public PaletteArea()
            {
            }

            public override void Redraw(Rect bounds)
            {
                base.Redraw(bounds);

                if (colors == null) return;
                var size = colors.GetLength(0);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        var clr = colors[y, x];

                        Driver.SetAttribute(new tgAttribute(clr.Fg, clr.Bg));
                        AddRune(x, y, clr.Chr);
                    }
                }
            }
        }

        public struct MyColor
        {
            public byte R, G, B;
            public float H, S, L;

            public Color Bg;
            public Color Fg;
            public char Chr;

            public MyColor(byte r, byte g, byte b, Color bg, Color fg, char chr)
            {
                Bg = bg;
                Fg = fg;
                Chr = chr;

                R = r; G = g; B = b;
                (H, S, L) = PaletteBuilder.RgbToHsl(r, g, b);
            }
        }

        public static class PaletteBuilder
        {
            public static readonly TrueColor[] Win16Palette =
            {
                new TrueColor(0, 0, 0),       // Black
                new TrueColor(0, 0, 128),     // DarkBlue
                new TrueColor(0, 128, 0),     // DarkGreen
                new TrueColor(0, 128, 128),   // DarkCyan
                new TrueColor(128, 0, 0),     // DarkRed
                new TrueColor(128, 0, 128),   // DarkMagenta
                new TrueColor(128, 128, 0),   // DarkYellow/Brown (0xC1, 0x9C, 0x00)
                new TrueColor(192, 192, 192), // Gray
                new TrueColor(128, 128, 128), // DarkGray
                new TrueColor(0, 0, 255),     // Blue
                new TrueColor(0, 255, 0),     // Green
                new TrueColor(0, 255, 255),   // Cyan
                new TrueColor(255, 0, 0),     // Red
                new TrueColor(255, 0, 255),   // Magenta
                new TrueColor(255, 255, 0),   // Yellow
                new TrueColor(255, 255, 255)  // White
            };

            public static readonly (char chr, float weight)[] Shades = {
                (' ', 0.0f), ('░', 0.25f), ('▒', 0.5f), ('▓', 0.75f)
            };

            public static (float h, float s, float l) RgbToHsl(byte r, byte g, byte b)
            {
                float rf = r / 255f;
                float gf = g / 255f;
                float bf = b / 255f;

                float max = Math.Max(rf, Math.Max(gf, bf));
                float min = Math.Min(rf, Math.Min(gf, bf));

                float h = 0f;
                float s = 0f;
                float l = (max + min) / 2f;

                if (max != min)
                {
                    float d = max - min;

                    s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

                    if (max == rf)
                        h = (gf - bf) / d + (gf < bf ? 6 : 0);
                    else if (max == gf)
                        h = (bf - rf) / d + 2;
                    else
                        h = (rf - gf) / d + 4;

                    h /= 6f;
                }

                return (h, s, l);
            }

            public static MyColor[,] CreateSnakePalette(List<MyColor> colors)
            {
                if (colors == null || colors.Count == 0) return new MyColor[0, 0];

                var sorted = colors
                    .OrderBy(c => c.H)
                    .ThenBy(c => c.S)
                    .ThenBy(c => c.L)
                    .ToList();

                int side = (int)Math.Ceiling(Math.Sqrt(sorted.Count));
                MyColor[,] grid = new MyColor[side, side];

                for (int i = 0; i < sorted.Count; i++)
                {
                    int row = i / side;
                    int col = i % side;
                    int targetCol = (row % 2 == 0) ? col : (side - 1 - col);

                    grid[row, targetCol] = sorted[i];
                }

                return grid;
            }
        }
    }
}
