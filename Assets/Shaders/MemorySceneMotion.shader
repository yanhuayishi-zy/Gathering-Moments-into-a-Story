Shader "UI/MemorySceneMotion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MotionRegion ("Motion Region", Vector) = (0,0,1,1)
        _MotionStrength ("Motion Strength", Float) = 0.004
        _MotionTime ("Motion Time", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            sampler2D _MainTex;
            float4 _Color;
            float4 _MotionRegion;
            float _MotionStrength;
            float _MotionTime;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 minimum = _MotionRegion.xy;
                float2 maximum = minimum + _MotionRegion.zw;
                float2 edge = min(input.uv - minimum, maximum - input.uv);
                float mask = smoothstep(0.0, 0.035, min(edge.x, edge.y));
                mask *= step(minimum.x, input.uv.x) * step(minimum.y, input.uv.y);
                mask *= step(input.uv.x, maximum.x) * step(input.uv.y, maximum.y);
                float vertical = saturate((input.uv.y - minimum.y) / max(_MotionRegion.w, 0.001));
                float slowSway = sin(_MotionTime * 0.82 + vertical * 5.2);
                float smallFold = sin(_MotionTime * 1.31 + vertical * 10.5) * 0.28;
                float sway = (slowSway + smallFold) * _MotionStrength * vertical * mask;
                float lift = sin(_MotionTime * 0.53 + input.uv.x * 4.0) * _MotionStrength * 0.12 * mask;
                float2 movedUv = input.uv + float2(sway, lift);
                return tex2D(_MainTex, movedUv) * input.color;
            }
            ENDCG
        }
    }
}
