using ProtoBuf;
using System.Collections.Generic;

namespace BankService.Fingerprints
{
    //TODO some new fields are filled in browser
    /// <summary>
    /// Fingerprint universal? The same for mbank and ing
    /// </summary>
    [ProtoContract]
    public class Fingerprint : FingerprintBase
    {
        [ProtoMember(1)] public HostInfo Host { get; set; }
        [ProtoMember(2)] public ScreenInfo Screen { get; set; }
        [ProtoMember(3)] public LocaleInfo Locale { get; set; }
        [ProtoMember(4)] public AudioInfo Audio { get; set; }
        [ProtoMember(5)] public byte[] Fonts { get; set; }
        [ProtoMember(6)] public List<string> Plugins { get; set; } = new List<string>();
        [ProtoMember(7)] public string Canvas { get; set; }
        [ProtoMember(8)] public uint Wait { get; set; }
        [ProtoMember(9)] public MediaInfo MediaDevices { get; set; }
        [ProtoMember(10)] public List<string> Ips { get; set; } = new List<string>();
        [ProtoMember(11)] public WebGLVendorInfo WebGLVendor { get; set; }
        [ProtoMember(12)] public string WebGLCanvas { get; set; }
        [ProtoMember(13)] public List<string> WebGLExtensions { get; set; } = new List<string>();
        [ProtoMember(14)] public WebGLInfo WebGL { get; set; }
    }

    [ProtoContract]
    public class HostInfo
    {
        [ProtoMember(1)] public string Platform { get; set; }
        [ProtoMember(2)] public string CpuClass { get; set; }
        [ProtoMember(3)] public uint Memory { get; set; }
    }

    [ProtoContract]
    public class ScreenInfo
    {
        [ProtoMember(1)] public uint ViewportW { get; set; }
        [ProtoMember(2)] public uint ViewportH { get; set; }
        [ProtoMember(3)] public uint WindowW { get; set; }
        [ProtoMember(4)] public uint WindowH { get; set; }
        [ProtoMember(5)] public uint ScreenW { get; set; }
        [ProtoMember(6)] public uint ScreenH { get; set; }
        [ProtoMember(7)] public string Orientation { get; set; }
    }

    [ProtoContract]
    public class LocaleInfo
    {
        [ProtoMember(1)] public string Lang { get; set; }
        [ProtoMember(2)] public List<TzTuple> Tz { get; set; } = new List<TzTuple>();
    }

    [ProtoContract]
    public class TzTuple
    {
        [ProtoMember(1)] public int Offset { get; set; }
        [ProtoMember(2)] public uint Ts { get; set; }
    }

    [ProtoContract]
    public class AudioInfo
    {
        [ProtoMember(1)] public uint SampleRate { get; set; }
        [ProtoMember(2)] public string ChannelCntMode { get; set; }
        [ProtoMember(3)] public uint ChannelCnt { get; set; }
        [ProtoMember(4)] public uint MaxChannelCnt { get; set; }
        [ProtoMember(5)] public string ChanInterpretation { get; set; }
        [ProtoMember(6)] public uint NumberOfInputs { get; set; }
        [ProtoMember(7)] public uint NumberOfOutputs { get; set; }
        [ProtoMember(8)] public float BaseLatency { get; set; }
        [ProtoMember(9)] public uint FftSize { get; set; }
        [ProtoMember(10)] public uint FrequencyBinCount { get; set; }
        [ProtoMember(11)] public int MaxDecibels { get; set; }
        [ProtoMember(12)] public int MinDecibels { get; set; }
        [ProtoMember(13)] public float SmoothingTimeConstant { get; set; }
    }

    [ProtoContract]
    public class MediaInfo
    {
        [ProtoMember(1)] public uint Audioinput { get; set; }
        [ProtoMember(2)] public uint Videoinput { get; set; }
        [ProtoMember(3)] public uint Audiooutput { get; set; }
    }

    [ProtoContract]
    public class WebGLVendorInfo
    {
        [ProtoMember(1)] public string Vendor { get; set; }
        [ProtoMember(2)] public string Renderer { get; set; }
    }

    [ProtoContract]
    public class WebGLInfo
    {
        [ProtoMember(37)] public uint RedBits { get; set; }
        [ProtoMember(38)] public uint GreenBits { get; set; }
        [ProtoMember(39)] public uint BlueBits { get; set; }
        [ProtoMember(40)] public uint AlphaBits { get; set; }
        [ProtoMember(41)] public uint DepthBits { get; set; }
        [ProtoMember(42)] public uint StencilBits { get; set; }
        [ProtoMember(43)] public uint MaxCubeMapTextureSize { get; set; }
        [ProtoMember(44)] public uint MaxCombinedTextureImageUnits { get; set; }
        [ProtoMember(45)] public uint MaxRenderBufferSize { get; set; }
        [ProtoMember(46)] public uint MaxFragmentUniformVectors { get; set; }
        [ProtoMember(47)] public uint MaxTextureImageUnits { get; set; }
        [ProtoMember(48)] public uint MaxTextureSize { get; set; }
        [ProtoMember(49)] public uint MaxVaryingVectors { get; set; }
        [ProtoMember(50)] public uint MaxVertexAttributes { get; set; }
        [ProtoMember(51)] public uint MaxVertexTextureImageUnits { get; set; }
        [ProtoMember(52)] public uint MaxVertexUniformVectors { get; set; }
        [ProtoMember(53)] public uint AliasedLineWidthMin { get; set; }
        [ProtoMember(54)] public uint AliasedLineWidthMax { get; set; }
        [ProtoMember(55)] public uint AliasedPointSizeMin { get; set; }
        [ProtoMember(56)] public uint AliasedPointSizeMax { get; set; }
        [ProtoMember(57)] public uint MaxViewportDimensionsMin { get; set; }
        [ProtoMember(58)] public uint MaxViewportDimensionsMax { get; set; }
    }
}
