using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Theme
    {
        public ThemeMode Mode { get; set; }

        public ThemeStyle Style { get; set; }

        public ThemeArea Area { get; set; }

        public string Layout { get; set; }

        public string HomeUrl { get; set; }

        public string ModeColor => Mode switch
        {
            ThemeMode.Light => "#f5f5f5",
            ThemeMode.Dark => "#232234",
            _ => throw new InvalidOperationException(),
        };

        public string StyleColor => Style switch
        {
            ThemeStyle.Gradient => "#775cdc",
            ThemeStyle.Air => "#3c97fe",
            ThemeStyle.Corporate => "#26B4FF",
            ThemeStyle.Cotton => "#e84c64",
            ThemeStyle.Paper => "#17b3a3",
            ThemeStyle.Shadow => "#7b83ff",
            ThemeStyle.Soft => "#1cbb84",
            ThemeStyle.Sunrise => "#fc5a5c",
            ThemeStyle.Twitlight => "#4c84ff",
            ThemeStyle.Vibrant => "#fc5a5c",
            _ => throw new InvalidOperationException(),
        };
    }

    public enum ThemeArea
    {
        Portal,
        Store,
        Company
    }

    public enum ThemeMode
    {
        Light,
        Dark
    }

    public enum ThemeStyle
    {
        Gradient,
        Air,
        Corporate,
        Cotton,
        Paper,
        Shadow,
        Soft,
        Sunrise,
        Twitlight,
        Vibrant
    }
}
