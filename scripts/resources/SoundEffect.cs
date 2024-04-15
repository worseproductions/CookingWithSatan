using Godot;

namespace CookingWithSatan.scripts.resources;

[GlobalClass]
[Icon("res://images/icons/AudioStreamMP3.svg")]
public partial class SoundEffect : Resource
{
    [Export(PropertyHint.Enum)] public AudioService.SoundEffectType Type { get; set; }
    [Export] public AudioStream Stream { get; set; }
    [Export] public bool UseGlobalVolume { get; set; } = true;
    [Export] public int VolumeDb { get; set; }
}