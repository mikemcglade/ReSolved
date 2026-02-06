Shader "Custom/LiquidShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveAmount ("Wave Amount", Float) = 0.05
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        fixed4 _Color;
        float _WaveSpeed;
        float _WaveAmount;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            float2 uv = IN.uv_MainTex;
            uv.y += sin(uv.x * 10 + _Time.y * _WaveSpeed) * _WaveAmount;
            fixed4 c = tex2D(_MainTex, uv) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}