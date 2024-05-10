Shader "Custom/DepthRead"
{
    Properties
    {
        _AspectRatio ("Aspect Ratio", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Blend Off
        ZTest Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
   
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //_LastCameraDepthTexture should mean aspect ratio isnt needed, but it doesnt seem to work?
            sampler2D _CameraDepthTexture;

            float _AspectRatio;
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x = ((uv.x-0.5)*_AspectRatio)+0.5;
                fixed4 col = tex2D(_CameraDepthTexture, uv);
                return col;
            }
            ENDCG
        }
    }
}