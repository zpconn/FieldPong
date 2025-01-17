sampler TextureSampler : register(s0);

float BloomThreshold;

float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original image color
    float4 c = tex2D(TextureSampler, texCoord);
    
    // Adjust it to keep only values brighter than the specified threshold
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}

technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}