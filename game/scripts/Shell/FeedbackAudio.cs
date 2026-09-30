using Godot;

namespace BearAdventure.Shell;

public partial class FeedbackAudio : Node
{
    private AudioStreamPlayer _player=null!;
    public bool Enabled { get; set; } = true;
    public override void _Ready()
    {
        _player=new AudioStreamPlayer {VolumeDb=-18}; AddChild(_player);
    }
    public void PlayConfirm() => PlayTone(660,0.055,0.18f);
    public void PlayNotice() => PlayTone(520,0.075,0.14f);
    private void PlayTone(double frequency,double seconds,float amplitude)
    {
        if(!Enabled || _player is null) return;
        const int rate=22050; int samples=Math.Max(1,(int)(rate*seconds));
        byte[] pcm=new byte[samples*2];
        for(int i=0;i<samples;i++)
        {
            double envelope=1.0-(double)i/samples;
            short value=(short)(Math.Sin(i*2.0*Math.PI*frequency/rate)*short.MaxValue*amplitude*envelope);
            pcm[i*2]=(byte)(value&0xff); pcm[i*2+1]=(byte)((value>>8)&0xff);
        }
        var wav=new AudioStreamWav {Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Stereo=false,Data=pcm};
        _player.Stream=wav; _player.Play();
    }
}
