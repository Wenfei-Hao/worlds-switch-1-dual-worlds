Shader "Custom/TimeDevicePortal"
{
    Properties
    {
        _OtherWorldTex ("Other World Texture", 2D) = "black" {}
    }
    SubShader
    {
        // 让它当普通不透明几何体画就好
        Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _OtherWorldTex;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                // 用当前正在渲染的 Camera（也就是 MainCamera）的 MVP
                o.pos = UnityObjectToClipPos(v.vertex);
                // 计算屏幕空间坐标（包含 w 分量）
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 透视除法，还原成 0~1 的屏幕 UV
                float2 uv = i.screenPos.xy / i.screenPos.w;
                // 直接用这个 UV 去采样另一世界的 RenderTexture
                return tex2D(_OtherWorldTex, uv);
            }
            ENDCG
        }
    }
}
