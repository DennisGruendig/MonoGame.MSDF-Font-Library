#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

matrix WorldViewProjection;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 TextureSize : TEXCOORD1;
    float PxRange : TEXCOORD2;
    float Smoothing : TEXCOORD3;
};

struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 TextureSize : TEXCOORD1;
    float PxRange : TEXCOORD2;
    float Smoothing : TEXCOORD3;
};

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

PixelShaderInput MainVS(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;
    output.TextureSize = input.TextureSize;
    output.PxRange = input.PxRange;
    output.Smoothing = input.Smoothing;
    return output;
}

float4 MainPS(PixelShaderInput input) : COLOR
{
    float2 coord = input.TextureCoordinate;
    float3 msd = tex2D(SpriteTextureSampler, coord).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float2 unitRange = input.PxRange / input.TextureSize;
    float2 screenTexSize = 1.0 / fwidth(coord);
    float screenPxDistance = max(0.5 * dot(unitRange, screenTexSize), 1.0) * (sd - 0.5);
    float softOpacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    float sharpOpacity = step(0.5, sd);
    float opacity = lerp(sharpOpacity, softOpacity, step(0.5, input.Smoothing));
    return lerp(0, input.Color, opacity);
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
