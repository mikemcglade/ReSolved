Shader "Custom/HellPortalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _SwirlingSpeed ("Swirling Speed", Float) = 1.0
        _DistortionStrength ("Distortion Strength", Float) = 0.1
        _ColorVariation ("Color Variation", Float) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        float _SwirlingSpeed;
        float _DistortionStrength;
        float _ColorVariation;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NoiseTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 uv = IN.uv_MainTex;
            float time = _Time.y * _SwirlingSpeed;
            
            // Apply swirling distortion
            float2 distortion = tex2D(_NoiseTex, IN.uv_NoiseTex + time).rg * 2.0 - 1.0;
            uv += distortion * _DistortionStrength;
            
            // Sample main texture with distorted UVs
            fixed4 c = tex2D(_MainTex, uv);
            
            // Add some color variation
            c.rgb += sin(time + uv.xyx * 10.0) * _ColorVariation;
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}