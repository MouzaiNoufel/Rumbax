namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for audio management.
    /// </summary>
    public interface IAudioService
    {
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        bool IsMusicMuted { get; set; }
        bool IsSfxMuted { get; set; }
        
        void PlayMusic(string clipName);
        void StopMusic();
        void PlaySfx(string clipName);
        void PlaySfxOneShot(string clipName);
    }
}
