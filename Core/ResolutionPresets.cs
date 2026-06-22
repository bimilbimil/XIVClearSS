namespace XIVClearSS.Core
{
    public record ResolutionPreset(string Label, uint Width, uint Height);

    public static class ResolutionPresets
    {
        public static readonly ResolutionPreset[] Standard = new[]
        {
            new ResolutionPreset("2K", 2560, 1440),
            new ResolutionPreset("4K", 3840, 2160),
            new ResolutionPreset("8K", 7680, 4320),
        };
    }
}
